using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dialog_Test : MonoBehaviour
{
    public Data_Dialog dialog;
    public Dialog_Base @base;
    public Button button;

    private void Start()
    {
        button.onClick.AddListener(delegate { SetDialogData(dialog); });
    }

    public void SetDialogData(Data_Dialog _dialogData)
    {
        @base.SetDialog(_dialogData.dialogInfos[0]);
    }
}
