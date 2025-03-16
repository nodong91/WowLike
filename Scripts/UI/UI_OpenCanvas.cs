using System.Collections;
using UnityEngine;

//[ExecuteInEditMode]
public class UI_OpenCanvas : MonoBehaviour
{
    public enum SideType
    {
        Left, Right, Top, Bottom, Alpha
    }
    public SideType sideType;
    RectTransform rect;
    CanvasGroup canvasGroup;
    public float safeSize;
    Coroutine moving;

    const float speed = 25f;

    public void SetCanvas()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        CloseCanvas();
    }

    public void OpenCanvas()
    {
        if (moving != null)
            StopCoroutine(moving);
        switch (sideType)
        {
            case SideType.Left:
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                moving = StartCoroutine(MovingCanvas(0f, safeSize));
                break;

            case SideType.Right:
                rect.anchorMin = new Vector2(1f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                moving = StartCoroutine(MovingCanvas(1f, -safeSize));
                break;

            case SideType.Top:
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                moving = StartCoroutine(MovingCanvas(1f, -safeSize));
                break;

            case SideType.Bottom:
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                moving = StartCoroutine(MovingCanvas(0f, safeSize));
                break;

            case SideType.Alpha:
                moving = StartCoroutine(AlphaCanvas(1f));
                break;
        }
    }

    public void CloseCanvas()
    {
        if (moving != null)
            StopCoroutine(moving);
        switch (sideType)
        {
            case SideType.Left:
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                moving = StartCoroutine(MovingCanvas(1f, -safeSize));
                break;

            case SideType.Right:
                rect.anchorMin = new Vector2(1f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                moving = StartCoroutine(MovingCanvas(0f, safeSize));
                break;

            case SideType.Top:
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                moving = StartCoroutine(MovingCanvas(0f, safeSize));
                break;

            case SideType.Bottom:
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                moving = StartCoroutine(MovingCanvas(1f, -safeSize));
                break;

            case SideType.Alpha:
                moving = StartCoroutine(AlphaCanvas(0f));
                break;
        }
    }

    IEnumerator MovingCanvas(float _pivot, float _anchoredPosition)
    {
        Vector2 prevPivot = rect.pivot;
        Vector2 prevAnchoredPosition = rect.anchoredPosition;
        float normalilze = 0f;
        while (normalilze < 1f)
        {
            normalilze += Time.deltaTime * (speed - normalilze * speed);
            switch (sideType)
            {
                case SideType.Left:
                case SideType.Right:
                    float pivot = Mathf.Lerp(prevPivot.x, _pivot, normalilze);
                    float anchoredPosition = Mathf.Lerp(prevAnchoredPosition.x, _anchoredPosition, normalilze);
                    rect.pivot = new Vector2(pivot, 0.5f);
                    rect.anchoredPosition = new Vector2(anchoredPosition, 0f);
                    break;

                case SideType.Top:
                case SideType.Bottom:
                    pivot = Mathf.Lerp(prevPivot.y, _pivot, normalilze);
                    anchoredPosition = Mathf.Lerp(prevAnchoredPosition.y, _anchoredPosition, normalilze);
                    rect.pivot = new Vector2(0.5f, pivot);
                    rect.anchoredPosition = new Vector2(0f, anchoredPosition);
                    break;
            }
            yield return null;
            Debug.LogWarning("ijefeijf");
        }
    }

    IEnumerator AlphaCanvas(float _targetAlpha)
    {
        if (canvasGroup == null)
            yield break;

        canvasGroup.interactable = _targetAlpha > 0;
        canvasGroup.blocksRaycasts = _targetAlpha > 0;
        float prevAlpha = canvasGroup.alpha;
        float normalilze = 0f;
        while (normalilze < 1f)
        {
            normalilze += Time.deltaTime * (speed - normalilze * speed);
            float alpha = Mathf.Lerp(prevAlpha, _targetAlpha, normalilze);
            canvasGroup.alpha = alpha;
            yield return null;
        }
    }
}
