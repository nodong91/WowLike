using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit_Battle : MonoBehaviour
{
    public enum SpawnType
    {
        Normal,
        Ambush
    }
    public SpawnType spawnType = SpawnType.Normal;
    public Map_Generator map;
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
    public Data_Spawn spawnData;

    public static Unit_Battle instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Time.timeScale = timeScale;
        map.SetMapGrid(spawnData);

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
    Coroutine cameraInput;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartBattle();
        }

        if (Input.GetMouseButtonDown(0))
        {
            //if (cameraInput != null)
            //    StopCoroutine(cameraInput);
            //cameraInput = StartCoroutine(MouseLeftDrag(true));
            InputBegin();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            //if (cameraInput != null)
            //    StopCoroutine(cameraInput);
            //cameraInput = StartCoroutine(MouseLeftDrag(false));
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
        // �ź������� �� �� ������ ������
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
        if (Singleton_Data.INSTANCE.Dict_Unit.ContainsKey(_unitID) == false)
            return;
        Unit_AI inst = InstnaceUnit(_node, _unitID);
        inst.SetUnit(_unitID, LayerMask.NameToLayer("Player"));
        inst.deadUnit += DeadPlayer;// ���� ī��Ʈ

        players.Add(inst);
    }

    void SpawnMonster(Node _node, string _unitID)
    {
        if (Singleton_Data.INSTANCE.Dict_Unit.ContainsKey(_unitID) == false)
            return;
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
        inst.playerList = PlayerList;// Ÿ���� ã�� ����
        inst.monsterList = MonsterList;// Ÿ���� ã�� ����

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
        if (EventSystem.current.IsPointerOverGameObject() == true)
            return;

        dragSlot = uiBattle.GetInventory.GetDragSlot;
        bool moveUnit = (dragSlot != null && dragSlot.itemType == UI_InvenSlot.ItemType.Unit)
            || (selectedObject == true);

        Node node = RayCasting(true);
        if (node != null)
        {
            asdfadsf.gameObject.SetActive(moveUnit);
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

        if (selectedObject == true)// ������ ������Ʈ�� �ְ�
        {
            if (objectNode.onObject?.layer == LayerMask.NameToLayer("Player"))
            {
                selectedObject = false;
                if (objectNode.onObject == node.onObject)// ���� ��带 ������ ��
                {
                    // ����
                    UnitRemove(node);
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
            else
            {

            }
        }
        else if (dragSlot != null)// �κ��丮���� ������
        {
            // ����
            if (node.nodeType != Node.NodeType.Player || dragSlot.itemType != UI_InvenSlot.ItemType.Unit)// �÷��̾� ���� �� �ִ� ���� �ƴϸ�
            {
                Debug.LogWarning("���� �� ����");
                return;
            }

            UnitInstance(node);
            dragSlot = null;
        }
    }

    void UnitRemove(Node _node)
    {
        UI_InvenSlot emptySlot = uiBattle.GetInventory.TryEmptySlot();
        if (emptySlot == null)// �󽽷� Ȯ��
        {
            return;
        }
        // ����
        Unit_AI unit = unitDict[_node.onObject];
        unit.deadUnit -= DeadPlayer;// ���� ī��Ʈ
        players.Remove(unit);
        unitDict.Remove(unit.gameObject);
        _node.UnitOnNode(null);
        // �κ��丮�� ����
        emptySlot.SetUnitSlot(unit.GetUnitStruct);

        Destroy(unit.gameObject);
    }

    void UnitMove(Node _node)
    {
        // �̵�
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
        if (_node.onObject == null)// ��ĭ�� ���� ��
        {
            // ����
            string unitID = dragSlot.unitStruct.ID;
            SpawnPlayer(_node, unitID);
            // �κ��丮���� ����

            dragSlot.SetEmptySlot();
        }
        else// ��ĭ�� �ƴϸ� ������
        {
            Debug.LogWarning("���� �� ����!!!");
        }
    }

    Node RayCasting(bool _input)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << 0;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Node node = map.GetNodeFromPosition(hit.point);
            if (_input == true)
            {
                map.ClickNode(node);// �׽�Ʈ��
            }
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
            Debug.LogWarning($"�¸� : monsters {monsters.Count:D2} >> ���� ����");
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
            Debug.LogWarning($"�¸� : units {players.Count:D2} >> ����");
            Reward();
        }
    }

    void Reward()
    {
        string[] lootingItems = new string[3];
        for (int i = 0; i < lootingItems.Length; i++)
        {
            lootingItems[i] = "U10012";
        }
        uiBattle.GetInventory.AddLooting(lootingItems);
    }











    //public enum RotateType
    //{
    //    Normal,
    //    Focus
    //}
    //[SerializeField] private RotateType rotateType;
    //public bool clickLeft, isLeftDrag, inputMouseRight;
    //public Vector2 clickPosition;

    //IEnumerator MouseLeftDrag(bool _input)
    //{
    //    rotateType = RotateType.Normal;
    //    CameraManager.instance.InputRotate(_input);
    //    if (_input == true)
    //    {
    //        isLeftDrag = false;
    //        while (isLeftDrag == false)
    //        {
    //            Vector2 tempPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
    //            float dist = (tempPosition - clickPosition).magnitude;
    //            if (dist > 0.01f)
    //            {
    //                isLeftDrag = true;
    //            }
    //            yield return null;
    //        }
    //    }
    //}

    //void InputMouseRight(bool _input)
    //{
    //    inputMouseRight = _input;
    //    if (_input == true)
    //    {
    //        rotateType = RotateType.Focus;
    //    }
    //    //else if (inputDir != 0)
    //    //{
    //    //    rotateType = RotateType.Normal;
    //    //}
    //    CameraManager.instance.InputRotate(_input);
    //}

    //void InputMouseWheel(bool _input)
    //{
    //    float input = _input ? 0.1f : -0.1f;
    //    CameraManager.instance.delegateInputScroll(input);
    //}
}
