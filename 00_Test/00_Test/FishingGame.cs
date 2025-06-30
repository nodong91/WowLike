using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FishingGame : MonoBehaviour
{
    public enum FishingState
    {
        None,
        Bobber,
        Hit,
        Complate
    }
    public FishingState state;
    public TMPro.TMP_Text stateText;
    public Image fishingRod, hitPointImage, fishObject, health;
    public Transform fish;
    public ParticleSystem targetParticles;
    public Image bobber;// ������
    public GameObject hitImage;

    public bool hitBool = false;
    float fillAmount;
    float fillAngle;
    float targetAngle;
    Coroutine actionCoroutine;

    bool fishingAction;
    float hitPoint;
    float bobberSpeed;
    float currentSpeed = 0f;
    float fishPower;
    float complatePoint;// �Ϸ� �ۼ�Ʈ

    [Header("[ ���� ������ ��ġ ]")]
    public FishingLodStruct fishingLodStruct;
    [System.Serializable]
    public struct FishingLodStruct
    {
        public float fishingAmount;
        public float lodPower;// �ʴ� �������� �� - ���� ���� ���� ������
        public float reelingSpeed;// ���� ȸ�� �ӵ�
        public float reelingSlip;// ���� ���� �� �и�
        public float hitPoint;// ����� ���� ��ġ
        public float hitBobberSpeed;// ����� �� ������
    }

    public FishStruct fishStruct;
    [System.Serializable]
    public struct FishStruct
    {
        public int fishLevel;
        public float fishPower;// �ʴ� �������� �� - ���� ���� ���� ������
        public float fishAddAngle;// ����� �̵� ����
        public float fishSpeed;
        public Vector2 fishDelay;// ���� �ٲ�� ������ �ð�
        public float fishBobberLength;// �ּ� 0~1����
    }

    void Start()
    {
        fishingRod.material = Instantiate(fishingRod.material);
        hitPointImage.material = Instantiate(hitPointImage.material);
        health.material = Instantiate(health.material);
        bobber.material = Instantiate(bobber.material);
        StateMachine(FishingState.None);
    }

    void Update()
    {
        switch (state)
        {
            case FishingState.None:
                if (Input.GetMouseButtonDown(0))
                {
                    // ���� ������ ��ġ
                    StateMachine(FishingState.Bobber);
                }
                break;

            case FishingState.Bobber:
                if (Input.GetMouseButtonDown(0))
                {
                    if (hitBool == true)
                    {
                        StateMachine(FishingState.Hit);
                    }
                    else
                    {
                        StateMachine(FishingState.None);
                    }
                }
                break;

            case FishingState.Hit:
                if (Input.GetMouseButtonDown(0))
                {
                    RotateTarget(fishingLodStruct.reelingSpeed);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    RotateTarget(0);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    RotateTarget(-fishingLodStruct.reelingSpeed);
                }
                else if (Input.GetMouseButtonUp(1))
                {
                    RotateTarget(0);
                }
                break;
        }
    }

    public void ResetGame(FishingLodStruct _fishingLodStruct, FishStruct _fishStruct)
    {
        fishingLodStruct = _fishingLodStruct;
        fishStruct = _fishStruct;
        StateMachine(FishingState.None);
    }

    void StateMachine(FishingState _state)
    {
        state = _state;
        stateText.text = _state.ToString();
        if (actionCoroutine != null)
            StopCoroutine(actionCoroutine);

        switch (state)
        {
            case FishingState.None:
                StateNone();
                break;

            case FishingState.Bobber:
                StateBobber();
                break;

            case FishingState.Hit:
                StateFishing();
                break;

            case FishingState.Complate:
                StateComplate();
                break;
        }
    }

    void StateNone()
    {
        SetFishing();
        hitPointImage.material.SetFloat("_FillAmount", 1f - hitPoint);
    }

    void SetFishing()
    {
        fillAmount = fishingLodStruct.fishingAmount;
        hitPoint = fishingLodStruct.hitPoint;
        bobberSpeed = fishingLodStruct.hitBobberSpeed - (fishStruct.fishLevel * 0.1f);
        fishPower = fishingLodStruct.lodPower + fishStruct.fishPower;
    }

    //==================================================================================================================================
    // ��Ʈ����Ʈ
    //==================================================================================================================================

    void StateBobber()
    {
        actionCoroutine = StartCoroutine(BobberMovement());
    }

    IEnumerator BobberMovement()
    {
        hitImage.SetActive(false);
        hitBool = false;
        float runningTime = 0f;
        float yPos = 0f;
        bool bobbering = true;
        float length = 0f;

        while (bobbering == true)
        {
            runningTime += Time.deltaTime * bobberSpeed;
            float mathfSin = (Mathf.Sin(runningTime) + 1f) / 2f;
            yPos = mathfSin * length;
            if (yPos > hitPoint)
            {
                hitBool = true;
                hitImage.SetActive(true);
            }
            bobber.material.SetFloat("_FillAmount", yPos);
            yield return null;

            Debug.LogWarning(mathfSin);
            if (yPos < 0.0001f)
            {
                length = Random.Range(fishStruct.fishBobberLength, 1f);
            }

            if (hitBool == true && yPos < hitPoint)
            {
                StateMachine(FishingState.None);
            }
        }
    }

    //==================================================================================================================================
    // ����
    //==================================================================================================================================

    private void StateFishing()
    {
        complatePoint = 0.1f;// �⺻ ����Ʈ 10%
        fish.rotation = Quaternion.Euler(0f, 0f, 0f);
        fishingRod.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        StartCoroutine(FishPosition());
        RotateTarget(0);
    }

    void RotateTarget(float _targetSpeed)
    {
        if (actionCoroutine != null)
            StopCoroutine(actionCoroutine);
        actionCoroutine = StartCoroutine(RotateTargeting(_targetSpeed));
    }

    IEnumerator RotateTargeting(float _targetSpeed)
    {
        while (fishingAction == true)
        {
            fish.rotation = Quaternion.Slerp(fish.rotation, Quaternion.Euler(0, 0, targetAngle), fishStruct.fishSpeed);// ����� ������ ����

            currentSpeed = Mathf.Lerp(currentSpeed, _targetSpeed, fishingLodStruct.reelingSlip);// ���ӵ�
            fishingRod.transform.rotation = Quaternion.Euler(0, 0, fishingRod.transform.rotation.eulerAngles.z + currentSpeed * Time.deltaTime);

            health.material.SetFloat("_FillAmount", complatePoint);

            fillAngle = 180f * fillAmount;
            fishingRod.material.SetFloat("_FillAmount", fillAmount);
            fishingRod.material.SetFloat("_RotateAngle", fillAngle + 180f);

            var main = targetParticles.main;
            if (VisibleTarget() == true)
            {
                main.startColor = Color.green;
                if (complatePoint < 1f)
                {
                    complatePoint += fishPower * Time.deltaTime;
                }
                else
                {
                    StateMachine(FishingState.Complate);
                }
            }
            else
            {
                main.startColor = Color.red;
                if (complatePoint > 0f)
                {
                    complatePoint -= fishPower * Time.deltaTime;
                }
                else
                {
                    fishingAction = false;
                    StateMachine(FishingState.None);
                }

                if (fillAmount > 0.0f)
                {
                    fillAmount -= 0.01f * Time.deltaTime;
                }
                else
                {
                    fishingAction = false;
                    StateMachine(FishingState.None);
                }
            }
            complatePoint = Mathf.Clamp01((float)complatePoint);
            yield return null;
        }
    }

    IEnumerator FishPosition()
    {
        fishingAction = true;
        while (fishingAction == true)
        {
            float addAngle = Random.Range(-fishStruct.fishAddAngle, fishStruct.fishAddAngle);
            targetAngle += addAngle;
            float random = Random.Range(fishStruct.fishDelay.x, fishStruct.fishDelay.y);
            yield return new WaitForSeconds(random);
        }
    }

    bool VisibleTarget()// ���̴��� Ȯ��
    {
        Vector3 offset = (fishObject.transform.position - fishingRod.transform.position);
        //Debug.LogWarning(Vector3.Angle(test.transform.up, offset.normalized));
        if (Vector3.Angle(fishingRod.transform.up, offset.normalized) < fillAngle)// �ޱ� �ȿ� ���� �Ǵ���
        {
            return true;
        }
        return false;
    }

    void StateComplate()
    {
        actionCoroutine = StartCoroutine(FishingComplate());
    }

    IEnumerator FishingComplate()
    {
        int index = 0;
        while (index < 3)
        {
            index++;
            stateText.text = index.ToString();
            yield return new WaitForSeconds(1f);
        }
        StateMachine(FishingState.None);
    }

    //==================================================================================================================================
    // �����
    //==================================================================================================================================

    public float arcSize;
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.green;
        Handles.DrawWireArc(fishingRod.transform.position, Vector3.forward, Vector3.up, 360f, arcSize);

        Vector3 viewAngleA = DirFromAngle(fillAngle, fishingRod.transform);
        Vector3 viewAngleB = DirFromAngle(-fillAngle, fishingRod.transform);

        Handles.DrawLine(fishingRod.transform.position, fishingRod.transform.position + viewAngleA * arcSize);
        Handles.DrawLine(fishingRod.transform.position, fishingRod.transform.position + viewAngleB * arcSize);

        Handles.DrawLine(fishObject.transform.position, fishingRod.transform.position);
    }

    Vector3 DirFromAngle(float _angleInDegrees, Transform _trans = null)
    {
        if (_trans != null)
        {
            // ���� ����
            _angleInDegrees += _trans.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(_angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(_angleInDegrees * Mathf.Deg2Rad), 0);
    }
#endif
}