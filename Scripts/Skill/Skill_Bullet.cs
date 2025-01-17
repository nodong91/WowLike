using System.Collections;
using TMPro;
using UnityEngine;
using static Skill_Bullet;

public class Skill_Bullet : MonoBehaviour
{
    public enum BulletType
    {
        Melee,
        Beeline,
        Parabola
    }
    public BulletType bulletType = BulletType.Melee;
    public Transform bullet;
    public Transform hitEffect;
    public float targetSize, bulletSpeed = 1f;

    public delegate void BulletDelegate(Skill_Bullet bullet);
    public BulletDelegate bulletDelegate;

    public void SetTarget(Transform _target, float _size)
    {
        targetSize = _size;
        switch (bulletType)
        {
            case BulletType.Melee:
                StartCoroutine(BulletMelee(_target));
                break;

            case BulletType.Beeline:
                StartCoroutine(BulletBeeline(_target));
                break;

            case BulletType.Parabola:

                break;
        }
    }

    IEnumerator BulletMelee(Transform _target)
    {
        hitEffect.gameObject.SetActive(false);
        bullet.gameObject.SetActive(true);

        //bool fire = true;
        //while (fire == true)
        //{
        //    yield return null;
        //}
        yield return new WaitForSeconds(0.1f);// 데미지 이펙트 사라질때까지 대기

        hitEffect.gameObject.SetActive(true);
        bullet.gameObject.SetActive(false);

        Vector3 targetPosition = new Vector3(_target.position.x, bullet.transform.position.y, _target.position.z);
        Vector3 offset = (targetPosition - bullet.transform.position).normalized;
        hitEffect.rotation = Quaternion.LookRotation(offset);
        hitEffect.position = targetPosition - offset * targetSize;

        yield return new WaitForSeconds(1f);// 데미지 이펙트 사라질때까지 대기
        // 끝
        hitEffect.gameObject.SetActive(false);
        bulletDelegate?.Invoke(this);
    }

    IEnumerator BulletBeeline(Transform _target)
    {
        hitEffect.gameObject.SetActive(false);
        bullet.gameObject.SetActive(true);
        Vector3 targetPosition = default;

        bool fire = true;
        while (fire == true)
        {
            targetPosition = new Vector3(_target.position.x, bullet.transform.position.y, _target.position.z);
            if ((targetPosition - bullet.transform.position).magnitude < targetSize)
            {
                fire = false;
            }
            bullet.transform.position = Vector3.Lerp(bullet.transform.position, _target.position, Time.deltaTime * bulletSpeed);// 발사
            yield return null;
        }
        hitEffect.gameObject.SetActive(true);
        bullet.gameObject.SetActive(false);

        Vector3 offset = (targetPosition - bullet.transform.position).normalized;
        hitEffect.rotation = Quaternion.LookRotation(offset);
        hitEffect.position = targetPosition - offset * targetSize;

        yield return new WaitForSeconds(1f);// 데미지 이펙트 사라질때까지 대기

        // 끝
        hitEffect.gameObject.SetActive(false);
        bulletDelegate?.Invoke(this);
    }
}
