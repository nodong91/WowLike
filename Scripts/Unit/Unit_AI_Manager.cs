using System.Collections.Generic;
using UnityEngine;

public class Unit_AI_Manager : MonoBehaviour
{
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
    //public Unit_AI selectUnit;
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
        NormalSpawn();
        //AmbushSpawn();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartBattle();
        }

        if (Input.GetMouseButtonDown(0))
        {
            RayCasting(true);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            RayCasting(false);
        }
        if (Input.GetMouseButton(0))
        {
            RayCasting(true);
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
            //switch (spawnNode[i].nodeType)
            //{
            //    case Node.NodeType.Player:
            //        SpawnPlayer(node, spawnNode[i].unitID);
            //        break;

            //    case Node.NodeType.Monster:
            //        SpawnMonster(node, spawnNode[i].unitID);
            //        break;
            //}
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
    }

    public GameObject asdfadsf;
    void RayCasting(bool _input)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << 0;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Node node = map.GetNodeFromPosition(hit.point);
            if (_input == true)
            {
                asdfadsf.gameObject.SetActive(true);
                asdfadsf.transform.position = node.worldPosition;
                if (unitDict.ContainsKey(hit.transform.gameObject))
                {
                    //selectUnit = unitDict[hit.transform.gameObject];
                }
                else
                {
                    //selectUnit = null;
                }
            }
            else
            //if (selectUnit != null)
            {
                asdfadsf.gameObject.SetActive(false);
                if (node != null)
                {
                    if (node.onObject == null)
                    {
                        SpawnPlayer(node, "U10010");
                    }
                    else if (node.onObject.layer == LayerMask.NameToLayer("Player"))
                    {
                        Unit_AI unit = unitDict[node.onObject];
                        unit.deadUnit -= DeadPlayer;// 죽음 카운트
                        players.Remove(unit);
                        unitDict.Remove(unit.gameObject);
                        node.UnitOnNode(null);
                        Destroy(unit.gameObject);
                    }
                }
                //selectUnit.CommendMoveing(targetPosition);
            }

            Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red, 0.3f);
        }
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
