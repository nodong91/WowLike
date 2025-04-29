using UnityEngine;
using UnityEngine.UI;

public class UI_StartMenu : MonoBehaviour
{
    public Button[] UnitButton;
    public TMPro.TMP_Text unitName;

    void Start()
    {
        for (int i = 0; i < UnitButton.Length; i++)
        {
            int index = i;
            UnitButton[i].onClick.AddListener(delegate { SelectUnit(index); });
        }
    }

    void SelectUnit(int _index)
    {
        unitName.text = _index.ToString();
    }
}
