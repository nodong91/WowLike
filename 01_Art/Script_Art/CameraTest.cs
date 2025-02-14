using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraTest : MonoBehaviour
{
    public Transform target, focus, tower;
    private Camera mainCam;
    private CinemachineBrain brain;
    public CinemachineBlendDefinition.Styles brainStyles;
    public float brainTime;

    void Start()
    {
        mainCam = Camera.main;
        brain = mainCam.GetComponent<CinemachineBrain>();
        brain.DefaultBlend.Style = brainStyles;
        brain.DefaultBlend.Time = brainTime;

        focus.gameObject.SetActive(false);

        SetCanvas();
    }

    private Vector2 prevVector;
    private Vector3 prevWorldVector;
    public float speed;
    bool zoomIn;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            prevVector = mainCam.ScreenToViewportPoint(Input.mousePosition);
            prevWorldVector = target.position;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Vector2 currentVector = mainCam.ScreenToViewportPoint(Input.mousePosition);
            float dist = (prevVector - currentVector).magnitude;
            if (dist < 0.01f)
            {
                Click();
                zoomIn = !zoomIn;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (zoomIn == false)
            {
                Vector2 currentVector = mainCam.ScreenToViewportPoint(Input.mousePosition);
                Vector2 offset = (prevVector - currentVector) * speed;
                target.position = prevWorldVector + new Vector3(offset.x, 0f, offset.y);
            }
        }
    }

    void Click()
    {
        if (canvasAction != null)
            StopCoroutine(canvasAction);

        if (zoomIn == true)
        {
            focus.gameObject.SetActive(false);
            RectStruct sideStruct = new RectStruct
            {
                anchor = new Vector2(anchor.x, 0f),
                pivot = new Vector2(0f, 0.5f)
            };

            RectStruct centerStruct = new RectStruct
            {
                anchor = new Vector2(0f,0f),
                pivot = new Vector2(0.5f, 0f)
            };
            canvasAction = StartCoroutine(CanvasActing(sideStruct, centerStruct));
        }
        else
        {
            Transform temp = RayCasting();
            if (temp != null)
            {
                target.position = temp.position;
                focus.gameObject.SetActive(true);

                RectStruct sideStruct = new RectStruct
                {
                    anchor = new Vector2(0f, 0f),
                    pivot = new Vector2(1f, 0.5f)
                };

                RectStruct centerStruct = new RectStruct
                {
                    anchor = new Vector2(0f, -anchor.y),
                    pivot = new Vector2(0.5f, 1f)
                };
                canvasAction = StartCoroutine(CanvasActing(sideStruct, centerStruct));
            }
        }
    }
    Coroutine canvasAction;
    public struct RectStruct
    {
        public Vector2 anchor;
        public Vector2 pivot;
    }
    IEnumerator CanvasActing(RectStruct _side, RectStruct _center)
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 5f;
            sideBox.anchoredPosition = Vector2.Lerp(sideBox.anchoredPosition, _side.anchor, normalize);
            sideBox.pivot = Vector2.Lerp(sideBox.pivot, _side.pivot, normalize);

            centerBox.anchoredPosition = Vector2.Lerp(centerBox.anchoredPosition, _center.anchor, normalize);
            centerBox.pivot = Vector2.Lerp(centerBox.pivot, _center.pivot, normalize);
            yield return null;
        }
    }

    Transform RayCasting()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //int layerMask = 1 << 8;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Debug.LogWarning("È÷Æ®!! " + hit.collider.gameObject.name);
            Debug.DrawLine(mainCam.transform.position, hit.point, Color.red, 0.3f);

            return hit.transform;
        }
        return null;
    }

    public RectTransform saftyRect, sideBox, centerBox;
    public Vector2 anchor;

    void SetCanvas()
    {
        anchor = saftyRect.offsetMin;
        Click();
    }
}
