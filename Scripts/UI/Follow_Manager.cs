using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow_Manager : MonoBehaviour
{
    public Camera uiCamera;
    public Vector3 offset;
    public Dictionary<GameObject, Follow_Target> dictFollow = new Dictionary<GameObject, Follow_Target>();
    Coroutine followUI;

    public void AddFollowUI(GameObject _object, Follow_Target _followTarget)
    {
        if (dictFollow.ContainsKey(_object) == false)
        {
            dictFollow.Add(_object, _followTarget);
            StartFollowing();
        }
    }

    public Follow_HP RemoveFollowHP(GameObject _object)
    {
        Follow_HP returnTarget = dictFollow[_object] as Follow_HP;
        dictFollow.Remove(_object);
        return returnTarget;
    }

    public Follow_Target RemoveFollowUI(GameObject _object)
    {
        Follow_Target returnTarget = dictFollow[_object];
        dictFollow.Remove(_object);
        return returnTarget;
    }

    public void ShakingUI(GameObject _object)
    {
        dictFollow[_object].ShakeStart();
    }

    //public void AddHPUI(Transform _target)
    //{
    //    if (_target == null)
    //        return;
    //    FollowStruct followStruct = new FollowStruct
    //    {
    //        followTarget = _target,
    //        followOffset = Vector3.up,
    //    };
    //    //AddFollowUI(testUI, followStruct);
    //}

    void StartFollowing()
    {
        if (followUI != null)
            StopCoroutine(followUI);
        followUI = StartCoroutine(StartFollowing(dictFollow));
    }

    IEnumerator StartFollowing(Dictionary<GameObject, Follow_Target> _follows)
    {
        while (dictFollow.Count > 0)
        {
            foreach (var child in _follows)
            {
                Transform target = child.Key.transform;
                Follow_Target followUI = child.Value;
                Vector3 offset = child.Value.followOffset;
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.position + offset);
                switch (child.Value.followType)// 카메라 캔버스인 경우
                {
                    case Follow_Target.FollowType.Overlay:
                        followUI.transform.position = screenPosition;
                        break;

                    case Follow_Target.FollowType.Camera:
                    case Follow_Target.FollowType.HP:
                        Vector3 followPosition = uiCamera.ScreenToWorldPoint(screenPosition);
                        followUI.transform.position = followPosition;
                        break;
                }
            }
            yield return null;
        }
    }

}
