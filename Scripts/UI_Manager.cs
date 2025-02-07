using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public Button openButton, exitButton;
    public CanvasGroup canvas;
    bool open;
    [Header("[ Instance ]")]
    public Dialog_Manager dialog;
    public TMPro.TMP_Text testText;

    public static UI_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        canvas.gameObject.SetActive(open);
        openButton.onClick.AddListener(OpenCanvas);
        exitButton.onClick.AddListener(QuitGame);
    }

    void OpenCanvas()
    {
        open = !open;
        canvas.gameObject.SetActive(open);
    }

    void QuitGame()
    {
        if (Application.isEditor == true)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            Application.Quit();
        }
    }







    public struct FollowStruct
    {
        public Transform followTarget;
        public Vector3 followOoffset;

        public FollowStruct(Transform _target, Vector3 _offset)
        {
            followTarget = _target;
            followOoffset = _offset;
        }
    }
    public Dictionary<Transform, FollowStruct> dictFollow = new Dictionary<Transform, FollowStruct>();
    Coroutine followUI;
    public GameObject hpUI;
    public Transform instanceParent;
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

    public void AddHP()
    {
        Transform target = Game_Manager.instance.target;
        FollowStruct followStruct = new FollowStruct
        {
            followTarget = target,
            followOoffset = Vector3.up,
        };
        GameObject inst = Instantiate(hpUI, instanceParent);
        AddFollowUI(inst.transform, followStruct);
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
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(child.Value.followTarget.position + child.Value.followOoffset);
                child.Key.position = screenPosition;
            }
            yield return null;
        }
    }
}
