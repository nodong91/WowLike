using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public UI_Option uiOption;

    public Button openButton, exitButton;
    public CanvasGroup canvas;
    bool open;
    [Header("[ Instance ]")]
    public Transform instanceParent;
    public Dialog_Manager dialog;
    public TMPro.TMP_Text testText;
    public Dictionary<Transform, FollowStruct> dictFollow = new Dictionary<Transform, FollowStruct>();
    Coroutine followUI;
    public UI_HP uiHP;

    public static UI_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    public void SetUIManager()
    {
        canvas.gameObject.SetActive(open);
        openButton.onClick.AddListener(OpenCanvas);
        exitButton.onClick.AddListener(QuitGame);
        uiOption.SetAudioManager();
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











    public void SkillText(string _text)
    {
        testText.text = _text;
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
            followOoffset = Vector3.up,
        };
        UI_HP inst = Instantiate(uiHP, instanceParent);
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










    public Skill_Slot slot;
    public Transform slotParent;
    public Skill_Slot[] slotArray;
    public delegate void DelegateAction(int index);

    public CanvasGroup castingCanvas;
    public Image castingBar;
    public TMPro.TMP_Text warningText;
    Coroutine warningCoroutine;




    public void SetSkillSlot(DelegateAction _action, Data_Manager.SkillStruct[] _skillStruct)
    {
        slotArray = new Skill_Slot[_skillStruct.Length];
        for (int i = 0; i < slotArray.Length; i++)
        {
            int index = i;
            slotArray[index] = Instantiate(slot, slotParent);
            slotArray[index].button.onClick.AddListener(delegate { _action(index); });
            string quickIndex = (index + 1).ToString();// ´ÜÃàÅ°
            slotArray[index].SetSlot(quickIndex, _skillStruct[index]);
        }
        SkillCasting(0f);

        inventory.SetInventory();
        inventory.SetQuickSlot(_action);
    }

    public void SkillCasting(float _value)
    {
        castingBar.fillAmount = _value;
        float alpha = _value > 0 ? 1f : 0f;
        castingCanvas.alpha = alpha;
    }

    public void SetWarning(int _type, string _text)
    {
        _text = SetText(_type, _text);

        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);
        warningCoroutine = StartCoroutine(WarningText(_text));
    }

    string SetText(int _type, string _text, string _color = "FFFFFF")
    {
        switch (_type)
        {
            case 0:
                return $"<b><color=#FF0000>{_text}</color></b>";

            case 1:
                return $"<b><color=#00FF00>{_text}</color></b>";

            case 2:
                return $"<b><color=#0000FF>{_text}</color></b>";

            case 3:
                return $"<b><color=#FFFF00>{_text}</color></b>";
        }
        return _text;
    }


    IEnumerator WarningText(string _text)
    {
        warningText.text = _text;
        warningText.alpha = 1f;
        yield return new WaitForSeconds(1f);

        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 3f;
            warningText.alpha = 1f - normalize;
            yield return null;
        }
        warningText.alpha = 0f;
    }
    public UI_Inventory inventory;
}
