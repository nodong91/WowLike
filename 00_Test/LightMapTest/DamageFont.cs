using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageFont : MonoBehaviour
{
    public Camera uiCamera;
    private Vector2 centerPosition;

    public RectTransform followUI;
    public TMPro.TMP_Text damageText;
    public Image critical;
    Coroutine damageTextAction;
    public float hitRadius = 100f;
    public float shakeSize, shakeDuration;
    public enum FontType
    {
        Shake,
        Move
    }
    public FontType fontType;

    public AnimationCurve sizeCurve;
    public Vector3 prevSize, nextSize;
    public AnimationCurve colorCurve;
    public Color prevColor, nextColor;
    public AnimationCurve positionCurve;
    public Vector2 prevPosition, nextPosition;

    public enum EndType
    {
        None,
        LastPosition,
        LastScale,
        Alpha
    }
    public EndType endType;
    public float holdTime = 1f;
    public float lastDuration = 0.3f;
    public Vector2 lastPosition;
    public Vector3 lastScale;

    [System.Serializable]
    public class TextPooling
    {
        public RectTransform followUI;
        public CanvasGroup canvasGroup;
        public TMPro.TMP_Text damageText;
        public Image critical;
    }
    public Queue<TextPooling> textPoolingQueue;
    public int targetFrameRate = 60;
    public Material normalMaterial, criticalMaterial;


    void Start()
    {
        //uiCamera.fieldOfView = Camera.main.fieldOfView;
        centerPosition = uiCamera.ViewportToScreenPoint(Vector2.one * 0.5f);
    }

    TextPooling instTexttest;
    Vector3 localScale;

    public void DisplayDamage(int _damage)
    {
        TextPooling instText = TryInstanceText();
        bool critical = (_damage > 1000);
        float alpha = critical ? 1f : 0f;
        instText.critical.CrossFadeAlpha(alpha, 0f, false);
        instText.damageText.fontMaterial = critical ? criticalMaterial : normalMaterial;
        localScale = critical ? Vector3.one * 0.3f : Vector3.zero;
        instText.damageText.text = _damage.ToString();
        StartCoroutine(DamageTextAction(instText));
        FollowUI(instText, Input.mousePosition);
    }

    public void DisplayDamage(Vector3 _point, string _damage)
    {
        TextPooling instText = TryInstanceText();
        bool critical = Random.Range(0, 10) > 7;
        float alpha = critical ? 1f : 0f;
        instText.critical.CrossFadeAlpha(alpha, 0f, false);
        instText.damageText.fontMaterial = critical ? criticalMaterial : normalMaterial;
        localScale = critical ? Vector3.one * 0.3f : Vector3.zero;
        instText.damageText.text = _damage.ToString();
        StartCoroutine(DamageTextAction(instText));
        FollowWorld(instText, _point);
    }

    TextPooling TryInstanceText()
    {
        if (textPoolingQueue == null)
            textPoolingQueue = new Queue<TextPooling>();

        if (textPoolingQueue.Count > 0)
        {
            return textPoolingQueue.Dequeue();
        }
        // »ý¼º
        RectTransform instFollowUI = Instantiate(followUI, this.transform);
        CanvasGroup instCanvasGroup = instFollowUI.GetComponent<CanvasGroup>();
        TMPro.TMP_Text instDamageText = Instantiate(damageText, instFollowUI.transform);
        instDamageText.rectTransform.anchoredPosition = Vector3.zero;
        Image instCritical = Instantiate(critical, instDamageText.transform);
        instCritical.rectTransform.anchoredPosition = Vector3.zero;
        TextPooling inst = new TextPooling
        {
            followUI = instFollowUI,
            canvasGroup = instCanvasGroup,
            damageText = instDamageText,
            critical = instCritical,
        };
        return inst;
    }

    void FollowUI(TextPooling _instText, Vector3 _point)
    {
        Vector2 randomCircle = Random.insideUnitCircle * hitRadius;
        Vector2 screenPosition = uiCamera.ViewportToScreenPoint(_point);
        Vector2 followPosition = uiCamera.ScreenToViewportPoint(screenPosition);
        followPosition -= centerPosition;

        _instText.followUI.anchoredPosition = followPosition + randomCircle;
    }

    void FollowWorld(TextPooling _instText, Vector3 _point)
    {
        Vector3 randomCircle = Random.insideUnitCircle * hitRadius;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_point);
        Vector3 followPosition = uiCamera.ScreenToWorldPoint(screenPosition);
        _instText.followUI.transform.position = followPosition + randomCircle;
    }

    IEnumerator DamageTextAction(TextPooling _instText)
    {
        _instText.canvasGroup.alpha = 1f;
        _instText.followUI.localScale = Vector3.one + localScale;

        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime / shakeDuration;

            float curveTime = sizeCurve.Evaluate(normalize);
            _instText.damageText.rectTransform.localScale = Vector3.Lerp(prevSize, nextSize, curveTime) + localScale;

            float curveColor = colorCurve.Evaluate(normalize);
            _instText.damageText.color = Color.Lerp(prevColor, nextColor, curveColor);

            switch (fontType)
            {
                case FontType.Shake:
                    Vector2 randomCircle = Random.insideUnitCircle * shakeSize * (1f - normalize);
                    Vector2 randomPos = new Vector3(randomCircle.x, randomCircle.y);

                    _instText.damageText.rectTransform.anchoredPosition = randomPos;
                    break;

                case FontType.Move:
                    float curvePosition = positionCurve.Evaluate(normalize);
                    _instText.damageText.rectTransform.anchoredPosition = Vector2.Lerp(prevPosition, nextPosition, curvePosition);

                    break;
            }
            yield return null;
        }
        _instText.damageText.rectTransform.anchoredPosition = Vector3.zero;
        yield return new WaitForSeconds(holdTime);

        normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime / lastDuration;

            switch (endType)
            {
                case EndType.None:

                    break;

                case EndType.LastPosition:
                    _instText.damageText.rectTransform.anchoredPosition = Vector2.Lerp(Vector2.zero, lastPosition, normalize);
                    _instText.canvasGroup.alpha = Mathf.Lerp(1f, 0f, normalize);
                    break;

                case EndType.LastScale:
                    _instText.followUI.localScale = Vector3.Lerp(_instText.followUI.localScale, lastScale + localScale, normalize);
                    _instText.canvasGroup.alpha = Mathf.Lerp(1f, 0f, normalize);
                    break;

                case EndType.Alpha:
                    _instText.canvasGroup.alpha = Mathf.Lerp(1f, 0f, normalize);
                    break;
            }
            yield return null;
        }
        textPoolingQueue.Enqueue(_instText);
    }
}
