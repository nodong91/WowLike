using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Skill_Slot : MonoBehaviour
{
    private bool inDist, cooling, isActive;
    public bool GetIsActive { get { return isActive; } }
    public delegate void Dele_Action(bool _action);
    public Dele_Action dele_Action;
    public delegate void Dele_SlotAction();
    public Dele_SlotAction dele_SlotAction;
    public Button button;

    public TMP_Text quickIndex;
    public Image icon;
    public Material material;
    Data_Manager.SkillStruct skillStruct;

    public Color enabledColor, disabledColor;

    public void SetSlot(string _index, Data_Manager.SkillStruct _skillStruct)
    {
        quickIndex.text = _index;// ¥‹√‡≈∞
        skillStruct = _skillStruct;
        icon.sprite = _skillStruct.icon;
        material = Instantiate(icon.material);
        icon.material = material;

        SetCooling(1f);
    }

    public void ActionButton()
    {
        //dele_Action(true);
        StartCoroutine(CoolingSkill());
    }

    public void InDistance(bool _inDist)
    {
        inDist = _inDist;
        CheckActive();
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

    IEnumerator CastingAction()
    {
        float castingTime = 1f / skillStruct.castingTime;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * castingTime;
            yield return null;
        }
    }
}
