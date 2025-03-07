using UnityEngine;
using System.Collections.Generic;

//#if UNITY_EDITOR
//using UnityEditor;

//[CustomEditor(typeof(Data_Spawn))]
//public class Data_Spawn_Editor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
//        fontStyle.fontSize = 15;
//        fontStyle.normal.textColor = Color.yellow;

//        Data_Spawn Inspector = target as Data_Spawn;
//        if (GUILayout.Button("Update Data", fontStyle, GUILayout.Height(30f)))
//        {
//            //Inspector.UpdateData();
//            EditorUtility.SetDirty(Inspector);
//        }
//        GUILayout.Space(10f);
//        base.OnInspectorGUI();
//    }
//}
//#endif
[CreateAssetMenu(fileName = "Data_Spawn", menuName = "Scriptable Objects/Data_Spawn")]
public class Data_Spawn : ScriptableObject
{
    [System.Serializable]
    public struct UnitNode
    {
        public string unitID;
        //public Node.NodeType nodeType;
        public Vector2Int spawnGrid;
    }
    public List<UnitNode> playerNodes = new List<UnitNode>();// 플레이어 영역
    public List<UnitNode> monsterNodes = new List<UnitNode>();// 몬스터 생성 포지션
}
