using UnityEngine;

public class Unit_Looting : MonoBehaviour
{
    public Data_Looting lootingData;
    public string[] GetLooting(int _num)// 죽는다면 미리 세팅
    {
        string[] ids = new string[_num];
        for (int i = 0; i < ids.Length; i++)
        {
            ids[i] = lootingData.GetResultItem();
        }
        return ids;
    }
}
