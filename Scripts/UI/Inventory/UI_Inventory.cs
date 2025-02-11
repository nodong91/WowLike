using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory : MonoBehaviour
{
    public Image dragIcon;
    public Transform slotParent;
    public UI_InvenSlot invenSlot;

    public Data_Manager.SkillStruct[] skillStruct;

    void Start()
    {
        for (int i = 0; i < 25; i++)
        {
            int index = Random.Range(0, skillStruct.Length);
            Data_Manager.SkillStruct slotStruct = skillStruct[index];

            UI_InvenSlot inst = Instantiate(invenSlot, slotParent);
            inst.SetSkillSlot(slotStruct);
            inst.onBeginDrag += OnBeginDrag;
            inst.onDrag += OnDrag;
            inst.onEndDrag += OnEndDrag;
            inst.onPointerEnter += OnPointerEnter;
        }
    }

    public UI_InvenSlot dragSlot, enterSlot;
    public void OnBeginDrag(UI_InvenSlot _slot)
    {
        dragSlot=_slot;
        dragIcon.sprite = _slot.icon.sprite;
        dragIcon.gameObject.SetActive(true);
    }

    private void OnDrag(Vector3 _position)
    {
        dragIcon.transform.position = _position;
    }

    public void OnEndDrag(UI_InvenSlot _slot)
    {
        dragIcon.gameObject.SetActive(false);
        enterSlot.ChangeSlot(dragSlot);
    }

    void ChangeSlot()
    {
        if (dragSlot is UI_InvenSlot_Skill)
        {
            UI_InvenSlot_Skill drag = dragSlot as UI_InvenSlot_Skill;
            Data_Manager.SkillStruct dragSkill = drag.skillStruct;
            if (enterSlot is UI_InvenSlot_Skill)
            {
                UI_InvenSlot_Skill temp = enterSlot as UI_InvenSlot_Skill;
                drag.SetSkillSlot(temp.skillStruct);
                temp.SetSkillSlot(dragSkill);
            }
        }

        if(dragSlot is UI_InvenSlot_Item)
        {

        }
    }

    public void OnPointerEnter(UI_InvenSlot _slot)
    {
        enterSlot = _slot;
        Debug.LogWarning("iojhoi"+_slot.name);
    }
}
