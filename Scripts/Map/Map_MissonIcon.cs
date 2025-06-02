using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Map_MissonIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public RectTransform iconRect;
    public CanvasGroup canvas;
    Coroutine iconAct;
    bool open;

    void Start()
    {
        SelectedIcon(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SelectedIcon(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SelectedIcon(false);
    }
  
    void SelectedIcon(bool _open)
    {
        open = _open;
        iconRect.SetAsLastSibling();
        if (iconAct != null)
            StopCoroutine(iconAct);
        iconAct = StartCoroutine(SelectedIconAction());
    }

    IEnumerator SelectedIconAction()
    {
        Vector2 prev = iconRect.sizeDelta;
        Vector2 target = open == true ? new Vector2(200, 150) : new Vector2(70, 70);
        float targetAlpha = open == true ? 1f : 0f;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 10f;
            Vector2 setSize = Vector2.Lerp(prev, target, normalize);
            iconRect.sizeDelta = setSize;
            canvas.alpha = Mathf.Lerp(canvas.alpha, targetAlpha, normalize);
            yield return null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }
}
