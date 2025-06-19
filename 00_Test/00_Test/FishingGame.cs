using NUnit.Framework.Internal;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FishingGame : MonoBehaviour
{
    public Image test, fishObject, health;
    public Transform fish;
    [Range(0f, 1f)]
    public float fillAmount;
    public float rotateAngle;
    public float target;
    public ParticleSystem targetParticles;

    void Start()
    {
        test.material = Instantiate(test.material);
        health.material = Instantiate(health.material);
        bobber.material = Instantiate(bobber.material);
        healthPoint = 0f;
        StartCoroutine(FishPosition());
        BobberMovement();
    }
    public float rotateSpeed = 10f;
    public float healthPoint;
    public float currentSpeed;
    Coroutine speedCoroutine;
    public Image bobber;// 낚시찌

    void Update()
    {
        health.material.SetFloat("_FillAmount", healthPoint);

        rotateAngle = 180f * fillAmount;
        test.material.SetFloat("_FillAmount", fillAmount);
        test.material.SetFloat("_RotateAngle", rotateAngle + 180f);

        fish.rotation = Quaternion.Slerp(fish.rotation, Quaternion.Euler(0, 0, target), 0.01f);
        if (VisibleTarget() == true)
        {
            if (healthPoint < 1f)
                healthPoint += 0.1f * Time.deltaTime;
            var main = targetParticles.main;
            main.startColor = Color.green;
        }
        else
        {
            if (healthPoint > 0f)
                healthPoint -= 0.1f * Time.deltaTime;
            if (fillAmount > 0.2f)
                fillAmount -= 0.1f * Time.deltaTime;
            var main = targetParticles.main;
            main.startColor = Color.red;
        }
        healthPoint = Mathf.Clamp01((float)healthPoint);

        if (Input.GetMouseButtonDown(0))
        {
            RotateTarget(rotateSpeed);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            RotateTarget(0);
        }

        if (Input.GetMouseButtonDown(1))
        {
            RotateTarget(-rotateSpeed);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            RotateTarget(0);
        }
    }

    void RotateTarget(float _targetSpeed)
    {
        if (speedCoroutine != null)
            StopCoroutine(speedCoroutine);
        speedCoroutine = StartCoroutine(RotateTargeting(_targetSpeed));
    }


    IEnumerator RotateTargeting(float _targetSpeed)
    {
        while (true)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, _targetSpeed, 0.01f);
            test.transform.rotation = Quaternion.Euler(0, 0, test.transform.rotation.eulerAngles.z + currentSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator FishPosition()
    {
        while (true)
        {
            float aaa = Random.Range(-100, 100f);
            target += aaa;
            float random = Random.Range(1f, 3f);
            yield return new WaitForSeconds(random);
        }
    }

    bool VisibleTarget()// 보이는지 확인
    {
        Vector3 offset = (fishObject.transform.position - test.transform.position);
        //Debug.LogWarning(Vector3.Angle(test.transform.up, offset.normalized));
        if (Vector3.Angle(test.transform.up, offset.normalized) < rotateAngle)// 앵글 안에 포함 되는지
        {
            return true;
        }
        return false;
    }

    Coroutine bobberCoroutine;
    public GameObject hitImage;
    public bool hit;
    public float yPos;
    void BobberMovement()
    {
        if (bobberCoroutine != null)
            StopCoroutine(bobberCoroutine);
        bobberCoroutine = StartCoroutine(BobberMoving());
    }

    IEnumerator BobberMoving()
    {
        hitImage.SetActive(false);
        float runningTime = 0f;
        float speed = 1f;
        float length = 0f;
        bool setting = true;
        float hitPoint = 0.65f;
        while (setting == true)
        {
            if (yPos <= 0f)
            {
                length = Random.Range(0.3f, 1f);
                Debug.LogWarning(length);
            }

            runningTime += Time.deltaTime * speed;
            yPos = Mathf.Sin(runningTime) * length;
            if (yPos > hitPoint)
            {
                hit = true;
                hitImage.SetActive(true);
            }
            bobber.material.SetFloat("_FillAmount", yPos);
            yield return null;
            if (hit == true && yPos < hitPoint)
            {
                setting = false;
            }
        }
    }

    public float arcSize;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.green;
        Handles.DrawWireArc(test.transform.position, Vector3.forward, Vector3.up, 360f, arcSize);

        Vector3 viewAngleA = DirFromAngle(rotateAngle, test.transform);
        Vector3 viewAngleB = DirFromAngle(-rotateAngle, test.transform);

        Handles.DrawLine(test.transform.position, test.transform.position + viewAngleA * arcSize);
        Handles.DrawLine(test.transform.position, test.transform.position + viewAngleB * arcSize);

        Handles.DrawLine(fishObject.transform.position, test.transform.position);
    }

    Vector3 DirFromAngle(float _angleInDegrees, Transform _trans = null)
    {
        if (_trans != null)
        {
            // 로컬 기준
            _angleInDegrees += _trans.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(_angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(_angleInDegrees * Mathf.Deg2Rad), 0);
    }
#endif
}