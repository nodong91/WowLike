using UnityEngine;

[CreateAssetMenu(fileName = "Data_Looting", menuName = "Scriptable Objects/Data_Looting")]
public class Data_Looting : ScriptableObject
{
    public string[] lootingID;
    [System.Serializable]
    public struct ResultStruct
    {
        public string itemID;// ���� ������ ID
        [Range(0f, 100f)]
        public float probability;// Ȯ��
    }
    public ResultStruct[] resultItems;

    public string GetResultItem()
    {
        float total = 0;

        foreach (var elem in resultItems)
        {
            total += elem.probability;
        }

        float randomPoint = Random.value * total;

        for (int i = 0; i < resultItems.Length; i++)
        {
            if (randomPoint < resultItems[i].probability)
            {
                return resultItems[i].itemID;
            }
            else
            {
                randomPoint -= resultItems[i].probability;
            }
        }
        return resultItems[^1].itemID;
    }
}
