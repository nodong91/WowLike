using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Skill_Instance))]
public class Skill_Instance_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
        fontStyle.fontSize = 15;
        fontStyle.normal.textColor = Color.yellow;

        Skill_Instance Inspector = target as Skill_Instance;
        if (GUILayout.Button("Data Parse", fontStyle, GUILayout.Height(30f)))
        {
            Inspector.UpdateData();
            EditorUtility.SetDirty(Inspector);
        }
        GUILayout.Space(10f);
        base.OnInspectorGUI();
    }
}
#endif

public class Skill_Instance : MonoBehaviour
{
    public enum BulletType
    {
        Melee,
        Straight,
        Parabola
    }
    public BulletType bulletType = BulletType.Melee;

    public ParticleSystem startEffect, hitEffect;
    public Transform bullet;
    public float startDeadTime, endDeadTime;

    public delegate void DestroyDelegate(Skill_Instance bullet);
    public DestroyDelegate destroyDelegate;

    bool start, end;
    [Header("")]
    public float targetSize;
    public float bulletSpeed = 1f;
    public float damage;
    public float penetration;// 관통 퍼센트(방어력 무시) 힐인경우 치유력 방해를 무시

    public void UpdateData()
    {
        if (startEffect != null)
        {
            ParticleSystem[] temp = startEffect.GetComponentsInChildren<ParticleSystem>();
            float deadTime = 0f;
            for (int i = 0; i < temp.Length; i++)
            {
                float totalTime = temp[i].main.startLifetimeMultiplier + temp[i].main.startDelayMultiplier + temp[i].main.duration;
                if (totalTime > deadTime) deadTime = totalTime;
            }
            startDeadTime = deadTime;
        }

        if (startEffect != null)
        {
            ParticleSystem[] temp = hitEffect.GetComponentsInChildren<ParticleSystem>();
            float deadTime = 0f;
            for (int i = 0; i < temp.Length; i++)
            {
                float totalTime = temp[i].main.startLifetimeMultiplier + temp[i].main.startDelayMultiplier + temp[i].main.duration;
                if (totalTime > deadTime) deadTime = totalTime;
            }
            endDeadTime = deadTime;
        }
    }

    public void SetTarget(Transform _target, float _size)
    {
        start = true;
        end = true;
        targetSize = _size;

        switch (bulletType)
        {
            case BulletType.Melee:
                StartCoroutine(BulletMelee(_target));
                break;

            case BulletType.Straight:
                StartCoroutine(BulletStraight(_target));
                break;

            case BulletType.Parabola:

                break;
        }
    }

    IEnumerator BulletMelee(Transform _target)
    {
        Skill_Start();
        yield return null;
        Vector3 targetPosition = new Vector3(_target.position.x, bullet.transform.position.y, _target.position.z);
        Skill_Impact(targetPosition);
    }

    IEnumerator BulletStraight(Transform _target)
    {
        Skill_Start();
        bullet.gameObject.SetActive(true);

        float normalize = 0f;
        Vector3 targetPosition = default;
        bool fire = true;
        while (fire == true)
        {
            normalize += Time.deltaTime * 0.3f;
            targetPosition = new Vector3(_target.position.x, transform.position.y, _target.position.z);
            bullet.transform.position = Vector3.MoveTowards(bullet.transform.position, targetPosition, Time.deltaTime * bulletSpeed);// 발사
            bullet.transform.LookAt(targetPosition);
            if ((targetPosition - bullet.transform.position).magnitude < targetSize)
            {
                fire = false;
            }
            yield return null;
        }
        bullet.gameObject.SetActive(false);
        bullet.transform.position = transform.position;
        Skill_Impact(targetPosition);
    }

    void Skill_Start()
    {
        startEffect.transform.rotation = transform.rotation;
        StartCoroutine(StartEffectCorutine(startDeadTime));
    }

    void Skill_Impact(Vector3 _target)
    {
        Vector3 offset = (bullet.transform.position - _target).normalized;
        hitEffect.transform.rotation = Quaternion.LookRotation(offset);
        hitEffect.transform.position = _target + offset * targetSize;

        CameraManager.instance.InputShake();

        StartCoroutine(HitEffectCorutine(endDeadTime));
    }

    IEnumerator StartEffectCorutine(float _delayTime)
    {
        startEffect?.Play();
        yield return new WaitForSeconds(_delayTime);// 데미지 이펙트 사라질때까지 대기
        startEffect?.Stop();
        start = false;
    }

    IEnumerator HitEffectCorutine(float _delayTime)
    {
        hitEffect?.Play();
        yield return new WaitForSeconds(_delayTime);// 데미지 이펙트 사라질때까지 대기
        hitEffect?.Stop();
        Debug.LogWarning("uhiuhilufhajshdfljkhalkjhsdlfuiehfahnf");
        destroyDelegate?.Invoke(this);
        end = false;
    }
}
