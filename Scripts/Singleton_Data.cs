using System.Collections.Generic;
using UnityEngine;

public class Singleton_Data : MonoSingleton<Singleton_Data>
{
    public Dictionary<string, Data_Manager.DialogInfoamtion> Dict_Dialog = new Dictionary<string, Data_Manager.DialogInfoamtion>();
    public void SetDictionary_Dialog(List<Data_Manager.DialogInfoamtion> _data)
    {
        Dict_Dialog = new Dictionary<string, Data_Manager.DialogInfoamtion>();
        for (int i = 0; i < _data.Count; i++)
        {
            string id = _data[i].ID;
            if (Dict_Dialog.ContainsKey(id) == true)
            {
                Debug.LogError($"{id}와 같은 이름이 존재 합니다.");
            }
            else
            {
                Dict_Dialog[id] = _data[i];
            }
        }
    }
}
