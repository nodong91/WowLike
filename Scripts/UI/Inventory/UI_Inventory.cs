using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory : MonoBehaviour
{
    public Image dragIcon;
    public UI_InvenGroup inventoryParent;
    public UI_OpenCanvas lootingParent, quickParent;
    public UI_InvenSlot baseSlot;
    private UI_InvenSlot dragSlot, enterSlot;
    public UI_InvenSlot GetDragSlot { get { return dragSlot; } }
    public UI_InvenSlot[] invenSlots, lootingSlots, quickSlots;
    public UI_InvenSlot[] GetQuickSlot { get { return quickSlots; } }
    public int inventoryAmount = 25;

    public UI_SlotInfo slotInfo;

    public Vector2Int inventorySize;
    //public UI_InvenSlot[,] inventorySlots;
    //public Dictionary<Vector2Int, UI_InvenSlot> inventorySlots = new Dictionary<Vector2Int, UI_InvenSlot>();

    public UI_InvenSlot[,] inventoryUnits, inventoryItems;

    public void SetInventory()
    {
        inventoryParent.SetCanvas();
        lootingParent.SetCanvas();
        quickParent.SetCanvas();

        InventorySetting();
        SetQuickSlot();
        SetLooting();

        TestSetting();
    }

    void InventorySetting()
    {
        inventoryUnits = new UI_InvenSlot[inventorySize.x, inventorySize.y];
        inventoryItems = new UI_InvenSlot[inventorySize.x, inventorySize.y];
        for (int x = 0; x < inventorySize.x; x++)
        {
            for (int y = 0; y < inventorySize.y; y++)
            {
                UI_InvenSlot unit = SetInventorySlot(x, y, inventoryParent.unitCanvas.transform);
                inventoryUnits[x, y] = unit;
                UI_InvenSlot item = SetInventorySlot(x, y, inventoryParent.itemCanvas.transform);
                inventoryItems[x, y] = item;
            }
        }
    }

    UI_InvenSlot SetInventorySlot(int _x, int _y, Transform _parent)
    {
        UI_InvenSlot inst = Instantiate(baseSlot, _parent);
        inst.SetSlot(UI_InvenSlot.SlotType.Inventory);
        inst.onBeginDrag += OnBeginDrag;
        inst.onDrag += OnDrag;
        inst.onEndDrag += OnEndDrag;
        inst.onPointerEnter += OnPointerEnter;
        inst.deleClockAction += SlotClick;
        inst.onCheck += CheckItemInventory;
        inst.SetSlot(null);

        Vector2Int inventoryNum = new Vector2Int(_x, _y);
        inst.name = inventoryNum.ToString();
        inst.InventoryNum = inventoryNum;
        return inst;
    }

    void TestSetting()
    {
        // 테스트 세팅
        List<string> IDS = new List<string>();
        foreach (var child in Singleton_Data.INSTANCE.Dict_Item)
        {
            IDS.Add(child.Key);
        }
        foreach (var child in Singleton_Data.INSTANCE.Dict_Unit)
        {
            IDS.Add(child.Key);
        }
        for (int i = 0; i < 20; i++)
        {
            string id = IDS[Random.Range(0, IDS.Count)];
            AddInventory(id);
        }
        CheckItemInventory();
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
                if (AddInventory(_slot.structID) == false)
                {
                    Debug.LogWarning("빈 슬롯이 없습니다.");
                    return;
                }
                //UI_InvenSlot emptySlot = TryEmptySlot();
                //if (emptySlot == null)
                //{
                //    Debug.LogWarning("빈 슬롯이 없습니다.");
                //    return;
                //}

                //emptySlot.LootingItem(_slot);
                //Debug.LogWarning(_slot.itemType);
                _slot.SetSlot(null);// 기존 슬롯 비우기
                break;

            case UI_InvenSlot.SlotType.Quick:
                // 사용
                break;
        }
    }

    public void OnBeginDrag(UI_InvenSlot _slot)
    {
        dragSlot = _slot.itemType == ItemType.Empty ? null : _slot;
        if (dragSlot != null)
        {
            dragIcon.sprite = _slot.icon.sprite;
            dragIcon.gameObject.SetActive(true);
        }
        Camera_Manager.instance.enabled = false;
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
        Camera_Manager.instance.enabled = true;
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
        if (_slot == null)
        {
            // 빠져 나올 때
            CheckSynergy(enterSlot, false);// 시너지 테스트
            slotInfo.OnInfomation(null);
            return;
        }

        enterSlot = _slot;
        if (dragSlot != null)
            return;

        CheckSynergy(_slot, true);// 시너지 테스트
    }











    void SetLooting()
    {
        int lootingAmount = 16;
        lootingSlots = new UI_InvenSlot[lootingAmount];
        for (int i = 0; i < lootingAmount; i++)
        {
            UI_InvenSlot inst = Instantiate(baseSlot, lootingParent.transform);
            inst.SetSlot(UI_InvenSlot.SlotType.Looting);
            inst.SetSlot(null);
            inst.onBeginDrag += OnBeginDrag;
            inst.onDrag += OnDrag;
            inst.onEndDrag += OnEndDrag;
            inst.onPointerEnter += OnPointerEnter;
            inst.deleClockAction += SlotClick;

            lootingSlots[i] = inst;
        }
        AddLooting(null);
    }

    public void AddLooting(List<string> _ids)// 루팅 창 세팅
    {
        if (_ids != null)
        {
            lootingParent.OpenCanvas();
            for (int i = 0; i < lootingSlots.Length; i++)
            {
                if (i < _ids.Count)
                {
                    string id = _ids[i];
                    lootingSlots[i].SetSlot(id);
                }
                else
                {
                    lootingSlots[i].SetSlot(null);
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
            //inst.quickSlotAction = delegate { Game_Manager.instance?.InputSlot(index); };
            quickSlots[index] = inst;
        }
        OpenQuick();
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













    // 시너지 체크
    void CheckSynergy(UI_InvenSlot _slot, bool _enter)
    {
        if (_slot == null || _slot.synergySlots == null)
            return;

        _slot.SynergySelect(_enter);
        Debug.LogWarning(_slot.synergySlots.Length);
    }









    public bool AddInventory(string _id)
    {
        if (_id.Contains('U'))
        {
            UI_InvenSlot slot = TryEmptyInventorySlot(inventoryUnits);
            if (slot == null)
                return false;
            slot.SetSlot(_id);
        }
        else if (_id.Contains('T'))
        {
            UI_InvenSlot slot = TryEmptyInventorySlot(inventoryItems);
            if (slot == null)
                return false;
            slot.SetSlot(_id);
        }
        return true;
    }

    UI_InvenSlot TryEmptyInventorySlot(UI_InvenSlot[,] _inventory)
    {
        foreach (var slot in _inventory)
        {
            if (slot.itemType == ItemType.Empty)
                return slot;
        }
        return null;
    }

    public void CheckItemInventory()
    {
        foreach (UI_InvenSlot child in inventoryItems)
        {
            List<UI_InvenSlot> addSynergy = new List<UI_InvenSlot>();
            Vector2Int slotNum = child.InventoryNum;
            if (child.synergySlots == null)// 빈슬롯
                continue;

            for (int i = 0; i < child.synergySlots.Length; i++)// 에어리어 안의 슬롯
            {
                int synergyX = slotNum.x + (int)child.synergySlots[i].x;
                int synergyY = slotNum.y + (int)child.synergySlots[i].y;

                if (synergyX < 0 || synergyY < 0 || synergyX >= inventorySize.x || synergyY >= inventorySize.y)
                    continue;

                //Debug.LogWarning($"클릭 : {slotNum} >> 시너지 슬롯 : {temp.InventoryNum}");
                UI_InvenSlot synergySlot = inventoryItems[synergyX, synergyY];
                addSynergy.Add(synergySlot);
            }
            child.AddSynergy(addSynergy);
        }
    }
}
