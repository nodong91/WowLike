using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements.Experimental;

public class Game_Manager : MonoBehaviour
{
    public string BGMSound;

    private Camera mainCamera;
    public Transform player;
    public Transform guide;

    [SerializeField] Vector3 direction;
    public float moveSpeed = 1f;
    public float rotateSpeed = 10f;
    Coroutine inputDirection;
    Coroutine mouseRight;
    [System.Flags]
    public enum InputDir
    {
        Up = 1 << 0,
        Left = 1 << 1,
        Down = 1 << 2,
        Right = 1 << 3
    }
    public InputDir inputDir;
    public enum RotateType
    {
        Normal,
        Focus
    }
    public RotateType rotateType;
    //[Header("Instance")]

    public static Game_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        TestSkillSetting();

        mainCamera = Camera.main;
        CameraManager.instance.SetCameraManager();
        CameraManager.instance.rotateDelegate = Rotate;
        FollowTest();
        Singleton_Controller.INSTANCE.SetController();

        SetMouse();
        SetSkillSlot();
        SetETC();

        UI_Manager.instance.SetUIManager();
        Singleton_Audio.INSTANCE.Audio_SetBGM(BGMSound);
    }

    void SetMouse()
    {
        Singleton_Controller.INSTANCE.key_MouseLeft = InputMouseLeft;
        Singleton_Controller.INSTANCE.key_MouseRight = InputMouseRight;
        Singleton_Controller.INSTANCE.key_MouseWheel = InputMouseWheel;
    }

    void SetSkillSlot()
    {
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
    Coroutine casting;
    void InputSlot(int _index)
    {
        currentIndex = _index;

        UI_InvenSlot[] quickSlots = UI_Manager.instance.inventory.GetQuickSlot;
        switch (quickSlots[_index].slotType)
        {
            case UI_InvenSlot.SlotType.Empty:
                Debug.LogWarning($"���� {_index} : ��� ����");
                break;

            case UI_InvenSlot.SlotType.Skill:
                Debug.LogWarning($"���� {_index} : ��ų");
                ActionSkill(quickSlots[_index]);
                break;

            case UI_InvenSlot.SlotType.Item:
                Debug.LogWarning($"���� {_index} : ������");
                break;
        }
        return;


    }

    void ActionSkill(UI_InvenSlot _Slot)
    {
        Data_Manager.SkillStruct skillStruct = _Slot.skillStruct;
        UI_Manager.instance.SkillText(skillStruct.skillDescription);
        //SetSkillText(_index);
        //Skill_Slot[] slotArray = UI_Manager.instance.slotArray;
        if (_Slot.GetIsActive == true)
        {
            Singleton_Audio.INSTANCE.Audio_SetBGM(BGMSound);// �������׽�Ʈ
            if (skillStruct.castingTime > 0)
            {
                if (casting != null)
                    StopCoroutine(casting);
                casting = StartCoroutine(Casting(_Slot));
            }
            else
            {
                Fire();
                _Slot.CoolingSlot();
            }
        }
        else
        {
            UI_Manager.instance.SetWarning(0, "����� �� ����");
        }
    }

    IEnumerator Casting(UI_InvenSlot _Slot)
    {
        float castingTime = 1f / _Slot.skillStruct.castingTime;
        float normalize = 0f;
        while (normalize < 1f && inputDir == 0)
        {
            normalize += Time.deltaTime * castingTime;
            UI_Manager.instance.SkillCasting(normalize);
            yield return null;
        }
        UI_Manager.instance.SkillCasting(0f);

        if (inputDir == 0)
        {
            Fire();
            _Slot.CoolingSlot();
        }
    }

    void InputKey01(bool _input)
    {
        if (_input == false)
            InputSlot(0);
    }

    void InputKey02(bool _input)
    {
        if (_input == false)
            InputSlot(1);
    }

    void InputKey03(bool _input)
    {
        if (_input == false)
            InputSlot(2);
    }

    void InputKey04(bool _input)
    {
        if (_input == false)
            InputSlot(3);
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

    public LayerMask layerMask;

    IEnumerator MouseLeftDrag(bool _input)
    {
        rotateType = RotateType.Normal;
        CameraManager.instance.InputRotate(_input);
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

    void LeftRayCasting()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //int layerMask = 1 << 8;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~layerMask))
        {
            Debug.LogWarning("��Ʈ!! " + hit.collider.gameObject.name);
            Debug.DrawLine(mainCamera.transform.position, hit.point, Color.red, 0.3f);

            target = hit.transform;
            targetGuide.transform.position = target.position;
            CheckDistance();
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
        CameraManager.instance.InputRotate(_input);
    }

    void InputMouseWheel(bool _input)
    {
        float input = _input ? 0.1f : -0.1f;
        CameraManager.instance.delegateInputScroll(input);
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
        while (inputDir != 0)
        {
            SetDirection(_direction);
            yield return null;
        }
    }

    void SetDirection(Vector3 _direction)
    {
        Vector3 dir = new Vector3(_direction.x, 0, _direction.y);
        Vector3 temp = mainCamera.transform.TransformDirection(dir).normalized;
        direction = player.transform.position + new Vector3(temp.x, 0f, temp.z);
        guide.transform.position = dir;
        Moving();
        Rotate();
    }

    void Moving()
    {
        Vector3 movePoint = Vector3.Lerp(player.transform.position, direction, Time.deltaTime * moveSpeed);
        player.transform.position = movePoint;
        CheckDistance();
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
            visibleTargets.Clear();
            Collider[] targetsInViewRadius = Physics.OverlapSphere(player.transform.position, viewRadius, targetMask);
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                Transform temp = targetsInViewRadius[i].transform;
                Vector3 dirToTarget = (temp.position - player.transform.position).normalized;
                if (Vector3.Angle(player.transform.forward, dirToTarget) < viewAngle * 0.5f)// �ޱ� �ȿ� ���� �Ǵ���
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
            CheckDistance();
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

            if (target != null)
            {
                Color color = Color.green;
                GUIStyle fontStyle = new()
                {
                    fontSize = 50,
                    normal = { textColor = color },
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                };

                Handles.color = Gizmos.color = color;
                Handles.DrawLine(player.transform.position, target.transform.position);
                Handles.Label(target.transform.position, targetDistance.ToString("N2"), fontStyle);
                Gizmos.DrawSphere(target.transform.position, 0.3f);
            }
        }
    }
#endif

    public Skill_Bullet bullet;
    public float unitSize = 1f;
    float targetDistance;
    public int currentIndex;
    //public Skill_Slot slot;
    //public Transform slotParent;
    //public Skill_Slot[] slotArray;
    //public Data_Manager.SkillStruct[] skillStructs;

    void TestSkillSetting()
    {
        //skillStructs = new Data_Manager.SkillStruct[4];
        //skillStructs[0] = Singleton_Data.INSTANCE.Dict_Skill["10001"];
        //skillStructs[1] = Singleton_Data.INSTANCE.Dict_Skill["10002"];
        //skillStructs[2] = Singleton_Data.INSTANCE.Dict_Skill["10003"];
        //skillStructs[3] = Singleton_Data.INSTANCE.Dict_Skill["10004"];
        TestSkillName();

        UI_Manager.instance.SetSkillSlot(InputSlot);// ���� ����
    }

    void TestSkillName()
    {
        //for (int i = 0; i < skillStructs.Length; i++)
        //{
        //    skillStructs[i].skillName = SetString(skillStructs[i].skillName);
        //    string explanation = SetString(skillStructs[i].skillDescription);
        //    string color = "FF0000";// ������
        //    string e = $"<color=#{color}>������ : {skillStructs[i].energyType.ToString()} {skillStructs[i].energyAmount.ToString()}</color>";
        //    explanation = explanation.Replace("{e}", e);

        //    color = "00FF00";// ����
        //    string l = $"<color=#{color}>���� : {skillStructs[i].level.ToString()}</color>";
        //    explanation = explanation.Replace("{l}", l);

        //    color = "0000FF";// ĳ���� Ÿ��
        //    string a = $"<color=#{color}>ĳ���� : {skillStructs[i].castingTime.ToString()}</color>";
        //    explanation = explanation.Replace("{a}", a);

        //    color = "000000";// ��Ÿ��
        //    string o = $"<color=#{color}>��Ÿ�� : {skillStructs[i].coolingTime.ToString()}</color>";
        //    explanation = explanation.Replace("{o}", o);

        //    color = "FFFF00";// ȿ�� �Ÿ�
        //    string d = $"<color=#{color}>ȿ�� �Ÿ� : {skillStructs[i].distance.ToString()}</color>";
        //    explanation = explanation.Replace("{d}", d);

        //    color = "FF00FF";// ȿ�� ���� (������ ����...)
        //    string v = $"<color=#{color}>ȿ�� ���� : {skillStructs[i].energyAmount.ToString()}</color>";
        //    explanation = explanation.Replace("{v}", v);

        //    skillStructs[i].skillDescription = explanation;
        //}
    }

    //void SetSkillText(int _index)
    //{
    //    UI_Manager.instance.SkillText(skillStructs[_index].skillDescription);
    //}

    string SetString(string _id)
    {
        Singleton_Data.Translation translation = Singleton_Data.INSTANCE.translation;
        Data_Manager.SkillString skill = Singleton_Data.INSTANCE.Dict_SkillString[_id];
        string temp = string.Empty;
        switch (translation)
        {
            case Singleton_Data.Translation.Korean:
                temp = skill.KR;
                break;

            case Singleton_Data.Translation.English:
                temp = skill.EN;
                break;

            case Singleton_Data.Translation.Japanese:
                temp = skill.JP;
                break;

            case Singleton_Data.Translation.Chinese:
                temp = skill.CN;
                break;
        }
        return temp;
    }

    //string SetDescription()
    //{
    //    string temp = string.Empty;
    //    return temp;
    //}

    private void Fire()
    {
        if (target == null)
            return;

        Skill_Bullet instBullet = Instantiate(bullet, this.transform);
        instBullet.transform.position = player.position;
        instBullet.SetTarget(target, unitSize);
    }

    public void CheckDistance()
    {
        if (target != null)
        {
            targetDistance = Vector3.Distance(target.position, player.transform.position);
            UI_InvenSlot[] quickSlots = UI_Manager.instance.inventory.GetQuickSlot;
            for (int i = 0; i < quickSlots.Length; i++)
            {
                //Skill_Slot[] slotArray = UI_Manager.instance.slotArray;
                quickSlots[i].InDistance(quickSlots[i].skillStruct.distance > targetDistance);
            }
        }
    }

    public Transform[] testTarget;
    void FollowTest()
    {
        CameraManager.instance.SetTarget(player);
        for (int i = 0; i < testTarget.Length; i++)
        {
            UI_Manager.instance.AddHPUI(testTarget[i]);
        }
    }
}
