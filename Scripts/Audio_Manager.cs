using UnityEngine;
using UnityEngine.UI;

public class Audio_Manager : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider fxSlider;

    public Button button;


    void CloseWindow()
    {
        gameObject.SetActive(false);
    }

    public void SetAudioManager()
    {
        button.onClick.AddListener(CloseWindow);

        Singleton_Audio.INSTANCE.SetAudio();

        bgmSlider.onValueChanged.AddListener(BGMValue);
        fxSlider.onValueChanged.AddListener(FXValue);
        bgmSlider.value = 0.3f;
        fxSlider.value = 1.0f;
    }

    void BGMValue(float _value)
    {
        Singleton_Audio.INSTANCE.SetBGMVolume(_value);
    }

    void FXValue(float _value)
    {
        Singleton_Audio.INSTANCE.SetFXVolume(_value);
    }
}
