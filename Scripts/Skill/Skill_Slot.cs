using System.Collections;
using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class Skill_Slot : MonoBehaviour
{
    public Unit_AI fromUnit;
    public Unit_AI toUnit;

    public Data_Manager.SkillStruct skillStruct;

    public float areaRadius;
    public float viewAngle;

    public LayerMask targetMask, obstacleMask;

    Coroutine projectiling;

    private void Start()
    {
        SetParticleDelay();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SetFromUnit(fromUnit);
        }
    }

    public void SetFromUnit(Unit_AI _fromUnit)
    {
        fromUnit = _fromUnit;
        skillStruct = fromUnit.currentSkill;

        if (projectiling != null)
            StopCoroutine(projectiling);

        switch (projectileType)
        {
            case ProjectileType.None:
                projectiling = StartCoroutine(Projectile_Melee());
                break;

            case ProjectileType.Straight:
                projectiling = StartCoroutine(Projectile_Straight(toUnit));
                break;

            case ProjectileType.Parabola:
                float clamp = Mathf.Clamp(areaRadius, 0f, 1f);
                Vector3 targetPoint = (fromUnit.transform.position - toUnit.transform.position).normalized * clamp;
                projectiling = StartCoroutine(Projectile_Parabola(toUnit.transform.position + targetPoint));
                break;
        }
    }

    void SplashArea(Vector3 _center)
    {
        foreach (var child in Unit_AI_Manager.instance.GetUnitDict)
        {
            Unit_AI target = child.Value;
            if (VisibleTarget(target.transform) == true)
            {
                float damage = fromUnit.GetDamage;
                target.TakeDamage(fromUnit, _center, damage, skillStruct);
            }
        }
    }

    bool VisibleTarget(Transform _target)// 보이는지 확인
    {
        Transform from = projectile == null ? transform : projectile.transform;
        Vector3 offset = (_target.position - from.position);
        if (offset.magnitude < areaRadius)
        {
            if (Vector3.Angle(from.forward, offset.normalized) < viewAngle * 0.5f)// 앵글 안에 포함 되는지
            {
                float distanceToTarget = offset.magnitude;
                if (_target.gameObject.layer == targetMask)
                //if (Physics.Raycast(from.position, offset.normalized, distanceToTarget, obstacleMask) == false)
                {
                    // 부딪히는게 없으면
                    return true;
                }
            }
        }
        return false;
    }

    public enum ProjectileType
    {
        None,
        Straight,
        Parabola
    }
    public ProjectileType projectileType = ProjectileType.None;











    public Transform projectile;
    public float projectileSpeed;
    public float firingAngle;
    public GameObject hitEffect;

    IEnumerator Projectile_Melee()
    {
        projectile.gameObject.SetActive(true);
        projectile.transform.position = fromUnit.transform.position;
        projectile.transform.rotation = fromUnit.transform.rotation;
        SplashArea(projectile.transform.position);
        yield return new WaitForSeconds(1f);

        projectile.gameObject.SetActive(false);
    }

    IEnumerator Projectile_Straight(Unit_AI _toUnit)
    {
        //Skill_Start();
        projectile.gameObject.SetActive(true);
        projectile.transform.position = fromUnit.transform.position;

        bool fire = true;
        while (fire == true)
        {
            Vector3 targetPosition = new Vector3(_toUnit.transform.position.x, transform.position.y, _toUnit.transform.position.z);
            projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, targetPosition, Time.deltaTime * projectileSpeed * 0.5f);// 발사
            projectile.transform.LookAt(targetPosition);
            if ((targetPosition - projectile.transform.position).magnitude < _toUnit.GetUnitSize)
            {
                fire = false;
            }
            yield return null;
            Debug.LogWarning("Projectile_Straight");
        }

        SplashArea(projectile.transform.position);
        projectile.gameObject.SetActive(false);
        //bullet.transform.position = transform.position;

        //Skill_Impact(targetPosition);
    }

    public IEnumerator Projectile_Parabola(Vector3 _toPoint)
    {
        projectile.gameObject.SetActive(true);
        projectile.transform.position = fromUnit.transform.position;
        // 시작점과 목표점 사이의 거리 계산
        float target_Distance = (fromUnit.transform.position - _toPoint).magnitude;

        // 초기 속도 계산
        float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / projectileSpeed);

        // XZ 평면에서의 속도 계산
        float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);

        // 비행 시간 계산
        float flightDuration = target_Distance / Vx;

        // 발사 방향 설정
        projectile.transform.rotation = Quaternion.LookRotation(_toPoint - fromUnit.transform.position);

        // 비행 시간 동안 이동
        float elapse_time = 0;
        while (elapse_time < flightDuration)
        {
            elapse_time += Time.deltaTime;
            projectile.transform.Translate(0, (Vy - (projectileSpeed * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);
            yield return null;
            Debug.LogWarning("Projectile_Parabola");
        }

        SplashArea(projectile.transform.position);
        projectile.gameObject.SetActive(false);
        OnHit();
    }

    void SetParticleDelay()
    {
        float tryTime = 0;
        ParticleSystem[] setEffect = hitEffect.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < setEffect.Length; i++)
        {
            float lifeTime = setEffect[i].main.startLifetimeMultiplier;
            float delay = setEffect[i].main.startDelayMultiplier;
            float totalTime = lifeTime + delay;
            if (tryTime < totalTime)
            {
                tryTime = totalTime;
            }
        }
        particleTime = tryTime;
    }

    public float particleTime;
    public Queue<GameObject> Queue_Projectile = new Queue<GameObject>();
    public Queue<GameObject> Queue_HitEffect = new Queue<GameObject>();

    private void OnHit()
    {
        GameObject hit = InstanceHitEffect();
        StartCoroutine(OnHitDelay(hit));
    }

    GameObject InstanceHitEffect()
    {
        if (Queue_HitEffect.Count == 0)
        {
            GameObject inst = Instantiate(hitEffect, transform);
            inst.gameObject.SetActive(false);
            Queue_HitEffect.Enqueue(inst);
        }
        return Queue_HitEffect.Dequeue();
    }

    IEnumerator OnHitDelay(GameObject _hit)
    {
        _hit.gameObject.SetActive(true);
        _hit.transform.position = projectile.transform.position;
        _hit.transform.rotation = projectile.transform.rotation;
        yield return new WaitForSeconds(particleTime);

        _hit.gameObject.SetActive(false);
        Queue_HitEffect.Enqueue(_hit);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.green;
        Transform center = projectile.transform;
        Handles.DrawWireArc(center.position, Vector3.up, Vector3.forward, 360f, areaRadius);

        Vector3 viewAngleA = DirFromAngle(viewAngle * 0.5f, center);
        Vector3 viewAngleB = DirFromAngle(-viewAngle * 0.5f, center);

        Handles.DrawLine(center.position, center.position + viewAngleA * areaRadius);
        Handles.DrawLine(center.position, center.position + viewAngleB * areaRadius);
    }

    Vector3 DirFromAngle(float _angleInDegrees, Transform _trans = null)
    {
        if (_trans != null)
        {
            // 로컬 기준
            _angleInDegrees += _trans.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(_angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(_angleInDegrees * Mathf.Deg2Rad));
    }
#endif
}
