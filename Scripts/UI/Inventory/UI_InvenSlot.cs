using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UI_InvenSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemType itemType;
    public enum SlotType
    {
        Inventory,
        Looting,
        Quick
    }
    public SlotType slotType;

    public Image icon, selected;
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












    public string structID;
    public Data_Manager.ItemStruct itemStruct;
    public Data_Manager.SkillStruct skillStruct;
    public Data_Manager.UnitStruct unitStruct;

    public delegate void DelegateAction();
    public DelegateAction quickSlotAction;// 퀵슬롯 액션용

    public delegate void DeleClickAction(UI_InvenSlot _slot);
    public DeleClickAction deleClockAction;

    public TMPro.TMP_Text itemIndex, quickIndex;
    public Color enabledColor, disabledColor;


    bool inDist, cooling, isActive;
    public bool GetIsActive { get { return isActive; } }

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

    public void LootingItem(string _id)
    {
        structID = _id;
        if (structID.Contains('U'))
        {

        }
        else if (structID.Contains('T'))
        {

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

    // 슬롯 아이템 변경
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
                SetSlot(null);
                break;

            case ItemType.Item:
                Data_Manager.ItemStruct itemSkill = _enterSlot.itemStruct;// 미리 저장
                _enterSlot.TakeSlot(this);
                SetSlot(itemSkill.ID);
                break;

            case ItemType.Skill:
                Data_Manager.SkillStruct slotSkill = _enterSlot.skillStruct;// 미리 저장
                _enterSlot.TakeSlot(this);// 가져온 슬롯 바꿈
                SetSlot(slotSkill.ID);// 내 슬롯 바꿈
                break;

            case ItemType.Unit:
                Data_Manager.UnitStruct unitSlot = _enterSlot.unitStruct;
                _enterSlot.TakeSlot(this);// 가져온 슬롯 바꿈
                SetSlot(unitSlot.ID);// 내 슬롯 바꿈
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
        //Game_Manager.instance?.checkDistance();// 퀵슬롯 스킬 거리 측정용
    }

    public void SetSlot(string _id)
    {
        quickIndex.gameObject.SetActive(slotType == SlotType.Quick);// 퀵슬롯 번호 출력
        if (_id == null)
        {
            itemType = ItemType.Empty;
            icon.sprite = null;

            icon.gameObject.SetActive(false);
            itemIndex.gameObject.SetActive(false);
            selected.gameObject.SetActive(false);
            return;
        }

        icon.gameObject.SetActive(true);// 아이콘 이미지 활성화
        switch (_id[0].ToString().ToUpper())// 첫번째 글자 대문자로 변경
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

    void CheckActive()
    {
        isActive = (inDist == true && cooling == false);
        quickIndex.color = isActive == true ? enabledColor : disabledColor;
    }













    private Vector2Int inventoryNum;
    public Vector2Int InventoryNum { get => inventoryNum; set => inventoryNum = value; }
    public List<UI_InvenSlot> addSynergy = new List<UI_InvenSlot>();
    public int empty, item, skill, unit;

    // 유닛 옆에 아이템 혹은 스킬을 놓으면 해당 아이템의 시너지 영역 활성화
    // 시너지 영역에 다른 아이템이 들어가면 다른 아이템의 시너지 영역 활성화
    public void AddSynergy(List<UI_InvenSlot> _addSynergy)
    {
        empty = 0; item = 0; skill = 0; unit = 0;
        addSynergy = _addSynergy;
        for (int i = 0; i < addSynergy.Count; i++)// 에어리어 안의 슬롯
        {
            switch (itemType)
            {
                case ItemType.Empty:

                    break;

                case ItemType.Item:
                    ItemTypeSlot(addSynergy[i]);
                    break;

                case ItemType.Skill:
                    SkillTypeSlot(addSynergy[i]);
                    break;

                case ItemType.Unit:
                    UnitTypeSlot(addSynergy[i]);
                    break;
            }
        }
    }

    void ItemTypeSlot(UI_InvenSlot _slot)
    {
        if (itemStruct.synergyType.itemType != _slot.itemType)// 시너지 타입과 같지 않으면 넘김
            return;

        switch (_slot.itemType)
        {
            case ItemType.Empty:
                // 빈칸도 시너지에 영향을 줄수도 있다.
                empty++;
                break;

            case ItemType.Item:
                item++;
                break;

            case ItemType.Skill:
                // 영역 안의 타입이 '공격형' '스킬'인 경우 아이템 공격성능 개수만큼 성능 향상
                skill++;
                if (_slot.skillStruct.skillType == itemStruct.synergyType.skillType)// 같은 스킬타입
                {

                }
                break;

            case ItemType.Unit:
                unit++;
                break;
        }
        //}
    }

    void SkillTypeSlot(UI_InvenSlot _slot)
    {
        // 영역 안의 타입이 '공격형' '스킬'인 경우 아이템 공격성능 개수만큼 성능 향상
        switch (_slot.itemType)
        {
            case ItemType.Empty:
                // 빈칸도 시너지에 영향을 줄수도 있다.
                empty++;
                break;

            case ItemType.Item:
                item++;
                break;

            case ItemType.Skill:
                skill++;
                break;

            case ItemType.Unit:
                unit++;
                break;
        }
    }

    void UnitTypeSlot(UI_InvenSlot _slot)// 자신이 유닛인 경우 영역 안의 아이템과 스킬의 능력을 받음
    {
        switch (_slot.itemType)
        {
            case ItemType.Empty:
                // 빈칸도 시너지에 영향을 줄수도 있다.
                empty++;
                break;

            case ItemType.Item:
                item++;
                break;

            case ItemType.Skill:
                skill++;
                break;

            case ItemType.Unit:
                unit++;
                break;
        }
    }

    public void SynergySelect(bool _active)
    {
        for (int i = 0; i < addSynergy.Count; i++)// 에어리어 안의 슬롯
        {
            addSynergy[i].icon.material.SetFloat("_FillAmount", _active ? 1f : 0f);
            addSynergy[i].selected.gameObject.SetActive(_active);
        }
    }

    // 하스스톤 (전장의 함성, 죽음의 메아리 등등)
    // 전장의 함성 - 전투 배치 시 효과 (주변 아군 언데드 공격력 향상)
    // 죽음의 메아리 - 사망 시 효과 (3초뒤 폭발, 모든 아군 체력 회복, 사망 시 1골드)
    // 같은 유닛 3마리면 진화
    // 아이템은 배치된 모든 유닛에 효과 (언데드 사망 시 1회 부활, 야수 공격력 향상, 죽음의 메아리 효과 2회 발동)
    // 아이템 배치로 아이템 효과 상승
    // 스킬은 몬스터에 고정 (인벤토리에서 제거) 인벤토리는 유닛 인벤토리와 아이템 인벤토리가 따로 
    // 이벤트 지형 - 미니게임으로 보상
}
