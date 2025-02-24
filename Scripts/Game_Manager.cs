using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Game_Manager : MonoBehaviour
{
    public string BGMSound;

    private Camera mainCamera;
    public Unit_Manager player;
    public Dictionary<GameObject, Unit_Manager> units = new Dictionary<GameObject, Unit_Manager>();

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
    [SerializeField] private RotateType rotateType;
    public RotateType TryRotateType()
    {
        return rotateType;
    }
    //[Header("Instance")]

    public static Game_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        player = Instantiate(player, transform);
        checkDistance += player.CheckDistance;

        units = new Dictionary<GameObject, Unit_Manager>();
        for (int i = 0; i < testTarget.Length; i++)
        {
            units[testTarget[i].gameObject] = testTarget[i];
        }
        //TestSkillSetting();

        mainCamera = Camera.main;
        CameraManager.instance.SetCameraManager();
        CameraManager.instance.rotateDelegate = player.Rotate;
        FollowTest();
        Singleton_Controller.INSTANCE.SetController();

        SetMouse();
        SetSkillSlot();
        SetETC();

        UI_Manager.instance.SetUIManager();
        lootingItem += UI_Manager.instance.GetInventory.AddLooting;

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
        Singleton_Controller.INSTANCE.key_SpaceBar = Test;
    }

    Coroutine casting;
    public void InputSlot(int _index)
    {
        currentIndex = _index;

        UI_InvenSlot slot = UI_Manager.instance.GetQuickSlot(_index);
        switch (slot.itemType)
        {
            case UI_InvenSlot.ItemType.Empty:
                Debug.LogWarning($"슬롯 {_index} : 비어 있음");
                break;

            case UI_InvenSlot.ItemType.Skill:
                Debug.LogWarning($"슬롯 {_index} : 스킬");
                ActionSkill(slot);
                break;

            case UI_InvenSlot.ItemType.Item:
                Debug.LogWarning($"슬롯 {_index} : 아이템");
                break;
        }
        return;
    }

    void ActionSkill(UI_InvenSlot _Slot)
    {
        if (target == null)
        {
            UI_Manager.instance.SetWarning(0, "대상이 필요합니다.");
            return;
        }

        if (VisibleTarget(target) == false)
        {
            UI_Manager.instance.SetWarning(0, "대상이 앞에 있어야 합니다.");
            return;
        }

        Data_Manager.SkillStruct skillStruct = _Slot.skillStruct;
        UI_Manager.instance.SkillText(TestSkillName());
        if (_Slot.GetIsActive == true)
        {
            Singleton_Audio.INSTANCE.Audio_SetBGM(BGMSound);// 삐지엠테스트
            if (skillStruct.castingTime > 0)
            {
                if (casting != null)
                    StopCoroutine(casting);
                casting = StartCoroutine(Casting(_Slot));
            }
            else
            {
                Fire(_Slot);// 즉시
            }
        }
        else
        {
            UI_Manager.instance.SetWarning(0, "사용할 수 없음");
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
            Fire(_Slot);// 캐스팅 완료
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
        Vector2 setDirection = Vector2.zero;
        // 같은 종류가 포함되어 있으면
        if ((inputDir & InputDir.Up) != 0)// 위쪽
        {
            setDirection = new Vector2(setDirection.x, 1);
        }
        if ((inputDir & InputDir.Left) != 0)// 왼쪽
        {
            setDirection = new Vector2(-1, setDirection.y);
        }
        if ((inputDir & InputDir.Down) != 0)// 아래쪽
        {
            setDirection = new Vector2(setDirection.x, -1);
        }
        if ((inputDir & InputDir.Right) != 0)// 오른쪽
        {
            setDirection = new Vector2(1, setDirection.y);
        }
        player.OutputDirection(setDirection);


        if (inputMouseRight == false && inputDir != 0)
        {
            rotateType = RotateType.Normal;
        }
    }


    public delegate void LootingItem(string[] _ids);
    public LootingItem lootingItem;

    public delegate float CheckDistance();
    public CheckDistance checkDistance;

    private Transform target;
    public Transform GetTarget { get { return target; } }
    public Transform targetGuide;
    public int targetIndex;
    public LayerMask targetMask, obstacleMask;
    public List<Transform> visibleTargets = new List<Transform>();

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

            float dist = player.CheckDistance();
            if (units.ContainsKey(target.gameObject))// 클릭한 오브젝트가 유닛인 경우
            {
                if (dist < 2f)
                {
                    Unit_Manager hitUnit = units[target.gameObject];
                    if (hitUnit.state == Unit_Manager.UnitState.Dead)
                    {
                        string[] ids = hitUnit.GetLooting();
                        lootingItem(ids);
                    }
                }
                else
                {

                }
            }
        }
        else
        {
            target = null;
            lootingItem(null);
        }
    }

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
            player.CheckDistance();
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

    bool VisibleTarget(Transform _target)// 보이는지 확인
    {
        Vector3 dirToTarget = (_target.position - player.transform.position).normalized;
        if (Vector3.Angle(player.transform.forward, dirToTarget) < viewAngle * 0.5f)// 앵글 안에 포함 되는지
        {
            float dstToTarget = (player.transform.position - _target.position).magnitude;
            if (Physics.Raycast(player.transform.position, dirToTarget, dstToTarget, obstacleMask) == false)
            {
                //visibleTargets.Add(_target);
                return true;
            }
        }
        return false;
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
                string tempColor = VisibleTarget(target) ? "0000FF" : "FF0000";
                tempColor = $"<color=#{tempColor}>{VisibleTarget(target)}</color>";
                Handles.Label(target.transform.position, $"{tempColor} : {checkDistance().ToString("N2")}", fontStyle);
                Gizmos.DrawSphere(target.transform.position, 0.3f);
            }
        }
    }
