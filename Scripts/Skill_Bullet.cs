using System.Collections;
using TMPro;
using UnityEngine;

public class Skill_Bullet : MonoBehaviour
{
    public Transform bullet;
    public Transform damageEffect;
    public float targetSize, bulletSpeed = 1f;

    public void SetTarget(Transform _target, float _size)
    {
        targetSize = _size;
        StartCoroutine(BulletUpdate(_target));
    }

    IEnumerator BulletUpdate(Transform _target)
    {
        bool fire = true;
        damageEffect.gameObject.SetActive(false);
        bullet.gameObject.SetActive(true);
        Vector3 targetPosition = default;
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
        damageEffect.gameObject.SetActive(true);
        bullet.gameObject.SetActive(false);

        Vector3 offset = (targetPosition - bullet.transform.position).normalized;
        damageEffect.rotation = Quaternion.LookRotation(offset);
        damageEffect.position = targetPosition - offset * targetSize;

        yield return new WaitForSeconds(1f);// 데미지 이펙트 사라질때까지 대기

        // 끝
        damageEffect.gameObject.SetActive(false);
    }
}
