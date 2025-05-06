using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_StartSet : MonoBehaviour
{
    public Toggle toggle;
    public CanvasGroup canvasGroup;

    void Start()
    {

    }

    public void OpenCanvas()
    {
        if(canvasAction != null)
            StopCoroutine(canvasAction);
        canvasAction = StartCoroutine(CanvasAction());
    }

    Coroutine canvasAction;
    IEnumerator CanvasAction()
    {
        float alpha = toggle.isOn == true ? 1f : 0f;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 3f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, alpha, normalize);
            yield return null;
        }
    }
}
