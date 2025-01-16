using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialog_Manager : MonoBehaviour
{
    public int currentSpeech;
     Data_Dialog dialogData;
    public Dialog_Base dialogBase;
    private Queue<Dialog_Base> instUIQueue = new Queue<Dialog_Base>();

     Dialog_Base currentDialog;
    public Dialog_SelectionMenu selectionMenu;// ���� �ɼ�

    public void SetDialogData(Data_Dialog _dialogData)
    {
        dialogData = _dialogData;
        currentSpeech = 0;
        //GameManager.current.SetHoldUnit = true;
        StartDialog();
    }

    public void StartDialog()
    {
        if (currentSpeech < 0)
        {
            EndDialog(currentDialog);
        }
        else
        {
            EnabledBubbleAction();
        }
    }

    void CheckTyping(Dialog_Base _dialog)// ��ȭ�� �������� üũ
    {
        currentDialog = _dialog;
        if (currentSpeech + 1 < dialogData.dialogInfos.Length)
        {
            currentSpeech++;// ���� ��ȭ
        }
        else
        {
            currentSpeech = -1;// ��ȭ �Ϸ�
        }
    }

    void EndDialog(Dialog_Base _dialog)
    {
        if (_dialog == null)
            return;

        instUIQueue.Enqueue(_dialog);
        DisabledBubbleAction();// ��ǳ�� ����
        _dialog.StopAllCoroutines();
        currentDialog = null;

        Debug.LogWarning("��ȭ �� --------------------------------" + dialogData.GetAfterType);
    }

    //==================================================================================================================================
    // �׼�
    //==================================================================================================================================
    Coroutine sizing, following;
    [SerializeField]
    private AnimationCurve animCurve;

    void EnabledBubbleAction()
    {
        if (sizing != null)
            StopCoroutine(sizing);

        SetDialogInfo setDialogInfo = dialogData.dialogInfos[currentSpeech];
        if (currentDialog != null && currentDialog.typing == true)
        {
            currentDialog.SkipDialog();// ��ŵ
        }
        else
        {
            if (currentDialog == null)
                currentDialog = TryDialog();
            currentDialog.SetDialog(setDialogInfo);
            switch (setDialogInfo.dialogType)
            {
                case SetDialogInfo.DialogType.Narration:
                    sizing = StartCoroutine(DialogActionB(true, currentDialog));
                    break;

                case SetDialogInfo.DialogType.Scream:
                case SetDialogInfo.DialogType.Bubble:
                    Transform target = GetTarget(setDialogInfo);
                    FollowTarget(target, currentDialog);// �ݱ�
                    sizing = StartCoroutine(DialogActionA(true, currentDialog));
                    break;
            }
        }
    }

    Dialog_Base TryDialog()
    {
        if (instUIQueue.Count > 0)
        {
            return instUIQueue.Dequeue();
        }
        else
        {
            Dialog_Base inst = Instantiate(dialogBase, transform);
            //inst.dele_CheckTyping = CheckTyping;
            return inst;
        }
    }

    Transform GetTarget(SetDialogInfo _setDialogInfo)
    {
        //switch (_setDialogInfo.focusTarget)
        //{
        //    case SetDialogInfo.FocusTargetType.Player:
        //        return GameManager.current.GetPlayer.GetInfomation.markerPoint;

        //    case SetDialogInfo.FocusTargetType.NPC:
        //        return Interact_Manger.current.GetInteractObject.GetMarkerPoint;
        //}
        return null;
    }

    void DisabledBubbleAction()
    {
        if (sizing != null)
            StopCoroutine(sizing);

        if (currentDialog != null)
        {
            switch (currentDialog.GetCurrentDialog.dialogType)
            {
                case SetDialogInfo.DialogType.Narration:
                    sizing = StartCoroutine(DialogActionB(false, currentDialog));
                    break;

                case SetDialogInfo.DialogType.Scream:
                case SetDialogInfo.DialogType.Bubble:
                    FollowTarget(null, currentDialog);// �ݱ�
                    sizing = StartCoroutine(DialogActionA(false, currentDialog));
                    break;
            }
        }
    }

    IEnumerator DialogActionA(bool _on, Dialog_Base _dialog)
    {
        _dialog.GetCurrentDialog.dialogCanvas.alpha = 1f;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.unscaledDeltaTime * 5f;
            float size = animCurve.Evaluate(_on == true ? normalize : 1f - normalize);
            _dialog.GetCurrentDialog.dialogCanvas.transform.localScale = Vector3.one * size;
            yield return null;
        }

        if (_on == false)//������ ��ǳ�� ���� ���� ����
            AfterDialog();
    }

    IEnumerator DialogActionB(bool _on, Dialog_Base _dialog)
    {
        float oldAlpha = _dialog.GetCurrentDialog.dialogCanvas.alpha;
        float targetAlpha = _on == true ? 1f : 0f;
        _dialog.GetCurrentDialog.dialogCanvas.transform.localScale = Vector3.one * 1f;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 5f;
            _dialog.GetCurrentDialog.dialogCanvas.alpha = Mathf.Lerp(oldAlpha, targetAlpha, normalize);
            yield return null;
        }

        if (_on == false)//������ ��ǳ�� ���� ���� ����
            AfterDialog();
    }

    void AfterDialog()
    {
        switch (dialogData.GetAfterType)
        {
            case Data_Dialog.AfterType.None:
                //Interact_Manger.current.ActionOver();// ��ȭ �Ϸ�
                //GameManager.current.SetHoldUnit = false;
                //UI_Manager.current.OpenMainUI(true);
                break;

            case Data_Dialog.AfterType.Options:
                selectionMenu.SetButton(dialogData.TryOptions);
                break;

            case Data_Dialog.AfterType.Shop:
                //UI_Manager.current.GetInventoryManager.OpenTrading(dialogData.itemSetting);
                break;

            case Data_Dialog.AfterType.Reward:
                int itemValue = dialogData.GetItemValue;
                Vector2Int boxSize = dialogData.boxSize;
                //Data_ItemSetting.SlotData[] slotDatas = dialogData.itemSetting.SetSlotDataArray(itemValue, boxSize.x * boxSize.y);
                //UI_Manager.current.GetInventoryManager.OpenLooting(slotDatas, boxSize);
                break;
        }
    }

    void FollowTarget(Transform _target, Dialog_Base _dialog)
    {
        if (following != null)
            StopCoroutine(following);
        following = StartCoroutine(FollowTarget(_target, _dialog.GetCurrentDialog.dialogCanvas.transform));
    }

    IEnumerator FollowTarget(Transform _target, Transform _dialog)
    {
        bool follow = _target != null;
        while (follow == true)
        {
            _dialog.position = WorldToScreenPoint(_target);
            yield return null;
            Debug.LogWarning(">> Follow Target");
        }
    }

    Vector2 WorldToScreenPoint(Transform _target)
    {
        Camera mainCam = Camera.main;
        Vector2 screenPosition = mainCam.WorldToScreenPoint(_target.position);
        return screenPosition;
    }
}
