using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SkillTree_Slot : MonoBehaviour, IPointerClickHandler
{
    public enum TreeSlotType
    {
        Unenable,
        Enable,
        Active
    }
    public TreeSlotType slotType;

    public Image enableImage, iconImage;
    public Image levelIcon, levelSlider;
    const float levelRange = 42;

    List<UI_SkillTree_Slot> takeSlotList = new List<UI_SkillTree_Slot>();// ������ �޴� ����
    public List<UI_SkillTree_Slot> influencingSlotEXP = new List<UI_SkillTree_Slot>();// ������ �ִ� ����
    public int currentLevel;
    public int maxLevel;
    public int GetLevel { get { return currentLevel; } }
    public int requiredLevel;// Ȱ��ȭ �� �� �ʿ��� ����ġ����

    public void SetSlot()
    {
        iconImage.material = Instantiate(iconImage.material);
        levelIcon.rectTransform.sizeDelta = new Vector2(levelRange * maxLevel, levelIcon.rectTransform.sizeDelta.y);
        for (int i = 0; i < influencingSlotEXP.Count; i++)
        {
            influencingSlotEXP[i].AddSlot(this);
        }
        CheckActive();
    }

    void AddSlot(UI_SkillTree_Slot _slot)
    {
        takeSlotList.Add(_slot);
    }

    void CheckActive()
    {
        int totalLevel = 0;
        for (int i = 0; i < takeSlotList.Count; i++)
        {
            // �� ���Կ� ������ �ִ� �ٸ� ���Ե��� ���� üũ
            totalLevel += takeSlotList[i].GetLevel;
        }

        if (totalLevel < requiredLevel)
        {
            // ���� ������ Ȱ��ȭ �������� ���� ��
            slotType = TreeSlotType.Unenable;
        }
        else if (currentLevel > 0)
        {
            slotType = TreeSlotType.Active;
        }
        else
        {
            slotType = TreeSlotType.Enable;
        }

        switch (slotType)
        {
            case TreeSlotType.Unenable:
                enableImage.gameObject.SetActive(true);
                iconImage.gameObject.SetActive(false);
                deleSkillPoint?.Invoke(-currentLevel);// ���� ����Ʈ �����ֱ�
                currentLevel = 0;
                break;

            case TreeSlotType.Enable:
                enableImage.gameObject.SetActive(false);
                iconImage.gameObject.SetActive(true);
                iconImage.material.SetFloat("_FillAmount", 0f);
                break;

            case TreeSlotType.Active:
                enableImage.gameObject.SetActive(false);
                iconImage.gameObject.SetActive(true);
                iconImage.material.SetFloat("_FillAmount", 1f);
                break;
        }
        LevelDisplay(currentLevel);
    }

    void LevelDisplay(int _level)
    {
        levelSlider.rectTransform.sizeDelta = new Vector2(levelRange * _level, levelSlider.rectTransform.sizeDelta.y);
    }

    public delegate int DeleSkillPoint(int _point);
    public DeleSkillPoint deleSkillPoint;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slotType == TreeSlotType.Unenable)
            return;

        int exp = 0;
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                exp = 1;
                break;

            case PointerEventData.InputButton.Right:
                exp = -1;
                break;
        }

        if (currentLevel + exp > maxLevel || currentLevel + exp < 0)
            return;

        int index = deleSkillPoint(exp);
        if (index == 0)
            return;

        currentLevel += index;
        LevelDisplay(currentLevel);

        float active = (currentLevel > 0) ? 1f : 0f;
        iconImage.material.SetFloat("_FillAmount", active);

        for (int i = 0; i < influencingSlotEXP.Count; i++)
        {
            influencingSlotEXP[i].CheckActive();
        }
    }
}
