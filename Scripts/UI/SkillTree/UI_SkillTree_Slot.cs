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

    public Image unEnableImage, enableImage, activeImage;
    public Image levelIcon, levelSlider;
    const float levelRange = 42;

    List<UI_SkillTree_Slot> takeSlotList = new List<UI_SkillTree_Slot>();// 영향을 받는 슬롯
    public List<UI_SkillTree_Slot> influencingSlotEXP = new List<UI_SkillTree_Slot>();// 영향을 주는 슬롯
    public int currentLevel;
    public int maxLevel;
    public int GetLevel { get { return currentLevel; } }
    public int requiredLevel;// 활성화 할 때 필요한 경험치레벨

    public void SetSlot()
    {
        //activeImage.material = Instantiate(activeImage.material);
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
            // 이 슬롯에 영향을 주는 다른 슬롯들의 레벨 체크
            totalLevel += takeSlotList[i].GetLevel;
        }

        if (totalLevel < requiredLevel)
        {
            // 총합 레벨이 활성화 레벨보다 낮을 때
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
                unEnableImage.gameObject.SetActive(true);
                enableImage.gameObject.SetActive(false);
                activeImage.gameObject.SetActive(false);
                deleSkillPoint?.Invoke(-currentLevel);// 남은 포인트 돌려주기
                currentLevel = 0;
                break;

            case TreeSlotType.Enable:
                unEnableImage.gameObject.SetActive(false);
                enableImage.gameObject.SetActive(true);
                activeImage.gameObject.SetActive(false);
                break;

            case TreeSlotType.Active:
                unEnableImage.gameObject.SetActive(false);
                enableImage.gameObject.SetActive(false);
                activeImage.gameObject.SetActive(true);
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
        Debug.LogWarning("oihjoijf");
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
        //LevelDisplay(currentLevel);
        CheckActive();

        for (int i = 0; i < influencingSlotEXP.Count; i++)
        {
            influencingSlotEXP[i].CheckActive();
        }
    }
}
