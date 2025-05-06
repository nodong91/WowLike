using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using static UI_InvenSlot;
using static Data_Manager;
using static Unit_AI;

public class UI_InvenSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum ItemType
    {
        Empty,
        Item,
        Skill,
        Unit
    }
    public ItemType itemType;
    public enum SlotType
    {
        Inventory,
        Looting,
        Quick
    }
    public SlotType slotType;

    public Image icon;
    public delegate void OnDragHandler(Vector3 _position);
    public OnDragHandler onDrag;
    public delegate void OnSlotHandler(UI_InvenSlot _slot);
    public OnSlotHandler onBeginDrag;
    public OnSlotHandler onEndDrag;
    public OnSlotHandler onPointerEnter;

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                ClickLeft();
                break;

            case PointerEventData.InputButton.Right:
                ClickRight();
                break;

            case PointerEventData.InputButton.Middle:
                ClickMiddle();
                break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onBeginDrag?.Invoke(this);
            icon.gameObject.SetActive(false);
            itemIndex.gameObject.SetActive(false);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onDrag?.Invoke(eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onEndDrag?.Invoke(this);
            icon.gameObject.SetActive(icon.sprite != null);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerEnter?.Invoke(null);
    }




    public Data_Manager.ItemStruct itemStruct;
    public Data_Manager.SkillStruct skillStruct;
    public Data_Manager.UnitStruct unitStruct;

    public delegate void DelegateAction();
    public DelegateAction quickSlotAction;// Äü½½·Ô ¾×¼Ç¿ë

    public void SetSlot(SlotType _slotType)
    {
        slotType = _slotType;
        icon.material = Instantiate(icon.material);
    }

    public void SetQuickIndex(string _quick)
    {
        quickIndex.text = _quick;
        SetSlot(null);
        quickIndex.gameObject.SetActive(true);
    }

    public delegate void DeleClickAction(UI_InvenSlot _slot);
    public DeleClickAction deleClockAction;

    public void LootingItem(UI_InvenSlot _slot)
    {
        switch (_slot.itemType)
        {
            case ItemType.Item:
                SetSlot(_slot.itemStruct.ID);
                break;

            case ItemType.Skill:
                SetSlot(_slot.skillStruct.ID);
                break;

            case ItemType.Unit:
                SetSlot(_slot.unitStruct.ID);
                break;
        }
    }

    void ClickLeft()
    {
        quickSlotAction?.Invoke();
        deleClockAction?.Invoke(this);
        Debug.LogWarning(slotType);
    }

    void ClickRight()
    {

    }

    void ClickMiddle()
    {

    }

    public void ChangeSlot(UI_InvenSlot _enterSlot)
    {
        if (_enterSlot == null)
        {
            TakeSlot(this);// ¿ø·¡ ÀÚ¸®·Î µ¹¸²
            return;
        }

        //_slot ¹Ù²Ü ½½·Ô
        switch (_enterSlot.itemType)
        {
            case ItemType.Empty:
                _enterSlot.TakeSlot(this);
                SetSlot(null);
                break;

            case ItemType.Item:
                Data_Manager.ItemStruct itemSkill = _enterSlot.itemStruct;// ¹Ì¸® ÀúÀå
                _enterSlot.TakeSlot(this);
                SetSlot(itemSkill.ID);
                break;

            case ItemType.Skill:
                Data_Manager.SkillStruct slotSkill = _enterSlot.skillStruct;// ¹Ì¸® ÀúÀå
                _enterSlot.TakeSlot(this);// °¡Á®¿Â ½½·Ô ¹Ù²Þ
                SetSlot(slotSkill.ID);// ³» ½½·Ô ¹Ù²Þ
                break;

            case ItemType.Unit:
                Data_Manager.UnitStruct unitSlot = _enterSlot.unitStruct;
                _enterSlot.TakeSlot(this);// °¡Á®¿Â ½½·Ô ¹Ù²Þ
                SetSlot(unitSlot.ID);// ³» ½½·Ô ¹Ù²Þ
                break;
        }
    }

    void TakeSlot(UI_InvenSlot _dragSlot)
    {
        switch (_dragSlot.itemType)
        {
            case ItemType.Empty:
                SetSlot(null);
                break;

            case ItemType.Item:
                SetSlot(_dragSlot.itemStruct.ID);
                break;

            case ItemType.Skill:
                SetSlot(_dragSlot.skillStruct.ID);
                break;

            case ItemType.Unit:
                SetSlot(_dragSlot.unitStruct.ID);
                break;
        }
        //Game_Manager.instance?.checkDistance();// Äü½½·Ô ½ºÅ³ °Å¸® ÃøÁ¤¿ë
    }

    public void SetSlot(string _id)
    {
        quickIndex.gameObject.SetActive(slotType == SlotType.Quick);// Äü½½·Ô ¹øÈ£ È®ÀÎ

        if (_id == null)
        {
            itemType = ItemType.Empty;
            icon.sprite = null;

            icon.gameObject.SetActive(false);
            itemIndex.gameObject.SetActive(false);
            return;
        }

        icon.gameObject.SetActive(true);// ¾ÆÀÌÄÜ ÀÌ¹ÌÁö È°¼ºÈ­
        switch (_id[0].ToString().ToUpper())// ´ë¹®ÀÚ·Î º¯°æ
        {
            case "S":
                itemType = ItemType.Skill;
                skillStruct = Singleton_Data.INSTANCE.Dict_Skill[_id];
                icon.sprite = skillStruct.icon;
                itemIndex.gameObject.SetActive(false);
                break;

            case "T":
                itemType = ItemType.Item;
                itemStruct = Singleton_Data.INSTANCE.Dict_Item[_id];
                icon.sprite = itemStruct.itemIcon;
                itemIndex.gameObject.SetActive(true);
                break;

            case "U":
                itemType = ItemType.Unit;
                unitStruct = Singleton_Data.INSTANCE.Dict_Unit[_id];
                icon.sprite = unitStruct.unitIcon;
                itemIndex.gameObject.SetActive(false);
                break;
        }
    }


    bool inDist, cooling, isActive;
    public bool GetIsActive { get { return isActive; } }

    public void InDistance(bool _inDist)
    {
        inDist = _inDist;
        CheckActive();
    }

    public void CoolingSlot()
    {
        StartCoroutine(CoolingSkill());
    }

    IEnumerator CoolingSkill()
    {
        cooling = true;
        CheckActive();
        float coolingTime = 1f / skillStruct.coolingTime;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * coolingTime;
            SetCooling(normalize);
            yield return null;
        }
        cooling = false;
        CheckActive();
    }

    public void SetCooling(float _value)
    {
        icon.material.SetFloat("_FillAmount", _value);
    }

    public TMPro.TMP_Text itemIndex, quickIndex;
    public Color enabledColor, disabledColor;
    void CheckActive()
    {
        isActive = (inDist == true && cooling == false);
        quickIndex.color = isActive == true ? enabledColor : disabledColor;
    }
}
