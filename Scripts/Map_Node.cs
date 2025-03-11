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

    [Tooltip("8방향 이동가능")]
    public bool diagonal;

    public Node[,] nodeMap;

    Data_Spawn spawnData;
    public Data_Spawn GetSpawnData { get { return spawnData; } }
    public List<Node> allNodes;

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

    public UnityEngine.UI.Button baseTile;
    public RectTransform tileParent;
    void InstanceNodeTile()
    {
        for (int i = 0; i < spawnData.playerNodes.Count; i++)
        {
            Vector2Int grid = spawnData.playerNodes[i].spawnGrid;
            Node node = nodeMap[grid.x, grid.y];
            UnityEngine.UI.Button inst = Instantiate(baseTile, tileParent);
            inst.onClick.AddListener(delegate { ClickEvent(grid); });
            inst.transform.position = node.worldPosition;

            node.neighbours = TryTile(node);
            Sprite sprite = SetTileSprite(node.neighbours, out float _angle);
            inst.image.sprite = sprite;
            inst.transform.localEulerAngles = new Vector3(0f, 0f, _angle);
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
        for (int i = 0; i < spawnData.playerNodes.Count; i++)
        {
            Vector2Int grid = spawnData.playerNodes[i].spawnGrid;
            if (_node.grid == grid)
            {
                return true;
            }
        }
        return false;
    }
    public Sprite[] tileSprites;
    Sprite SetTileSprite(string _dir, out float _angle)
    {
        int index = 0;
        for (int i = 0; i < _dir.Length; i++)
        {
            if (_dir[i] == 'O')
            {
                index++;
            }
        }

        Sprite sprite = null;
        float angle = 0;
        switch (index)
        {
            case 1:

                break;

            case 2:
                sprite = tileSprites[2];
                break;
        }

        if (_dir[1] == 'O')// 좌 있고
        {
            if (_dir[3] == 'O')// 아래 있고
            {
                if (_dir[4] == 'O') // 위 있고
                {
                    if (_dir[6] == 'O')// 오른 있고
                    {
                        // 대각선 위치
                        if (_dir[0] == 'O')
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')// 주변 전부 잇음
                                    {
                                        sprite = tileSprites[14];
                                    }
                                    else
                                    {
                                        angle = -90f;
                                        sprite = tileSprites[13];
                                    }
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[13];
                                    }
                                    else
                                    {
                                        angle = -90f;
                                        sprite = tileSprites[11];
                                    }
                                }
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        sprite = tileSprites[13];
                                    }
                                    else
                                    {
                                        angle = 0f;
                                        sprite = tileSprites[11];
                                    }
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        sprite = tileSprites[11];
                                    }
                                    else
                                    {
                                        sprite = tileSprites[10];
                                    }
                                }
                            }
                        }
                        else// 0 없음
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {

                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[11];
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 90f;
                                        sprite = tileSprites[11];
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    else// 좌 있고 아래 있고 위 있고 오른 없고
                    {
                        if (_dir[0] == 'O')
                        {
                            angle = -90f;
                            sprite = tileSprites[9];
                        }
                        else
                        {
                            if (_dir[2] == 'O')
                            {

                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = -90f;
                                        sprite = tileSprites[4];
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                }
                else // 좌 있고 아래 있고 위 없음
                {
                    if (_dir[6] == 'O')// 우 있음
                    {
                        if (_dir[0] == 'O')
                        {
                            angle = 0f;
                            sprite = tileSprites[9];
                        }
                        else
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {

                                        angle = 0f;
                                        sprite = tileSprites[8];
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 0f;
                                        sprite = tileSprites[4];
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                    else
                    {
                        if (_dir[0] == 'O')
                        {
                            angle = -90f;
                            sprite = tileSprites[5];
                        }
                        else
                        {
                            angle = -90f;
                            sprite = tileSprites[2];
                        }
                    }
                }
            }
            else // 좌측 있고 아래 없음
            {
                if (_dir[4] == 'O') // 위쪽 있음
                {
                    if (_dir[6] == 'O')// 오른 잇음
                    {
                        if (_dir[0] == 'O')
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[9];
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                     
                                    }
                                    else
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[4];
                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                        else// 0 없고
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                     
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[9];
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {

                                    }
                                    else
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[4];
                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    else// 오른 없고
                    {
                        if (_dir[0] == 'O')
                        {
                            if (_dir[2] == 'O')
                            {
                                angle = 180f;
                                sprite = tileSprites[5];
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {

                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[2];
                                    }
                                    else
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[2];
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {

                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {

                                    }
                                    else
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[5];
                                    }
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                }
                else
                {
                    if (_dir[6] == 'O')
                    {
                        angle = 0f;
                        sprite = tileSprites[3];
                    }
                    else
                    {
                        angle = -90f;
                        sprite = tileSprites[1];
                    }
                }
            }
        }
        else// 좌측 없음
        {
            if (_dir[3] == 'O')// 아래 있고
            {
                if (_dir[4] == 'O')// 위 있고
                {
                    if (_dir[6] == 'O')// 오른 있고
                    {
                        if (_dir[0] == 'O')
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {

                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {

                                    }
                                    else
                                    {
                                        angle = 90f;
                                        sprite = tileSprites[4];
                                    }
                                }
                            }
                            else
                            {
                               
                            }
                        }
                        else
                        {
                            if (_dir[2] == 'O')
                            {

                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 90f;
                                        sprite = tileSprites[9];
                                    }
                                    else
                                    {
                                    
                                    }
                                }
                                else
                                {
                                  
                                }
                            }
                        }
                    }
                    else// 오른쪽 없음
                    {
                        angle = 90f;
                        sprite = tileSprites[3];
                    }
                }
                else// 위 없고
                {
                    if (_dir[6] == 'O')// 오른 있고
                    {
                        if (_dir[0] == 'O')
                        {
                            if (_dir[2] == 'O')
                            {

                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 0f;
                                        sprite = tileSprites[5];
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 0f;
                                        sprite = tileSprites[2];
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                        }
                        else
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {

                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                      
                                    }
                                    else
                                    {
                                        angle = 0f;
                                        sprite = tileSprites[5];
                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    else// 오른쪽 없음
                    {
                        angle = 0f;
                        sprite = tileSprites[1];
                    }
                }
            }
            else// 아래 없음
            {
                if (_dir[4] == 'O')// 위 있고
                {
                    if (_dir[6] == 'O')// 오른 있고
                    {
                        if (_dir[0] == 'O')
                        {
                           
                        }
                        else
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 90f;
                                        sprite = tileSprites[5];
                                    }
                                    else
                                    {
                                        angle = 90f;
                                        sprite = tileSprites[2];
                                    }
                                }
                                else
                                {
                                
                                }
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 90f;
                                        sprite = tileSprites[5];
                                    }
                                    else
                                    {
                                        //angle = 90f;
                                        //sprite = tileSprites[5];
                                    }
                                }
                            }
                        }
                     
                    }
                    else// 오른쪽 없음
                    {
                        angle = 180f;
                        sprite = tileSprites[1];
                    }
                }
                else// 위없음
                {
                    if (_dir[6] == 'O')
                    {
                        angle = 90f;
                        sprite = tileSprites[1];
                    }
                    else// 오른쪽 없음
                    {
                        angle = 0f;
                        sprite = tileSprites[0];
                    }
                }
            }
        }
        _angle = angle;
        return sprite;
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