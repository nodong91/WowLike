using System.Collections;
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting;
using UnityEngine;
using static RewardMaterial;

public class RewardMaterial : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TMPro.TMP_Text rewardText;

    public AnimationCurve animationCurve;
    public GameObject target;
    public float delayTime;

    public delegate void MovingOver(RewardMaterial reward);
    public MovingOver movingOver;

    public void Reward_Train(Vector3 _prevPoint, GameObject _target)
    {
        target = _target;
        rewardText.gameObject.SetActive(false);
        StartCoroutine(RewardMoving(_prevPoint, target.transform.position));
    }

    IEnumerator RewardMoving(Vector3 _prevPoint, Vector3 _targetPoint)
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime / delayTime;
            float curve = animationCurve.Evaluate(normalize);
            Vector3 targetPoint = Vector3.Lerp(_prevPoint, _targetPoint, curve);
            //Vector3 targetPoint = Vector3.Slerp(_prevPoint, _targetPoint, curve);
            canvasGroup.transform.position = targetPoint;
            canvasGroup.alpha = 1f - curve;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        movingOver?.Invoke(this);
    }

    public void Reward_Up(Vector3 _prevPoint, int _num)
    {
        Vector3 targetPoint = _prevPoint + Vector3.up * 100f;
        rewardText.gameObject.SetActive(true);
        rewardText.text = "+" + _num.ToString();
        StartCoroutine(RewardMoving(_prevPoint, targetPoint));
    }

    public void Reward_Num(Vector3 _prevPoint, int _num)
    {
        rewardText.gameObject.SetActive(true);
        StartCoroutine(RewardNum(_prevPoint, _num));
    }

    IEnumerator RewardNum(Vector3 _prevPoint, int _num)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.transform.position = _prevPoint;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime / delayTime;
            int index = Mathf.RoundToInt((1f - normalize) * _num);
            rewardText.text = Mathf.Clamp(index, 0, _num).ToString();
            yield return null;
        }

        normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime / delayTime;
            canvasGroup.alpha = 1f - normalize;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        movingOver?.Invoke(this);
    }

    public void Reward_Boom(Vector3 _prevPoint, GameObject _target)
    {
        target = _target;
        rewardText.gameObject.SetActive(false);
        StartCoroutine(RewardBoom(_prevPoint));
    }

    IEnumerator RewardBoom(Vector3 _prevPoint)
    {
        Vector3 randomCircle = Random.insideUnitCircle * 300f;

        canvasGroup.alpha = 1f;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 5f;
            float curve = animationCurve.Evaluate(normalize);
            canvasGroup.transform.position = Vector3.Lerp(_prevPoint, _prevPoint + randomCircle, curve);
            yield return null;
        }
        Reward_Train(canvasGroup.transform.position, target);
    }
    public void Reward_Parabola(Vector3 _prevPoint, GameObject _target, float _speed, float _firingAngle)
    {
        target = _target;
        rewardText.gameObject.SetActive(false);
        StartCoroutine(RewardParabola(_prevPoint, _speed, _firingAngle));
        //StartCoroutine(RewardParabola(_prevPoint));
    }

    public IEnumerator RewardParabola(Vector3 _prevPoint, float _speed, float _firingAngle)
    {
        transform.position = _prevPoint;
        canvasGroup.alpha = 1f;

        Vector3 offset = (_prevPoint - target.transform.position).normalized;
        Vector3 targetPoint = target.transform.position + offset;

        // 시작점과 목표점 사이의 거리 계산
        float target_Distance = (_prevPoint - targetPoint).magnitude;

        // 초기 속도 계산
        float projectile_Velocity = target_Distance / (Mathf.Sin(2 * _firingAngle * Mathf.Deg2Rad) / _speed);

        // XZ 평면에서의 속도 계산
        float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(_firingAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(_firingAngle * Mathf.Deg2Rad);

        // 비행 시간 계산
        float flightDuration = target_Distance / Vx;

        // 발사 방향 설정
        //transform.rotation = Quaternion.LookRotation(_targetPoint - transform.position);

        // 비행 시간 동안 이동
        float elapse_time = 0;
        while (elapse_time < flightDuration)
        {
            elapse_time += Time.deltaTime;
            float x = Vx * Time.deltaTime;
            float y = (Vy - (_speed * elapse_time)) * Time.deltaTime;
            float z = 0f;
            transform.Translate(x, y, z);
            yield return null;
        }
        //canvasGroup.alpha = 0f;
        //movingOver?.Invoke(this);
    }
}
