using UnityEngine;

public class Unit_Looting : MonoBehaviour
{
    public Data_Looting lootingData;
    public string[] GetLooting(int _num)// �״´ٸ� �̸� ����
    {
        string[] ids = new string[_num];
        for (int i = 0; i < ids.Length; i++)
        {
            ids[i] = lootingData.GetResultItem();
        }
        return ids;
    }
}
