using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
[CustomEditor(typeof(Data_Animation))]
public class Data_Animation_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
        fontStyle.fontSize = 15;
        fontStyle.normal.textColor = Color.yellow;

        Data_Animation Inspector = target as Data_Animation;
        if (GUILayout.Button("UpdateData", fontStyle, GUILayout.Height(30f)))
        {
            Inspector.UpdateData();
            EditorUtility.SetDirty(Inspector);
        }
        GUILayout.Space(10f);
        base.OnInspectorGUI();
    }
}
#endif

[CreateAssetMenu(fileName = "Data_Animation", menuName = "Scriptable Objects/Data_Animation")]
public class Data_Animation : ScriptableObject
{

#if UNITY_EDITOR
    public List<Object> resourceFolder = new List<Object>();

    public void UpdateData()
    {
        SetDataEditor();
    }

    public void SetDataEditor()
    {
        if (resourceFolder.Count == 0)
        {
            Debug.LogError("애니메이션 폴더 필요");
            return;
        }

        List<AnimationClip> animationClips = new List<AnimationClip>();
        string[] paths = new string[resourceFolder.Count];

        for (int i = 0; i < resourceFolder.Count; i++)
        {
            paths[i] = AssetDatabase.GetAssetPath(resourceFolder[i]);
            Debug.LogWarning("File paths : " + paths[i]);
        }
        // 애니메이션 클립 추출
        var assets = AssetDatabase.FindAssets("t: AnimationClip", paths);

        for (int i = 0; i < assets.Length; i++)
        {
            var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(AnimationClip));
            animationClips.Add(data as AnimationClip);
            //Debug.LogWarning("assetPath : " + data);
        }

        AnimationDatas.Clear();
        for (int i = 0; i < (int)AnimationType.Die + 1; i++)
        {
            AniClipClass aniClipClass = new AniClipClass();
            animationType = (AnimationType)i;

            List<AnimationClip> aniList = new List<AnimationClip>();
            string str = animationType.ToString();

            for (int j = 0; j < animationClips.Count; j++)
            {
                Debug.LogWarning("assetPath : " + animationClips[j].name);
                if (animationClips[j].name.Contains(str, System.StringComparison.OrdinalIgnoreCase))
                {
                    bool loopTime = false;
                    // 애니메이션 클립 이름으로 세팅
                    switch (str)
                    {
                        case "Walk":
                        case "Run":

                        case "Idle":
                            loopTime = true;
                            aniClipClass.playType = AniClipClass.PlayType.Trigger;
                            break;

                        case "Die":
                            aniClipClass.playType = AniClipClass.PlayType.Once;
                            break;

                        case "Dodge":
                        case "DamageSmall":
                        case "DamageBig":
                            aniClipClass.playType = AniClipClass.PlayType.Single;
                            break;
                    }
                    // 애니메이션 루프 변경
                    SerializedProperty AniClip = new SerializedObject(animationClips[j]).FindProperty("m_AnimationClipSettings.m_LoopTime");
                    AniClip.boolValue = loopTime;
                    AniClip.serializedObject.ApplyModifiedProperties();

                    // 애니메이션 스타일(스트링)이 포함되어 있는 경우 추가
                    aniList.Add(animationClips[j]);
                }
            }
            aniClipClass.typeName = i + " : " + str + " (" + aniList.Count + ")";// 클래스 표기용
            aniClipClass.animationClips = aniList;
            AnimationDatas.Add(aniClipClass);
        }
    }
#endif
    // 필요한 기본 애니메이션 이름
    //"Default_Walk"
    //"Default_Run"
    //"Default_Idle"
    //"Default_Battle"
    //"Default_Action01"
    //"Default_Action02"

    [System.Serializable]
    public class AniClipClass
    {
        [HideInInspector]
        public string typeName;
        public enum PlayType
        {
            Trigger,
            Single,
            Once,
            Loop
        }
        public PlayType playType;
        public List<AnimationClip> animationClips;
    }
    public List<AniClipClass> AnimationDatas;

    public enum AnimationType
    {
        Idle,
        Walk,
        Run,
        Skill,
        Dodge,
        DamageSmall,
        DamageBig,
        Die// 마지막 타입
    }
    private AnimationType animationType;
}

public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }

    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
    }
}