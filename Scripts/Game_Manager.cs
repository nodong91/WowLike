using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
    [Header("Instance")]
    public Audio_Manager audioManager;

    public static Game_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        TestSkillSetting();
        FollowTest();

        mainCamera = Camera.main;
        CameraManager.current.rotateDelegate = Rotate;
        Singleton_Controller.INSTANCE.SetController();

        SetMouse();
        SetSkillSlot();
        SetETC();

        audioManager.SetAudioManager();
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

    void InputSlot(int _index)
    {
        Debug.LogWarning("슬롯 : " + _index.ToString());
        currentIndex = _index;
        SetSkillText(currentIndex);
        if (slotArray[currentIndex].GetIsActive == true)
        {
            slotArray[currentIndex].ActionButton();
            Fire();
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
    public Skill_Slot slot;
    public Transform slotParent;
    public Skill_Slot[] slotArray;
    public Data_Manager.SkillStruct[] skillStructs;

    void TestSkillSetting()
    {
        skillStructs = new Data_Manager.SkillStruct[4];
        skillStructs[0] = Singleton_Data.INSTANCE.Dict_Skill["10001"];
        skillStructs[1] = Singleton_Data.INSTANCE.Dict_Skill["10002"];
        skillStructs[2] = Singleton_Data.INSTANCE.Dict_Skill["10003"];
        skillStructs[3] = Singleton_Data.INSTANCE.Dict_Skill["10004"];
        TestSkillName();

        slotArray = new Skill_Slot[skillStructs.Length];
        for (int i = 0; i < slotArray.Length; i++)
        {
            int index = i;
            slotArray[index] = Instantiate(slot, slotParent);
            slotArray[index].button.onClick.AddListener(delegate { InputSlot(index); });
            string quickIndex = (index + 1).ToString();// 단축키
            slotArray[index].SetSlot(quickIndex, skillStructs[index]);
        }
    }

    void TestSkillName()
    {
        for (int i = 0; i < skillStructs.Length; i++)
        {
            skillStructs[i].skillName = SkillName(skillStructs[i].skillName);
            string explanation = SkillName(skillStructs[i].skillDescription);
            string color = "FF0000";// 에너지
            string e = $"<color=#{color}>에너지 : {skillStructs[i].energyType.ToString()} {skillStructs[i].energyAmount.ToString()}</color>";
            explanation = explanation.Replace("{e}", e);

            color = "00FF00";// 레벨
            string l = $"<color=#{color}>레벨 : {skillStructs[i].level.ToString()}</color>";
            explanation = explanation.Replace("{l}", l);

            color = "0000FF";// 캐스팅 타임
            string a = $"<color=#{color}>캐스팅 : {skillStructs[i].castingTime.ToString()}</color>";
            explanation = explanation.Replace("{a}", a);

            color = "000000";// 쿨타임
            string o = $"<color=#{color}>쿨타임 : {skillStructs[i].coolingTime.ToString()}</color>";
            explanation = explanation.Replace("{o}", o);

            color = "FFFF00";// 효과 거리
            string d = $"<color=#{color}>효과 거리 : {skillStructs[i].distance.ToString()}</color>";
            explanation = explanation.Replace("{d}", d);

            color = "FF00FF";// 효과 정도 (데미지 같은...)
            string v = $"<color=#{color}>효과 정도 : {skillStructs[i].energyAmount.ToString()}</color>";
            explanation = explanation.Replace("{v}", v);

            skillStructs[i].skillDescription = explanation;
        }
    }

    void SetSkillText(int _index)
    {
        UI_Manager.instance.SkillText(skillStructs[_index].skillDescription);
    }

    string SkillName(string _id)
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

    string SetExplanationValue()
    {
        string temp = string.Empty;
        return temp;
    }

    private void Fire()
    {
        if (target == null)
            return;

        Skill_Bullet instBullet = Instantiate(bullet, this.transform);
        instBullet.transform.position = player.position;
        instBullet.SetTarget(target, unitSize);
    }

    void CheckDistance()
    {
        if (target != null)
        {
            targetDistance = Vector3.Distance(target.position, player.transform.position);
            for (int i = 0; i < skillStructs.Length; i++)
            {
                slotArray[i].InDistance(skillStructs[i].distance > targetDistance);
            }
        }
    }

    public Transform[] testTarget;
    void FollowTest()
    {
        for (int i = 0; i < testTarget.Length; i++)
        {
            UI_Manager.instance.AddHP(testTarget[i]);
        }
    }
}
