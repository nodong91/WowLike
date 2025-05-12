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

    private List<Node> randomNodes;// �ź�
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
        instUIManager.AddFollow(asdfadsf);// �ӽ� �׽�Ʈ

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

    void AmbushSpawn()// �ź�
    {
        // �ź������� �� �� ������ ������
        randomNodes = instMapGenerator.RandomNodes();
        Queue<Node> queueNodes = new Queue<Node>();
        for (int i = 0; i < randomNodes.Count; i++)
        {
            queueNodes.Enqueue(randomNodes[i]);
        }
        // �׽�Ʈ
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
                inst.deadUnit += DeadPlayer;// ���� ī��Ʈ
                players.Add(inst);
                break;

            case "Monster":
                teamColor = Color.blue;
                inst.deadUnit += DeadMonster;
                monsters.Add(inst);
                break;
        }
        // ������ ����
        Follow_HP hp = instUIManager.AddFollow_Unit(inst);
        hp.sliderImage.color = teamColor;
        inst.deleUpdateHP = hp.SetHP;// ü�� ��
        inst.deleUpdateAction = hp.SetAction;// �׼� ��
        inst.deleDamage = instUIManager.DamageText;// ������
        inst.SetUnit(_unitID, LayerMask.NameToLayer(_layer));
    }

    Unit_AI InstnaceUnit(Node _node)
    {
        Unit_AI inst = Instantiate(unitBase, unitParent.transform);
        inst.transform.position = _node.worldPosition;
        inst.transform.rotation = Quaternion.Euler(_node.worldPosition);
        inst.playerList = PlayerList;// Ÿ���� ã�� ����
        inst.monsterList = MonsterList;// Ÿ���� ã�� ����
        inst.tryPoint = TryUnitPoint;// Ÿ�� ����Ʈ�� �� �� �ִ��� üũ

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
                    child.Value.SetStart();// ���� ���� ����
                }
                instMapGenerator.OnTileCanvas(false);// �ʾƷ� ĵ���� ����
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
            instMapGenerator.ClickNode(node);// �׽�Ʈ��
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
            instMapGenerator.ClickNode(node);// �׽�Ʈ��
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
            //    instMapGenerator.ClickNode(node);// �׽�Ʈ��
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
            Debug.LogWarning($"���� ��{node.grid}  {selectedNode.onObject.layer}");
            if (selectedNode.onObject.layer == LayerMask.NameToLayer("Player"))// �÷��̾� ����
            {
                if (selectedNode.onObject == node.onObject)// ���� ��带 ������ ��
                {
                    UnitRemove(node);  // ����
                }
                else if (node.nodeType == Node.NodeType.Player)
                {
                    if (node.onObject == null)
                    {
                        // �̵�
                        UnitMove(node);
                    }
                    else
                    {
                        // ��ü
                    }
                }
            }
        }
        else if (dragSlot != null)// �κ��丮���� ������
        {
            // ����
            if (node.nodeType == Node.NodeType.Player && dragSlot.itemType == UI_InvenSlot.ItemType.Unit)// �÷��̾� ���� �� �ִ� ���� �ƴϸ�
            {
                UnitInstance(node);
                dragSlot = null;
            }
        }
        else
        {
            Debug.LogWarning($"�κ��丮�� �ƴϰ�{node.grid}");
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
                instMapGenerator.ClickNode(node);// �׽�Ʈ��
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
        if (emptySlot == null)// �󽽷� Ȯ��
        {
            return;
        }
        // ����
        Unit_AI unit = allUnitDict[_node.onObject];
        instUIManager.RemoveFollowHP(unit.gameObject);
        unit.deadUnit -= DeadPlayer;// ���� ī��Ʈ
        players.Remove(unit);
        allUnitDict.Remove(unit.gameObject);
        _node.UnitOnNode(null);
        // �κ��丮�� ����
        //emptySlot.SetUnitSlot(unit.GetUnitStruct);
        emptySlot.SetSlot(unit.GetUnitStruct.ID);

        Destroy(unit.gameObject);
    }

    void UnitMove(Node _node)
    {
        // �̵�
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
        if (_node.onObject == null)// ��ĭ�� ���� ��
        {
            // ����
            string unitID = dragSlot.unitStruct.ID;
            SpawnPlayer(_node, unitID);
            // �κ��丮���� ����

            dragSlot.SetSlot(null);
        }
        else// ��ĭ�� �ƴϸ� ������
        {
            Debug.LogWarning("���� �� ����!!!");
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
            // ���� ����
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
