using UnityEngine;
using UnityEngine.UI;

public class UI_Title : MonoBehaviour
{
    public UI_Option option;
    UI_Option instOption;

    public Button b_Start;

    void Start()
    {
        if (instOption != null)
            instOption = Instantiate(option, transform);

        b_Start.onClick.AddListener(StartButton);
    }

    void StartButton()
    {
        gameObject.SetActive(false);
    }
}
