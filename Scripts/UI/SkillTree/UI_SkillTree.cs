using System.Collections.Generic;
using UnityEngine;

public class UI_SkillTree : MonoBehaviour
{
    public int skillPoint;
    public TMPro.TMP_Text skillPointText;
    public List<UI_SkillTree_Slot> slotList;

    void Start()
    {
        for (int i = 0; i < slotList.Count; i++)
        {
            slotList[i].SetSlot();
            slotList[i].deleSkillPoint += OverSkillPoint;
        }
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
