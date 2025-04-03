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
    public List<UnitNode> unitNodes = new List<UnitNode>();// ���� ��� ����
                                                           // (�÷��̾ �ٷ� ���������ϰ�)
                                                           // �ź� ���� ��쿡 �κ��丮 �ȿ��� ���� �������� �̾Ƽ� ��� ����
}
