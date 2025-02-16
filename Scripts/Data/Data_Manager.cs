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
        DataSetting();
    }

    public override void DataSetting()
    {
        base.DataSetting();
        for (int i = 0; i < GetCSV_Data.Count; i++)
        {
            string csv_Type = GetCSV_Data[i].name;
            if (csv_Type.Contains("Dialog"))
            {
                SetDialogData(GetCSV_Data[i]);
            }
            else if (csv_Type.Contains("Skill"))
            {
                if (csv_Type.Contains("String"))
                {
                    skillString = SetTranslateString(GetCSV_Data[i]);
                }
                else
                {
                    SetSkill(GetCSV_Data[i]);
                }
            }
            else if (csv_Type.Contains("Unit"))
            {
                if (csv_Type.Contains("String"))
                {
                    unitString = SetTranslateString(GetCSV_Data[i]);
                }
                else
                {
                    SetUnit(GetCSV_Data[i]);
                }
            }
            else if (csv_Type.Contains("Item"))
            {
                if (csv_Type.Contains("String"))
                {
                    itemString = SetTranslateString(GetCSV_Data[i]);
                }
                else
                {
                    SetItem(GetCSV_Data[i]);
                }
            }
        }
    }

    void SetDialogData(TextAsset _textAsset)
    {
        dialog.Clear();
        string[] data = _textAsset.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)// 첫째 라인(목록) 빼고 리스팅
        {
            string[] elements = data[i].Split(new char[] { ',' });
            if (elements[0].Trim().Length == 0)// 아이디 표기가 없으면 제외
                continue;

            DialogInfoamtion tempData = new DialogInfoamtion
            {
                ID = elements[0].Trim(),
                textStyle = elements[1].Trim().Length > 0 ?
            (Data_DialogType.TextStyle)Enum.Parse(typeof(Data_DialogType.TextStyle), elements[1]) : 0,
                size = IntTryParse(elements[2]),
                color = elements[3],
                bold = elements[4] == "TRUE" ? true : false,
                speed = FloatTryParse(elements[5].Trim()),

                KR = elements[6],
                EN = elements[7],
                JP = elements[8],
                CN = elements[9],
            };
            dialog.Add(tempData);
        }
    }

    void SetSkill(TextAsset _textAsset)
    {
        skillStruct.Clear();
        string[] data = _textAsset.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)// 첫째 라인(목록) 빼고 리스팅
        {
            string[] elements = data[i].Split(new char[] { ',' });
            if (elements[0].Trim().Length == 0)// 아이디 표기가 없으면 제외
                continue;

            SkillStruct tempData = new SkillStruct
            {
                ID = elements[0].Trim(),
                skillName = elements[1],
                skillDescription = elements[2],
                animationType = (Unit_Animation.AnimationType)Enum.Parse(typeof(Unit_Animation.AnimationType), elements[3]),
                icon = FindSprite(elements[4]),
                level = IntTryParse(elements[5]),
                skillType = (SkillStruct.SkillType)Enum.Parse(typeof(SkillStruct.SkillType), elements[6]),// 기본 데미지의 몇%
                value = FloatTryParse(elements[7]),
                energyType = (SkillStruct.EnergyType)Enum.Parse(typeof(SkillStruct.EnergyType), elements[8]),// 기본 에너지의 몇%
                energyAmount = FloatTryParse(elements[9]),
                castingTime = FloatTryParse(elements[10]),// 0일 경우 즉시시전
                coolingTime = FloatTryParse(elements[11]),
                distance = FloatTryParse(elements[12]),
            };
            skillStruct.Add(tempData);
        }
    }

    void SetUnit(TextAsset _textAsset)
    {
        unitStruct.Clear();
        string[] data = _textAsset.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)// 첫째 라인 빼고 리스팅
        {
            string[] elements = data[i].Split(new char[] { ',' });

            UnitStruct tempData = new UnitStruct
            {
                ID = elements[0].Trim(),
                unitName = elements[1].Trim(),
                unitDescription = elements[2].Trim(),
                unitIcon = FindSprite(elements[3]),

                strength = FloatTryParse(elements[4]),
                agility = FloatTryParse(elements[5]),
                intelligence = FloatTryParse(elements[6]),
                constitution = FloatTryParse(elements[7]),
            };
            unitStruct.Add(tempData);
        }
    }

    void SetItem(TextAsset _textAsset)
    {
        itemStruct.Clear();
        string[] data = _textAsset.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)// 첫째 라인 빼고 리스팅
        {
            string[] elements = data[i].Split(new char[] { ',' });

            ItemStruct tempData = new ItemStruct
            {
                ID = elements[0].Trim(),
                itemName = elements[1].Trim(),
                itemDescription = elements[2].Trim(),
                itemIcon = FindSprite(elements[3]),

                //strength = FloatTryParse(elements[4]),
                //agility = FloatTryParse(elements[5]),
                //intelligence = FloatTryParse(elements[6]),
                //constitution = FloatTryParse(elements[7]),
            };
            itemStruct.Add(tempData);
        }
    }

    List<TranslateString> SetTranslateString(TextAsset _textAsset)
    {
        List<TranslateString> tempString = new List<TranslateString>();
        string[] data = _textAsset.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)// 첫째 라인(목록) 빼고 리스팅
        {
            string[] elements = data[i].Split(new char[] { ',' });
            if (elements[0].Trim().Length == 0)// 아이디 표기가 없으면 제외
                continue;

            TranslateString tempData = new TranslateString
            {
                ID = elements[0].Trim(),
                KR = elements[1],
                EN = elements[2],
                JP = elements[3],
                CN = elements[4],
            };
            tempString.Add(tempData);
        }
        return tempString;
    }
