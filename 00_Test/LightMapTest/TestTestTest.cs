using NUnit.Framework.Internal;
using UnityEngine;

public class TestTestTest : MonoBehaviour
{
    public SettingLightMap instance;
    SettingLightMap test;
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            test = Instantiate(instance);
        }
    }
}
