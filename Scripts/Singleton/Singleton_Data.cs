using System.Collections.Generic;
using UnityEngine;

public class Singleton_Data : MonoSingleton<Singleton_Data>
{
    public Dictionary<string, Data_Manager.DialogStruct> Dict_Dialog = new Dictionary<string, Data_Manager.DialogStruct>();
    public Dictionary<string, Data_Manager.TranslateString> Dict_DialogString = new Dictionary<string, Data_Manager.TranslateString>();
    public Dictionary<string, Data_Manager.TranslateString> Dict_TranslateString = new Dictionary<string, Data_Manager.TranslateString>();
    public Dictionary<string, Data_Manager.SkillStruct> Dict_Skill = new Dictionary<string, Data_Manager.SkillStruct>();
    public Dictionary<string, Data_Manager.UnitStruct> Dict_Unit = new Dictionary<string, Data_Manager.UnitStruct>();
    public Dictionary<string, Data_Manager.ItemStruct> Dict_Item = new Dictionary<string, Data_Manager.ItemStruct>();
    public Dictionary<string, AudioClip> Dict_Audio = new Dictionary<string, AudioClip>();
    public Translation translation;

    public void SetDictionary_Dialog(List<Data_Manager.DialogStruct> _data)
    {
        Dict_Dialog = new Dictionary<string, Data_Manager.DialogStruct>();
        for (int i = 0; i < _data.Count; i++)
        {
            string id = _data[i].ID;
            if (Dict_Dialog.ContainsKey(id) == true)
            {
                Debug.LogError($"{id}�� ���� �̸��� ���� �մϴ�.");
            }
            else
            {
                Dict_Dialog[id] = _data[i];
            }
        }
    }

    //public void SetDictionary_SkillTranslation(List<Data_Manager.TranslateString> _data)
    //{
    //    Dict_SkillString = SetTranslation(_data);
    //}

    //public void SetDictionary_UnitTranslation(List<Data_Manager.TranslateString> _data)
    //{
    //    Dict_UnitString = SetTranslation(_data);
    //}

    //public void SetDictionary_ItemTranslation(List<Data_Manager.TranslateString> _data)
    //{
    //    Dict_ItemString = SetTranslation(_data);
    //}

    public void SetDictionary_DialogString(List<Data_Manager.TranslateString> _data)
    {
        Dict_DialogString = SetTranslation(_data);
    }

    public void SetDictionary_TranslationString(List<Data_Manager.TranslateString> _data)
    {
        Dict_TranslateString = SetTranslation(_data);
    }

    public string TryTranslation(int _type, string _id)
    {
        Dictionary<string, Data_Manager.TranslateString> temp = default;
        switch (_type)
        {
            case 0:
                temp = Dict_DialogString;
                break;

            case 1:
                temp = Dict_TranslateString;
                break;
        }

        switch (translation)
        {
            case Translation.Korean:
                return temp[_id].KR;

            case Translation.English:
                return temp[_id].EN;

            case Translation.Japanese:
                return temp[_id].JP;

            case Translation.Chinese:
                return temp[_id].CN;
        }
        return null;
    }

    Dictionary<string, Data_Manager.TranslateString> SetTranslation(List<Data_Manager.TranslateString> _data)
    {
        Dictionary<string, Data_Manager.TranslateString> Dict_Temp = new Dictionary<string, Data_Manager.TranslateString>();
        for (int i = 0; i < _data.Count; i++)
        {
            string id = _data[i].ID;
            if (Dict_Temp.ContainsKey(id) == true)
            {
                Debug.LogError($"{id}�� ���� �̸��� ���� �մϴ�.");
            }
            else
            {
                Dict_Temp[id] = _data[i];
            }
        }
        return Dict_Temp;
    }

    public void SetDictionary_Skill(List<Data_Manager.SkillStruct> _data)
    {
        Dict_Skill = new Dictionary<string, Data_Manager.SkillStruct>();
        for (int i = 0; i < _data.Count; i++)
        {
            string id = _data[i].ID;
            if (Dict_Skill.ContainsKey(id) == true)
            {
                Debug.LogError($"{id}�� ���� �̸��� ���� �մϴ�.");
            }
            else
            {
                Dict_Skill[id] = _data[i];
            }
        }
    }

    public void SetDictionary_Unit(List<Data_Manager.UnitStruct> _data)
    {
        Dict_Unit = new Dictionary<string, Data_Manager.UnitStruct>();
        for (int i = 0; i < _data.Count; i++)
        {
            string id = _data[i].ID;
            if (Dict_Unit.ContainsKey(id) == true)
            {
                Debug.LogError($"{id}�� ���� �̸��� ���� �մϴ�.");
            }
            else
            {
                Dict_Unit[id] = _data[i];
            }
        }
    }

    public void SetDictionary_Item(List<Data_Manager.ItemStruct> _data)
    {
        Dict_Item = new Dictionary<string, Data_Manager.ItemStruct>();
        for (int i = 0; i < _data.Count; i++)
        {
            string id = _data[i].ID;
            if (Dict_Item.ContainsKey(id) == true)
            {
                Debug.LogError($"{id}�� ���� �̸��� ���� �մϴ�.");
            }
            else
            {
                Dict_Item[id] = _data[i];
            }
        }
    }

    public enum Translation
    {
        Korean,
        English,
        Japanese,
        Chinese
    }

    //public string TryDialogTranslation(string _ID)
    //{
    //    Data_Manager.DialogInfoamtion dialogTranslation = Dict_Dialog[_ID];
    //    string temp = "";
    //    switch (translation)
    //    {
    //        case Translation.Korean:
    //            temp = dialogTranslation.KR;
    //            break;

    //        case Translation.English:
    //            temp = dialogTranslation.EN;
    //            break;

    //        case Translation.Japanese:
    //            temp = dialogTranslation.JP;
    //            break;

    //        case Translation.Chinese:
    //            temp = dialogTranslation.CN;
    //            break;
    //    }
    //    return temp;
    //}

    public void SetDictionary_Audio(List<AudioClip> _data)
    {
        Dict_Audio = new Dictionary<string, AudioClip>();
        for (int i = 0; i < _data.Count; i++)
        {
            string id = _data[i].name;
            if (Dict_Audio.ContainsKey(id) == true)
            {
                Debug.LogError($"{id}�� ���� �̸��� ���� �մϴ�.");
            }
            else
            {
                Dict_Audio[id] = _data[i];
            }
        }
    }
}
