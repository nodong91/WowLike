using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class Data_Parse : MonoBehaviour
{
    public List<AudioClip> audioClip = new List<AudioClip>();

#if UNITY_EDITOR
    [Header(" [ EDITOR ] ")]
    public List<Object> ResourceFolders = new List<Object>();

    [Header(" [ CSV ] ")]
    /* 구글 스플레트 시트에서 "파일 - 다운로드 - 쉼표로 구분된 값" 으로 저장*/
    public TextAsset[] CSV_Data;

    public virtual void DataSetting()
    {
        if (ResourceFolders.Count == 0)
        {
            Debug.LogError("DataFolders 폴더 필요");
            return;
        }
        //prop = new List<GameObject>();
        //sprite = new List<Sprite>();
        audioClip = new List<AudioClip>();
        //defaultItem = new List<Data_ItemSet>();

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
            var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(GameObject));
            GameObject propData = data as GameObject;
            //prop.Add(propData);
            EditorUtility.SetDirty(data);
        }

        assets = AssetDatabase.FindAssets("t: Sprite", paths);
        // 데이터 추가
        for (int i = 0; i < assets.Length; i++)
        {
            var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(Sprite));
            Sprite iconSpriteData = data as Sprite;
            //sprite.Add(iconSpriteData);
            EditorUtility.SetDirty(data);
        }

        assets = AssetDatabase.FindAssets("t: AudioClip", paths);
        for (int i = 0; i < assets.Length; i++)
        {
            var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(AudioClip));
            AudioClip audioClipData = data as AudioClip;
            audioClip.Add(audioClipData);
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
    }

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

    //public GameObject FindProp(string _str)
    //{
    //    for (int i = 0; i < prop.Count; i++)
    //    {
    //        if (_str.Equals(prop[i].name))
    //            return prop[i];
    //    }
    //    return null;
    //}

    //public Sprite FindSprite(string _str)
    //{
    //    for (int i = 0; i < sprite.Count; i++)
    //    {
    //        if (_str.Equals(sprite[i].name))
    //            return sprite[i];
    //    }
    //    return null;
    //}

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
}
