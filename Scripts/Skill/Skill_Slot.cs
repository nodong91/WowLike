using System.Collections;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Skill_Slot : MonoBehaviour
{
    Unit_AI fromUnit, toUnit;

    Data_Manager.SkillStruct skillStruct;
    public LayerMask targetMask;

    public enum ProjectileType
    {
        None,
        Straight,
        Parabola
    }
    public ProjectileType projectileType = ProjectileType.None;

    public ParticleSystem projectile;
    public ParticleSystem hitEffect;

    float particleLife;
    float skillArea;// 타격 했을 때 거리
    public float viewAngle;// 타격 했을 때 각도
    public float projectileSpeed;// 탄이 있을 때 탄 속도
    const float firingAngle = 50f;// 포물선일때 날아가는 각도

    public Queue<ParticleSystem> Queue_Projectile = new Queue<ParticleSystem>();
    public Queue<ParticleSystem> Queue_HitEffect = new Queue<ParticleSystem>();
    Transform lastProjectile;

    public void SetSkillSlot(Unit_AI _fromUnit)
    {
        fromUnit = _fromUnit;
        targetMask = (1 << fromUnit.gameObject.layer);
        skillStruct = fromUnit.currentSkill.skillStruct;
        SetParticleDelay();
    }
    void SetParticleDelay()
    {
        if (hitEffect == null)
            return;

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
        particleLife = tryTime;
    }

    public void SetAction(Unit_AI _toUnit)
    {
        toUnit = _toUnit;
        if (projectile == null)
            return;

        switch (projectileType)
        {
            case ProjectileType.None:
                StartCoroutine(Projectile_Melee(toUnit));
                break;

            case ProjectileType.Straight:
                StartCoroutine(Projectile_Straight(toUnit));
                break;

            case ProjectileType.Parabola:
                StartCoroutine(Projectile_Parabola(toUnit));
                break;
        }
    }

    void TargetHIt(Unit_AI _target, Transform _lastProjcetile)
    {
        float damage = fromUnit.GetDamage;
        _target.TakeDamage(fromUnit, _lastProjcetile.position, damage, skillStruct);
    }

    void SplashArea(Transform _lastProjcetile)
    {
        foreach (var child in Unit_AI_Manager.instance.GetUnitDict)
        {
            Unit_AI target = child.Value;
            if (VisibleTarget(_lastProjcetile, target) == true)
            {
                Debug.LogWarning($"Hit Unit : {target.name}");
                float damage = fromUnit.GetDamage;
                target.TakeDamage(fromUnit, _lastProjcetile.position, damage, skillStruct);
            }
        }
    }

    bool VisibleTarget(Transform _lastProjcetile, Unit_AI _target)// 보이는지 확인
    {
        Vector3 offset = (_target.transform.position - _lastProjcetile.position);
        float area = (projectileType == ProjectileType.None) ? fromUnit.GetSkillRange.y + fromUnit.GetUnitSize : 0f;
        skillArea = area + skillStruct.splashRange;
        if (offset.magnitude < skillArea + _target.GetUnitSize)
        {
            if (Vector3.Angle(_lastProjcetile.forward, offset.normalized) < viewAngle * 0.5f)// 앵글 안에 포함 되는지
            {
                return ((targetMask & (1 << _target.gameObject.layer)) == 0);// 같은 레이어가 아니면
                //if (Physics.Raycast(from.position, offset.normalized, distanceToTarget, obstacleMask) == false)
                //{
                //    return true;
                //}
            }
        }
        return false;
    }










    ParticleSystem InstanceProjectile()
    {
        if (Queue_Projectile.Count == 0)
        {
            ParticleSystem inst = Instantiate(projectile, transform);
            Queue_Projectile.Enqueue(inst);
        }
        return Queue_Projectile.Dequeue();
    }

    IEnumerator Projectile_Melee(Unit_AI _toUnit)
    {
        ParticleSystem inst = InstanceProjectile();
        inst.transform.position = fromUnit.transform.position;
        inst.transform.rotation = fromUnit.transform.rotation;
        yield return null;

        inst.gameObject.SetActive(true);
        inst.Play();

        SetHit(_toUnit, inst.transform);
        OnHitEffect(inst.transform);
        yield return new WaitForSeconds(1f);

        inst.gameObject.SetActive(false);
        Queue_Projectile.Enqueue(inst);
    }

    private void SetHit(Unit_AI _unit, Transform _projectile)
    {
        if (skillStruct.splashRange > 0)
        {
            SplashArea(_projectile);
        }
        else
        {
            TargetHIt(_unit, _projectile);
        }
    }

    IEnumerator Projectile_Straight(Unit_AI _toUnit)
    {
        ParticleSystem inst = InstanceProjectile();
        inst.transform.position = fromUnit.transform.position;
        inst.transform.rotation = fromUnit.transform.rotation;
        yield return null;

        inst.gameObject.SetActive(true);
        inst.Play();

        bool fire = true;
        while (fire == true)
        {
            Vector3 targetPosition = new Vector3(_toUnit.transform.position.x, transform.position.y, _toUnit.transform.position.z);
            inst.transform.position = Vector3.MoveTowards(inst.transform.position, targetPosition, Time.deltaTime * projectileSpeed * 0.5f);// 발사
            inst.transform.LookAt(targetPosition);
            if ((inst.transform.position - targetPosition).magnitude < _toUnit.GetUnitSize)
            {
                fire = false;
            }
            yield return null;
        }

        if (skillStruct.splashRange > 0)
        {
            SplashArea(inst.transform);
        }
        else
        {
            TargetHIt(_toUnit, inst.transform);
        }

        OnHitEffect(inst.transform);
        inst.gameObject.SetActive(false);
        Queue_Projectile.Enqueue(inst);
    }

    public IEnumerator Projectile_Parabola(Unit_AI _toUnit)
    {
        ParticleSystem inst = InstanceProjectile();
        inst.transform.position = fromUnit.transform.position;
        inst.transform.rotation = fromUnit.transform.rotation;
        yield return null;

        inst.gameObject.SetActive(true);
        inst.Play();

        float clamp = Mathf.Clamp(skillArea, 0f, 0.5f);
        Vector3 offset = (fromUnit.transform.position - toUnit.transform.position).normalized * clamp;
        Vector3 targetPoint = _toUnit.transform.position + offset;

        // 시작점과 목표점 사이의 거리 계산
        float target_Distance = (fromUnit.transform.position - targetPoint).magnitude;

        // 초기 속도 계산
        float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / projectileSpeed);

        // XZ 평면에서의 속도 계산
        float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);

        // 비행 시간 계산
        float flightDuration = target_Distance / Vx;

        // 발사 방향 설정
        inst.transform.rotation = Quaternion.LookRotation(targetPoint - fromUnit.transform.position);

        // 비행 시간 동안 이동
        float elapse_time = 0;
        while (elapse_time < flightDuration)
        {
            elapse_time += Time.deltaTime;
            inst.transform.Translate(0, (Vy - (projectileSpeed * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);
            yield return null;
        }

        if (skillStruct.splashRange > 0)
        {
            SplashArea(inst.transform);
        }
        else
        {
            TargetHIt(_toUnit, inst.transform);
        }

        OnHitEffect(inst.transform);
        inst.gameObject.SetActive(false);
        Queue_Projectile.Enqueue(inst);
    }

    private void OnHitEffect(Transform _trans)
    {
        if (hitEffect == null)
            return;

        lastProjectile = _trans;
        StartCoroutine(OnHitDelay());
    }

    ParticleSystem InstanceHitEffect()
    {
        if (Queue_HitEffect.Count == 0)
        {
            ParticleSystem inst = Instantiate(hitEffect, transform);
            Queue_HitEffect.Enqueue(inst);
        }
        return Queue_HitEffect.Dequeue();
    }

    IEnumerator OnHitDelay()
    {
        ParticleSystem inst = InstanceHitEffect();
        inst.transform.position = lastProjectile.position;
        inst.transform.rotation = lastProjectile.rotation;
        yield return null;

        inst.gameObject.SetActive(true);
        inst.Play();

        yield return new WaitForSeconds(particleLife);

        inst.gameObject.SetActive(false);
        Queue_HitEffect.Enqueue(inst);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (lastProjectile == null)
            return;

        Handles.color = Color.green;
        Handles.DrawWireArc(lastProjectile.position, Vector3.up, Vector3.forward, 360f, skillArea);

        Vector3 viewAngleA = DirFromAngle(viewAngle * 0.5f, lastProjectile);
        Vector3 viewAngleB = DirFromAngle(-viewAngle * 0.5f, lastProjectile);

        Handles.DrawLine(lastProjectile.position, lastProjectile.position + viewAngleA * skillArea);
        Handles.DrawLine(lastProjectile.position, lastProjectile.position + viewAngleB * skillArea);
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
