using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillTree : MonoBehaviour
{
    public int skillPoint;
    public TMPro.TMP_Text skillPointText;
    //public List<UI_SkillTree_Slot> slotList;

    public UI_SkillTree_Slot slot;
    public RectTransform slotParent;

    public class SlotClass
    {
        public List<UI_SkillTree_Slot> slots;
    }
    public List<UI_SkillTree_Slot>[] slotClass;

    void Start()
    {
        int randomSeed = 0;
        Random.InitState(randomSeed);
        slotClass = new List<UI_SkillTree_Slot>[10];
        for (int i = 0; i < 10; i++)
        {
            GameObject instObject = new GameObject("Test_" + i);
            RectTransform instRect = instObject.AddComponent<RectTransform>();
            instRect.SetParent(slotParent.transform, false);
            instRect.sizeDelta = new Vector2(100f, 120f);
            //GridLayoutGroup instParent = instObject.AddComponent<GridLayoutGroup>();
            //instParent.transform.SetParent(slotParent.transform, false);
            //instParent.cellSize = new Vector2(100f, 100f);
            //instParent.spacing = new Vector2(20f, 20f);
            //instParent.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            //instParent.constraintCount = 1;

            List<UI_SkillTree_Slot> slotList = new List<UI_SkillTree_Slot>();
            int slotIndex = Random.Range(1, 5);
            float startPoint = (100f + 20f) * (slotIndex - 1) * 0.5f;
            for (int s = 0; s < slotIndex; s++)
            {
                UI_SkillTree_Slot inst = Instantiate(slot, instObject.transform);
                if (i > 0)
                {
                    // ·£´ý ½½·Ô ¼³Á¤
                    int randomIndex = Random.Range(0, slotClass[i - 1].Count);
                    inst.takeSlotList.Add(slotClass[i - 1][randomIndex]);
                }
                inst.requiredLevel = i;
                inst.SetSlot();
                inst.deleSkillPoint += OverSkillPoint;
                slotList.Add(inst);

                instRect = inst.GetComponent<RectTransform>();
                float randomRange = 10f;
                float x = Random.Range(-randomRange, randomRange) + (100f + 20f) * s - startPoint;
                float y = Random.Range(-randomRange, randomRange);
                instRect.anchoredPosition = new Vector2(x, y);
            }
            slotClass[i] = slotList;
        }

        //for (int i = 0; i < slotList.Count; i++)
        //{
        //    slotList[i].SetSlot();
        //    slotList[i].deleSkillPoint += OverSkillPoint;
        //}
        OverSkillPoint(0);// Ãâ·Â¿ë
    }

    int OverSkillPoint(int _point)
    {
        int temp = skillPoint - _point;
        if (temp >= 0)
        {
            skillPoint -= _point;
            skillPointText.text = skillPoint.ToString();
            return _point;
        }
        return 0;
    }
}
