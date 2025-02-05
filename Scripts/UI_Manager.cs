using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public Button openButton, exitButton;
    public CanvasGroup canvas;
    bool open;

    void Start()
    {
        canvas.gameObject.SetActive(open);
        openButton.onClick.AddListener(OpenCanvas);
        //closeButton.onClick.AddListener(delegate { OpenCanvas(false); });
        exitButton.onClick.AddListener(QuitGame);
    }

    void OpenCanvas()
    {
        open = !open;
        canvas.gameObject.SetActive(open);
    }

    void QuitGame()
    {
        if (Application.isEditor == true)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            Application.Quit();
        }
    }
}
