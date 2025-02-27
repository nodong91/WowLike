using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class UI_Battle : MonoBehaviour
{
    public Button[] timeScaleButtons;
    public delegate void DeleTimeScale(float timeScale);
    public DeleTimeScale deleTimeScale;

    void Start()
    {
        deleTimeScale = Unit_AI_Manager.instance.SetTimeScale;
        timeScaleButtons[0].onClick.AddListener(delegate { SetTimeScale(0f); });
        timeScaleButtons[1].onClick.AddListener(delegate { SetTimeScale(1f); });
        timeScaleButtons[2].onClick.AddListener(delegate { SetTimeScale(3f); });
        timeScaleButtons[3].onClick.AddListener(delegate { SetTimeScale(5f); });
    }

    void SetTimeScale(float _timeScale)
    {
        deleTimeScale(_timeScale);
    }
}
