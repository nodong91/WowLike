using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory : MonoBehaviour
{
    public Image dragIcon;
    public Transform slotParent;
    public UI_InvenSlot invenSlot;
    private UI_InvenSlot dragSlot, enterSlot;
    public Transform lootingParent, quickParent;
    public UI_InvenSlot[] lootingSlots, quickSlots;
    public UI_InvenSlot[] GetQuickSlot { get { return quickSlots; } }

    public void SetInventory()
    {
        List<string> skillIDs = new List<string>();
        List<string> itemIDs = new List<string>();

        foreach (var child in Singleton_Data.INSTANCE.Dict_Skill)
        {
            skillIDs.Add(child.Key);
        }
        foreach (var child in Singleton_Data.INSTANCE.Dict_Item)
        {
            itemIDs.Add(child.Key);
        }


        for (int i = 0; i < 25; i++)
        {
            int randomType = Random.Range(0, 3);
            UI_InvenSlot inst = Instantiate(invenSlot, slotParent);
            inst.SetSlot(UI_InvenSlot.SlotType.Inventory);
            switch (randomType)
            {
                case 0:
                    string id = skillIDs[Random.Range(0, skillIDs.Count)];
                    Data_Manager.SkillStruct skillSlot = Singleton_Data.INSTANCE.Dict_Skill[id];
                    inst.SetSkillSlot(skillSlot);
                    break;

                case 1:
                    id = itemIDs[Random.Range(0, itemIDs.Count)];
                    Data_Manager.ItemStruct itemSlot = Singleton_Data.INSTANCE.Dict_Item[id];
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
        SetQuickSlot();
        SetLooting();
    }

    public void OnBeginDrag(UI_InvenSlot _slot)
    {
        dragSlot = _slot.itemType == UI_InvenSlot.ItemType.Empty ? null : _slot;
        if (dragSlot != null)
        {
            dragIcon.sprite = _slot.icon.sprite;
            dragIcon.gameObject.SetActive(true);
        }
    }

    private void OnDrag(Vector3 _position)
    {
        if (dragSlot == null)
            return;

        dragIcon.transform.position = _position;
    }

    public void OnEndDrag(UI_InvenSlot _slot)
    {
        if (dragSlot == null)
        {
            return;
        }
        dragSlot.ChangeSlot(enterSlot);
        dragIcon.gameObject.SetActive(false);
    }

    public void OnEndDrag_Quick(UI_InvenSlot _slot)
    {
        if (dragSlot == null)
        {
            return;
        }
        dragSlot.ChangeSlot(enterSlot);
        dragIcon.gameObject.SetActive(false);
    }

    public void OnPointerEnter(UI_InvenSlot _slot)
    {
        enterSlot = _slot;
    }





    void SetLooting()
    {
        int lootingAmount = 16;
        lootingSlots = new UI_InvenSlot[lootingAmount];
        for (int i = 0; i < lootingAmount; i++)
        {
            UI_InvenSlot inst = Instantiate(invenSlot, lootingParent);
            inst.SetSlot(UI_InvenSlot.SlotType.Looting);
            inst.SetEmptySlot();
            inst.onBeginDrag += OnBeginDrag;
            inst.onDrag += OnDrag;
            inst.onEndDrag += OnEndDrag;
            inst.onPointerEnter += OnPointerEnter;

            lootingSlots[i] = inst;
        }
        AddLooting(null);
    }

    public void AddLooting(string[] _ids)
    {
        if (_ids != null)
        {
            lootingParent.gameObject.SetActive(true);
            for (int i = 0; i < lootingSlots.Length; i++)
            {
                if (i < _ids.Length)
                {
                    string id = _ids[i];
                    switch (id[0].ToString().ToLower())
                    {
                        case "s":
                            Data_Manager.SkillStruct skillSlot = Singleton_Data.INSTANCE.Dict_Skill[id];
                            lootingSlots[i].SetSkillSlot(skillSlot);
                            break;

                        case "t":
                            Data_Manager.ItemStruct itemSlot = Singleton_Data.INSTANCE.Dict_Item[id];
                            lootingSlots[i].SetItemSlot(itemSlot);
                            break;
                    }
                }
                else
                {
                    lootingSlots[i].SetEmptySlot();
                }
            }
        }
        else
        {
            lootingParent.gameObject.SetActive(false);
        }
    }

    void SetQuickSlot()
    {
        int quickAmount = 4;
        quickSlots = new UI_InvenSlot[quickAmount];
        for (int i = 0; i < quickAmount; i++)
        {
            UI_InvenSlot inst = Instantiate(invenSlot, quickParent);
            inst.SetSlot(UI_InvenSlot.SlotType.Quick);
            inst.SetQuickIndex((i + 1).ToString());

            inst.onBeginDrag += OnBeginDrag;
            inst.onDrag += OnDrag;
            inst.onEndDrag += OnEndDrag_Quick;
            inst.onPointerEnter += OnPointerEnter;

            int index = i;
            //inst.deleGateAction = delegate { _action(index); };
            inst.deleGateAction = delegate { Game_Manager.instance.InputSlot(index); };
            quickSlots[index] = inst;
        }
    }
}
