using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public CinemachineCamera cinemachineCamera;
    public Transform target;
    CinemachineOrbitalFollow orbitalFollow;
    CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;

    Vector2 prevInput;
    Vector2 currentInput;
    float currentX, currentY;
    const float rotateSpeed = 0.1f;

    Coroutine stoping, zooming, shaking;
    Coroutine onRotate;
    float x, y;
    public float smoothSpeed = 10f;
    public float shakeValue = 5f;
    public float shakeTime = 1f;

    public delegate void RotateDelegate();
    public RotateDelegate rotateDelegate, stopRotateDelegate;

    public delegate void DelegateInputScroll(float _input);
    public DelegateInputScroll delegateInputScroll;

    public static CameraManager current;

    private void Awake()
    {
        current = this;
        delegateInputScroll = InputScroll;
    }

    void Start()
    {
        orbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
        cinemachineBasicMultiChannelPerlin = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        GetTarget(target);
    }

    public void GetTarget(Transform _trans)
    {
        cinemachineCamera.Target.TrackingTarget = _trans;
    }

    public void InputRotate(bool _input)
    {
        if (onRotate != null)
            StopCoroutine(onRotate);
        onRotate = StartCoroutine(OnRotate(_input));
    }

    IEnumerator OnRotate(bool _input)
    {
        SetPrevMousePosition();
        while (_input == true)
        {
            InputRotate();
            yield return null;
            rotateDelegate?.Invoke();
        }
        StopRotate();
    }

    void SetPrevMousePosition()
    {
        prevInput = Input.mousePosition;
        currentX = orbitalFollow.HorizontalAxis.Value;
        currentY = orbitalFollow.VerticalAxis.Value;
    }

    void InputRotate()
    {
        currentInput = Input.mousePosition;
        x = currentX + (currentInput.x - prevInput.x) * rotateSpeed;
        y = currentY + (prevInput.y - currentInput.y) * rotateSpeed;

        Rotate();
    }

    void StopRotate()
    {
        if (stoping != null)
            StopCoroutine(stoping);
        stoping = StartCoroutine(StopRotating());
    }

    IEnumerator StopRotating()
    {
        bool stopRotate = true;
        while (stopRotate == true)
        {
            float distX = orbitalFollow.HorizontalAxis.Value - x;
            float distY = orbitalFollow.VerticalAxis.Value - y;
            if (Mathf.Abs(distX) < 0.1f && Mathf.Abs(distY) < 0.1f)
            {
                stopRotate = false;
            }
            yield return null;

            rotateDelegate?.Invoke();
            Rotate();
        }
        stopRotateDelegate?.Invoke();
        Debug.LogWarning("StopRotating ½÷ֵי!!");
    }

    private void Rotate()
    {
        Vector2 verticalLimit = orbitalFollow.VerticalAxis.Range;
        y = Mathf.Clamp(y, verticalLimit.x, verticalLimit.y);
        float speed = Time.deltaTime * smoothSpeed;
        orbitalFollow.HorizontalAxis.Value = Mathf.Lerp(orbitalFollow.HorizontalAxis.Value, x, speed);
        orbitalFollow.VerticalAxis.Value = Mathf.Lerp(orbitalFollow.VerticalAxis.Value, y, speed);
    }

    void InputScroll(float _input)
    {
        if (zooming != null)
            StopCoroutine(zooming);
        zooming = StartCoroutine(ScrollWheeling(_input));

    }

    IEnumerator ScrollWheeling(float _input)
    {
        Vector2 limit = orbitalFollow.RadialAxis.Range;
        float zoom = (limit.y - limit.x) * _input;
        float originValue = orbitalFollow.RadialAxis.Value;
        float targetValue = originValue + zoom;

        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * smoothSpeed;
            yield return null;

            float asdf = Mathf.Lerp(originValue, targetValue, normalize);
            zoom = Mathf.Clamp(asdf, limit.x, limit.y);
            orbitalFollow.RadialAxis.Value = zoom;
        }
        Debug.LogWarning("ScrollWheeling ½÷ֵי!!");
    }

    void InputShake()
    {
        if (shaking != null)
            StopCoroutine(shaking);
        shaking = StartCoroutine(Shaking());
    }

    IEnumerator Shaking()
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * (1f / shakeTime);
            float shakeAmount = Mathf.Lerp(shakeValue, 0f, normalize);
            cinemachineBasicMultiChannelPerlin.AmplitudeGain = shakeAmount;
            yield return null;
        }
    }
}
