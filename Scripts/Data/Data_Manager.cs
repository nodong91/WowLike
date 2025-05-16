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
        translateString = new List<TranslateString>();
        dialogString = new List<TranslateString>();
        for (int i = 0; i < GetCSV_Data.Count; i++)
        {
            string csv_Type = GetCSV_Data[i].name;
            if (csv_Type.Contains("Translate"))
            {
                if (csv_Type.Contains("Dialog"))
                {
                    dialogString = SetTranslateString(dialogString, GetCSV_Data[i]);
                }
                else
                {
                    translateString = SetTranslateString(translateString, GetCSV_Data[i]);
                }
            }
            else if (csv_Type.Contains("Dialog"))
            {
                SetDialogData(GetCSV_Data[i]);
            }
            else if (csv_Type.Contains("Skill"))
            {
                SetSkill(GetCSV_Data[i]);
            }
            else if (csv_Type.Contains("Unit"))
            {
                SetUnit(GetCSV_Data[i]);
            }
            else if (csv_Type.Contains("Item"))
            {
                SetItem(GetCSV_Data[i]);
            }

        }
    }

    void SetDialogData(TextAsset _textAsset)
    {
        dialogStruct.Clear();
        string[] data = _textAsset.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)// ù° ����(���) ���� ������
        {
            string[] elements = data[i].Split(new char[] { ',' });
            if (elements[0].Trim().Length == 0)// ���̵� ǥ�Ⱑ ������ ����
                continue;

            DialogStruct tempData = new DialogStruct
            {
                ID = elements[0].Trim(),
                textStyle = elements[1].Trim().Length > 0 ?
            (Data_DialogType.TextStyle)Enum.Parse(typeof(Data_DialogType.TextStyle), elements[1]) : 0,
                size = IntTryParse(elements[2]),
                color = elements[3],
                bold = elements[4] == "TRUE" ? true : false,
                speed = FloatTryParse(elements[5].Trim()),

                //KR = elements[6],
                //EN = elements[7],
                //JP = elements[8],
                //CN = elements[9],
            };
            dialogStruct.Add(tempData);
        }
    }

    void SetSkill(TextAsset _textAsset)
    {
        skillStruct.Clear();
        string[] data = _textAsset.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)// ù° ����(���) ���� ������
        {
            string[] elements = data[i].Split(new char[] { ',' });
            if (elements[0].Trim().Length == 0)// ���̵� ǥ�Ⱑ ������ ����
                continue;

            SkillStruct tempData = new SkillStruct
            {
                ID = elements[0].Trim(),
                skillName = elements[1],
                skillDescription = elements[2],
                animationType = (Unit_Animation.AnimationType)Enum.Parse(typeof(Unit_Animation.AnimationType), elements[3]),
                icon = FindSprite(elements[4].Trim()),
                synergy = Parse_Vector2Int(elements[5].Trim()),
                level = IntTryParse(elements[6]),
                skillType = (SkillStruct.SkillType)Enum.Parse(typeof(SkillStruct.SkillType), elements[7]),// �⺻ �������� ��%
                ccType = (SkillStruct.CCType)Enum.Parse(typeof(SkillStruct.CCType), elements[8]),// �⺻ �������� ��%
                energyType = (SkillStruct.EnergyType)Enum.Parse(typeof(SkillStruct.EnergyType), elements[9]),// �⺻ �������� ��%
                energyAmount = FloatTryParse(elements[10]),
                castingTime = FloatTryParse(elements[11]),// 0�� ��� ��ý���
                coolingTime = FloatTryParse(elements[12]),
                range = Parse_Vector2(elements[13]),
                //range = new Vector2(FloatTryParse(elements[13]), FloatTryParse(elements[14])),
                influence = Parse_Vector3(elements[14]),
                //influence = new Vector3(FloatTryParse(elements[14]), FloatTryParse(elements[16]), FloatTryParse(elements[17])),
                aggro = FloatTryParse(elements[15]),
                skillSet = elements[16].Trim(),
                splashRange = FloatTryParse(elements[17]),
                splashAngle = FloatTryParse(elements[18]),
                projectileSpeed = FloatTryParse(elements[19]),
            };
            skillStruct.Add(tempData);
        }
    }

    void SetUnit(TextAsset _textAsset)
    {
        unitStruct.Clear();
        string[] data = _textAsset.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)// ù° ���� ���� ������
        {
            string[] elements = data[i].Split(new char[] { ',' });

            UnitStruct tempData = new UnitStruct
            {
                ID = elements[0].Trim(),
                unitName = elements[1].Trim(),
                unitDescription = elements[2].Trim(),
                unitIcon = FindSprite(elements[3].Trim()),
                synergy = Parse_Vector2Int(elements[4].Trim()),
                unitProp = FindUnit(elements[0].Trim()),
                unitSize = FloatTryParse(elements[5].Trim()),
                defaultSkill01 = elements[6].Trim(),
                defaultSkill02 = elements[7].Trim(),
                // �ɷ�ġ
                strength = FloatTryParse(elements[8]),
                agility = FloatTryParse(elements[9]),
                intelligence = FloatTryParse(elements[10]),
                constitution = FloatTryParse(elements[11]),
            };
            unitStruct.Add(tempData);
        }
    }

    void SetItem(TextAsset _textAsset)
    {
        itemStruct.Clear();
        string[] data = _textAsset.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)// ù° ���� ���� ������
        {
            string[] elements = data[i].Split(new char[] { ',' });

            ItemStruct tempData = new ItemStruct
            {
                ID = elements[0].Trim(),
                itemName = elements[1].Trim(),
                itemDescription = elements[2].Trim(),
                itemIcon = FindSprite(elements[3].Trim()),
                synergy = Parse_Vector2Int(elements[4].Trim()),
            };
            itemStruct.Add(tempData);
        }
    }

    List<TranslateString> SetTranslateString(List<TranslateString> _tempString, TextAsset _textAsset)
    {
        string[] data = _textAsset.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)// ù° ����(���) ���� ������
        {
            string[] elements = data[i].Split(new char[] { ',' });
            if (elements[0].Trim().Length == 0)// ���̵� ǥ�Ⱑ ������ ����
                continue;

            TranslateString tempData = new TranslateString
            {
                ID = elements[0].Trim(),
                KR = elements[1],
                EN = elements[2],
                JP = elements[3],
                CN = elements[4],
            };
            _tempString.Add(tempData);
        }
        return _tempString;
    }
