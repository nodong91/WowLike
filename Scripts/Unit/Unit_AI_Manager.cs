using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UI_Battle;

public class Unit_AI_Manager : MonoBehaviour
{
    public enum SpawnType
    {
        Normal,
        Ambush
    }
    public SpawnType spawnType = SpawnType.Normal;
    public Map_Node map;
    public float timeScale = 1f;

    [SerializeField] private List<Unit_AI> players = new List<Unit_AI>();
    [SerializeField] private List<Unit_AI> monsters = new List<Unit_AI>();

    public List<Unit_AI> PlayerList()
    {
        return players;
    }
    public List<Unit_AI> MonsterList()
    {
        return monsters;
    }

    private Dictionary<GameObject, Unit_AI> unitDict = new Dictionary<GameObject, Unit_AI>();
    public Dictionary<GameObject, Unit_AI> GetUnitDict { get { return unitDict; } }
    public UI_Battle uiBattle;
    public List<Node> randomNodes;

    public static Unit_AI_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Time.timeScale = timeScale;
        map.SetMapGrid();

        players.Clear();
        monsters.Clear();
        switch (spawnType)
        {
            case SpawnType.Normal:
                NormalSpawn();
                break;

            case SpawnType.Ambush:
                AmbushSpawn();
                break;
        }
        uiBattle.deleTimeScale = SetTimeScale;
        uiBattle.deleBattleStart = StartBattle;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartBattle();
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
        }


        //if (Input.GetMouseButtonDown(1))
        //{
        //    RayCasting(true);
        //}
        //else if (Input.GetMouseButtonUp(1))
        //{
        //    RayCasting(false);
        //}
    }

    public void SetTimeScale(float _timeScale)
    {
        timeScale = _timeScale;
        Time.timeScale = timeScale;
    }

    void NormalSpawn()
    {
        List<Data_Spawn.UnitNode> spawnNode = map.GetSpawnData.monsterNodes;
        for (int i = 0; i < spawnNode.Count; i++)
        {
            Vector2Int grid = spawnNode[i].spawnGrid;
            Node node = map.nodeMap[grid.x, grid.y];
            SpawnMonster(node, spawnNode[i].unitID);
        }
    }

    void AmbushSpawn()
    {
        // 매복당했을 때 막 섞여서 나오게
        randomNodes = map.RandomNodes();
        Queue<Node> queueNodes = new Queue<Node>();
        for (int i = 0; i < randomNodes.Count; i++)
        {
            queueNodes.Enqueue(randomNodes[i]);
        }

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
        Unit_AI inst = InstnaceUnit(_node, _unitID);
        inst.SetUnit(_unitID, LayerMask.NameToLayer("Player"));
        inst.deadUnit += DeadPlayer;// 죽음 카운트

        players.Add(inst);
    }

    void SpawnMonster(Node _node, string _unitID)
    {
        Unit_AI inst = InstnaceUnit(_node, _unitID);
        inst.SetUnit(_unitID, LayerMask.NameToLayer("Monster"));
        inst.deadUnit += DeadMonster;

        monsters.Add(inst);
    }

    Unit_AI InstnaceUnit(Node _node, string _unitID)
    {
        Unit_AI unit = Singleton_Data.INSTANCE.Dict_Unit[_unitID].unitProp;
        Unit_AI inst = Instantiate(unit, transform);
        inst.transform.position = _node.worldPosition;
        inst.transform.rotation = Quaternion.Euler(_node.worldPosition);
        inst.playerList = PlayerList;// 타겟을 찾기 위해
        inst.monsterList = MonsterList;// 타겟을 찾기 위해

        unitDict[inst.gameObject] = inst;
        _node.UnitOnNode(inst.gameObject);

        return inst;
    }

    void StartBattle()
    {
        foreach (var child in unitDict)
        {
            child.Value.StartBattle();
        }
        Debug.LogWarning("Battle Start");
    }

    public GameObject asdfadsf;
    bool selectedObject;
    public Node objectNode;
    UI_InvenSlot dragSlot;

    void InputBegin()
    {
        Node node = RayCasting(true);
        selectedObject = node?.onObject != null;
        if (selectedObject == true)
        {
            objectNode = node;
        }
    }
    
    void InputIng()
    {
        dragSlot = uiBattle.inventory.GetDragSlot;
        if (EventSystem.current.IsPointerOverGameObject() == true ||
            dragSlot == null || dragSlot.itemType != UI_InvenSlot.ItemType.Unit)
            return;

        Node node = RayCasting(true);
        if (node != null)
        {
            asdfadsf.gameObject.SetActive(dragSlot != null || selectedObject == true);
            asdfadsf.transform.position = node.worldPosition;
        }
    }

    void InputEnd()
    {
        Node node = RayCasting(false);
        asdfadsf.gameObject.SetActive(false);

        if (EventSystem.current.IsPointerOverGameObject() == true || node == null)
        {
            return;
        }

        if (selectedObject == true)// 선택한 오브젝트가 있고
        {
            //if (node.onObject?.layer != LayerMask.NameToLayer("Player"))
            //    return;

            selectedObject = false;
            if (objectNode.onObject == node.onObject)// 같은 노드를 눌렀을 때
            {
                UnitRemove(node);
            }
            else
            {
                UnitMove(node);
            }
        }
        else if (dragSlot != null)
        {
            // 생성
            if (node.nodeType != Node.NodeType.Player)
            {
                Debug.LogWarning("놓을 수 없음");
                return;
            }

            UnitInstance(node);
            dragSlot = null;
        }
    }

    void UnitRemove(Node _node)
    {
        UI_InvenSlot emptySlot = uiBattle.inventory.TryEmptySlot();
        if (emptySlot == null)// 빈슬롯 확인
        {
            return;
        }
        // 제거
        Unit_AI unit = unitDict[_node.onObject];
        unit.deadUnit -= DeadPlayer;// 죽음 카운트
        players.Remove(unit);
        unitDict.Remove(unit.gameObject);
        _node.UnitOnNode(null);
        // 인벤토리에 생성
        emptySlot.SetUnitSlot(unit.GetUnitStruct);

        Destroy(unit.gameObject);
    }

    void UnitMove(Node _node)
    {
        // 이동
        GameObject from = objectNode.onObject;
        GameObject to = _node.onObject;

        objectNode.onObject = to;
        if (to != null)
            to.transform.position = objectNode.worldPosition;
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

            dragSlot.SetEmptySlot();
        }
        else// 빈칸이 아니면 못놓게
        {
            Debug.LogWarning("놓을 수 없음!!!");
        }
    }

    Node RayCasting(bool _input)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << 0;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Node node = map.GetNodeFromPosition(hit.point);
            Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red, 0.3f);
            return node;
        }
        return null;
    }

    void DeadPlayer(Unit_AI _unit)
    {
        Debug.LogWarning(_unit.gameObject.name);
        players.Remove(_unit);
        if (players.Count == 0)
        {
            for (int i = 0; i < monsters.Count; i++)
            {
                monsters[i].BattleOver();
            }
            Debug.LogWarning($"승리 : monsters {monsters.Count:D2}");
        }
    }

    void DeadMonster(Unit_AI _unit)
    {
        Debug.LogWarning(_unit.gameObject.name);
        monsters.Remove(_unit);
        if (monsters.Count == 0)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].BattleOver();
            }
            Debug.LogWarning($"승리 : units {players.Count:D2}");
        }
    }
}
