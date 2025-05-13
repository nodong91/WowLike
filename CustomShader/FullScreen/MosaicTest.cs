using UnityEngine;

public class MosaicTest : MonoBehaviour
{
    public Material fullScreen;
    public Vector2 mosaicPoint;

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            mosaicPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            fullScreen.SetVector("_MosaicPoint", mosaicPoint);
        }
    }
}
