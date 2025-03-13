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
    }
    public List<UnitNode> playerNodes = new List<UnitNode>();// �÷��̾� ����
    public List<UnitNode> monsterNodes = new List<UnitNode>();// ���� ���� ������
}
