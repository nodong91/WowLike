using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Data_Manager;

public class UI_InvenSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum SlotType
    {
        Empty,
        Item,
        Skill,
    }
    public SlotType slotType;

    public Image icon;
    public delegate void OnDragHandler(Vector3 _position);
    public OnDragHandler onDrag;
    public delegate void OnSlotHandler(UI_InvenSlot _slot);
    public OnSlotHandler onBeginDrag;
    public OnSlotHandler onEndDrag;
    public OnSlotHandler onPointerEnter;

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

    }






    public Data_Manager.SkillStruct skillStruct;

    public void ChangeSlot(UI_InvenSlot _slot)
    {
        switch (_slot.slotType)
        {
            case SlotType.Empty:

                break;

            case SlotType.Item:
                _slot.SetItemSlot(this);
                break;

            case SlotType.Skill:
                Data_Manager.SkillStruct slotSkill = _slot.skillStruct;
                _slot.SetSkillSlot(skillStruct);
                SetSkillSlot(slotSkill);
                break;


        }
    }

    public void SetSkillSlot(Data_Manager.SkillStruct _skillStruct)
    {
        slotType = SlotType.Skill;
        skillStruct = _skillStruct;
        icon.sprite = _skillStruct.icon;
    }

    public void SetItemSlot(UI_InvenSlot _slot)
    {
        slotType = SlotType.Item;
        icon.sprite = _slot.icon.sprite;
    }
}
