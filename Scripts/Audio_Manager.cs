using System.Collections;
using UnityEngine;

public class Audio_Manager : MonoBehaviour
{
    public AudioSource bgmSource;
    public AudioSource fxSource;

    public bool BGMMute;
    public float BGMVolume;
    public bool fxMute;
    public float fxVolume;

    public static Audio_Manager current;

    void Awake()
    {
        current = this;
    }

    public void BackGroundMusic(string BGM)
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();

            bgmSource.Stop();
            bgmSource.clip = Singleton_Data.INSTANCE.Dict_Audio[BGM];
            bgmSource.loop = true;

            bgmSource.mute = BGMMute;
            bgmSource.volume = BGMVolume;

            bgmSource.Play();
        }
        else
        {
            //  추가 음악 변경
            AudioSource addSource = gameObject.AddComponent<AudioSource>();
            addSource.clip = Singleton_Data.INSTANCE.Dict_Audio[BGM];
            addSource.loop = true;

            addSource.Play();
            StartCoroutine(CrossFadeAudio(addSource));
        }
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

    public void FXAudio(string audio)
    {
        if (fxSource == null)
        {
            fxSource = gameObject.AddComponent<AudioSource>();

            fxSource.mute = fxMute;
            fxSource.volume = fxVolume;
        }
        fxSource.Stop();
        fxSource.pitch = Random.Range(0.7f, 1.3f);
        fxSource.clip = Singleton_Data.INSTANCE.Dict_Audio[audio];
        fxSource.Play();
    }
}
