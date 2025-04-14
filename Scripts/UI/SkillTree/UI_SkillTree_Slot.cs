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

    public List<UI_SkillTree_Slot> takeSlotList = new List<UI_SkillTree_Slot>();// 영향을 받는 슬롯
    List<UI_SkillTree_Slot> influencingSlot = new List<UI_SkillTree_Slot>();// 영향을 주는 슬롯
    public int currentLevel;
    public int maxLevel;
    public int GetLevel { get { return currentLevel; } }
    public int requiredLevel;// 활성화 할 때 필요한 경험치레벨
    public Transform pipeImage;

    public void SetSlot()
    {
        //activeImage.material = Instantiate(activeImage.material);
        levelIcon.rectTransform.sizeDelta = new Vector2(levelRange * maxLevel, levelIcon.rectTransform.sizeDelta.y);
        for (int i = 0; i < takeSlotList.Count; i++)
        {
            pipeImage.gameObject.SetActive(true);
            UI_SkillTree_Slot target = takeSlotList[i];
            float angle = Mathf.Atan2(target.transform.position.y - pipeImage.position.y, target.transform.position.x - pipeImage.position.x) * Mathf.Rad2Deg;
            pipeImage.rotation = Quaternion.Euler(new Vector3(0, 0f, angle + 90f));
            target.AddSlot(this);
        }
        CheckActive();
    }

    //private void Update()
    //{
    //    for (int i = 0; i < takeSlotList.Count; i++)
    //    {
    //        Transform target = takeSlotList[i].transform;
    //        float angle = Mathf.Atan2(target.position.y - pipeImage.position.y, target.position.x - pipeImage.position.x) * Mathf.Rad2Deg;
    //        pipeImage.rotation = Quaternion.Euler(new Vector3(0, 0f, angle + 90f));
    //        //Vector3 offset = (takeSlotList[i].transform.position - transform.position).normalized;
    //        //pipeImage.transform.rotation = Quaternion.LookRotation(offset);
    //        takeSlotList[i].AddSlot(this);
    //    }
    //}

    void AddSlot(UI_SkillTree_Slot _slot)
    {
        influencingSlot.Add(_slot);
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

        for (int i = 0; i < influencingSlot.Count; i++)
        {
            influencingSlot[i].CheckActive();
        }
    }
}
