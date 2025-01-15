using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Game_Manager : MonoBehaviour
{
    private Camera mainCamera;
    public Transform unit;
    public Transform guide;

    [SerializeField] Vector3 direction;
    public float moveSpeed = 1f;
    public float rotateSpeed = 10f;
    Coroutine inputDirection;
    Coroutine mouseRight;

    public enum InputDir
    {
        Up = 1 << 0,
        Left = 1 << 1,
        Down = 1 << 2,
        Right = 1 << 3
    }
    [EnumFlags]
    public InputDir inputDir;
    public enum RotateType
    {
        Normal,
        Focus
    }
    public RotateType rotateType;
    public Skill_Slot slot;
    public Transform slotParent;
    public Skill_Slot[] slotArray;

    void Start()
    {
        mainCamera = Camera.main;

        Singleton_Controller.INSTANCE.SetController();

        SetMouse();
        SetSkillSlot();
        SetETC();

        CameraManager.current.rotateDelegate = Rotate;
        CameraManager.current.stopRotateDelegate = StopRotate;
    }

    void SetMouse()
    {
        Singleton_Controller.INSTANCE.key_MouseLeft = InputMouseLeft;
        Singleton_Controller.INSTANCE.key_MouseRight = InputMouseRight;
        Singleton_Controller.INSTANCE.key_MouseWheel = InputMouseWheel;
    }

    void SetSkillSlot()
    {
        slotArray = new Skill_Slot[4];
        for (int i = 0; i < slotArray.Length; i++)
        {
            int index = i;
            slotArray[index] = Instantiate(slot, slotParent);
            slotArray[index].button.onClick.AddListener(delegate { InputSlot(index); });
        }
        Singleton_Controller.INSTANCE.key_1 = InputKey01;
        Singleton_Controller.INSTANCE.key_2 = InputKey02;
        Singleton_Controller.INSTANCE.key_3 = InputKey03;
        Singleton_Controller.INSTANCE.key_4 = InputKey04;
    }

    void SetETC()
    {
        Singleton_Controller.INSTANCE.key_W = InputUp;
        Singleton_Controller.INSTANCE.key_A = InputLeft;
        Singleton_Controller.INSTANCE.key_S = InputDown;
        Singleton_Controller.INSTANCE.key_D = InputRight;

        Singleton_Controller.INSTANCE.key_Tab = NextTarget;
    }

    void InputSlot(int _index)
    {
        Debug.LogWarning(_index);
        switch (_index)
        {
            case 0:
                InputKey01(true);
                break;

            case 1:
                InputKey02(true);
                break;

            case 2:
                InputKey03(true);
                break;

            case 3:
                InputKey04(true);
                break;
        }
    }

    void InputKey01(bool _input)
    {

    }

    void InputKey02(bool _input)
    {

    }

    void InputKey03(bool _input)
    {

    }

    void InputKey04(bool _input)
    {

    }

    void InputMouseLeft(bool _input)
    {
        if (clickLefting != null)
            StopCoroutine(clickLefting);

        if (_input == true)
        {
            clickLeft = false;
            clickTime = Time.time;
            clickPosition = mainCamera.ScreenToViewportPoint(Input.mousePosition);
            adfdf.position = Input.mousePosition;
        }
        else if (isDrag == false)
        {
            clickTime = Time.time - clickTime;
            if (clickTime < 0.15f)
            {
                clickLeft = true;
                LeftRayCasting();
            }
        }
        clickLefting = StartCoroutine(MouseLeftDrag(_input));
    }
    public bool clickLeft, isDrag;
    public float clickTime;
    public Vector2 clickPosition;
    public Coroutine clickLefting;
    public Transform adfdf;
    IEnumerator MouseLeftDrag(bool _input)
    {
        rotateType = RotateType.Normal;
        CameraManager.current.InputRotate(_input);
        if (_input == true)
        {
            isDrag = false;
            while (isDrag == false)
            {
                Vector2 tempPosition = mainCamera.ScreenToViewportPoint(Input.mousePosition);
                float dist = (tempPosition - clickPosition).magnitude;
                if (dist > 0.01f)
                {
                    isDrag = true;
                }
                yield return null;
            }
        }
    }
    public Transform jjjeffie;
    public LayerMask layerMask;
    void LeftRayCasting()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //int layerMask = 1 << 8;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~layerMask))
        {
            Debug.LogWarning("Hit Green " + hit.collider.gameObject.name);
            //Debug.DrawRay(mainCamera.transform.position, hit.transform.position, Color.red,1f);
            Debug.DrawLine(mainCamera.transform.position, hit.point, Color.red, 0.3f);
            jjjeffie.position = hit.point;
        }
    }

    void InputMouseRight(bool _input)
    {
        rotateType = _input == true ? RotateType.Focus : RotateType.Normal;
        CameraManager.current.InputRotate(_input);
    }

    void InputMouseWheel(bool _input)
    {
        float input = _input ? 0.1f : -0.1f;
        CameraManager.current.delegateInputScroll(input);
    }

    void InputUp(bool _input)
    {
        InputDirection(_input, InputDir.Up);
    }
    void InputLeft(bool _input)
    {
        InputDirection(_input, InputDir.Left);
    }
    void InputDown(bool _input)
    {
        InputDirection(_input, InputDir.Down);
    }
    void InputRight(bool _input)
    {
        InputDirection(_input, InputDir.Right);
    }

    void InputDirection(bool _input, InputDir _inputDir)
    {
        if (_input)
        {
            inputDir |= _inputDir;
        }
        else
        {
            inputDir &= ~_inputDir;

        }
        SetDirection();
    }

    void SetDirection()
    {
        direction = Vector2.zero;
        // ���� ������ ���ԵǾ� ������
        if ((inputDir & InputDir.Up) != 0)// ����
        {
            direction = new Vector2(direction.x, 1);
        }
        if ((inputDir & InputDir.Left) != 0)// ����
        {
            direction = new Vector2(-1, direction.y);
        }
        if ((inputDir & InputDir.Down) != 0)// �Ʒ���
        {
            direction = new Vector2(direction.x, -1);
        }
        if ((inputDir & InputDir.Right) != 0)// ������
        {
            direction = new Vector2(1, direction.y);
        }
        OutputDirection(direction);
    }

    void OutputDirection(Vector2 _direction)
    {
        if (inputDirection != null)
            StopCoroutine(inputDirection);
        inputDirection = StartCoroutine(Co_OutputDirection(_direction));
    }

    IEnumerator Co_OutputDirection(Vector2 _direction)
    {
        while (direction != Vector3.zero)
        {
            SetDirection(_direction);
            yield return null;
        }
    }

    void SetDirection(Vector3 _direction)
    {
        Vector3 temp = mainCamera.transform.TransformDirection(_direction);
        direction = unit.transform.position + new Vector3(temp.x, unit.transform.position.y, temp.z).normalized;
        guide.transform.position = direction;
        Moving();
        Rotate();
    }

    void Moving()
    {
        Vector3 movePoint = Vector3.Lerp(unit.transform.position, direction, Time.deltaTime * moveSpeed);
        unit.transform.position = movePoint;
    }

    void Rotate()
    {
        switch (rotateType)
        {
            case RotateType.Normal:
                if (direction == Vector3.zero)
                    return;

                Vector3 offset = (direction - unit.transform.position).normalized;
                Quaternion rotatePoint = Quaternion.Lerp(unit.transform.rotation, Quaternion.LookRotation(offset), Time.deltaTime * rotateSpeed);
                unit.transform.rotation = rotatePoint;
                break;

            case RotateType.Focus:
                Vector3 temp = mainCamera.transform.TransformDirection(Vector3.forward);
                Vector3 front = unit.transform.position + new Vector3(temp.x, unit.transform.position.y, temp.z).normalized;
                guide.transform.position = front;

                offset = (front - unit.transform.position).normalized;
                rotatePoint = Quaternion.Lerp(unit.transform.rotation, Quaternion.LookRotation(offset), Time.deltaTime * rotateSpeed);
                unit.transform.rotation = rotatePoint;
                break;
        }
    }

    void StopRotate()
    {
        if (inputDir == 0)
            rotateType = RotateType.Normal;
    }













    public Transform target;
    public Transform targetGuide;
    public int targetIndex;
    public LayerMask targetMask, obstacleMask;
    public List<Transform> visibleTargets = new List<Transform>();
    void NextTarget(bool _input)
    {
        if (_input == false)
        {
            visibleTargets.Clear();
            Collider[] targetsInViewRadius = Physics.OverlapSphere(unit.transform.position, viewRadius, targetMask);
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                Transform temp = targetsInViewRadius[i].transform;
                Vector3 dirToTarget = (temp.position - unit.transform.position).normalized;
                if (Vector3.Angle(unit.transform.forward, dirToTarget) < viewAngle * 0.5f)
                {
                    float dstToTarget = Vector3.Distance(unit.transform.position, temp.position);
                    if (Physics.Raycast(unit.transform.position, dirToTarget, dstToTarget, obstacleMask) == false)
                    {
                        visibleTargets.Add(temp);
                    }
                }
            }
            if (visibleTargets.Count == 0)
                return;

            targetIndex++;
            if (targetIndex >= visibleTargets.Count)
                targetIndex = 0;
            target = visibleTargets[targetIndex].transform;
            targetGuide.transform.position = target.position;
        }
    }

    public float viewRadius;
    public float viewAngle;

    public Vector3 DirFromAngle(float _angleInDegrees, bool _angleIsGlobal)
    {
        if (_angleIsGlobal == false)
        {
            _angleInDegrees += unit.transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(_angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(_angleInDegrees * Mathf.Deg2Rad));
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
        Handles.DrawWireArc(unit.transform.position, Vector3.up, Vector3.forward, 360f, viewRadius);

        Vector3 viewAngleA = DirFromAngle(viewAngle * 0.5f, false);
        Vector3 viewAngleB = DirFromAngle(-viewAngle * 0.5f, false);

        Handles.DrawLine(unit.transform.position, unit.transform.position + viewAngleA * viewRadius);
        Handles.DrawLine(unit.transform.position, unit.transform.position + viewAngleB * viewRadius);
    }
}
