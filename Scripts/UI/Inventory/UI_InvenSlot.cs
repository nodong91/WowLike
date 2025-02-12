using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UI_InvenSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum SlotType
    {
        Empty,
        Item,
        Skill
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
        onBeginDrag?.Invoke(this);
        icon.gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        onDrag?.Invoke(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        onEndDrag?.Invoke(this);
        icon.gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerEnter?.Invoke(null);
    }




    public UI_InvenSlot slotClone;
    public Data_Manager.ItemStruct itemStruct;
    public Data_Manager.SkillStruct skillStruct;
    public bool quick;

    public delegate void DeleGateAction();
    public DeleGateAction deleGateAction;

    void Start()
    {
        icon.material = Instantiate(icon.material);
    }

    void ClickLeft()
    {
        deleGateAction?.Invoke();
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
        switch (_enterSlot.slotType)
        {
            case SlotType.Empty:
                _enterSlot.TakeSlot(this);
                SetEmptySlot();
                break;

            case SlotType.Item:
                Data_Manager.ItemStruct itemSkill = _enterSlot.itemStruct;// ¹Ì¸® ÀúÀå
                _enterSlot.TakeSlot(this);
                SetItemSlot(itemSkill);
                break;

            case SlotType.Skill:
                Data_Manager.SkillStruct slotSkill = _enterSlot.skillStruct;// ¹Ì¸® ÀúÀå
                _enterSlot.TakeSlot(this);// °¡Á®¿Â ½½·Ô ¹Ù²Þ
                SetSkillSlot(slotSkill);// ³» ½½·Ô ¹Ù²Þ
                break;
        }
    }

    void TakeSlot(UI_InvenSlot _dragSlot)
    {
        switch (_dragSlot.slotType)
        {
            case SlotType.Empty:
                break;

            case SlotType.Item:
                SetItemSlot(_dragSlot.itemStruct);
                break;

            case SlotType.Skill:
                SetSkillSlot(_dragSlot.skillStruct);
                break;
        }
        Game_Manager.instance.CheckDistance();
    }

    public void SetEmptySlot()
    {
        slotType = SlotType.Empty;
        icon.sprite = null;
    }

    public void SetSkillSlot(Data_Manager.SkillStruct _skillStruct)
    {
        slotType = SlotType.Skill;
        skillStruct = _skillStruct;
        icon.sprite = _skillStruct.icon;
    }

    public void SetItemSlot(Data_Manager.ItemStruct _itemStruct)
    {
        slotType = SlotType.Item;
        itemStruct = _itemStruct;
        icon.sprite = _itemStruct.icon;
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

    public TMP_Text quickIndex;
    public Color enabledColor, disabledColor;
    void CheckActive()
    {
        quickIndex.gameObject.SetActive(slotType != SlotType.Empty);

        isActive = (inDist == true && cooling == false);
        quickIndex.color = isActive == true ? enabledColor : disabledColor;
    }
}
