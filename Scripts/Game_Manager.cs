using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Game_Manager : MonoBehaviour
{
    [SerializeField] UI_Manager uiManager;
    UI_Manager instUIManager;
    [SerializeField] Map_Generator mapGenerator;
    Map_Generator instMapGenerator;
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

    public float timeScale = 1f;
    public int targetFrameRate = 60;

    private List<Unit_AI> players, monsters;

    public List<Unit_AI> PlayerList()
    {
        return players;
    }

    public List<Unit_AI> MonsterList()
    {
        return monsters;
    }

    private Dictionary<GameObject, Unit_AI> allUnitDict = new Dictionary<GameObject, Unit_AI>();
    public Dictionary<GameObject, Unit_AI> GetUnitDict { get { return allUnitDict; } }

    private List<Node> randomNodes;// 매복
    public Data_Spawn spawnData;

    public GameObject asdfadsf;
    public Unit_AI unitBase;
    //[SerializeField] 
    Node selectedNode;
    UI_InvenSlot dragSlot;

    Coroutine cameraInput;

    public static Game_Manager current;

    private void Awake()
    {
        current = this;
    }
    GameObject unitParent;
    private void Start()
    {
        Application.targetFrameRate = targetFrameRate;

        SetTimeScale(timeScale);
        if (instMapGenerator == null)
            instMapGenerator = Instantiate(mapGenerator, transform);
        instMapGenerator.SetMapGrid(spawnData);

        if (instUIManager == null)
            instUIManager = Instantiate(uiManager, transform);
        instUIManager.deleTimeScale = SetTimeScale;
        instUIManager.deleBattleStart = StartBattle;
        instUIManager.AddFollow(asdfadsf);// 임시 테스트

        if (unitParent != null)
            Destroy(unitParent);
        unitParent = new GameObject("[ Unit Parnet ]");
        players = new List<Unit_AI>();
        monsters = new List<Unit_AI>();
        switch (spawnType)
        {
            case SpawnType.Normal:
                NormalSpawn();
                break;

            case SpawnType.Ambush:
                AmbushSpawn();
                break;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (timeScale > 0)
                SetTimeScale(0f);
            else
                SetTimeScale(1f);
        }

        if (Input.GetMouseButtonDown(0))
        {
            InputBegin();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            InputEnd();
        }
        if (Input.GetMouseButton(0))
        {
            InputIng();
            instUIManager.ShakingUI(asdfadsf);
        }
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

        Color teamColor = Color.white;
        switch (_layer)
        {
            case "Player":
                teamColor = Color.red;
                inst.deadUnit += DeadPlayer;// 죽음 카운트
                players.Add(inst);
                break;

            case "Monster":
                teamColor = Color.blue;
                inst.deadUnit += DeadMonster;
                monsters.Add(inst);
                break;
        }
        // 유아이 세팅
        Follow_HP hp = instUIManager.AddFollow_Unit(inst);
        hp.sliderImage.color = teamColor;
        inst.deleUpdateHP = hp.SetHP;// 체력 바
        inst.deleUpdateAction = hp.SetAction;// 액션 바
        inst.deleDamage = instUIManager.DamageText;// 데미지
        inst.SetUnit(_unitID, LayerMask.NameToLayer(_layer));
    }

    Unit_AI InstnaceUnit(Node _node)
    {
        Unit_AI inst = Instantiate(unitBase, unitParent.transform);
        inst.transform.position = _node.worldPosition;
        inst.transform.rotation = Quaternion.Euler(_node.worldPosition);
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
        Debug.LogWarning("Battle Start");
    }

    void InputBegin()
    {
        if (EventSystem.current.IsPointerOverGameObject() == true)
            return;
        Node node = null;
        if (RayCasting(out Vector3 hitPoint) != null)
        {
            node = instMapGenerator.GetNodeFromPosition(hitPoint);
            instMapGenerator.ClickNode(node);// 테스트용
        }
        //Node node = RayCasting(true);
        selectedNode = node;
    }

    void InputIng()
    {
        if (EventSystem.current.IsPointerOverGameObject() == true)
            return;

        dragSlot = instUIManager.GetInventory.GetDragSlot;
        bool moveUnit = (dragSlot?.itemType == UI_InvenSlot.ItemType.Unit) || (selectedNode?.onObject != null);
        Node node = null;
        if (RayCasting(out Vector3 hitPoint) != null)
        {
            node = instMapGenerator.GetNodeFromPosition(hitPoint);
            instMapGenerator.ClickNode(node);// 테스트용
        }
        //Node node = RayCasting(true);
        if (node != null)
        {
            asdfadsf.gameObject.SetActive(moveUnit);
            asdfadsf.transform.position = node.worldPosition;
        }
    }

    void InputEnd()
    {
        Node node = null;
        if (RayCasting(out Vector3 hitPoint) != null)
        {
            node = instMapGenerator.GetNodeFromPosition(hitPoint);
            //if (_input == true)
            //{
            //    instMapGenerator.ClickNode(node);// 테스트용
            //}
        }

        //Node node = RayCasting(false);
        asdfadsf.gameObject.SetActive(false);

        if (EventSystem.current.IsPointerOverGameObject() == true || node == null)
        {
            return;
        }

        if (selectedNode?.onObject != null)
        {
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
            if (node.nodeType == Node.NodeType.Player && dragSlot.itemType == UI_InvenSlot.ItemType.Unit)// 플레이어 놓을 수 있는 곳이 아니면
            {
                UnitInstance(node);
                dragSlot = null;
            }
        }
        else
        {
            Debug.LogWarning($"인벤토리도 아니고{node.grid}");
        }
        selectedNode = default;
    }

    Node RayCasting(bool _input)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << 0;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Node node = instMapGenerator.GetNodeFromPosition(hit.point);
            if (_input == true)
            {
                instMapGenerator.ClickNode(node);// 테스트용
            }
            Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red, 0.3f);
            Debug.LogWarning(hit.transform.name);
            return node;
        }
        return null;
    }

    Transform RayCasting(out Vector3 _vector3)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << 0;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            _vector3 = hit.point;
            return hit.transform;
        }
        _vector3 = default;
        return null;
    }

    void UnitRemove(Node _node)
    {
        UI_InvenSlot emptySlot = instUIManager.GetInventory.TryEmptySlot();
        if (emptySlot == null)// 빈슬롯 확인
        {
            return;
        }
        // 제거
        Unit_AI unit = allUnitDict[_node.onObject];
        instUIManager.RemoveFollowHP(unit.gameObject);
        unit.deadUnit -= DeadPlayer;// 죽음 카운트
        players.Remove(unit);
        allUnitDict.Remove(unit.gameObject);
        _node.UnitOnNode(null);
        // 인벤토리에 생성
        //emptySlot.SetUnitSlot(unit.GetUnitStruct);
        emptySlot.SetSlot(unit.GetUnitStruct.ID);

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
}
