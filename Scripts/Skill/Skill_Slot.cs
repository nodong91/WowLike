using UnityEngine;
using UnityEngine.UI;

public class Skill_Slot : MonoBehaviour
{
    public delegate void Dele_Action(bool _action);
    public Dele_Action dele_Action;
    public delegate void Dele_SlotAction();
    public Dele_SlotAction dele_SlotAction;
    public Button button;

    void Start()
    {
        //button.onClick.AddListener(ActionButton);
    }

    public void ActionButton()
    {
        dele_Action(true);
    }
}