#endif
















    public Skill_Instance instEffect;
    Skill_Instance instBullet;
    public float unitSize = 1f;
    public int currentIndex;

    string TestSkillName()
    {
        UI_InvenSlot quickSlot = UI_Manager.instance.GetQuickSlot(currentIndex);
        string explanation = SetString(quickSlot.skillStruct.skillDescription);
        string color = "FF0000";// 에너지
        string e = $"<color=#{color}>에너지 : {quickSlot.skillStruct.energyType.ToString()} {quickSlot.skillStruct.energyAmount.ToString()}</color>";
        explanation = explanation.Replace("{e}", e);

        color = "00FF00";// 레벨
        string l = $"<color=#{color}>레벨 : {quickSlot.skillStruct.level.ToString()}</color>";
        explanation = explanation.Replace("{l}", l);

        color = "0000FF";// 캐스팅 타임
        string a = $"<color=#{color}>캐스팅 : {quickSlot.skillStruct.castingTime.ToString()}</color>";
        explanation = explanation.Replace("{a}", a);

        color = "000000";// 쿨타임
        string o = $"<color=#{color}>쿨타임 : {quickSlot.skillStruct.coolingTime.ToString()}</color>";
        explanation = explanation.Replace("{o}", o);

        color = "FFFF00";// 효과 거리
        string d = $"<color=#{color}>효과 거리 : {quickSlot.skillStruct.range.y.ToString()}</color>";
        explanation = explanation.Replace("{d}", d);

        color = "FF00FF";// 효과 정도 (데미지 같은...)
        string v = $"<color=#{color}>효과 정도 : {quickSlot.skillStruct.energyAmount.ToString()}</color>";
        explanation = explanation.Replace("{v}", v);

        return explanation;
    }

    string SetString(string _id)
    {
        Singleton_Data.Translation translation = Singleton_Data.INSTANCE.translation;
        Data_Manager.TranslateString skill = Singleton_Data.INSTANCE.Dict_TranslateString[_id];
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

    private void Fire(UI_InvenSlot _Slot)
    {
        if (target == null || VisibleTarget(target) == false)
            return;

        _Slot.CoolingSlot();
        if (instBullet == null)
        {
            instBullet = Instantiate(instEffect, this.transform);
        }
        instBullet.transform.position = player.transform.position;
        instBullet.transform.rotation = player.transform.rotation;
        instBullet.SetTarget(target, unitSize);

        SlotAction();
    }

    void SlotAction()
    {
        UI_InvenSlot quickSlot = UI_Manager.instance.GetQuickSlot(currentIndex);
        Data_Manager.SkillStruct skillStruct = quickSlot.skillStruct;
        int index = (int)skillStruct.animationType;
        player.PlayAnimation(index);
        Debug.LogWarning($"{skillStruct.ID} + {skillStruct.animationType.ToString()}");
    }

    void Test(bool _input)
    {
        if (_input)
        {
            SlotAction();
        }
    }

    public Unit_Manager[] testTarget;
    void FollowTest()
    {
        CameraManager.instance.SetTarget(player.transform);
        for (int i = 0; i < testTarget.Length; i++)
        {
            UI_Manager.instance.AddHPUI(testTarget[i].transform);
        }
    }
}
