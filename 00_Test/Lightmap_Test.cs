using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightMap_Test : MonoBehaviour
{
    //public Map_Infomation[] mapArray;
    //public List<LightmapTextures> newLightmap = new List<LightmapTextures>();

    //private void Start()
    //{
    //    AddLightmapTextures();
    //}

    //void AddLightmapTextures()
    //{
    //    // ����Ʈ�� �ε��� �߰�
    //    int arrayStartIndex = 0;
    //    for (int i = 0; i < mapArray.Length; i++)
    //    {
    //        Map_Infomation newMap = Instantiate(mapArray[i]);
    //        newMap.arrayStartIndex = arrayStartIndex;
    //        arrayStartIndex += newMap.lightmapTextures.Count;
    //        newMap.SetMap();
    //        // ����Ʈ�� ���� ��ġ��
    //        for (var j = 0; j < newMap.lightmapTextures.Count; j++)
    //        {
    //            newLightmap.Add(newMap.lightmapTextures[j]);
    //        }
    //    }
    //    LoadLightmapData(newLightmap);
    //}

    //void LoadLightmapData(List<LightmapTextures> newLightmap)
    //{
    //    LightmapData[] lightmaparray = new LightmapData[newLightmap.Count];
    //    for (var i = 0; i < lightmaparray.Length; i++)
    //    {
    //        LightmapData mapdata = new LightmapData();
    //        mapdata.lightmapDir = newLightmap[i].dir;
    //        mapdata.lightmapColor = newLightmap[i].light;
    //        if (newLightmap[i].shadow != null)
    //            mapdata.shadowMask = newLightmap[i].shadow;

    //        lightmaparray[i] = mapdata;
    //        Debug.LogWarning("LoadLightmapData");
    //    }
    //    LightmapSettings.lightmaps = lightmaparray;
    //}
}
