using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map_World : MonoBehaviour
{
    [Header("[ Map Grid ]")]
    public float nodeSize = 2f;
    public Vector2Int worldGrid;
    Vector2 worldSize;

    public Node[,] nodeMap;
    List<Node> allNodes = new List<Node>();
    void Start()
    {
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

                Map_MissionIcon inst = Instantiate(missonIcon, missonParent);
                inst.deleClickAction = ClickAction;
                dictMisson[inst] = worldPoint;
            }
        }
        StartCoroutine(SetMisson());
    }

    void ClickAction(Map_MissionIcon _mission)
    {
        Debug.LogWarning("Mission Click!!!!!!!!!!");
    }
    public Map_MissionIcon missonIcon;
    public RectTransform missonParent;
    public Dictionary<Map_MissionIcon, Vector3> dictMisson = new Dictionary<Map_MissionIcon, Vector3>();
    IEnumerator SetMisson()
    {
        //foreach (var child in dictMisson)
        //{
        //    Vector3 screenPosition = Camera.main.WorldToScreenPoint(child.Value);
        //    child.Key.transform.position = screenPosition;
        //}
        while (true)
        {
            foreach (var child in dictMisson)
            {
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(child.Value);
                child.Key.transform.position = screenPosition;
            }
            yield return null;
        }
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
                UnityEditor.Handles.Label(n.worldPosition, $"{n.grid.x}/{n.grid.y}", fontStyle);
            }
        }
    }
#endif
}
