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
        instHP.followType = Follow_Target.FollowType.Camera;

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
        instTarget.followType = Follow_Target.FollowType.Overlay;
        if (dictFollow.ContainsKey(_object) == false)
        {
            dictFollow.Add(_object, instTarget);
            StartFollowing();
        }
    }

    public void AddFollowWorld(GameObject _object)
    {
        Follow_Target instTarget = TryFollowTarget();
        instTarget.followType = Follow_Target.FollowType.Camera;
        if (dictFollow.ContainsKey(_object) == false)
        {
            dictFollow.Add(_object, instTarget);
            StartFollowing();
        }
    }
    Follow_Target closestFollow;
    GameObject closestTarget;
    Coroutine closest;
    public void FollowClosestTarget(GameObject _object)
    {
        closestTarget = _object;
        if (closestFollow == null)
        {
            closestFollow = TryFollowTarget();
            closestFollow.followType = Follow_Target.FollowType.Overlay;
            closestFollow.transform.SetParent(overlayParent);
        }
        if(closest!= null)
            StopCoroutine(closest);
        closest= StartCoroutine(FollowClosestTarget());
    }

    IEnumerator FollowClosestTarget()
    {
        while (closestTarget != null)
        {
            closestFollow.transform.localScale = Vector3.one;
            Vector3 offset = closestFollow.followOffset;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(closestTarget.transform.position + offset);
            closestFollow.transform.position = screenPosition;
            yield return null;
        }
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
                followUI.transform.localScale = Vector3.one;

                Vector3 offset = followUI.followOffset;
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.position + offset);
                switch (followUI.followType)// 카메라 캔버스인 경우
                {
                    case Follow_Target.FollowType.Overlay:
                        followUI.transform.SetParent(overlayParent);
                        followUI.transform.position = screenPosition;
                        break;

                    case Follow_Target.FollowType.Camera:
                        Vector3 followPosition = UICamera.ScreenToWorldPoint(screenPosition);
                        followUI.transform.SetParent(cameraParent);
                        followUI.transform.position = followPosition;
                        break;
                }
            }
            yield return null;
        }
    }

    public void RemoveFollowHP(GameObject _object)
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


    [Header(" [ Follow ]")]
    public Follow_Target followTarget;
    public Follow_HP followHP;
    public RectTransform overlayParent;
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
        Follow_HP instTarget = Instantiate(followHP, overlayParent);
        return instTarget;
    }

    Follow_Target TryFollowTarget()
    {
        if (followQueue.Count > 0)
        {
            Follow_Target follow = followQueue.Dequeue();
            follow.gameObject.SetActive(true);
            return follow;
        }
        Follow_Target instTarget = Instantiate(followTarget, overlayParent);
        instTarget.SetFollowCamera();

        return instTarget;
    }






    // 노드 위에 아이콘 올리기
    public Dictionary<GameObject, Vector3> followVectors = new Dictionary<GameObject, Vector3>();
    Coroutine vectorFollow;
    public GameObject followBuff;
    public Queue<GameObject> buffQueue = new Queue<GameObject>();

    public void OnBuff(Node _node)
    {
        BuffClear();
        if (_node.nodeType == Node.NodeType.Player && _node.onObject != null)
        {
            Unit_AI unit = Game_Manager.current.GetUnitDict[_node.onObject];
            if (unit != null)
            {
                for (int i = 0; i < unit.synergy.Length; i++)
                {
                    Vector2Int grid = _node.grid + unit.synergy[i];
                    Node synergy = Game_Manager.current.GetMapGenerator.nodeMap[grid.x, grid.y];
                    GameObject inst = TryFollowBuff();
                    followVectors[inst] = synergy.worldPosition;
                    VectorFollow();
                }
            }
        }
    }

    GameObject TryFollowBuff()
    {
        if (buffQueue.Count > 0)
        {
            GameObject buff = buffQueue.Dequeue();
            buff.gameObject.SetActive(true);
            return buff;
        }
        GameObject inst = Instantiate(followBuff, cameraParent);
        return inst;
    }

    public void BuffClear()
    {
        foreach (var child in followVectors)
        {
            buffQueue.Enqueue(child.Key);
            child.Key.SetActive(false);
        }
        followVectors.Clear();
    }

    void VectorFollow()
    {
        if (vectorFollow != null)
            StopCoroutine(vectorFollow);
        vectorFollow = StartCoroutine(VectorFollowing());
    }

    IEnumerator VectorFollowing()
    {
        while (followVectors.Count > 0)
        {
            foreach (var child in followVectors)
            {
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(child.Value);
                Vector3 followPosition = UICamera.ScreenToWorldPoint(screenPosition);
                child.Key.transform.position = followPosition;
            }
            yield return null;
        }
    }






    public RectTransform cameraParent;
    public DamageFont damageFont;

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            int index = Random.Range(0, 1500);
            Vector3 screenPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            Vector3 followPosition = UICamera.ViewportToScreenPoint((screenPosition - Vector3.one * 0.5f) * 2f);
            damageFont.FollowUI(index, followPosition);
        }
    }

    public void DamageText(int _damage, Transform _target)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_target.position);
        Vector3 followPosition = UICamera.ScreenToWorldPoint(screenPosition);
        damageFont.FollowWorld(_damage, followPosition);
        Debug.LogWarning(_target.gameObject.name);
    }
}
