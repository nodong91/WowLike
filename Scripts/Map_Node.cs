using System.Collections.Generic;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
#endif
[ExecuteInEditMode]
public class Map_Node : MonoBehaviour
{
    [Header("[ Map Grid ]")]
    public Vector2 worldSize;
    public float nodeSize;

    [Tooltip("8방향 이동가능")]
    public bool diagonal;

    public Node[,] nodeMap;

    float nodeDiameter;
    public int gridX, gridY;
    public Data_Spawn spawnData;
    public Data_Spawn GetSpawnData { get { return spawnData; } }
    public List<Node> allNodes;
    public GameObject adsfafd;

#if UNITY_EDITOR
    private void Update()
    {
        SetMapGrid();
    }
#endif
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
    //Vector2Int prevGrid;
    //public void EditorMapSetting(Vector3 _worldPosition, bool _input, bool _left)
    //{
    //    if (_input)
    //    {
    //        Node node = GetNodeFromPosition(_worldPosition);
    //        prevGrid = node.grid;
    //    }
    //    else
    //    {
    //        Node node = GetNodeFromPosition(_worldPosition);
    //        int minX = Mathf.Min(prevGrid.x, node.grid.x);
    //        int minY = Mathf.Min(prevGrid.y, node.grid.y);
    //        int maxX = Mathf.Max(prevGrid.x, node.grid.x) + 1;
    //        int maxY = Mathf.Max(prevGrid.y, node.grid.y) + 1;

    //        for (int x = minX; x < maxX; x++)
    //        {
    //            for (int y = minY; y < maxY; y++)
    //            {
    //                Vector2Int grid = new Vector2Int(x, y);
    //                if (_left)
    //                {
    //                    if (spawnData.playerNode.Contains(grid))
    //                    {
    //                        spawnData.playerNode.Remove(grid);
    //                        nodeMap[x, y].SetNodeType(Node.NodeType.None);
    //                    }
    //                    else
    //                    {
    //                        spawnData.playerNode.Add(grid);
    //                        nodeMap[x, y].SetNodeType(Node.NodeType.Player);
    //                    }
    //                }
    //                else
    //                {
    //                    if (spawnData.monsterNode.Contains(grid))
    //                    {
    //                        spawnData.monsterNode.Remove(grid);
    //                        nodeMap[x, y].SetNodeType(Node.NodeType.None);
    //                    }
    //                    else
    //                    {
    //                        spawnData.monsterNode.Add(grid);
    //                        nodeMap[x, y].SetNodeType(Node.NodeType.Monster);
    //                    }
    //                }
    //            }
    //        }
    //        Debug.LogWarning($"{prevGrid} {node.grid} : {node.nodeType}");
    //    }

    //}

    // 노드 세팅
    [ContextMenu("SetMapGrid")]
    public void Setasdfaf()
    {
        SetMapGrid();
    }

    public void SetMapGrid()
    {
        if (worldSize.x * worldSize.y <= 0)
            return;

        nodeDiameter = nodeSize * 2;
        gridX = Mathf.RoundToInt(worldSize.x / nodeDiameter);
        gridY = Mathf.RoundToInt(worldSize.y / nodeDiameter);

        nodeMap = new Node[gridX, gridY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * worldSize.x / 2 - Vector3.forward * worldSize.y / 2;
        allNodes.Clear();

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeSize) + Vector3.forward * (y * nodeDiameter + nodeSize);
                Vector2Int grid = new Vector2Int(x, y);
                nodeMap[x, y] = new Node(false, worldPoint, grid);
                allNodes.Add(nodeMap[x, y]);
            }
        }

        for (int i = 0; i < spawnData.monsterNodes.Count; i++)
        {
            int x = spawnData.monsterNodes[i].spawnGrid.x;
            int y = spawnData.monsterNodes[i].spawnGrid.y;
            if (x < 0 || y < 0 || x >= gridX || y >= gridY)
                continue;
            Node node = nodeMap[x, y];
            node.SetNodeType(Node.NodeType.Monster);
        }
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

                if (checkX >= 0 && checkX < gridX && checkY >= 0 && checkY < gridY)
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

    //public Node GetNodeFromPosition(Vector3 worldPosition)
    //{
    //    float percentX = (worldPosition.x + gridSize.x * 0.5f) / gridSize.x;
    //    float percentY = (worldPosition.z + gridSize.y * 0.5f) / gridSize.y;

    //    percentX = Mathf.Clamp01(percentX);
    //    percentY = Mathf.Clamp01(percentY);

    //    int x = Mathf.RoundToInt((gridX - 1f) * percentX);
    //    int y = Mathf.RoundToInt((gridY - 1f) * percentY);
    //    Debug.LogWarning(worldPosition);
    //    adsfafd.transform.position = nodeMap[x, y].worldPosition;
    //    return nodeMap[x, y];
    //}
    public Node GetNodeFromPosition(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + worldSize.x * 0.5f) / worldSize.x;
        float percentY = (worldPosition.z + worldSize.y * 0.5f) / worldSize.y;

        //percentX = Mathf.Clamp01(percentX);
        //percentY = Mathf.Clamp01(percentY);

        //int x = Mathf.RoundToInt((gridX - 1f) * percentX);
        //int y = Mathf.RoundToInt((gridY - 1f) * percentY);
        int x = Mathf.Clamp((int)(gridX * percentX), 0, gridX - 1);
        int y = Mathf.Clamp((int)(gridY * percentY), 0, gridY - 1);
        Debug.LogWarning($"{percentX}:{x}");
        adsfafd.transform.position = nodeMap[x, y].worldPosition;
        return nodeMap[x, y];
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

                    if (neighbourX >= 0 && neighbourX < gridX && neighbourY >= 0 && neighbourY < gridY)
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
                if (neighbour.close || closedSet.Contains(neighbour))
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
    public bool close;
    public Vector3 worldPosition;
    public Transform onObject;
    //public Renderer tileRenderer;
    //public NodeDisplay nodeDisplay;

    public Vector2Int grid;
    public Vector2Int cost;
    public Vector2Int parentNode;

    //public string neighbours;

    public Node(bool _close, Vector3 _worldPos, Vector2Int _grid)
    {
        close = _close;
        worldPosition = _worldPos;
        grid = _grid;

        nodeName = $"{nodeType} : {grid}, {cost}";
    }

    public void SetNodeType(NodeType _nodeType)
    {
        nodeType = _nodeType;
        nodeName = $"{nodeType} : {grid}, {cost}";
    }

    public void UnitOnNode(Transform _onObject = null)
    {
        close = (_onObject != null);
        onObject = _onObject;
    }

    //public Node(Node _node)
    //{
    //    walkable = _node.walkable;
    //    worldPosition = _node.worldPosition;
    //    onObject = _node.onObject;

    //    gridX = _node.gridX;
    //    gridY = _node.gridY;

    //    gCost = _node.gCost;
    //    hCost = _node.hCost;
    //    parent = _node.parent;
    //}

    public int fCost
    {
        get
        {
            return cost.x + cost.y;
        }
    }
}