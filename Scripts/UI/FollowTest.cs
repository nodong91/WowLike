using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTest : MonoBehaviour
{
    public Transform image;
    public Transform target;
    public Camera uiCam;
    public Vector3 offset;

    void Start()
    {
        FollowStruct followTarget = new FollowStruct(target, offset);
        AddFollowUI_UICamera(image, followTarget, uiCam);
    }














    public struct FollowStruct
    {
        public Transform followTarget;
        public Vector3 followOffset;

        public FollowStruct(Transform _target, Vector3 _offset)
        {
            followTarget = _target;
            followOffset = _offset;
        }
    }
    public Dictionary<Transform, FollowStruct> dictFollow = new Dictionary<Transform, FollowStruct>();
    Coroutine followUI;
    //public Transform testUI;

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
                child.Key.position = screenPosition;
            }
            yield return null;
        }
    }

    public void AddFollowUI_UICamera(Transform _addFollow, FollowStruct _addStruct, Camera _uiCam)
    {
        if (dictFollow.ContainsKey(_addFollow) == false)
        {
            dictFollow.Add(_addFollow, _addStruct);
            StartFollowing_UICamera(_uiCam);
        }
    }

    void StartFollowing_UICamera(Camera _uiCam)
    {
        if (followUI != null)
            StopCoroutine(followUI);
        followUI = StartCoroutine(StartFollowing_UICamera(dictFollow, _uiCam));
    }

    IEnumerator StartFollowing_UICamera(Dictionary<Transform, FollowStruct> _follows, Camera _uiCam)
    {
        while (dictFollow.Count > 0)
        {
            foreach (var child in _follows)
            {
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(child.Value.followTarget.position + child.Value.followOffset);
                Vector3 followPosition = _uiCam.ScreenToWorldPoint(screenPosition);
                child.Key.position = followPosition;
            }
            yield return null;
        }
    }

}
