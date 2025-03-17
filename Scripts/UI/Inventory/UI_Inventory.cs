using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory : MonoBehaviour
{
    public Image dragIcon;
    public UI_OpenCanvas inventoryParent, lootingParent, quickParent;
    public UI_InvenSlot baseSlot;
    private UI_InvenSlot dragSlot, enterSlot;
    public UI_InvenSlot GetDragSlot { get { return dragSlot; } }
    public UI_InvenSlot[] invenSlots, lootingSlots, quickSlots;
    public UI_InvenSlot[] GetQuickSlot { get { return quickSlots; } }
    public int inventoryAmount = 25;

    public UI_SlotInfo slotInfo;

    public void SetInventory()
    {
        inventoryParent.SetCanvas();
        lootingParent.SetCanvas();
        quickParent.SetCanvas();

        List<string> skillIDs = new List<string>();
        List<string> itemIDs = new List<string>();
        List<string> unitIDs = new List<string>();

        foreach (var child in Singleton_Data.INSTANCE.Dict_Skill)
        {
            skillIDs.Add(child.Key);
        }
        foreach (var child in Singleton_Data.INSTANCE.Dict_Item)
        {
            itemIDs.Add(child.Key);
        }
        foreach (var child in Singleton_Data.INSTANCE.Dict_Unit)
        {
            unitIDs.Add(child.Key);
        }
        invenSlots = new UI_InvenSlot[inventoryAmount];
        for (int i = 0; i < invenSlots.Length; i++)
        {
            UI_InvenSlot inst = Instantiate(baseSlot, inventoryParent.transform);
            inst.SetSlot(UI_InvenSlot.SlotType.Inventory);
            inst.onBeginDrag += OnBeginDrag;
            inst.onDrag += OnDrag;
            inst.onEndDrag += OnEndDrag;
            inst.onPointerEnter += OnPointerEnter;
            inst.deleClockAction += SlotClick;
            invenSlots[i] = inst;

            // 테스트 세팅
            int randomType = Random.Range(0, 4);
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
                    id = unitIDs[Random.Range(0, unitIDs.Count)];
                    Data_Manager.UnitStruct unitSlot = Singleton_Data.INSTANCE.Dict_Unit[id];
                    inst.SetUnitSlot(unitSlot);
                    break;

                case 3:
                    inst.SetEmptySlot();
                    break;
            }
        }
        SetQuickSlot();
        SetLooting();
    }

    void SlotClick(UI_InvenSlot _slot)
    {
        switch (_slot.slotType)
        {
            case UI_InvenSlot.SlotType.Inventory:
                // 정보 보여주기
                slotInfo.OnInfomation(_slot);
                break;

            case UI_InvenSlot.SlotType.Looting:
                // 루팅
                UI_InvenSlot emptySlot = TryEmptySlot();
                if (emptySlot == null)
                {
                    Debug.LogWarning("빈 슬롯이 없습니다.");
                    return;
                }

                emptySlot.LootingItem(_slot);
                Debug.LogWarning(_slot.itemType);
                _slot.SetEmptySlot();// 기존 슬롯 비우기
                break;

            case UI_InvenSlot.SlotType.Quick:
                // 사용
                break;
        }
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

        slotInfo.OnInfomation(null);
        dragIcon.transform.position = _position;
    }

    public void OnEndDrag(UI_InvenSlot _slot)
    {
        if (dragSlot == null)
        {
            return;
        }
        dragSlot.ChangeSlot(enterSlot);
        dragSlot = null;
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
        if (dragSlot != null)
            return;

        // 정보 보여주기
        slotInfo.OnInfomation(_slot);
    }

    public UI_InvenSlot TryEmptySlot()
    {
        for (int i = 0; i < invenSlots.Length; i++)
        {
            UI_InvenSlot slot = invenSlots[i];
            if (slot.itemType == UI_InvenSlot.ItemType.Empty)
            {
                return slot;
            }
        }
        return null;
    }










    void SetLooting()
    {
        int lootingAmount = 16;
        lootingSlots = new UI_InvenSlot[lootingAmount];
        for (int i = 0; i < lootingAmount; i++)
        {
            UI_InvenSlot inst = Instantiate(baseSlot, lootingParent.transform);
            inst.SetSlot(UI_InvenSlot.SlotType.Looting);
            inst.SetEmptySlot();
            inst.onBeginDrag += OnBeginDrag;
            inst.onDrag += OnDrag;
            inst.onEndDrag += OnEndDrag;
            inst.onPointerEnter += OnPointerEnter;
            inst.deleClockAction += SlotClick;

            lootingSlots[i] = inst;
        }
        AddLooting(null);
    }

    public void AddLooting(string[] _ids)
    {
        if (_ids != null)
        {
            lootingParent.OpenCanvas();
            for (int i = 0; i < lootingSlots.Length; i++)
            {
                if (i < _ids.Length)
                {
                    string id = _ids[i];
                    switch (id[0].ToString().ToUpper())// 소문자로 변경
                    {
                        case "S":
                            Data_Manager.SkillStruct skillSlot = Singleton_Data.INSTANCE.Dict_Skill[id];
                            lootingSlots[i].SetSkillSlot(skillSlot);
                            break;

                        case "T":
                            Data_Manager.ItemStruct itemSlot = Singleton_Data.INSTANCE.Dict_Item[id];
                            lootingSlots[i].SetItemSlot(itemSlot);
                            break;

                        case "U":
                            Data_Manager.UnitStruct unitSlot = Singleton_Data.INSTANCE.Dict_Unit[id];
                            lootingSlots[i].SetUnitSlot(unitSlot);
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
            lootingParent.CloseCanvas();
        }
    }

    void SetQuickSlot()
    {
        int quickAmount = 4;
        quickSlots = new UI_InvenSlot[quickAmount];
        for (int i = 0; i < quickAmount; i++)
        {
            UI_InvenSlot inst = Instantiate(baseSlot, quickParent.transform);
            inst.SetSlot(UI_InvenSlot.SlotType.Quick);
            inst.SetQuickIndex((i + 1).ToString());

            inst.onBeginDrag += OnBeginDrag;
            inst.onDrag += OnDrag;
            inst.onEndDrag += OnEndDrag_Quick;
            inst.onPointerEnter += OnPointerEnter;
            inst.deleClockAction += SlotClick;

            int index = i;
            inst.quickSlotAction = delegate { Game_Manager.instance?.InputSlot(index); };
            quickSlots[index] = inst;
        }
    }













    public void OpenAllCanvas()
    {
        inventoryParent.OpenCanvas();
        lootingParent.OpenCanvas();
        quickParent.OpenCanvas();
    }

    public void CloseAllCanvas()
    {
        inventoryParent.CloseCanvas();
        lootingParent.CloseCanvas();
        quickParent.CloseCanvas();
    }

    public void OpenInventory()
    {
        inventoryParent.TryOpenCanvas();
    }

    public void OpenLooting()
    {
        lootingParent.TryOpenCanvas();
    }

    public void OpenQuick()
    {
        quickParent.TryOpenCanvas();
    }
}
