using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Data_Dialog))]
public class Data_Dialog_Editor : Editor
{
    private UnityEditorInternal.ReorderableList list;
    //public Data_ItemSetting randomReward;

    private void OnEnable()
    {
        list = new UnityEditorInternal.ReorderableList(serializedObject, serializedObject.FindProperty("dialogOptions"), true,
          true, true, true);

        list.drawElementCallback = (rect, index, active, focused) =>
        {
            rect.y += 2;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            var baseWidth = (rect.width / 2) - 5;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, baseWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("title"), GUIContent.none);
            EditorGUI.PropertyField(
                new Rect(rect.x + baseWidth + 5, rect.y, baseWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("dialogData"), GUIContent.none);
            //EditorGUI.PropertyField(
            //    new Rect(rect.x + (baseWidth * 2) + 10, rect.y, baseWidth, EditorGUIUtility.singleLineHeight),
            //    element.FindPropertyRelative("value"), GUIContent.none);
        };

        list.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Options (출력 옵션)");
        };
        //list.onAddCallback = list =>
        //{
        //    UnityEditorInternal.ReorderableList.defaultBehaviours.DoAddButton(list);

        //    int index = list.serializedProperty.arraySize - 1;
        //    SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);

        //    //element.FindPropertyRelative("title").stringValue = "DefaultName";
        //    //element.FindPropertyRelative("dialogData").serializedObject;
        //    //element.FindPropertyRelative("CharacterLevel").intValue = 1;
        //    //element.FindPropertyRelative("value").floatValue = 0.0f;
        //};
    }

    public override void OnInspectorGUI()
    {

        base.OnInspectorGUI();
        GUILayout.Space(10f);

        GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
        fontStyle.fontSize = 15;
        fontStyle.normal.textColor = Color.yellow;

        Data_Dialog Inspector = target as Data_Dialog;
        //if (GUILayout.Button("Dialog", fontStyle, GUILayout.Height(30f)))
        //{
        //    //Inspector.UpdateData();
        //    EditorUtility.SetDirty(Inspector);
        //}

        GUILayout.Space(10f);
        switch (Inspector.afterType)
        {
            case Data_Dialog.AfterType.None:
                break;

            case Data_Dialog.AfterType.Options:
                list.DoLayoutList();
                break;

            case Data_Dialog.AfterType.Reward:
            case Data_Dialog.AfterType.Shop:
                //Inspector.itemValue = EditorGUILayout.IntField("Item Value", Inspector.itemValue);
                Inspector.boxSize = EditorGUILayout.Vector2IntField("Box Size", Inspector.boxSize);
                //Inspector.itemSetting = (Data_ItemSetting)EditorGUILayout.ObjectField("Item Setting", Inspector.itemSetting, typeof(Data_ItemSetting), true);

                //Rect rect = GUILayoutUtility.GetLastRect();
                //float width = rect.width / 5f;
                float width = EditorGUIUtility.currentViewWidth / 10f;
                //EditorGUILayout.LabelField(width.ToString());
                EditorGUILayout.LabelField("Item Value");
                GUILayout.BeginHorizontal();
                Inspector.limit.x = EditorGUILayout.IntField(Inspector.limit.x, GUILayout.Width(width));
                EditorGUILayout.LabelField(((int)Inspector.itemValue.x).ToString(), fontStyle, GUILayout.Width(width));
                EditorGUILayout.MinMaxSlider(ref Inspector.itemValue.x, ref Inspector.itemValue.y, Inspector.limit.x, Inspector.limit.y);
                EditorGUILayout.LabelField(((int)Inspector.itemValue.y).ToString(), fontStyle, GUILayout.Width(width));
                Inspector.limit.y = EditorGUILayout.IntField(Inspector.limit.y, GUILayout.Width(width));
                GUILayout.EndHorizontal();
                break;
        }
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(Inspector);
    }
}
#endif

[CreateAssetMenu(fileName = "Data_Dialog_Normal", menuName = "Create Data/Data_Dialog/Data_Dialog_Normal")]
public class Data_Dialog : ScriptableObject
{
    public AfterType afterType;
    [Header(" [ Data_Dialog ] ")]
    public string ID;
    public SetDialogInfo[] dialogInfos;

    public enum AfterType
    {
        None,
        Options,// 대화 선택지
        Shop,
        Reward// 보상
    }
    [System.Serializable]
    public struct OptionTitle
    {
        public string title;
        public Data_Dialog dialogData;
    }
    [HideInInspector]
    public Vector2 itemValue = new Vector2(1f, 3f);
    [HideInInspector]
    public Vector2Int limit = new Vector2Int(0, 10);
    public int GetItemValue
    {
        get
        {
            int getValue = (int)Random.Range(itemValue.x, itemValue.y);
            return getValue;
        }
    }
    [HideInInspector]
    public List<OptionTitle> dialogOptions = new List<OptionTitle>();
    [HideInInspector]
    public Vector2Int boxSize;
    [HideInInspector]
    //public Data_ItemSetting itemSetting;
    public AfterType GetAfterType { get { return afterType; } }
    public List<OptionTitle> TryOptions { get { return dialogOptions; } }
}















//======================================================================================================
// Struct
//======================================================================================================

[System.Serializable]
public struct TextStruct
{
    [Header("[ Default ]")]
    public string text;
    public bool lineEnd;// 줄넘기기
    public int size;
    public Color color;

    [Header("[ Animation ]")]
    public TextType textType;
    public enum TextType
    {
        None,
        Moving,
        Wave,
        Jitter
    }
    public Vector2 actionAngle;
    public float length, animSpeed;
}

[System.Serializable]
public struct SetDialogInfo
{
    public DialogType dialogType;
    public enum DialogType
    {
        Narration,
        Bubble,
        Scream
    }
    public string speech;// 대사
    public TextStruct[] textStruct;
    public enum FocusTargetType
    {
        Player,
        NPC
    }
    public FocusTargetType focusTarget;
    [HideInInspector]
    public List<TextAnimation> animatingTextCount;// 움직이는 글자 개수(계산용)
    [System.Serializable]
    public struct TextAnimation
    {
        public int textNum;
        public int textType;
        public TextAnimation(int _textNum, int _textType)
        {
            textNum = _textNum;
            textType = _textType;
        }
    }
    public string GetSpeech()
    {
        animatingTextCount = new List<TextAnimation>();
        string setSpeech = string.Empty;
        int animIndex = 0;
        for (int i = 0; i < textStruct.Length; i++)
        {
            if (textStruct[i].textType != TextStruct.TextType.None)// 애니메이션 문자열
            {
                int start = animIndex;
                int end = start + textStruct[i].text.Length;
                for (int c = start; c < end; c++)
                {
                    TextAnimation textAnimation = new TextAnimation(c, i);
                    animatingTextCount.Add(textAnimation);
                }
            }

            string textStr = textStruct[i].text;
            if (textStruct[i].lineEnd == true)// 다음 문자열에서 빼기
            {
                textStr += "/n";
                animIndex++;
            }
            animIndex += textStruct[i].text.Length;

            setSpeech += "<color=#" + ColorUtility.ToHtmlStringRGB(textStruct[i].color) + ">";
            setSpeech += "<size=" + textStruct[i].size + ">";
            setSpeech += textStr;
            setSpeech += "</size>";
            setSpeech += "</color>";
        }
        return setSpeech;
    }
}