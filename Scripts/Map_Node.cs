using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//[ExecuteInEditMode]
public class Map_Node : MonoBehaviour
{
    [Header("[ Map Grid ]")]
    public float nodeSize = 2f;
    Vector2Int worldGrid;
    Vector2 worldSize;

    [Tooltip("8���� �̵�����")]
    public bool diagonal;

    public Node[,] nodeMap;

    Data_Spawn spawnData;
    public Data_Spawn GetSpawnData { get { return spawnData; } }
    List<Node> allNodes;

    public List<Node> RandomNodes()
    {
        List<Node> randomNodes = ShuffleList(allNodes, 0);
        return randomNodes;
    }

    // ����Ʈ ����
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

        for (int i = 0; i < spawnData.playerNodes.Count; i++)
        {
            int x = spawnData.playerNodes[i].spawnGrid.x;
            int y = spawnData.playerNodes[i].spawnGrid.y;
            if (x < 0 || y < 0 || x >= worldGrid.x || y >= worldGrid.y)
                continue;
            Node node = nodeMap[x, y];
            node.SetNodeType(Node.NodeType.Player);
        }

        for (int i = 0; i < spawnData.monsterNodes.Count; i++)
        {
            int x = spawnData.monsterNodes[i].spawnGrid.x;
            int y = spawnData.monsterNodes[i].spawnGrid.y;
            if (x < 0 || y < 0 || x >= worldGrid.x || y >= worldGrid.y)
                continue;
            Node node = nodeMap[x, y];
            node.SetNodeType(Node.NodeType.Monster);
        }
        InstanceNodeTile();
    }
    public GameObject baseTile;
    void InstanceNodeTile()
    {
        for (int i = 0; i < spawnData.playerNodes.Count; i++)
        {
            Vector2Int grid = spawnData.playerNodes[i].spawnGrid;
            GameObject inst = Instantiate(baseTile, transform);
            inst.transform.position = nodeMap[grid.x, grid.y].worldPosition;
        }
    }

    // ��ó Ÿ�� ������
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
                    if (diagonal || (x == 0 || y == 0))    // �밢�� ������ (8����)
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

        //percentX = Mathf.Clamp01(percentX);
        //percentY = Mathf.Clamp01(percentY);

        //int x = Mathf.RoundToInt((gridX - 1f) * percentX);
        //int y = Mathf.RoundToInt((gridY - 1f) * percentY);
        int x = Mathf.Clamp((int)(worldGrid.x * percentX), 0, worldGrid.x - 1);
        int y = Mathf.Clamp((int)(worldGrid.y * percentY), 0, worldGrid.y - 1);
        //Debug.LogWarning($"{percentX}:{x}");
        return nodeMap[x, y];
    }

    //=====================================================================================================
    // ��ã��
    //=====================================================================================================

    // ����Ʈ�ȿ� ���� Ÿ���� �ִ��� üũ
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

    // �ֺ� ��� üũ
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
                            neighbours += "O";// ���� Ÿ��
                        }
                        else if (TryAccessibleTiles(tile, _activeArea) == false)
                        {
                            neighbours += "X";// ���� Ÿ�� �ƴ�
                        }
                    }
                    else
                    {
                        neighbours += "I"; //Ÿ�� ����
                    }
                }
            }
        }
        return neighbours;
    }


    //=====================================================================================================
    // ��ã��
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
    public GameObject onObject;
    //public Renderer tileRenderer;
    //public NodeDisplay nodeDisplay;

    public Vector2Int grid;
    public Vector2Int cost;
    public Vector2Int parentNode;

    //public string neighbours;

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