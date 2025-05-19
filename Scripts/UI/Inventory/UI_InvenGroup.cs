using UnityEngine;
using UnityEngine.UI;

public class UI_InvenGroup : UI_OpenCanvas
{
    public ToggleGroup toggleGroup;
    public Toggle toggleUnit, toggleItem;
    public GameObject unitCanvas, itemCanvas;

    public override void SetCanvas()
    {
        base.SetCanvas();
        toggleGroup.IsActive();
        toggleUnit.onValueChanged.AddListener(delegate { ToggleInventory(0); });
        toggleItem.onValueChanged.AddListener(delegate { ToggleInventory(1); });
    }

    void ToggleInventory(int _code)
    {
        unitCanvas.gameObject.SetActive(toggleUnit.isOn);
        itemCanvas.gameObject.SetActive(toggleItem.isOn);
        switch (_code)
        {
            case 0:

                break;

            case 1:

                break;
        }
    }
}
