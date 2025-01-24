using System.Collections.Generic;
using UnityEngine;
using static Singleton_Data;

public class Singleton_Data : MonoSingleton<Singleton_Data>
{
    public Dictionary<string, Data_Manager.DialogInfoamtion> Dict_Dialog = new Dictionary<string, Data_Manager.DialogInfoamtion>();
    //public Dictionary<string, Data_Manager.DialogTranslation> Dict_DialogTranslation = new Dictionary<string, Data_Manager.DialogTranslation>();
    public Dictionary<string, AudioClip> Dict_Audio = new Dictionary<string, AudioClip>();
    public Translation translation;

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

    //public void SetDictionary_DialogTranslation(List<Data_Manager.DialogTranslation> _data)
    //{
    //    Dict_DialogTranslation = new Dictionary<string, Data_Manager.DialogTranslation>();
    //    for (int i = 0; i < _data.Count; i++)
    //    {
    //        string id = _data[i].ID;
    //        if (Dict_DialogTranslation.ContainsKey(id) == true)
    //        {
    //            Debug.LogError($"{id}와 같은 이름이 존재 합니다.");
    //        }
    //        else
    //        {
    //            Dict_DialogTranslation[id] = _data[i];
    //        }
    //    }
    //}
    public enum Translation
    {
        Korean,
        English,
        Japanese,
        Chinese
    }

    public string TryDialogTranslation(string _ID)
    {
        Data_Manager.DialogInfoamtion dialogTranslation = Dict_Dialog[_ID];
        string temp = "";
        switch (translation)
        {
            case Translation.Korean:
                temp = dialogTranslation.KR;
                break;

            case Translation.English:
                temp = dialogTranslation.EN;
                break;

            case Translation.Japanese:
                temp = dialogTranslation.JP;
                break;

            case Translation.Chinese:
                temp = dialogTranslation.CN;
                break;
        }
        return temp;
    }

    public void SetDictionary_Audio(List<AudioClip> _data)
    {
        Dict_Audio = new Dictionary<string, AudioClip>();
        for (int i = 0; i < _data.Count; i++)
        {
            string id = _data[i].name;
            if (Dict_Audio.ContainsKey(id) == true)
            {
                Debug.LogError($"{id}와 같은 이름이 존재 합니다.");
            }
            else
            {
                Dict_Audio[id] = _data[i];
            }
        }
    }
}
