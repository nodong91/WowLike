using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog_SelectionMenu : MonoBehaviour
{
    public GridLayoutGroup parent;
    public Button selectionButton;
    Queue<InstButton> instButtons = new Queue<InstButton>();
    List<InstButton> menuButtonList = new List<InstButton>();
    [System.Serializable]
    public struct InstButton
    {
        public Button button;
        public TMPro.TMP_Text text;
        public InstButton(Button _button)
        {
            button = _button;
            text = _button.GetComponentInChildren<TMPro.TMP_Text>();
        }
        public void SetTitle(string _name)
        {
            text.text = _name;
        }
    }

    public void SetButton(List<Data_Dialog.OptionTitle> _dialogOptions)
    {
        for (int i = 0; i < _dialogOptions.Count; i++)
        {
            InstButton menu = TryInstButton();
            Data_Dialog dd = _dialogOptions[i].dialogData;
            menu.button.onClick.AddListener(delegate { SelectedButton(dd); });
            string title = _dialogOptions[i].title;// 선택창에 출력 되는 타이틀
            menu.SetTitle(title);
            menuButtonList.Add(menu);
        }
    }

    InstButton TryInstButton()
    {
        if (instButtons.Count > 0)
        {
            InstButton inst = instButtons.Dequeue();
            inst.button.gameObject.SetActive(true);
            return inst;
        }
        else
        {
            Button button = Instantiate(selectionButton, parent.transform);// 생성
            InstButton inst = new InstButton(button);
            return inst;
        }
    }

    IEnumerator HideMenu()
    {
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < menuButtonList.Count; i++)
        {
            menuButtonList[i].button.gameObject.SetActive(false);
            instButtons.Enqueue(menuButtonList[i]);
        }
        menuButtonList.Clear();
    }

    void SelectedButton(Data_Dialog dd)
    {
        //UI_Manager.current.GetDialogManager.SetDialogData(dd);
        Debug.LogWarning(" -> " + dd.name);

        StartCoroutine(HideMenu());
    }
}
