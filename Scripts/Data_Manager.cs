using UnityEngine;
using System.Collections.Generic;
using System;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Data_Manager))]
public class DataManager_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
        fontStyle.fontSize = 15;
        fontStyle.normal.textColor = Color.yellow;

        Data_Manager Inspector = target as Data_Manager;
        if (GUILayout.Button("Data Parse", fontStyle, GUILayout.Height(30f)))
        {
            Inspector.UpdateData();
            EditorUtility.SetDirty(Inspector);
        }
        GUILayout.Space(10f);
        base.OnInspectorGUI();
    }
}
#endif

public class Data_Manager : Data_Parse
{
#if UNITY_EDITOR
    public void UpdateData()
    {
        dialog = new List<DialogInfoamtion>();
        DataSetting();
    }

    public override void DataSetting()
    {
        base.DataSetting();
        TextAsset[] csv_data = CSV_Data;
        for (int i = 0; i < csv_data.Length; i++)
        {
            string csv_Type = csv_data[i].name;
            if (csv_Type.Contains("Dialog"))
            {
                SetDialogData(csv_data[i]);
            }
        }
    }

    void SetDialogData(TextAsset _textAsset)
    {
        string[] data = _textAsset.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)// 첫째 라인(목록) 빼고 리스팅
        {
            string[] elements = data[i].Split(new char[] { ',' });
            DialogInfoamtion tempData = new DialogInfoamtion
            {
                ID = elements[0].Trim(),
                text = elements[1],
                //text = elements[2],
                animType = (DialogInfoamtion.AnimType)Enum.Parse(typeof(DialogInfoamtion.AnimType), elements[3]),
                size = FloatTryParse(elements[4]),
                color = elements[5],
                lineEnd = elements[6] == "TRUE" ? true : false,
                speed = FloatTryParse(elements[7].Trim()),
            };
            dialog.Add(tempData);
        }
    }

    //void SetUnitData(TextAsset _textAsset)
    //{
    //    string[] data = _textAsset.text.Split(new char[] { '\n' });
    //    for (int i = 1; i < data.Length; i++)// 첫째 라인 빼고 리스팅
    //    {
    //        string[] elements = data[i].Split(new char[] { ',' });

    //        List<Data_ItemSet> tempList = new List<Data_ItemSet>();
    //        for (int m = 0; m < 3; m++)
    //        {
    //            Data_ItemSet tempItem = FindDefaultItem(elements[11 + m].Trim());
    //            if (tempItem != null)
    //                tempList.Add(tempItem);
    //        }

    //        UnitStruct tempData = new UnitStruct
    //        {
    //            ID = elements[0].Trim(),
    //            unitName = elements[1].Trim(),
    //            description = elements[2].Trim(),
    //            icon = FindSprite(elements[3]),
    //            unitClass = (UnitStruct.UnitClass)Enum.Parse(typeof(UnitStruct.UnitClass), elements[4]),

    //            attack = FloatTryParse(elements[5]),
    //            defense = FloatTryParse(elements[6]),
    //            maxHealth = FloatTryParse(elements[7]),
    //            criticalDamage = FloatTryParse(elements[8]),
    //            recoveryHealth = FloatTryParse(elements[9]),
    //            lifeSteal = FloatTryParse(elements[10]),
    //            defaultItem = tempList
    //        };
    //        unitData.Add(tempData);
    //    }
    //}
#endif

    [System.Serializable]
    public class DialogInfoamtion
    {
        public string ID;
        public string text;
        public string color;
        public float size;
        public bool lineEnd;
        public enum AnimType
        {
            None,
            Moving,
            Wave,
            Jitter
        }
        public AnimType animType;
        public Vector2 angle;
        public float length;
        public float speed;
    }
    public List<DialogInfoamtion> dialog;

    private void Awake()
    {
        Singleton_Data.INSTANCE.SetDictionary_Dialog(dialog);
        Singleton_Data.INSTANCE.SetDictionary_Audio(audioClip);
    }
}
