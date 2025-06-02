using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//[ExecuteInEditMode]
public class Map_Generator : Map_Tile
{
    [Header("[ Map Grid ]")]
    public float nodeSize = 2f;
    Vector2Int worldGrid;
    Vector2 worldSize;

    [Tooltip("8방향 이동가능")]
    public bool diagonal;

    public Node[,] nodeMap;

    Data_Spawn spawnData;
    public Data_Spawn GetSpawnData { get { return spawnData; } }
    private List<Node> allNodes;
    private GameObject instParent;
    public UnityEngine.UI.Image baseTile;
    public CanvasGroup tileParent;

    public List<Node> RandomNodes()
    {
        List<Node> randomNodes = ShuffleList(allNodes, 0);
        return randomNodes;
    }

    // 리스트 섞기
    public static List<T> ShuffleList<T>(List<T> _list, int seed)
    {
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < _list.Count - 1; i++)
        {
            int randomIndex = prng.Next(i, _list.Count);
            T tempItem = _list[randomIndex];
            _list[randomIndex] = _list[i];
            _list[i] = tempItem;
        }
        return _list;
    }

    public void SetMapGrid(Data_Spawn _spawnData)
    {
        spawnData = _spawnData;
        if (spawnData == null)
            return;

        worldGrid = spawnData.worldGrid;
        worldSize = (Vector2)worldGrid * nodeSize;

        nodeMap = new Node[worldGrid.x, worldGrid.y];
        Vector3 worldBottomLeft = transform.position - Vector3.right * worldSize.x / 2 - Vector3.forward * worldSize.y / 2;
        if (allNodes == null)
            allNodes = new List<Node>();
        allNodes.Clear();

        for (int x = 0; x < worldGrid.x; x++)
        {
            for (int y = 0; y < worldGrid.y; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeSize + nodeSize * 0.5f) + Vector3.forward * (y * nodeSize + nodeSize * 0.5f);
                Vector2Int grid = new Vector2Int(x, y);
                nodeMap[x, y] = new Node(worldPoint, grid);
                allNodes.Add(nodeMap[x, y]);
            }
        }
        // 노드 타입 변경
        for (int i = 0; i < spawnData.unitNodes.Count; i++)
        {
            int x = spawnData.unitNodes[i].spawnGrid.x;
            int y = spawnData.unitNodes[i].spawnGrid.y;
            if (x < 0 || y < 0 || x >= worldGrid.x || y >= worldGrid.y)
                continue;
            Node node = nodeMap[x, y];
            node.SetNodeType(spawnData.unitNodes[i].nodeType);
        }
        InstanceNodeTile();
    }

    void ClickEvent(Vector2Int _grid)
    {
        Debug.LogWarning("World Position : " + _grid);
    }

    // 근처 타일 리스팅
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.grid.x + x;
                int checkY = node.grid.y + y;

                if (checkX >= 0 && checkX < worldGrid.x && checkY >= 0 && checkY < worldGrid.y)
                {
                    if (diagonal || (x == 0 || y == 0))    // 대각선 움직임 (8방향)
                    {
                        neighbours.Add(nodeMap[checkX, checkY]);
                    }
                }
            }
        }
        return neighbours;
    }

    public Node GetNodeFromPosition(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + worldSize.x * 0.5f) / worldSize.x;
        float percentY = (worldPosition.z + worldSize.y * 0.5f) / worldSize.y;

        int x = Mathf.Clamp((int)(worldGrid.x * percentX), 0, worldGrid.x - 1);
        int y = Mathf.Clamp((int)(worldGrid.y * percentY), 0, worldGrid.y - 1);
        return nodeMap[x, y];
    }

    public void ClickNode(Node _node)
    {
        Debug.Log($"ClickNode : {_node.grid}");
    }

    //=====================================================================================================
    // 길찾기
    //=====================================================================================================

    // 리스트안에 같은 타일이 있는지 체크
    public bool TryAccessibleTiles(Node _selectNode, List<Node> _checkList)
    {
        Vector2 selectGrid = new Vector2(_selectNode.grid.x, _selectNode.grid.y);
        for (int i = 0; i < _checkList.Count; i++)
        {
            Vector2 accessibleGrid = new Vector2(_checkList[i].grid.x, _checkList[i].grid.y);
            if (selectGrid == accessibleGrid)
                return true;
        }
        return false;
    }

    // 주변 노드 체크
    public string SetNeighbourArea(Node _node, List<Node> _activeArea)
    {
        string neighbours = "";
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != 0 || y != 0)
                {
                    int neighbourX = _node.grid.x + x;
                    int neighbourY = _node.grid.y + y;

                    if (neighbourX >= 0 && neighbourX < worldGrid.x && neighbourY >= 0 && neighbourY < worldGrid.y)
                    {
                        Node tile = nodeMap[neighbourX, neighbourY];
                        if (TryAccessibleTiles(tile, _activeArea) == true)
                        {
                            neighbours += "O";// 영역 타일
                        }
                        else if (TryAccessibleTiles(tile, _activeArea) == false)
                        {
                            neighbours += "X";// 영역 타일 아님
                        }
                    }
                    else
                    {
                        neighbours += "I"; //타일 없음
                    }
                }
            }
        }
        return neighbours;
    }


    //=====================================================================================================
    // 길찾기
    //=====================================================================================================

    public List<Node> FindPath(Node _startNode, Node _targetNode)
    {
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(_startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    openSet[i].fCost == currentNode.fCost &&
                    openSet[i].cost.y < currentNode.cost.y)
                {
                    currentNode = openSet[i];
                }
            }
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == _targetNode)
            {
                List<Node> newPath = RetracePath(_startNode, _targetNode);
                return newPath;
            }

            foreach (Node neighbour in GetNeighbours(currentNode))
            {
                if (neighbour.onObject || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.cost.x + GetDistance(currentNode, neighbour);

                if (newMovementCostToNeighbour < neighbour.cost.x || !openSet.Contains(neighbour))
                {
                    neighbour.cost.x = newMovementCostToNeighbour;
                    neighbour.cost.y = GetDistance(neighbour, _targetNode);
                    neighbour.parentNode = new Vector2Int(currentNode.grid.x, currentNode.grid.y);

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
        return default;
    }

    List<Node> RetracePath(Node _startNode, Node _endNode)
    {
        List<Node> newPath = new List<Node>();
        Node currentNode = _endNode;

        while (currentNode != _startNode)
        {
            newPath.Add(currentNode);

            Node parentNode = nodeMap[currentNode.parentNode.x, currentNode.parentNode.y];
            currentNode = parentNode;
        }

        newPath.Reverse();
        return newPath;
    }

    int GetDistance(Node _nodeA, Node _nodeB)
    {
        int distX = Mathf.Abs(_nodeA.grid.x - _nodeB.grid.x);
        int distY = Mathf.Abs(_nodeA.grid.y - _nodeB.grid.y);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }

    void InstanceNodeTile()// 플레이어 생성 가능 위치
    {
        if (instParent != null)
            Destroy(instParent);
        instParent = new GameObject();
        instParent.transform.SetParent(tileParent.transform);
        instParent.transform.localPosition = Vector3.zero;
        instParent.transform.rotation = tileParent.transform.rotation;
        instParent.transform.localScale = Vector3.one;

        List<Data_Spawn.UnitNode> tempList = spawnData.unitNodes;
        for (int i = 0; i < tempList.Count; i++)
        {
            if (tempList[i].nodeType != Node.NodeType.Player)// 플레이어 칸이 아니면 넘김
                continue;

            Vector2Int grid = tempList[i].spawnGrid;
            Node node = nodeMap[grid.x, grid.y];
            UnityEngine.UI.Image inst = Instantiate(baseTile, instParent.transform);
            //inst.onClick.AddListener(delegate { ClickEvent(node.grid); });
            inst.transform.position = node.worldPosition + instParent.transform.position;

            node.neighbours = TryTile(node);
            SetTile(inst, node.neighbours);
        }
    }

    public string TryTile(Node _node)
    {
        string neighbours = "";
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != 0 || y != 0)
                {
                    int neighbourX = _node.grid.x + x;
                    int neighbourY = _node.grid.y + y;

                    if (neighbourX >= 0 && neighbourX < worldGrid.x && neighbourY >= 0 && neighbourY < worldGrid.y)
                    {
                        Node tile = nodeMap[neighbourX, neighbourY];
                        if (TryAccessibleTiles(tile) == true)
                        {
                            neighbours += "O";// 영역 타일
                        }
                        else if (TryAccessibleTiles(tile) == false)
                        {
                            neighbours += "X";// 영역 타일 아님
                        }
                    }
                    else
                    {
                        neighbours += "I"; //타일 없음
                    }
                }
            }
        }
        return neighbours;
    }

    bool TryAccessibleTiles(Node _node)
    {
        for (int i = 0; i < spawnData.unitNodes.Count; i++)
        {
            List<Data_Spawn.UnitNode> tempList = spawnData.unitNodes;
            if (tempList[i].nodeType != Node.NodeType.Player)// 플레이어 칸이 아니면 넘김
                continue;

            Vector2Int grid = tempList[i].spawnGrid;
            if (_node.grid == grid)
            {
                return true;
            }
        }
        return false;
    }

    public void OnTileCanvas(bool _on)
    {
        tileParent.alpha = _on == true ? 1f : 0f;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(worldSize.x, 1, worldSize.y));

        if (nodeMap != null)
        {
            foreach (Node n in nodeMap)
            {
                Color color = Color.gray;
                switch (n.nodeType)
                {
                    case Node.NodeType.None:

                        break;

                    case Node.NodeType.Player:
                        color = Color.red;
                        break;

                    case Node.NodeType.Monster:
                        color = Color.green;
                        break;
                }
                GUIStyle fontStyle = new()
                {
                    fontSize = 20,
                    normal = { textColor = color },
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                };
                Handles.Label(n.worldPosition, $"{n.grid.x}/{n.grid.y}", fontStyle);
            }
        }
    }
#endif
}

[System.Serializable]
public class Node
{
    [HideInInspector]
    public string nodeName;
    public enum NodeType
    {
        None,
        Player,
        Monster
    }
    public NodeType nodeType;
    public Vector3 worldPosition;
    public GameObject onObject;// 유닛이 아니라 다른게 올라올 수도??
    public string neighbours;

    public Vector2Int grid;
    public Vector2Int cost;
    public Vector2Int parentNode;

    public Node(Vector3 _worldPos, Vector2Int _grid)
    {
        worldPosition = _worldPos;
        grid = _grid;

        nodeName = $"{nodeType} : {grid}, {cost}";
    }

    public void SetNodeType(NodeType _nodeType)
    {
        nodeType = _nodeType;
        nodeName = $"{nodeType} : {grid}, {cost}";
    }

    public void UnitOnNode(GameObject _onObject)
    {
        onObject = _onObject;
    }

    public int fCost
    {
        get
        {
            return cost.x + cost.y;
        }
    }
}