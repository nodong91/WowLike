using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using static UI_InvenSlot;

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
    public DelegateAction quickSlotAction;

    public void SetSlot(SlotType _slotType)
    {
        slotType = _slotType;
        icon.material = Instantiate(icon.material);
    }

    public void SetQuickIndex(string _quick)
    {
        quickIndex.text = _quick;
        SetEmptySlot();
        quickIndex.gameObject.SetActive(true);
    }

    public delegate void DeleLooting(UI_InvenSlot _slot);
    public DeleLooting deleLooting;

    public void LootingItem(UI_InvenSlot _slot)
    {
        switch (_slot.itemType)
        {
            case ItemType.Item:
                SetItemSlot(_slot.itemStruct);
                break;

            case ItemType.Skill:
                SetSkillSlot(_slot.skillStruct);
                break;

            case ItemType.Unit:
                SetUnitSlot(_slot.unitStruct);
                break;
        }
    }

    void ClickLeft()
    {
        quickSlotAction?.Invoke();
        deleLooting?.Invoke(this);
        //switch (slotType)
        //{
        //    case SlotType.Inventory:
        //        // 정보 보여주기
        //        break;

        //    case SlotType.Looting:
        //        // 루팅
        //        deleLooting?.Invoke(this);
        //        //SetEmptySlot();
        //        break;

        //    case SlotType.Quick:
        //        // 사용
        //        break;
        //}
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
            TakeSlot(this);// 원래 자리로 돌림
            return;
        }

        //_slot 바꿀 슬롯
        switch (_enterSlot.itemType)
        {
            case ItemType.Empty:
                _enterSlot.TakeSlot(this);
                SetEmptySlot();
                break;

            case ItemType.Item:
                Data_Manager.ItemStruct itemSkill = _enterSlot.itemStruct;// 미리 저장
                _enterSlot.TakeSlot(this);
                SetItemSlot(itemSkill);
                break;

            case ItemType.Skill:
                Data_Manager.SkillStruct slotSkill = _enterSlot.skillStruct;// 미리 저장
                _enterSlot.TakeSlot(this);// 가져온 슬롯 바꿈
                SetSkillSlot(slotSkill);// 내 슬롯 바꿈
                break;

            case ItemType.Unit:
                Data_Manager.UnitStruct unitSlot = _enterSlot.unitStruct;
                _enterSlot.TakeSlot(this);// 가져온 슬롯 바꿈
                SetUnitSlot(unitSlot);// 내 슬롯 바꿈
                break;
        }
    }

    void TakeSlot(UI_InvenSlot _dragSlot)
    {
        switch (_dragSlot.itemType)
        {
            case ItemType.Empty:
                SetEmptySlot();
                break;

            case ItemType.Item:
                SetItemSlot(_dragSlot.itemStruct);
                break;

            case ItemType.Skill:
                SetSkillSlot(_dragSlot.skillStruct);
                break;

            case ItemType.Unit:
                SetUnitSlot(_dragSlot.unitStruct);
                break;
        }
        Game_Manager.instance?.checkDistance();// 퀵슬롯 스킬 거리 측정용
    }

    public void SetEmptySlot()
    {
        itemType = ItemType.Empty;
        icon.sprite = null;

        icon.gameObject.SetActive(false);
        itemIndex.gameObject.SetActive(false);
        quickIndex.gameObject.SetActive(slotType == SlotType.Quick);
    }

    public void SetSkillSlot(Data_Manager.SkillStruct _skillStruct)
    {
        itemType = ItemType.Skill;
        skillStruct = _skillStruct;
        icon.sprite = _skillStruct.icon;

        icon.gameObject.SetActive(true);
        itemIndex.gameObject.SetActive(false);
        quickIndex.gameObject.SetActive(slotType == SlotType.Quick);
    }

    public void SetItemSlot(Data_Manager.ItemStruct _itemStruct)
    {
        itemType = ItemType.Item;
        itemStruct = _itemStruct;
        icon.sprite = _itemStruct.itemIcon;

        icon.gameObject.SetActive(true);
        itemIndex.gameObject.SetActive(true);
        quickIndex.gameObject.SetActive(slotType == SlotType.Quick);
    }

    public void SetUnitSlot(Data_Manager.UnitStruct _unitStruct)
    {
        itemType = ItemType.Unit;
        unitStruct = _unitStruct;
        icon.sprite = _unitStruct.unitIcon;

        icon.gameObject.SetActive(true);
        itemIndex.gameObject.SetActive(false);
        quickIndex.gameObject.SetActive(slotType == SlotType.Quick);
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
