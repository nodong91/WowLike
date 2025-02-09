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
                    SetSkillString(GetCSV_Data[i]);
                }
                else
                {
                    SetSkill(GetCSV_Data[i]);
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
        skills.Clear();
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
                icon = FindSprite(elements[3]),
                level = IntTryParse(elements[4]),
                skillType = (SkillStruct.SkillType)Enum.Parse(typeof(SkillStruct.SkillType), elements[5]),// 기본 데미지의 몇%
                value = FloatTryParse(elements[6]),
                energyType = (SkillStruct.EnergyType)Enum.Parse(typeof(SkillStruct.EnergyType), elements[7]),// 기본 에너지의 몇%
                energyAmount = FloatTryParse(elements[8]),
                castingTime = FloatTryParse(elements[9]),// 0일 경우 즉시시전
                coolingTime = FloatTryParse(elements[10]),
                distance = FloatTryParse(elements[11]),
            };
            skills.Add(tempData);
        }
    }

    void SetSkillString(TextAsset _textAsset)
    {
        skillString.Clear();
        string[] data = _textAsset.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)// 첫째 라인(목록) 빼고 리스팅
        {
            string[] elements = data[i].Split(new char[] { ',' });
            if (elements[0].Trim().Length == 0)// 아이디 표기가 없으면 제외
                continue;

            SkillString tempData = new SkillString
            {
                ID = elements[0].Trim(),
                KR = elements[1],
                EN = elements[2],
                JP = elements[3],
                CN = elements[4],
            };
            skillString.Add(tempData);
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

    [System.Serializable]
    public class SkillString
    {
        public string ID;
        public string KR;
        public string EN;
        public string JP;
        public string CN;
    }
    public List<SkillString> skillString;

    [System.Serializable]
    public struct SkillStruct
    {
        public string ID;
        public string skillName;
        [TextArea]
        public string skillDescription;
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
    public List<SkillStruct> skills;

    [System.Serializable]
    public struct UnitInfomation
    {
        public string ID;
        public string Name;
        public string Description;// 설명
        //General
        public int Strength;
        public int Agility;
        public int Constitution;
        public int Intelligence;
        public int Wisdom;

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
            attributes.Health = Constitution;
            attributes.Mana = Wisdom;
        }
    }
    public List <UnitInfomation> unitInfomations;

    public Singleton_Data.Translation translation;// 번역 타입

    private void Awake()
    {
        Singleton_Data.INSTANCE.translation = translation;

        Singleton_Data.INSTANCE.SetDictionary_Dialog(dialog); 
        Singleton_Data.INSTANCE.SetDictionary_SkillTranslation(skillString);
        Singleton_Data.INSTANCE.SetDictionary_Skill(skills);
        Singleton_Data.INSTANCE.SetDictionary_Audio(audioClip);
    }
}
