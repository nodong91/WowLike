using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public CinemachineCamera cinemachineCamera;
    public Vector3 offset;
    CinemachineRotationComposer rotationComposer;
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
    public RotateDelegate rotateDelegate;

    public delegate void DelegateInputScroll(float _input);
    public DelegateInputScroll delegateInputScroll;

    CinemachineBrain brain;

    public static CameraManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SetCameraManager();
    }

    public void SetCameraManager()
    {
        brain = Camera.main.gameObject.GetComponent<CinemachineBrain>();
        if (brain == null)
            brain = Camera.main.gameObject.AddComponent<CinemachineBrain>();

        delegateInputScroll = InputScroll;
        orbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
        cinemachineBasicMultiChannelPerlin = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        rotationComposer = cinemachineCamera.GetComponent<CinemachineRotationComposer>();
        SetDefault();
        //GetTarget(target);
    }

    void SetDefault()
    {
        Vector2 zoomLimit = new Vector2(2.0f, 5.0f);
        orbitalFollow.RadialAxis.Range = zoomLimit;
        orbitalFollow.RadialAxis.Value = (zoomLimit.x + zoomLimit.y) * 0.5f;
        orbitalFollow.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.ThreeRing;
        Cinemachine3OrbitRig.Settings newSetting = new Cinemachine3OrbitRig.Settings
        {
            SplineCurvature = 0.5f,
            Top = new Cinemachine3OrbitRig.Orbit { Height = 7, Radius = 2 },
            Center = new Cinemachine3OrbitRig.Orbit { Height = 4f, Radius = 3 },
            Bottom = new Cinemachine3OrbitRig.Orbit { Height = 1f, Radius = 2.5f }
        };
        orbitalFollow.Orbits = newSetting;
    }

    public void SetTarget(Transform _target)
    {
        cinemachineCamera.Target.TrackingTarget = _target;
        rotationComposer.TargetOffset = offset;
    }

    public void InputRotate(bool _input)
    {
        if (onRotate != null)
            StopCoroutine(onRotate);

        if (_input == true)
            onRotate = StartCoroutine(OnRotate(_input));
        else
            onRotate = StartCoroutine(StopRotating());
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
        //StopRotate();
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
        if (onRotate != null)
            StopCoroutine(onRotate);
        onRotate = StartCoroutine(StopRotating());
    }

    IEnumerator StopRotating()
    {
        bool stopRotate = true;
        while (stopRotate == true)
        {
            float distX = orbitalFollow.HorizontalAxis.Value - x;
            float distY = orbitalFollow.VerticalAxis.Value - y;
            if (Mathf.Abs(distX) < 0.01f && Mathf.Abs(distY) < 0.01f)
            {
                stopRotate = false;
            }
            yield return null;

            rotateDelegate?.Invoke();
            Rotate();
        }
        Debug.LogWarning("StopRotating ����!!");
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

            float value = Mathf.Lerp(originValue, targetValue, normalize);
            zoom = Mathf.Clamp(value, limit.x, limit.y);
            orbitalFollow.RadialAxis.Value = zoom;
        }
        Debug.LogWarning("ScrollWheeling ����!!");
    }

    public void InputShake()
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
