using System.Collections;
using UnityEngine;

public class Singleton_Audio : MonoSingleton<Singleton_Audio>
{
    public void Call()
    {
        Debug.LogWarning("SoundManager Call()");
    }
    [SerializeField]
    private AudioSource bgmSource, fxSource;

    public bool BGMMute;
    public float BGMVolume;
    public bool fxMute;
    public float fxVolume;

    public void SetAudio()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.mute = BGMMute;
        bgmSource.volume = BGMVolume;

        fxSource = gameObject.AddComponent<AudioSource>();
        fxSource.mute = fxMute;
        fxSource.volume = fxVolume;
    }

    public void Audio_SetBGM(string _id)
    {
        //  추가 음악 변경
        AudioSource addSource = gameObject.AddComponent<AudioSource>();
        addSource.clip = Singleton_Data.INSTANCE.Dict_Audio[_id];
        addSource.loop = true;

        addSource.Play();
        StartCoroutine(CrossFadeAudio(addSource));
    }

    public void SetBGMVolume(float _value)
    {
        BGMVolume = _value;
        bgmSource.volume = BGMVolume;
    }

    IEnumerator CrossFadeAudio(AudioSource newSource)
    {
        newSource.mute = BGMMute;
        float normalize = 0.0f;
        while (normalize < 1.0f)
        {
            normalize += Time.fixedDeltaTime * 0.5f;
            newSource.volume = Mathf.Lerp(0.0f, BGMVolume, normalize);
            bgmSource.volume = BGMVolume - newSource.volume;
            yield return null;
        }
        Destroy(bgmSource);
        bgmSource = newSource;
    }

    public void Audio_SetFX(string _id)
    {
        fxSource.Stop();
        fxSource.pitch = Random.Range(0.7f, 1.3f);
        fxSource.clip = Singleton_Data.INSTANCE.Dict_Audio[_id];
        fxSource.Play();
    }

    public void SetFXVolume(float _value)
    {
        fxVolume = _value;
        fxSource.volume = fxVolume;
    }
}
