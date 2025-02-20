using UnityEngine;

public class FollowTest : MonoBehaviour
{
    public Transform image;
    public Transform target;
    public Camera uiCam;
    public Vector3 offset;

    void Start()
    {
        UI_Manager.FollowStruct followTarget = new UI_Manager.FollowStruct
        {
            followTarget = target,
            followOffset = offset
        };
        UI_Manager.instance.AddFollowUI1(image, followTarget, uiCam);
    }
}
