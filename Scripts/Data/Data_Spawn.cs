using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Data_Spawn", menuName = "Scriptable Objects/Data_Spawn")]
public class Data_Spawn : ScriptableObject
{
    public Vector2Int worldGrid;
    [System.Serializable]
    public struct UnitNode
    {
        public string unitID;
        public Vector2Int spawnGrid;
        public Node.NodeType nodeType;
    }
    public List<UnitNode> unitNodes = new List<UnitNode>();// 유닛 노드 통합
                                                           // (플레이어도 바로 생성가능하게)
                                                           // 매복 당한 경우에 인벤토리 안에서 유닛 랜덤으로 뽑아서 섞어서 생성
}
