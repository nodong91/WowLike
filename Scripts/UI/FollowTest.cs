using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTest : MonoBehaviour
{
    public Transform image;
    public Transform target;
    public Camera uiCamera;
    public Vector3 offset;

    public struct FollowStruct
    {
        public Transform followTarget;
        public Vector3 followOffset;
        public bool camera;

        public FollowStruct(Transform _target, Vector3 _offset, bool _camera)
        {
            followTarget = _target;
            followOffset = _offset;
            camera = _camera;
        }
    }
    public Dictionary<Transform, FollowStruct> dictFollow = new Dictionary<Transform, FollowStruct>();
    Coroutine followUI;

    void Start()
    {
        FollowStruct followTarget = new FollowStruct(target, offset, true);
        AddFollowUI(image, followTarget);
    }

    public void AddFollowUI(Transform _addFollow, FollowStruct _addStruct)
    {
        if (dictFollow.ContainsKey(_addFollow) == false)
        {
            dictFollow.Add(_addFollow, _addStruct);
            StartFollowing();
        }
    }

    public void RemoveFollowUI(Transform _addFollow)
    {
        dictFollow.Remove(_addFollow);
    }

    public void AddHPUI(Transform _target)
    {
        if (_target == null)
            return;
        FollowStruct followStruct = new FollowStruct
        {
            followTarget = _target,
            followOffset = Vector3.up,
        };
        //AddFollowUI(testUI, followStruct);
    }

    void StartFollowing()
    {
        if (followUI != null)
            StopCoroutine(followUI);
        followUI = StartCoroutine(StartFollowing(dictFollow));
    }

    IEnumerator StartFollowing(Dictionary<Transform, FollowStruct> _follows)
    {
        while (dictFollow.Count > 0)
        {
            foreach (var child in _follows)
            {
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(child.Value.followTarget.position + child.Value.followOffset);
                if (child.Value.camera == true)// 카메라 캔버스인 경우
                {
                    Vector3 followPosition = uiCamera.ScreenToWorldPoint(screenPosition);
                    child.Key.position = followPosition;
                }
                else
                {
                    child.Key.position = screenPosition;
                }
            }
            yield return null;
        }
    }

}
