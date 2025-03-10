using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
//[ExecuteInEditMode]
public class Map_Setting : EditorWindow
{
    [MenuItem("Graphics Tool/09. Map_Setting")]
    public static void OpenWindow()
    {
        Map_Setting window = EditorWindow.GetWindow<Map_Setting>("Map_Setting");
        window.minSize = new Vector2(500f, 200f);
        window.Show();
    }

    Vector2 scrollPosition;

    public List<Data_Spawn.UnitNode> playerNodes = new List<Data_Spawn.UnitNode>();// 플레이어 영역
    public List<Data_Spawn.UnitNode> monsterNodes = new List<Data_Spawn.UnitNode>();// 몬스터 생성 포지션
    string id;

    public Data_Spawn spawnData;
    public Node.NodeType type;
    Vector2Int worldGrid;
    void OnGUI()
    {
        GUIStyle fontStyle = new("button")
        {
            fontSize = 20,
            normal = { textColor = Color.yellow },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
        };

        if (spawnData == null)
            type = Node.NodeType.None;

        spawnData = EditorGUILayout.ObjectField("Spawn Data", spawnData, typeof(Data_Spawn), true) as Data_Spawn;
        id = EditorGUILayout.TextField("Unit ID", id);
        worldGrid = EditorGUILayout.Vector2IntField("", worldGrid);

        fontStyle.normal.textColor = type == Node.NodeType.Player ? Color.red : Color.blue;
        if (GUILayout.Button(type.ToString(), fontStyle, GUILayout.Height(30f)))
        {
            switch (type)
            {
                case Node.NodeType.Player:
                    type = Node.NodeType.Monster;
                    break;

                case Node.NodeType.Monster:
                    type = Node.NodeType.Player;
                    break;

                case Node.NodeType.None:
                    if (spawnData != null)
                    {
                        worldGrid = spawnData.worldGrid;
                        playerNodes = spawnData.playerNodes;
                        monsterNodes = spawnData.monsterNodes;

                        EditorUtility.SetDirty(spawnData);
                        type = Node.NodeType.Player;
                    }
                    break;
            }
        }
        if (type == Node.NodeType.None)
            return;

        fontStyle.normal.textColor = Color.yellow;
        float gapSpace = (worldGrid.x + 1f) * 3f;
        float width = (position.width - gapSpace) / worldGrid.x;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        for (int y = worldGrid.y - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < worldGrid.x; x++)
            {
                Vector2Int grid = new Vector2Int(x, y);
                Color teamColor = TryGridColor(grid, out string id);
                string label = id + "\n" + grid.ToString();
                GUI.color = teamColor;
                if (GUILayout.Button(label, GUILayout.Width(width), GUILayout.Height(width)))
                {
                    TryUnitNode(grid);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUI.color = Color.white;
        EditorGUILayout.BeginHorizontal();
        float buttonHight = 40f;
        if (GUILayout.Button("Reset", fontStyle, GUILayout.Height(buttonHight)))
        {
            worldGrid = Vector2Int.zero;
            playerNodes = new List<Data_Spawn.UnitNode>();
            monsterNodes = new List<Data_Spawn.UnitNode>();

            EditorUtility.SetDirty(spawnData);
        }
        if (GUILayout.Button("Load", fontStyle, GUILayout.Height(buttonHight)))
        {
            worldGrid = spawnData.worldGrid;
            playerNodes = spawnData.playerNodes;
            monsterNodes = spawnData.monsterNodes;

            EditorUtility.SetDirty(spawnData);
        }
        if (GUILayout.Button("Save", fontStyle, GUILayout.Height(buttonHight)))
        {
            spawnData.worldGrid = worldGrid;
            spawnData.playerNodes = playerNodes;
            spawnData.monsterNodes = monsterNodes;

            EditorUtility.SetDirty(spawnData);
        }
        EditorGUILayout.EndHorizontal();
    }

    bool TryUnitNode(Vector2Int _grid)
    {
        List<Data_Spawn.UnitNode> unitNodes;
        switch (type)
        {
            default:
                return false;

            case Node.NodeType.Player:
                unitNodes = playerNodes;
                break;

            case Node.NodeType.Monster:
            case Node.NodeType.None:
                unitNodes = monsterNodes;
                break;
        }
        for (int i = 0; i < unitNodes.Count; i++)
        {
            if (unitNodes[i].spawnGrid == _grid)
            {
                unitNodes.Remove(unitNodes[i]);
                return true;
            }
        }
        Data_Spawn.UnitNode unitNode = new Data_Spawn.UnitNode
        {
            unitID = id,
            spawnGrid = _grid,
        };
        unitNodes.Add(unitNode);
        return false;
    }

    Color TryGridColor(Vector2Int _grid, out string _id)
    {
        Color color = Color.gray;
        bool doubleCheck = false;
        string id = "";
        for (int i = 0; i < playerNodes.Count; i++)
        {
            if (playerNodes[i].spawnGrid == _grid)
            {
                doubleCheck = true;
                color = Color.red;
                id = playerNodes[i].unitID;
            }
        }
        for (int i = 0; i < monsterNodes.Count; i++)
        {
            if (monsterNodes[i].spawnGrid == _grid)
            {
                if (doubleCheck == true)
                {
                    color = Color.magenta;
                    _id = "double";
                    return color;
                }
                else
                {
                    color = Color.green;
                    _id = monsterNodes[i].unitID;
                    return color;
                }
            }
        }
        _id = id;
        return color;
    }
}
