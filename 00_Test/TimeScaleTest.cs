using UnityEngine;

public class TimeScaleTest : MonoBehaviour
{
    public float unscaledTime, timer;
    public float timeScale;

    void Update()
    {
        timeScale = Time.timeScale;
        if (Input.GetMouseButtonDown(1))
        {
            Time.timeScale = Time.timeScale > 0f ? 0f : 1f;
            if (Time.timeScale == 0)
                timer = Time.time;
        }
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 0f;
            unscaledTime = Time.unscaledTime;
            Shader.SetGlobalFloat("_UnscaledTime", unscaledTime);
        }
    }
}
