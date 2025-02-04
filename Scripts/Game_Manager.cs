using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Game_Manager : MonoBehaviour
{
    private Camera mainCamera;
    public Transform player;
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
        //Singleton_Controller.INSTANCE.key_SpaceBar = Fire;
    }

    void InputSlot(int _index)
    {
        switch (_index)
        {
            case 0:
                InputKey01(false);
                break;

            case 1:
                InputKey02(false);
                break;

            case 2:
                InputKey03(false);
                break;

            case 3:
                InputKey04(false);
                break;
        }
    }

    void InputKey01(bool _input)
    {
        if (_input == false)
            Fire();
        Debug.LogWarning("InputKey01");
    }

    void InputKey02(bool _input)
    {

        Debug.LogWarning("InputKey02");
    }

    void InputKey03(bool _input)
    {

        Debug.LogWarning("InputKey03");
    }

    void InputKey04(bool _input)
    {

        Debug.LogWarning("InputKey04");
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
        }
        else if (isLeftDrag == false)
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
    public bool clickLeft, isLeftDrag, inputMouseRight;
    public float clickTime;
    public Vector2 clickPosition;
    public Coroutine clickLefting;
    IEnumerator MouseLeftDrag(bool _input)
    {
        rotateType = RotateType.Normal;
        CameraManager.current.InputRotate(_input);
        if (_input == true)
        {
            isLeftDrag = false;
            while (isLeftDrag == false)
            {
                Vector2 tempPosition = mainCamera.ScreenToViewportPoint(Input.mousePosition);
                float dist = (tempPosition - clickPosition).magnitude;
                if (dist > 0.01f)
                {
                    isLeftDrag = true;
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
            Debug.LogWarning("히트!! " + hit.collider.gameObject.name);
            Debug.DrawLine(mainCamera.transform.position, hit.point, Color.red, 0.3f);

            target = hit.transform;
            targetGuide.transform.position = target.position;
        }
    }

    void InputMouseRight(bool _input)
    {
        inputMouseRight = _input;
        if (_input == true)
        {
            rotateType = RotateType.Focus;
        }
        else if (inputDir != 0)
        {
            rotateType = RotateType.Normal;
        }
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
        // 같은 종류가 포함되어 있으면
        if ((inputDir & InputDir.Up) != 0)// 위쪽
        {
            direction = new Vector2(direction.x, 1);
        }
        if ((inputDir & InputDir.Left) != 0)// 왼쪽
        {
            direction = new Vector2(-1, direction.y);
        }
        if ((inputDir & InputDir.Down) != 0)// 아래쪽
        {
            direction = new Vector2(direction.x, -1);
        }
        if ((inputDir & InputDir.Right) != 0)// 오른쪽
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
        while (inputDir != 0)
        {
            SetDirection(_direction);
            yield return null;
        }
    }

    void SetDirection(Vector3 _direction)
    {
        Vector3 temp = mainCamera.transform.TransformDirection(_direction);
        direction = player.transform.position + new Vector3(temp.x, 0f, temp.z).normalized;
        guide.transform.position = direction;
        Moving();
        Rotate();
    }

    void Moving()
    {
        Vector3 movePoint = Vector3.Lerp(player.transform.position, direction, Time.deltaTime * moveSpeed);
        player.transform.position = movePoint;
    }

    void Rotate()
    {
        switch (rotateType)
        {
            case RotateType.Normal:
                if (direction == Vector3.zero)
                    return;

                Vector3 offset = (direction - player.transform.position).normalized;
                Quaternion rotatePoint = Quaternion.Lerp(player.transform.rotation, Quaternion.LookRotation(offset), Time.deltaTime * rotateSpeed);
                player.transform.rotation = rotatePoint;
                break;

            case RotateType.Focus:
                Vector3 temp = mainCamera.transform.TransformDirection(Vector3.forward);
                Vector3 front = player.transform.position + new Vector3(temp.x, 0f, temp.z).normalized;
                guide.transform.position = front;

                offset = (front - player.transform.position).normalized;
                rotatePoint = Quaternion.Lerp(player.transform.rotation, Quaternion.LookRotation(offset), Time.deltaTime * rotateSpeed);
                player.transform.rotation = rotatePoint;

                if (inputMouseRight == false && inputDir != 0)
                {
                    rotateType = RotateType.Normal;
                }
                break;
        }
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
            CameraManager.current.InputShake();

            visibleTargets.Clear();
            Collider[] targetsInViewRadius = Physics.OverlapSphere(player.transform.position, viewRadius, targetMask);
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                Transform temp = targetsInViewRadius[i].transform;
                Vector3 dirToTarget = (temp.position - player.transform.position).normalized;
                if (Vector3.Angle(player.transform.forward, dirToTarget) < viewAngle * 0.5f)// 앵글 안에 포함 되는지
                {
                    float dstToTarget = (player.transform.position - temp.position).magnitude;
                    if (Physics.Raycast(player.transform.position, dirToTarget, dstToTarget, obstacleMask) == false)
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
            _angleInDegrees += player.transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(_angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(_angleInDegrees * Mathf.Deg2Rad));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (player != null)
        {
            Handles.color = Color.red;
            Handles.DrawWireArc(player.transform.position, Vector3.up, Vector3.forward, 360f, viewRadius);

            Vector3 viewAngleA = DirFromAngle(viewAngle * 0.5f, false);
            Vector3 viewAngleB = DirFromAngle(-viewAngle * 0.5f, false);

            Handles.DrawLine(player.transform.position, player.transform.position + viewAngleA * viewRadius);
            Handles.DrawLine(player.transform.position, player.transform.position + viewAngleB * viewRadius);
        }
    }
#endif

    //public Transform ijijfeiifej, damageEffect;
    //public float unitSize;
    //private void Update()
    //{
    //    if (target != null)
    //    {
    //        Vector3 targetPosition = new Vector3(target.position.x, damageEffect.transform.position.y, target.position.z);
    //        if ((targetPosition - damageEffect.transform.position).magnitude < unitSize)
    //        {
    //            damageEffect.gameObject.SetActive(false);
    //            ijijfeiifej.gameObject.SetActive(true);
    //        }
    //        else
    //        {
    //            damageEffect.gameObject.SetActive(true);
    //            ijijfeiifej.gameObject.SetActive(false);
    //        }
    //        Vector3 offset = (targetPosition - damageEffect.transform.position).normalized;
    //        ijijfeiifej.rotation = Quaternion.LookRotation(offset);
    //        ijijfeiifej.position = targetPosition - offset * unitSize;
    //    }
    //}
    public Skill_Bullet bullet;
    public float unitSize = 1f;
    private void Fire()
    {
        if (target == null)
            return;
        Skill_Bullet instBullet = Instantiate(bullet);
        instBullet.transform.position = player.position;
        instBullet.SetTarget(target, unitSize);
    }
}
