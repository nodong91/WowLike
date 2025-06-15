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

    private List<Node> randomNodes;// �ź�
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


        deleAddFollowUI(asdfadsf);// �ӽ� �׽�Ʈ
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

    void SetPlayer()
    {
        unitPlayer.SetUnit(unitPlayer.unitID, LayerMask.NameToLayer("Player"));
        deleAddFollowHP(unitPlayer);
        unitPlayer.deadUnit += DeadPlayer;// ���� ī��Ʈ
        players.Add(unitPlayer);
        unitPlayer.playerList = PlayerList;// Ÿ���� ã�� ����
        unitPlayer.monsterList = MonsterList;// Ÿ���� ã�� ����
        unitPlayer.tryPoint = TryUnitPoint;// Ÿ�� ����Ʈ�� �� �� �ִ��� üũ
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
                inst.deadUnit += DeadPlayer;// ���� ī��Ʈ
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
        //if (EventSystem.current.IsPointerOverGameObject() == true)// uiŬ�� Ȯ��
        //    return;
        Node node = null;
        Vector3 hitPoint = RayCasting();
        if (hitPoint != Vector3.zero)
        {
            node = instMapGenerator.GetNodeFromPosition(hitPoint);
            instMapGenerator.ClickNode(node);// �׽�Ʈ��
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
                instMapGenerator.ClickNode(node);// �׽�Ʈ��
                if (displayNode != node)
                {
                    // ���� ��ġ�� ���� ��� (�������� ��� Ȯ�ο�)
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
            // ��� �̵�
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
            if (node.nodeType == Node.NodeType.Player && dragSlot.itemType == ItemType.Unit)// �÷��̾� ���� �� �ִ� ���� �ƴϸ�
            {
                UnitInstance(node);
                dragSlot = null;
            }
        }
        else
        {
            Debug.LogWarning($"�κ��丮�� �ƴϰ�{node?.grid}");
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
        // ����
        Unit_AI unit = allUnitDict[_node.onObject];
        if (instUIManager.GetInventory.AddInventory(unit.GetUnitStruct.ID) == false)
        {
            Debug.LogWarning("�� ������ �����ϴ�.");
            return;
        }
        deleRemoveFollowHP(unit.gameObject);
        unit.deadUnit -= DeadPlayer;// ���� ī��Ʈ
        players.Remove(unit);
        allUnitDict.Remove(unit.gameObject);
        _node.UnitOnNode(null);

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

    public delegate UI_InvenSlot[] GetQuickSlots();
    public GetQuickSlots getQuickSlots;
    public void QuickSlotAction(int _index)
    {
        UI_InvenSlot slot = getQuickSlots()[_index];
        Debug.LogWarning("������ ���!!!!!!!!!!! : " + slot.name);
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
