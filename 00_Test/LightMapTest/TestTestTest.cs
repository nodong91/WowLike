using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TestTestTest : MonoBehaviour
{
    public SettingLightMap instance;
    SettingLightMap test;

    public List<Light> lightList = new List<Light>();

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            //test = Instantiate(instance);
            Light[] lights = (Light[])FindObjectsByType(typeof(Light), FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                Light temp = lights[i];
                bool Realtime = temp.lightmapBakeType == LightmapBakeType.Realtime;
                bool Directional = temp.type == LightType.Directional;
                if (Realtime == true && Directional == true)
                {
                    lightList.Add(temp);
                }
            }
        }
    }
}
