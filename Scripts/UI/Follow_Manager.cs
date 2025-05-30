using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Follow_Manager : MonoBehaviour
{
    public Camera UICamera;
    public Vector3 offset;
    public Dictionary<GameObject, Follow_Target> dictFollow = new Dictionary<GameObject, Follow_Target>();
    Coroutine followUI;

    public void SetUICamera()
    {
        Camera mainCamera = Camera.main;
        var cameraData = mainCamera.GetUniversalAdditionalCameraData();
        if (cameraData.cameraStack.Contains(UICamera) == false)
        {
            UICamera.fieldOfView = mainCamera.fieldOfView;
            cameraData.cameraStack.Add(UICamera);
        }
    }

    public void AddFollowHP(Unit_AI _unit)
    {
        Follow_HP instHP = TryFollowHPTarget();

        Color teamColor = _unit.gameObject.layer == LayerMask.NameToLayer("Player") ? Color.red : Color.green;
        instHP.sliderImage.color = teamColor;
        instHP.SetFollowUnit(_unit);

        _unit.deleUpdateHP = instHP.SetHP;// 체력 바
        _unit.deleUpdateAction = instHP.SetAction;// 액션 바
        _unit.deleDamage = DamageText;// 데미지

        if (dictFollow.ContainsKey(_unit.gameObject) == false)
        {
            dictFollow.Add(_unit.gameObject, instHP);
            StartFollowing();
        }
    }

    public void AddFollowUI(GameObject _object)
    {
        Follow_Target instTarget = TryFollowTarget();
        if (dictFollow.ContainsKey(_object) == false)
        {
            dictFollow.Add(_object, instTarget);
            StartFollowing();
        }
    }

    public void RemoveFollow(GameObject _object)
    {
        if (dictFollow[_object] as Follow_HP)
        {
            Follow_HP instTarget = dictFollow[_object] as Follow_HP;
            followHPQueue.Enqueue(instTarget);
        }
        else
        {
            Follow_Target instTarget = dictFollow[_object];
            followQueue.Enqueue(instTarget);
        }
        dictFollow[_object].gameObject.SetActive(false);
        dictFollow.Remove(_object);
    }

    public void ShakingUI(GameObject _object)
    {
        dictFollow[_object].ShakeStart();
    }

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
                        Vector3 followPosition = UICamera.ScreenToWorldPoint(screenPosition);
                        followUI.transform.position = followPosition;
                        break;
                }
            }
            yield return null;
        }
    }


    [Header(" [ Follow ]")]
    public Follow_Target followTarget;
    public Follow_HP followHP;
    public RectTransform followParent;
    private Queue<Follow_HP> followHPQueue = new Queue<Follow_HP>();
    private Queue<Follow_Target> followQueue = new Queue<Follow_Target>();

    Follow_HP TryFollowHPTarget()
    {
        if (followHPQueue.Count > 0)
        {
            Follow_HP follow = followHPQueue.Dequeue();
            follow.gameObject.SetActive(true);
            return follow;
        }
        Follow_HP instTarget = Instantiate(followHP, followParent);
        return instTarget;
    }

    //public void RemoveFollowHP(GameObject _target)
    //{
    //    Follow_HP instTarget = RemoveFollowHP(_target);
    //    instTarget.gameObject.SetActive(false);
    //    followHPQueue.Enqueue(instTarget);
    //}

    Follow_Target TryFollowTarget()
    {
        if (followQueue.Count > 0)
        {
            Follow_Target follow = followQueue.Dequeue();
            follow.gameObject.SetActive(true);
            return follow;
        }
        Follow_Target instTarget = Instantiate(followTarget, followParent);
        instTarget.SetFollow();

        return instTarget;
    }





    public RectTransform cameraCanvas;
    public DamageFont baseDamage;
    DamageFont instDamageFont;

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            instDamageFont = Instantiate(baseDamage, cameraCanvas);
            int index = Random.Range(0, 1500);
            instDamageFont.DisplayDamage(index);
        }
    }

    public void DamageText(Transform _target, string _damage)
    {
        instDamageFont = Instantiate(baseDamage, cameraCanvas);
        instDamageFont.DisplayDamage(_target, _damage);
    }
}
