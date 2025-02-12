using UnityEngine;
public class ExampleScript : MonoBehaviour
{
    void OnMouseDown()
    {
        ScreenCapture.CaptureScreenshot("SomeLevel.png");
    }
}