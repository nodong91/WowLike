using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Game_Manager : Unit_Generator
{
    public float timeScale = 1f;
    public int targetFrameRate = 60;
    [SerializeField] UI_StartMenu uiStartMenu;
    UI_StartMenu instUIStartMenu;
    [SerializeField] UI_Manager uiManager;
    UI_Manager instUIManager;
    [SerializeField] Map_Generator mapGenerator;
    Map_Generator instMapGenerator;
    public Map_Generator GetMapGenerator { get { return instMapGenerator; } }

    public enum SpawnType
    {
        Normal,
        Ambush
    }
    public SpawnType spawnType = SpawnType.Normal;

    public enum CurrentMode
    {
        None,
        Game,
    }
    public CurrentMode currentMode;

    public Unit_AI unitBase;
    GameObject instParent;
    public Transform GetInstParent { get { return instParent.transform; } }

    public Unit_Player unitPlayer;
    private List<Unit_AI> players, monsters;
    public List<Unit_AI> PlayerList() { return players; }
    public List<Unit_AI> MonsterList() { return monsters; }

    Dictionary<GameObject, Unit_AI> allUnitDict = new Dictionary<GameObject, Unit_AI>();
    public Dictionary<GameObject, Unit_AI> GetUnitDict { get { return allUnitDict; } }

    private List<Node> randomNodes;// 매복
    public Data_Spawn spawnData;

    public GameObject asdfadsf;
    //[SerializeField] 
    Node selectedNode, displayNode;
    UI_InvenSlot dragSlot;

    public static Game_Manager current;

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        Application.targetFrameRate = targetFrameRate;

        SetTimeScale(timeScale);

        if (instUIStartMenu == null)
            instUIStartMenu = Instantiate(uiStartMenu, transform);
        instUIStartMenu.SetStartMenu();

        if (instMapGenerator == null)
            instMapGenerator = Instantiate(mapGenerator, transform);
        instMapGenerator.SetMapGrid(spawnData);
        SetInstUIManager();

        if (instParent != null)
            Destroy(instParent);
        instParent = new GameObject("[ Instance Parnet ]");

        players = new List<Unit_AI>();
        monsters = new List<Unit_AI>();
        SetPlayer();
        switch (spawnType)
        {
            case SpawnType.Normal:
                NormalSpawn();
                break;

            case SpawnType.Ambush:
                AmbushSpawn();
                break;
        }
        SetMouse();
    }

    public delegate void DeleUITarget(GameObject _target);
    public DeleUITarget deleShakingUI;
    public DeleUITarget deleAddFollowUI;
    public DeleUITarget deleRemoveFollowHP;
    public DeleUITarget deleFollowClosestTarget;

    public delegate void DeleUnitTarget(Unit_AI _target);
    public DeleUnitTarget deleAddFollowHP;

    void SetInstUIManager()
    {
        if (instUIManager == null)
            instUIManager = Instantiate(uiManager, transform);
        instUIManager.deleTimeScale = SetTimeScale;
        instUIManager.deleBattleStart = StartBattle;

        deleShakingUI = instUIManager.followManager.ShakingUI;
        deleAddFollowUI = instUIManager.followManager.AddFollowUI;
        deleAddFollowHP = instUIManager.followManager.AddFollowHP;
        deleRemoveFollowHP = instUIManager.followManager.RemoveFollowHP;
        deleFollowClosestTarget = instUIManager.followManager.FollowClosestTarget;


        deleAddFollowUI(asdfadsf);// 임시 테스트
    }

    void SetMouse()
    {
        Singleton_Controller.INSTANCE.key_MouseLeft += InputMousetLeft;
        //Singleton_Controller.INSTANCE.key_MouseRight += InputMouseRight;
        //Singleton_Controller.INSTANCE.key_MouseWheel += InputMouseWheel;
    }

    public void SetTimeScale(float _timeScale)
    {
        timeScale = _timeScale;
        Time.timeScale = timeScale;
    }

    void NormalSpawn()
    {
        List<Data_Spawn.UnitNode> spawnNode = instMapGenerator.GetSpawnData.unitNodes;
        for (int i = 0; i < spawnNode.Count; i++)
        {
            Vector2Int grid = spawnNode[i].spawnGrid;
            Node node = instMapGenerator.nodeMap[grid.x, grid.y];
            if (spawnNode[i].nodeType == Node.NodeType.Player)
            {
                SpawnPlayer(node, spawnNode[i].unitID);
            }
            else if (spawnNode[i].nodeType == Node.NodeType.Monster)
            {
                SpawnMonster(node, spawnNode[i].unitID);
            }
        }
    }

    void AmbushSpawn()// 매복
    {
        // 매복당했을 때 막 섞여서 나오게
        randomNodes = instMapGenerator.RandomNodes();
        Queue<Node> queueNodes = new Queue<Node>();
        for (int i = 0; i < randomNodes.Count; i++)
        {
            queueNodes.Enqueue(randomNodes[i]);
        }

        // 테스트
        for (int i = 0; i < 3; i++)
        {
            SpawnPlayer(queueNodes.Dequeue(), "U10010");
        }

        for (int i = 0; i < 3; i++)
        {
            SpawnMonster(queueNodes.Dequeue(), "U10012");
        }
    }

    void SetPlayer()
    {
        unitPlayer.SetUnit(unitPlayer.unitID, LayerMask.NameToLayer("Player"));
        deleAddFollowHP(unitPlayer);
        unitPlayer.deadUnit += DeadPlayer;// 죽음 카운트
        players.Add(unitPlayer);
        unitPlayer.playerList = PlayerList;// 타겟을 찾기 위해
        unitPlayer.monsterList = MonsterList;// 타겟을 찾기 위해
        unitPlayer.tryPoint = TryUnitPoint;// 타겟 포인트에 갈 수 있는지 체크
        unitPlayer.SetPlayer();

        allUnitDict[unitPlayer.gameObject] = unitPlayer;
        battleOver += unitPlayer.BattleOver;
    }

    void SpawnPlayer(Node _node, string _unitID)
    {
        SetSpawnUnit(_node, _unitID, "Player");
    }

    void SpawnMonster(Node _node, string _unitID)
    {
        SetSpawnUnit(_node, _unitID, "Monster");
    }

    void SetSpawnUnit(Node _node, string _unitID, string _layer)
    {
        if (Singleton_Data.INSTANCE.Dict_Unit.ContainsKey(_unitID) == false)
            return;

        Unit_AI inst = InstnaceUnit(_node);
        inst.gameObject.name = $"{_layer} : {_node.grid}";
        inst.SetUnit(_unitID, LayerMask.NameToLayer(_layer));
        deleAddFollowHP(inst);

        switch (_layer)
        {
            case "Player":
                inst.deadUnit += DeadPlayer;// 죽음 카운트
                players.Add(inst);
                break;

            case "Monster":
                inst.deadUnit += DeadMonster;
                monsters.Add(inst);
                break;
        }
    }

    Unit_AI InstnaceUnit(Node _node)
    {
        Unit_AI inst = Instantiate(unitBase, GetInstParent);
        Vector3 pos = _node.worldPosition;
        Quaternion rot = Quaternion.Euler(_node.worldPosition);
        inst.transform.SetPositionAndRotation(pos, rot);
        inst.playerList = PlayerList;// 타겟을 찾기 위해
        inst.monsterList = MonsterList;// 타겟을 찾기 위해
        inst.tryPoint = TryUnitPoint;// 타겟 포인트에 갈 수 있는지 체크

        allUnitDict[inst.gameObject] = inst;
        _node.UnitOnNode(inst.gameObject);

        battleOver += inst.BattleOver;
        return inst;
    }

    void StartBattle()
    {
        if (players.Count * monsters.Count <= 0)
            return;

        switch (currentMode)
        {
            case CurrentMode.None:
                currentMode = CurrentMode.Game;
                foreach (var child in allUnitDict)
                {
                    child.Value.SetStart();// 유닛 전투 시작
                }
                instMapGenerator.OnTileCanvas(false);// 맵아래 캔버스 제거
                break;

            case CurrentMode.Game:

                break;
        }
        Debug.LogWarning($"Battle Start{players.Count} {monsters.Count}");
    }
    Coroutine leftClick;
    void InputMousetLeft(bool _input)
    {
        if (_input == true)
        {
            InputBegin();
        }
        else
        {
            InputEnd();
        }
    }

    public void InputBegin()
    {
        leftClick = StartCoroutine(InputIng());
        //if (EventSystem.current.IsPointerOverGameObject() == true)// ui클릭 확인
        //    return;
        Node node = null;
        Vector3 hitPoint = RayCasting();
        if (hitPoint != Vector3.zero)
        {
            node = instMapGenerator.GetNodeFromPosition(hitPoint);
            instMapGenerator.ClickNode(node);// 테스트용
        }
        selectedNode = node;
    }

    IEnumerator InputIng()
    {
        deleShakingUI(asdfadsf);
        dragSlot = instUIManager.GetInventory.GetDragSlot;
        bool moveUnit = (dragSlot?.itemType == ItemType.Unit) || (selectedNode?.onObject != null);
        while (true)
        {
            Node node = null;
            Vector3 hitPoint = RayCasting();
            if (hitPoint != Vector3.zero)
            {
                node = instMapGenerator.GetNodeFromPosition(hitPoint);
                instMapGenerator.ClickNode(node);// 테스트용
                if (displayNode != node)
                {
                    // 유닛 배치할 때만 사용 (버프가능 노드 확인용)
                    displayNode = node;
                    instUIManager.followManager.OnBuff(node);
                }
            }

            if (node != null)
            {
                asdfadsf.gameObject.SetActive(moveUnit);
                asdfadsf.transform.position = node.worldPosition;
            }
            yield return null;
            Debug.LogWarning("oij");
        }
    }

    void InputEnd()
    {
        if (leftClick != null)
            StopCoroutine(leftClick);

        Node node = null;
        Vector3 hitPoint = RayCasting();
        if (hitPoint != Vector3.zero)
        {
            node = instMapGenerator.GetNodeFromPosition(hitPoint);
        }
        asdfadsf.gameObject.SetActive(false);

        if (selectedNode?.onObject != null)
        {
            // 노드 이동
            Debug.LogWarning($"놓을 수{node.grid}  {selectedNode.onObject.layer}");
            if (selectedNode.onObject.layer == LayerMask.NameToLayer("Player"))// 플레이어 선택
            {
                if (selectedNode.onObject == node.onObject)// 같은 노드를 눌렀을 때
                {
                    UnitRemove(node);  // 제거
                }
                else if (node.nodeType == Node.NodeType.Player)
                {
                    if (node.onObject == null)
                    {
                        // 이동
                        UnitMove(node);
                    }
                    else
                    {
                        // 교체
                    }
                }
            }
        }
        else if (dragSlot != null)// 인벤토리에서 꺼내고
        {
            // 생성
            if (node.nodeType == Node.NodeType.Player && dragSlot.itemType == ItemType.Unit)// 플레이어 놓을 수 있는 곳이 아니면
            {
                UnitInstance(node);
                dragSlot = null;
            }
        }
        else
        {
            Debug.LogWarning($"인벤토리도 아니고{node?.grid}");
        }
        selectedNode = default;
    }

    Vector3 RayCasting()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << 0;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red, 0.3f);
            Debug.LogWarning(hit.transform.name);
            return hit.point;
        }
        return Vector3.zero;
    }

    void UnitRemove(Node _node)
    {
        // 제거
        Unit_AI unit = allUnitDict[_node.onObject];
        if (instUIManager.GetInventory.AddInventory(unit.GetUnitStruct.ID) == false)
        {
            Debug.LogWarning("빈 슬롯이 없습니다.");
            return;
        }
        deleRemoveFollowHP(unit.gameObject);
        unit.deadUnit -= DeadPlayer;// 죽음 카운트
        players.Remove(unit);
        allUnitDict.Remove(unit.gameObject);
        _node.UnitOnNode(null);

        Destroy(unit.gameObject);
    }

    void UnitMove(Node _node)
    {
        // 이동
        GameObject from = selectedNode.onObject;
        GameObject to = _node.onObject;

        selectedNode.onObject = to;
        if (to != null)
            to.transform.position = selectedNode.worldPosition;
        _node.onObject = from;
        if (from != null)
            from.transform.position = _node.worldPosition;
    }

    void UnitInstance(Node _node)
    {
        if (_node.onObject == null)// 빈칸에 놓을 때
        {
            // 생성
            string unitID = dragSlot.unitStruct.ID;
            SpawnPlayer(_node, unitID);
            // 인벤토리에서 제거

            dragSlot.SetSlot(null);
        }
        else// 빈칸이 아니면 못놓게
        {
            Debug.LogWarning("놓을 수 없음!!!");
        }
    }

    bool TryUnitPoint(Vector3 _target, float _unitSize)
    {
        foreach (var child in allUnitDict)
        {
            float dist = (child.Key.transform.position - _target).magnitude;
            float unitSize = child.Value.GetUnitSize + _unitSize;
            if (dist < unitSize)
            {
                return false;
            }
        }
        return true;
    }

    void DeadPlayer(Unit_AI _unit)
    {
        players.Remove(_unit);
        if (players.Count == 0)
        {
            GameOver(false);
        }
    }

    public List<string> resultItems = new List<string>();
    void DeadMonster(Unit_AI _unit)
    {
        for (int i = 0; i < _unit.lootings.Length; i++)
        {
            // 보상 세팅
            resultItems.Add(_unit.lootings[i]);
        }

        monsters.Remove(_unit);
        if (monsters.Count == 0)
        {
            GameOver(true);
        }
    }

    public delegate UI_InvenSlot[] GetQuickSlots();
    public GetQuickSlots getQuickSlots;
    public void QuickSlotAction(int _index)
    {
        UI_InvenSlot slot = getQuickSlots()[_index];
        Debug.LogWarning("퀵슬롯 사용!!!!!!!!!!! : " + slot.name);
    }

    public delegate void DeleBattleOver();
    public DeleBattleOver battleOver;

    void GameOver(bool _win)
    {
        SetTimeScale(1f);
        battleOver();
        if (_win)
            Reward();
    }

    void Reward()
    {
        instUIManager.GetInventory.AddLooting(resultItems);
    }













    public void FollowClosestTarget(GameObject _target)
    {
        deleFollowClosestTarget(_target);
    }
}
