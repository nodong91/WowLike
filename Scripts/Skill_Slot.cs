using UnityEngine;
using UnityEngine.UI;

public class Skill_Slot : MonoBehaviour
{
    public delegate void Dele_Action(bool _action);
    public Dele_Action dele_Action;
    public Button button;

    void Start()
    {
        button.onClick.AddListener(ActionButton);
    }

    void ActionButton()
    {
        dele_Action(true);
    }
}
