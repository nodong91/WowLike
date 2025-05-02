using UnityEngine;

public class DamageFontTest : MonoBehaviour
{
    public Camera uiCamera;
    void Start()
    {

    }
    //public Vector3 offset;
    public RectTransform followUI;
    // Update is called once per frame
    public Vector3 screenPosition;
    public Vector3 followPosition;
    void Update()
    {
        //Transform target = child.Key.transform;
        //screenPosition = Camera.main.WorldToScreenPoint(Input.mousePosition);
        screenPosition = Input.mousePosition;
        screenPosition.z = 0f;
        followPosition = uiCamera.ScreenToWorldPoint(screenPosition);
        screenPosition.z = 0f;
        followUI.transform.position = Input.mousePosition;
    }
}
