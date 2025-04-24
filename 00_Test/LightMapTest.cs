using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(LightMapTest))]
public class LightMapTest_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
        fontStyle.fontSize = 15;
        fontStyle.normal.textColor = Color.yellow;

        LightMapTest Inspector = target as LightMapTest;
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
public class LightMapTest : MonoBehaviour
{
    public GameObject setTest;
    public LightmapData[] lightmaps;

    public void UpdateData()
    {
        SetLightMap();
    }
    [System.Serializable]
    public struct LightMapMesh
    {
        public MeshRenderer lightMeshRenderer;
        public int lightmapIndex;
        public Vector4 lightmapScaleOffset;
    }
    public List<LightMapMesh> lightMapMeshes = new List<LightMapMesh>();
    [System.Serializable]
    public struct LightMapSetting
    {
        public Texture2D lightmapColor;
        public Texture2D lightmapDir;
        public Texture2D shadowMask;
    }
    public List<LightMapSetting> lightMapSettings;

    void SetLightMap()
    {
        lightmaps = LightmapSettings.lightmaps;

        lightMapMeshes = new List<LightMapMesh>();
        //MeshFilter[] meshArray = setTest.GetComponentsInChildren<MeshFilter>();
        MeshRenderer[] meshRenderer = setTest.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < meshRenderer.Length; i++)
        {
            //Mesh tempMesh = meshArray[i].sharedMesh;
            LightMapMesh tempLightMap = new LightMapMesh
            {
                lightMeshRenderer = meshRenderer[i],
                lightmapIndex = meshRenderer[i].lightmapIndex,
                lightmapScaleOffset = meshRenderer[i].lightmapScaleOffset,
            };
            lightMapMeshes.Add(tempLightMap);
        }
        lightMapSettings = new List<LightMapSetting>();

        for (int i = 0; i < lightmaps.Length; i++)
        {
            LightMapSetting lightmapData = new LightMapSetting
            {
                //lightmapMesh = mesh,
                lightmapColor = lightmaps[i].lightmapColor,
                lightmapDir = lightmaps[i].lightmapDir,
                shadowMask = lightmaps[i].shadowMask,
            };
            lightMapSettings.Add(lightmapData);
        }
        //lightmapIndex = meshRenderer.lightmapIndex;
        //lightmapScaleOffset = meshRenderer.lightmapScaleOffset;

        //copyRenderer.lightmapIndex = lightmapIndex;
        //copyRenderer.lightmapScaleOffset = lightmapScaleOffset;
        //Mesh asdf = Instantiate(mesh);
    }
    private void Start()
    {
        //lightmaps = new LightmapData[lightMapSettings.Count];
        //for (int i = 0; i < lightmaps.Length; i++)
        //{
        //    LightmapData lightmapData = new LightmapData
        //    {
        //        lightmapColor = lightMapSettings[i].lightmapColor,
        //        lightmapDir = lightMapSettings[i].lightmapDir,
        //        shadowMask = lightMapSettings[i].shadowMask,
        //    };
        //    lightmaps[i] = lightmapData;
        //}
        //LightmapSettings.lightmaps = lightmaps;

        for (int i = 0; i < lightMapMeshes.Count; i++)
        {
            MeshRenderer tempMesh = lightMapMeshes[i].lightMeshRenderer;
            tempMesh.lightmapIndex = lightMapMeshes[i].lightmapIndex;
            tempMesh.lightmapScaleOffset = lightMapMeshes[i].lightmapScaleOffset;
        }
    }
}
