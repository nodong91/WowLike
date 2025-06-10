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
    public DelegateAction quickSlotAction;// ������ �׼ǿ�

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

    // ���� ������ ����
    public void ChangeSlot(UI_InvenSlot _enterSlot)
    {
        if (_enterSlot == null)
        {
            TakeSlot(this);// ���� �ڸ��� ����
            return;
        }

        //_slot �ٲ� ����
        switch (_enterSlot.itemType)
        {
            case ItemType.Empty:
                _enterSlot.TakeSlot(this);
                SetSlot(null);
                break;

            case ItemType.Item:
                Data_Manager.ItemStruct itemSkill = _enterSlot.itemStruct;// �̸� ����
                _enterSlot.TakeSlot(this);
                SetSlot(itemSkill.ID);
                break;

            case ItemType.Skill:
                Data_Manager.SkillStruct slotSkill = _enterSlot.skillStruct;// �̸� ����
                _enterSlot.TakeSlot(this);// ������ ���� �ٲ�
                SetSlot(slotSkill.ID);// �� ���� �ٲ�
                break;

            case ItemType.Unit:
                Data_Manager.UnitStruct unitSlot = _enterSlot.unitStruct;
                _enterSlot.TakeSlot(this);// ������ ���� �ٲ�
                SetSlot(unitSlot.ID);// �� ���� �ٲ�
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
        //Game_Manager.instance?.checkDistance();// ������ ��ų �Ÿ� ������
    }

    public void SetSlot(string _id)
    {
        quickIndex.gameObject.SetActive(slotType == SlotType.Quick);// ������ ��ȣ ���
        if (_id == null)
        {
            itemType = ItemType.Empty;
            icon.sprite = null;

            icon.gameObject.SetActive(false);
            itemIndex.gameObject.SetActive(false);
            selected.gameObject.SetActive(false);
            return;
        }

        icon.gameObject.SetActive(true);// ������ �̹��� Ȱ��ȭ
        switch (_id[0].ToString().ToUpper())// ù��° ���� �빮�ڷ� ����
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

    // ���� ���� ������ Ȥ�� ��ų�� ������ �ش� �������� �ó��� ���� Ȱ��ȭ
    // �ó��� ������ �ٸ� �������� ���� �ٸ� �������� �ó��� ���� Ȱ��ȭ
    public void AddSynergy(List<UI_InvenSlot> _addSynergy)
    {
        empty = 0; item = 0; skill = 0; unit = 0;
        addSynergy = _addSynergy;
        for (int i = 0; i < addSynergy.Count; i++)// ����� ���� ����
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
        if (itemStruct.synergyType.itemType != _slot.itemType)// �ó��� Ÿ�԰� ���� ������ �ѱ�
            return;

        switch (_slot.itemType)
        {
            case ItemType.Empty:
                // ��ĭ�� �ó����� ������ �ټ��� �ִ�.
                empty++;
                break;

            case ItemType.Item:
                item++;
                break;

            case ItemType.Skill:
                // ���� ���� Ÿ���� '������' '��ų'�� ��� ������ ���ݼ��� ������ŭ ���� ���
                skill++;
                if (_slot.skillStruct.skillType == itemStruct.synergyType.skillType)// ���� ��ųŸ��
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
        // ���� ���� Ÿ���� '������' '��ų'�� ��� ������ ���ݼ��� ������ŭ ���� ���
        switch (_slot.itemType)
        {
            case ItemType.Empty:
                // ��ĭ�� �ó����� ������ �ټ��� �ִ�.
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

    void UnitTypeSlot(UI_InvenSlot _slot)// �ڽ��� ������ ��� ���� ���� �����۰� ��ų�� �ɷ��� ����
    {
        switch (_slot.itemType)
        {
            case ItemType.Empty:
                // ��ĭ�� �ó����� ������ �ټ��� �ִ�.
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
        for (int i = 0; i < addSynergy.Count; i++)// ����� ���� ����
        {
            addSynergy[i].icon.material.SetFloat("_FillAmount", _active ? 1f : 0f);
            addSynergy[i].selected.gameObject.SetActive(_active);
        }
    }

    // �Ͻ����� (������ �Լ�, ������ �޾Ƹ� ���)
    // ������ �Լ� - ���� ��ġ �� ȿ�� (�ֺ� �Ʊ� �𵥵� ���ݷ� ���)
    // ������ �޾Ƹ� - ��� �� ȿ�� (3�ʵ� ����, ��� �Ʊ� ü�� ȸ��, ��� �� 1���)
    // ���� ���� 3������ ��ȭ
    // �������� ��ġ�� ��� ���ֿ� ȿ�� (�𵥵� ��� �� 1ȸ ��Ȱ, �߼� ���ݷ� ���, ������ �޾Ƹ� ȿ�� 2ȸ �ߵ�)
    // ������ ��ġ�� ������ ȿ�� ���
    // ��ų�� ���Ϳ� ���� (�κ��丮���� ����) �κ��丮�� ���� �κ��丮�� ������ �κ��丮�� ���� 
    // �̺�Ʈ ���� - �̴ϰ������� ����
}
