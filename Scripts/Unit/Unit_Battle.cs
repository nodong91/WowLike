using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit_Battle : MonoBehaviour
{
    [SerializeField] UI_Battle uiBattle;
    UI_Battle instUIBattle;
    public enum SpawnType
    {
        Normal,
        Ambush
    }
    public SpawnType spawnType = SpawnType.Normal;
    [SerializeField] Map_Generator mapGenerator;
    Map_Generator instMapGenerator;
    public float timeScale = 1f;

    private List<Unit_AI> players, monsters;

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

    private List<Node> randomNodes;// �ź�
    public Data_Spawn spawnData;

    public GameObject asdfadsf;
    public Unit_AI unitBase;
    //[SerializeField] 
    Node selectedNode;
    UI_InvenSlot dragSlot;

    Coroutine cameraInput;

    public static Unit_Battle INSTANCE;

    private void Awake()
    {
        INSTANCE = this;
    }

    private void Start()
    {
        Time.timeScale = timeScale;
        if (instMapGenerator == null)
            instMapGenerator = Instantiate(mapGenerator, transform);
        instMapGenerator.SetMapGrid(spawnData);

        if (instUIBattle == null)
            instUIBattle = Instantiate(uiBattle, transform);
        instUIBattle.deleTimeScale = SetTimeScale;
        instUIBattle.deleBattleStart = StartBattle;
        instUIBattle.AddFollow(asdfadsf);// �ӽ� �׽�Ʈ

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
            //StartBattle();
            for (int i = 0; i < monsters.Count; i++)
            {
                //monsters[i].StateMachine(Unit_AI.State.Damage);
                //monsters[i].StartBattle();
                Data_Manager.SkillStruct newStruct = new Data_Manager.SkillStruct
                {
                    aggro = 0,
                    ccType = Data_Manager.SkillStruct.CCType.Normal,
                };
                monsters[i].TakeDamage(monsters[i], monsters[i].transform.position, 0f, newStruct);
            }
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
            instUIBattle.ShakingUI(asdfadsf);
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
        List<Data_Spawn.UnitNode> spawnNode = instMapGenerator.GetSpawnData.monsterNodes;
        for (int i = 0; i < spawnNode.Count; i++)
        {
            Vector2Int grid = spawnNode[i].spawnGrid;
            Node node = instMapGenerator.nodeMap[grid.x, grid.y];
            SpawnMonster(node, spawnNode[i].unitID);
        }
    }

    void AmbushSpawn()
    {
        // �ź������� �� �� ������ ������
        randomNodes = instMapGenerator.RandomNodes();
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
        //    if (Singleton_Data.INSTANCE.Dict_Unit.ContainsKey(_unitID) == false)
        //        return;
        //    Unit_AI inst = InstnaceUnit(_node, _unitID);
        //    Follow_HP hp = instUIBattle.AddFollow_Unit(inst);
        //    inst.deadUnit += DeadPlayer;// ���� ī��Ʈ
        //    inst.deleUpdateHP = hp.SetHP;// ü�� ��
        //    inst.SetUnit(_unitID, LayerMask.NameToLayer("Player"));

        if (Singleton_Data.INSTANCE.Dict_Unit.ContainsKey(_unitID) == false)
            return;

        Unit_AI inst = SetSpawnUnit(_node, _unitID, "Player");
        players.Add(inst);
    }

    void SpawnMonster(Node _node, string _unitID)
    {
        if (Singleton_Data.INSTANCE.Dict_Unit.ContainsKey(_unitID) == false)
            return;

        Unit_AI inst = SetSpawnUnit(_node, _unitID, "Monster");
        monsters.Add(inst);
    }

    Unit_AI SetSpawnUnit(Node _node, string _unitID, string _layer)
    {
        Unit_Animation unit = Singleton_Data.INSTANCE.Dict_Unit[_unitID].unitProp;
        Unit_AI inst = InstnaceUnit(_node, unit);

        Color teamColor = Color.white;
        switch (_layer)
        {
            case "Player":
                inst.deadUnit += DeadPlayer;// ���� ī��Ʈ
                teamColor = Color.red;
                break;
            case "Monster":
                inst.deadUnit += DeadMonster;
                teamColor = Color.blue;
                break;
        }
        // ������ ����
        Follow_HP hp = instUIBattle.AddFollow_Unit(inst);
        hp.sliderImage.color = teamColor;
        inst.deleUpdateHP = hp.SetHP;// ü�� ��
        inst.SetUnit(_unitID, LayerMask.NameToLayer(_layer));
        return inst;
    }

    Unit_AI InstnaceUnit(Node _node, Unit_Animation _unit)
    {
        Unit_AI inst = Instantiate(unitBase, transform);
        inst.transform.position = _node.worldPosition;
        inst.transform.rotation = Quaternion.Euler(_node.worldPosition);
        inst.playerList = PlayerList;// Ÿ���� ã�� ����
        inst.monsterList = MonsterList;// Ÿ���� ã�� ����

        //Unit_AI unit = Singleton_Data.INSTANCE.Dict_Unit[_unitID].unitProp;
        Unit_Animation instUnit = Instantiate(_unit, inst.transform);
        inst.unitAnimation = instUnit;
        instUnit.SetAnimator();
        instUnit.PlayAnimation(1);

        unitDict[inst.gameObject] = inst;
        _node.UnitOnNode(inst.gameObject);

        return inst;
    }

    void StartBattle()
    {
        foreach (var child in unitDict)
        {
            child.Value.StateMachine(Unit_AI.State.Idle);
        }
        instMapGenerator.OnTileCanvas(false);// �ʾƷ� ĵ���� ����
        Debug.LogWarning("Battle Start");
    }

    void InputBegin()
    {
        if (EventSystem.current.IsPointerOverGameObject() == true)
            return;

        Node node = RayCasting(true);
        selectedNode = node;
    }

    void InputIng()
    {
        if (EventSystem.current.IsPointerOverGameObject() == true)
            return;

        dragSlot = instUIBattle.GetInventory.GetDragSlot;
        bool moveUnit = (dragSlot?.itemType == UI_InvenSlot.ItemType.Unit) || (selectedNode?.onObject != null);

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
                Debug.LogWarning($"���� {node.grid}");
            }
        }
        else
        {
            Debug.LogWarning($"�κ��丮�� �ƴϰ�{node.grid}");
        }
        selectedNode = default;
    }

    void UnitRemove(Node _node)
    {
        UI_InvenSlot emptySlot = instUIBattle.GetInventory.TryEmptySlot();
        if (emptySlot == null)// �󽽷� Ȯ��
        {
            return;
        }
        // ����
        Unit_AI unit = unitDict[_node.onObject];
        instUIBattle.RemoveFollowHP(unit.gameObject);
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
            Node node = instMapGenerator.GetNodeFromPosition(hit.point);
            if (_input == true)
            {
                instMapGenerator.ClickNode(node);// �׽�Ʈ��
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
        instUIBattle.GetInventory.AddLooting(lootingItems);
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
