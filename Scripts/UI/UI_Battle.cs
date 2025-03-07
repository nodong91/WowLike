using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Battle : MonoBehaviour
{
    public Button[] timeScaleButtons;
    public delegate void DeleTimeScale(float timeScale);
    public DeleTimeScale deleTimeScale;
    public UI_Inventory inventory;
    public Button battleStart;
    
    public delegate void DeleBattleStart();
    public DeleBattleStart deleBattleStart;

    void Start()
    {
        timeScaleButtons[0].onClick.AddListener(delegate { SetTimeScale(0f); });
        timeScaleButtons[1].onClick.AddListener(delegate { SetTimeScale(1f); });
        timeScaleButtons[2].onClick.AddListener(delegate { SetTimeScale(3f); });
        timeScaleButtons[3].onClick.AddListener(delegate { SetTimeScale(5f); });

        battleStart.onClick.AddListener(BattleStart);
    }

    void SetTimeScale(float _timeScale)
    {
        deleTimeScale(_timeScale);
    }

    void BattleStart()
    {
        deleBattleStart();
        inventory.gameObject.SetActive(false);
    }
}
