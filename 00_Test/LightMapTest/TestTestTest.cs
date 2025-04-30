using NUnit.Framework.Internal;
using UnityEngine;

public class TestTestTest : MonoBehaviour
{
    public LightMapTest instance;
    LightMapTest test;
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            test = Instantiate(instance);
        }
    }
}
