using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory : MonoBehaviour
{
    public Image dragIcon;
    public Transform slotParent;
    public UI_InvenSlot invenSlot;

    public Data_Manager.SkillStruct[] skillStruct;
    public Data_Manager.ItemStruct[] itemStruct;

    public void SetInventory()
    {
        for (int i = 0; i < 25; i++)
        {
            int randomType = Random.Range(0, 3);
            UI_InvenSlot inst = Instantiate(invenSlot, slotParent);
            switch (randomType)
            {
                case 0:
                    int index = Random.Range(0, skillStruct.Length);
                    Data_Manager.SkillStruct skillSlot = skillStruct[index];
                    inst.SetSkillSlot(skillSlot);
                    break;

                case 1:
                    index = Random.Range(0, itemStruct.Length);
                    Data_Manager.ItemStruct itemSlot = itemStruct[index];
                    inst.SetItemSlot(itemSlot);
                    break;

                case 2:
                    inst.SetEmptySlot();
                    break;
            }
            inst.onBeginDrag += OnBeginDrag;
            inst.onDrag += OnDrag;
            inst.onEndDrag += OnEndDrag;
            inst.onPointerEnter += OnPointerEnter;
        }
    }

    public UI_InvenSlot dragSlot, enterSlot;
    public void OnBeginDrag(UI_InvenSlot _slot)
    {
        dragSlot = _slot;

        if (_slot.slotType != UI_InvenSlot.SlotType.Empty)
        {
            dragIcon.sprite = _slot.icon.sprite;
            dragIcon.gameObject.SetActive(true);
        }
    }

    private void OnDrag(Vector3 _position)
    {
        if (dragSlot.slotType == UI_InvenSlot.SlotType.Empty)
            return;

        dragIcon.transform.position = _position;
    }

    public void OnEndDrag(UI_InvenSlot _slot)
    {
        if (dragSlot.slotType != UI_InvenSlot.SlotType.Empty)
        {
            dragSlot.ChangeSlot(enterSlot);
        }
        dragIcon.gameObject.SetActive(false);
    }

    public void OnPointerEnter(UI_InvenSlot _slot)
    {
        enterSlot = _slot;
    }





    public Transform quickParent;
    public void SetQuickSlot(UI_Manager.DelegateAction _action)
    {
        for (int i = 0; i < 4; i++)
        {
            UI_InvenSlot inst = Instantiate(invenSlot, quickParent);

            inst.onBeginDrag += OnBeginDrag;
            inst.onDrag += OnDrag;
            inst.onEndDrag += OnEndDrag;
            inst.onPointerEnter += OnPointerEnter;

            int index = i;
            inst.deleGateAction = delegate { _action(index); };
            //inst.SetQuickSlot(null);
        }
    }
}