#endif

    [System.Serializable]
    public class DialogStruct
    {
        public string ID;
        public string color;
        public int size;
        public bool bold;
        public Data_DialogType.TextStyle textStyle;
        public float speed;
    }
    [Header(" [ String ]")]
    public List<DialogStruct> dialogStruct = new List<DialogStruct>();

    [System.Serializable]
    public class TranslateString
    {
        public string ID;
        public string KR;
        public string EN;
        public string JP;
        public string CN;
    }
    public List<TranslateString> dialogString = new List<TranslateString>();
    public List<TranslateString> translateString = new List<TranslateString>();

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
        public Vector2Int[] synergy;
        public enum SkillType
        {
            Damage,
            Heal,

        }
        public SkillType skillType;
        public enum CCType
        {
            Normal,
            KnockBack,
        }
        public CCType ccType;
        public enum EnergyType
        {
            Mana,
            Rage,
            Stamina
        }
        public EnergyType energyType;// �⺻ �������� ��%
        public float energyAmount;
        public float castingTime;// 0�� ��� ��ý���
        public float coolingTime;
        public Vector2 range;
        public Vector3 influence;// ��,��,�� ����
        public float aggro;
        public string skillSet;
        public float splashRange;
        public float splashAngle;// Ÿ�� ���� �� ����
        public float projectileSpeed;// ź�� ���� �� ź �ӵ�

        public float GetDamage(float _ap, float _sp, float _rp)
        {
            float damage = _ap * influence.x + _sp * influence.y + _rp * influence.z;
            return damage;
        }
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
        public Vector2Int[] synergy;
    }
    public List<ItemStruct> itemStruct = new List<ItemStruct>();

    [System.Serializable]
    public struct UnitStruct
    {
        public string ID;
        public string unitName;
        public string unitDescription;// ����
        public Sprite unitIcon;
        public Unit_Animation unitProp;
        public float unitSize;
        public string defaultSkill01;
        public string defaultSkill02;
        public Vector2Int[] synergy;

        //General
        public float strength;
        public float agility;
        public float intelligence;
        public float constitution;
        [System.Serializable]
        public struct UnitAttributes
        {
            public float Health;// ü��
            public float Mana;// ����
            public float Defense;// ���
            public float AttackPower;// �� ���ݷ�
            public float SpellPower;// ���� ���ݷ�
            public float RangePower;// ���Ÿ� ���ݷ�
            public float MoveSpeed;
        }

        public UnitAttributes TryAttributes()
        {
            UnitAttributes attributes = new UnitAttributes
            {
                Health = constitution * 10f,
                Mana = intelligence,
                Defense = strength,
                AttackPower = strength,
                SpellPower = intelligence,
                RangePower = agility,
                MoveSpeed = agility,
            };
            return attributes;
        }
    }
    public List<UnitStruct> unitStruct = new List<UnitStruct>();

    private void Awake()
    {
        Singleton_Data.INSTANCE.SetDictionary_Dialog(dialogStruct);
        Singleton_Data.INSTANCE.SetDictionary_DialogString(dialogString);

        Singleton_Data.INSTANCE.SetDictionary_Skill(skillStruct);
        Singleton_Data.INSTANCE.SetDictionary_Unit(unitStruct);
        Singleton_Data.INSTANCE.SetDictionary_Item(itemStruct);
        Singleton_Data.INSTANCE.SetDictionary_TranslationString(translateString);

        Singleton_Data.INSTANCE.SetDictionary_Audio(audioClip);
        Singleton_Data.INSTANCE.SetSkillSet(skillSet);
    }
}
