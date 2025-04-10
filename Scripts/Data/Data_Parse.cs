using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Data_Parse : MonoBehaviour
{
#if UNITY_EDITOR
    [Header(" [ EDITOR ] ")]
    public List<Object> ResourceFolders = new List<Object>();

    public virtual void DataSetting()
    {
        units = new List<Unit_Animation>();
        skillSet = new List<Skill_Set>();
        sprites = new List<Sprite>();
        audioClip.Clear();
        CSV_Data.Clear();

        if (ResourceFolders.Count == 0)
        {
            Debug.LogError("DataFolders 폴더 필요");
            return;
        }

        string[] paths = new string[ResourceFolders.Count];
        for (int i = 0; i < ResourceFolders.Count; i++)
        {
            paths[i] = AssetDatabase.GetAssetPath(ResourceFolders[i]);
            Debug.LogWarning("File paths : " + paths[i]);
        }

        string[] assets = AssetDatabase.FindAssets("t: Prefab", paths);
        // 데이터 추가
        for (int i = 0; i < assets.Length; i++)
        {
            var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(Unit_Animation));
            if (data as Unit_Animation)
            {
                Unit_Animation addData = data as Unit_Animation;
                units.Add(addData);
                EditorUtility.SetDirty(data);
            }
            data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(Skill_Set));
            if (data as Skill_Set)
            {
                Skill_Set addData = data as Skill_Set;
                skillSet.Add(addData);
                EditorUtility.SetDirty(data);
            }
        }

        //assets = AssetDatabase.FindAssets("t: Prefab", paths);
        // 데이터 추가
        //for (int i = 0; i < assets.Length; i++)
        //{
        //    var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(Skill_Set));
        //    //if (data as Unit_AI)
        //    //{
        //    //    Unit_AI addData = data as Unit_AI;
        //    //    units.Add(addData);
        //    //    EditorUtility.SetDirty(data);
        //    //}
        //    //else 
        //    if (data as Skill_Set)
        //    {
        //        Skill_Set addData = data as Skill_Set;
        //        skillSet.Add(addData);
        //        EditorUtility.SetDirty(data);
        //    }
        //}

        assets = AssetDatabase.FindAssets("t: Sprite", paths);
        // 데이터 추가
        for (int i = 0; i < assets.Length; i++)
        {
            var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(Sprite));
            Sprite addData = data as Sprite;
            sprites.Add(addData);
            EditorUtility.SetDirty(data);
        }

        assets = AssetDatabase.FindAssets("t: AudioClip", paths);
        for (int i = 0; i < assets.Length; i++)
        {
            var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(AudioClip));
            AudioClip addData = data as AudioClip;
            audioClip.Add(addData);
            EditorUtility.SetDirty(data);
        }

        assets = AssetDatabase.FindAssets("t: ScriptableObject", paths);
        for (int i = 0; i < assets.Length; i++)
        {
            //var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(Data_ItemSet));
            //Data_ItemSet temp = data as Data_ItemSet;
            //defaultItem.Add(temp);
            //EditorUtility.SetDirty(data);
        }

        assets = AssetDatabase.FindAssets("t: TextAsset", paths);// CSV
        for (int i = 0; i < assets.Length; i++)
        {
            var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(TextAsset));
            TextAsset addData = data as TextAsset;
            CSV_Data.Add(addData);
            EditorUtility.SetDirty(data);
        }
    }

    //void SetCSVData()
    //{
    //    if (CSVFolders.Count == 0)
    //    {
    //        Debug.LogError("CSVFolders 폴더 필요");
    //        return;
    //    }

    //    string[] paths = new string[CSVFolders.Count];
    //    for (int i = 0; i < CSVFolders.Count; i++)
    //    {
    //        paths[i] = AssetDatabase.GetAssetPath(CSVFolders[i]);
    //        Debug.LogWarning("File paths : " + paths[i]);
    //    }

    //    string[] assets = AssetDatabase.FindAssets("t: TextAsset", paths);// CSV
    //    for (int i = 0; i < assets.Length; i++)
    //    {
    //        var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(TextAsset));
    //        TextAsset addData = data as TextAsset;
    //        CSV_Data.Add(addData);
    //        EditorUtility.SetDirty(data);
    //    }
    //}

    //==================================================================================
    // Parse
    //==================================================================================

    public int IntTryParse(string _str)
    {
        if (int.TryParse(_str, out int value))
            return value;
        return 0;
    }

    public float FloatTryParse(string _str)
    {
        if (float.TryParse(_str, out float value))
            return value;
        return 0.0f;
    }

    public Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }

    public Unit_Animation FindUnit(string _str)
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (_str.Equals(units[i].ID))
                return units[i];
        }
        return null;
    }

    public Sprite FindSprite(string _str)
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            if (_str.Equals(sprites[i].name))
                return sprites[i];
        }
        return null;
    }

    //public Data_ItemSet FindDefaultItem(string _str)
    //{
    //    for (int i = 0; i < defaultItem.Count; i++)
    //    {
    //        if (_str.Contains(defaultItem[i].name))
    //            return defaultItem[i];
    //    }
    //    return null;
    //}
#endif

    [Header(" [ Resource ] ")]
    /* 구글 스플레트 시트에서 "파일 - 다운로드 - 쉼표로 구분된 값" 으로 저장*/
    [SerializeField] List<TextAsset> CSV_Data = new List<TextAsset>();
    public List<TextAsset> GetCSV_Data { get { return CSV_Data; } }
    public List<AudioClip> audioClip = new List<AudioClip>();
    public List<Sprite> sprites = new List<Sprite>();
    public List<Skill_Set> skillSet = new List<Skill_Set>();
    public List<Unit_Animation> units = new List<Unit_Animation>();
}
