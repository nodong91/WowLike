using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow_Manager : MonoBehaviour
{
    public Follow_Target followTarget;
    public Transform target;
    public Camera uiCamera;
    public Vector3 offset;

    public struct FollowStruct
    {
        public Transform followTarget;
        public Vector3 followOffset;
        public Follow_Target.FollowType followType;

        public FollowStruct(Transform _target, Vector3 _offset, Follow_Target.FollowType _followType)
        {
            followTarget = _target;
            followOffset = _offset;
            followType = _followType;
        }
    }
    public Dictionary<Follow_Target, FollowStruct> dictFollow = new Dictionary<Follow_Target, FollowStruct>();
    Coroutine followUI;

    void Start()
    {
        FollowStruct newFollow = new FollowStruct(target, offset, Follow_Target.FollowType.Camera);
        AddFollowUI(followTarget, newFollow);
    }

    public void AddFollowUI(Follow_Target _addFollow, FollowStruct _addStruct)
    {
        if (dictFollow.ContainsKey(_addFollow) == false)
        {
            dictFollow.Add(_addFollow, _addStruct);
            StartFollowing();
        }
    }

    public void RemoveFollowUI(Follow_Target _addFollow)
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

    IEnumerator StartFollowing(Dictionary<Follow_Target, FollowStruct> _follows)
    {
        while (dictFollow.Count > 0)
        {
            foreach (var child in _follows)
            {
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(child.Value.followTarget.position + child.Value.followOffset);
                switch (child.Value.followType)// 카메라 캔버스인 경우
                {
                    case Follow_Target.FollowType.Overlay:
                        child.Key.transform.position = screenPosition;
                        break;

                    case Follow_Target.FollowType.Camera:
                    case Follow_Target.FollowType.HP:
                        Vector3 followPosition = uiCamera.ScreenToWorldPoint(screenPosition);
                        child.Key.transform.position = followPosition;
                        break;
                }
            }
            yield return null;
        }
    }

}
