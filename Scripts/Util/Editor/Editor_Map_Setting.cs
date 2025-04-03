using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static Data_Spawn;



#if UNITY_EDITOR
using UnityEditor;
//[ExecuteInEditMode]
public class Editor_Map_Setting : EditorWindow
{
    [MenuItem("Graphics Tool/09. Editor_Map_Setting")]
    public static void OpenWindow()
    {
        Editor_Map_Setting window = EditorWindow.GetWindow<Editor_Map_Setting>("Editor_Map_Setting");
        window.minSize = new Vector2(500f, 200f);
        window.Show();
    }

    Vector2 scrollPosition;

    public List<Data_Spawn.UnitNode> unitNodes = new List<Data_Spawn.UnitNode>();// 통합 생성 포지션
    string id;

    public Data_Spawn spawnData;
    public Node.NodeType brushType;
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
            brushType = Node.NodeType.None;

        spawnData = EditorGUILayout.ObjectField("Spawn Data", spawnData, typeof(Data_Spawn), true) as Data_Spawn;
        id = EditorGUILayout.TextField("Unit ID", id);
        worldGrid = EditorGUILayout.Vector2IntField("", worldGrid);

        fontStyle.normal.textColor = brushType == Node.NodeType.Player ? Color.red : Color.blue;
        if (GUILayout.Button(brushType.ToString(), fontStyle, GUILayout.Height(30f)))
        {
            switch (brushType)
            {
                case Node.NodeType.Player:
                    brushType = Node.NodeType.Monster;
                    break;

                case Node.NodeType.Monster:
                    brushType = Node.NodeType.None;
                    break;

                case Node.NodeType.None:
                    brushType = Node.NodeType.Player;
                    break;
            }
        }

        fontStyle.normal.textColor = Color.yellow;
        float gapSpace = (worldGrid.x + 1f) * 3f;
        float width = (position.width - gapSpace) / worldGrid.x;

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        for (int y = worldGrid.y - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < worldGrid.x; x++)
            {
                // 월드 버튼
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
            unitNodes = new List<Data_Spawn.UnitNode>();

            EditorUtility.SetDirty(spawnData);
        }
        if (GUILayout.Button("Load", fontStyle, GUILayout.Height(buttonHight)))
        {
            worldGrid = spawnData.worldGrid;
            unitNodes = spawnData.unitNodes;

            EditorUtility.SetDirty(spawnData);
        }
        if (GUILayout.Button("Save", fontStyle, GUILayout.Height(buttonHight)))
        {
            spawnData.worldGrid = worldGrid;
            spawnData.unitNodes = unitNodes;

            EditorUtility.SetDirty(spawnData);
        }
        EditorGUILayout.EndHorizontal();
    }

    void TryUnitNode(Vector2Int _grid)
    {
        //List<Data_Spawn.UnitNode> unitNodes;
        //switch (brushType)
        //{
        //    default:
        //        return false;

        //    case Node.NodeType.Player:
        //        unitNodes = playerNodes;
        //        break;

        //    case Node.NodeType.Monster:
        //        unitNodes = monsterNodes;
        //        break;
        //}

        //for (int i = 0; i < unitNodes.Count; i++)
        //{
        //    if (unitNodes[i].spawnGrid == _grid)
        //    {
        //        unitNodes.Remove(unitNodes[i]);
        //        return true;
        //    }
        //}

        //Data_Spawn.UnitNode unitNode = new Data_Spawn.UnitNode
        //{
        //    unitID = id,
        //    spawnGrid = _grid,
        //};
        //unitNodes.Add(unitNode);
        for (int i = 0; i < unitNodes.Count; i++)
        {
            if (unitNodes[i].spawnGrid == _grid)
            {
                unitNodes.Remove(unitNodes[i]);
                return;
            }
        }

        if (brushType == Node.NodeType.None)
            return;

        Data_Spawn.UnitNode unitNode = new Data_Spawn.UnitNode
        {
            unitID = id,
            spawnGrid = _grid,
            nodeType = brushType,
        };
        unitNodes.Add(unitNode);
    }

    Color TryGridColor(Vector2Int _grid, out string _id)
    {
        Color color = Color.gray;
        //bool doubleCheck = false;
        string id = "";
        //for (int i = 0; i < playerNodes.Count; i++)
        //{
        //    if (playerNodes[i].spawnGrid == _grid)
        //    {
        //        doubleCheck = true;
        //        color = Color.red;
        //        id = playerNodes[i].unitID;
        //    }
        //}
        //for (int i = 0; i < monsterNodes.Count; i++)
        //{
        //    if (monsterNodes[i].spawnGrid == _grid)
        //    {
        //        if (doubleCheck == true)
        //        {
        //            color = Color.magenta;
        //            _id = "double";
        //            return color;
        //        }
        //        else
        //        {
        //            color = Color.green;
        //            _id = monsterNodes[i].unitID;
        //            return color;
        //        }
        //    }
        //}
        for (int i = 0; i < unitNodes.Count; i++)
        {
            if (unitNodes[i].spawnGrid == _grid)
            {
                color = unitNodes[i].nodeType == Node.NodeType.Player ? Color.red : Color.green;
                id = unitNodes[i].unitID;
            }
        }
        _id = id;
        return color;
    }
}

#endif