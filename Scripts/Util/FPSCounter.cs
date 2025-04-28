using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TMPro.TMP_Text fpsText; // Assign this in the Inspector
    private float deltaTime;
    private float fps;

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        fps = 1f / deltaTime;
        if (fpsText != null)
        {
            fpsText.text = Mathf.Round(fps).ToString() + " FPS";
        }
    }
}
