using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

//[ExecuteInEditMode]
public class UI_OpenCanvas : MonoBehaviour
{
    public enum SideType
    {
        Left, Right, Top, Bottom
    }
    public SideType sideType;
    RectTransform rect;
    public float safeSize;
    public Camera UICamera;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        CloseCanvas();
        if (UICamera == null)
            return;
        SetUICamera();
    }

    void SetUICamera()
    {
        Camera mainCamera = Camera.main;
        var cameraData = mainCamera.GetUniversalAdditionalCameraData();
        if (cameraData.cameraStack.Contains(UICamera) == false)
        {
            cameraData.cameraStack.Add(UICamera);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OpenCanvas();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            CloseCanvas();
        }
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
        }
    }
    Coroutine moving;
    IEnumerator MovingCanvas(float _pivot, float _anchoredPosition)
    {
        float normalilze = 0f;
        while (normalilze < 1f)
        {
            normalilze += Time.deltaTime * 3f;
            switch (sideType)
            {
                case SideType.Left:
                case SideType.Right:
                    float pivot = Mathf.Lerp(rect.pivot.x, _pivot, normalilze);
                    float anchoredPosition = Mathf.Lerp(rect.anchoredPosition.x, _anchoredPosition, normalilze);
                    rect.pivot = new Vector2(pivot, 0.5f);
                    rect.anchoredPosition = new Vector2(anchoredPosition, 0f);
                    break;

                case SideType.Top:
                case SideType.Bottom:
                    pivot = Mathf.Lerp(rect.pivot.y, _pivot, normalilze);
                    anchoredPosition = Mathf.Lerp(rect.anchoredPosition.y, _anchoredPosition, normalilze);
                    rect.pivot = new Vector2(0.5f, pivot);
                    rect.anchoredPosition = new Vector2(0f, anchoredPosition);
                    break;
            }
            yield return null;
        }
    }
}