#endif

    public Singleton_Data.Translation translation;// 번역 타입

    [System.Serializable]
    public class DialogInfoamtion
    {
        public string ID;
        public string color;
        public int size;
        public bool bold;
        public Data_DialogType.TextStyle textStyle;
        public float speed;

        public string KR;
        public string EN;
        public string JP;
        public string CN;
    }
    [Header(" [ String ]")]
    public List<DialogInfoamtion> dialog = new List<DialogInfoamtion>();

    [System.Serializable]
    public class TranslateString
    {
        public string ID;
        public string KR;
        public string EN;
        public string JP;
        public string CN;
    }
    public List<TranslateString> skillString = new List<TranslateString>();
    public List<TranslateString> unitString = new List<TranslateString>();
    public List<TranslateString> itemString = new List<TranslateString>();

    [System.Serializable]
    public struct SkillStruct
    {
        public string ID;
        public string skillName;
        [TextArea]
        public string skillDescription;
        public Unit_Animation.AnimationType animationType;
        public Sprite icon;
        public int level;
        public enum SkillType
        {
            Damage,
            Heal,

        }
        public SkillType skillType;
        public float value;
        public enum EnergyType
        {
            Mana,
            Rage,
            Stamina
        }
        public EnergyType energyType;// 기본 에너지의 몇%
        public float energyAmount;
        public float castingTime;// 0일 경우 즉시시전
        public float coolingTime;
        public float distance;
    }
    [Header(" [ Data ]")]
    public List<SkillStruct> skillStruct = new List<SkillStruct>();

    [System.Serializable]
    public struct ItemStruct
    {
        public string ID;
        public string itemName;
        [TextArea]
        public string itemDescription;
        public Sprite itemIcon;
    }
    public List<ItemStruct> itemStruct = new List<ItemStruct>();

    [System.Serializable]
    public struct UnitStruct
    {
        public string ID;
        public string unitName;
        public string unitDescription;// 설명
        public Sprite unitIcon;
        //General
        public float strength;
        public float agility;
        public float intelligence;
        public float constitution;

        public struct UnitAttributes
        {
            public float Health;// 체력
            public float Mana;// 마나
            public float Defense;// 방어
            public float AttackPower;// 힘 공격력
            public float SpellPower;// 지능 공격력
            public float RangePower;// 원거리 공격력
            public float MoveSpeed;
        }
        public UnitAttributes attributes;

        public void SetUnitAttributes()
        {
            attributes.Health = constitution;
            attributes.Mana = intelligence;
        }
    }
    public List<UnitStruct> unitStruct = new List<UnitStruct>();

    private void Awake()
    {
        Singleton_Data.INSTANCE.translation = translation;

        Singleton_Data.INSTANCE.SetDictionary_Dialog(dialog);
        Singleton_Data.INSTANCE.SetDictionary_SkillTranslation(skillString);
        Singleton_Data.INSTANCE.SetDictionary_Skill(skillStruct);
        Singleton_Data.INSTANCE.SetDictionary_UnitTranslation(unitString);
        Singleton_Data.INSTANCE.SetDictionary_Unit(unitStruct);
        Singleton_Data.INSTANCE.SetDictionary_ItemTranslation(itemString);
        Singleton_Data.INSTANCE.SetDictionary_Item(itemStruct);
        Singleton_Data.INSTANCE.SetDictionary_Audio(audioClip);
    }
}
