using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class DamageFontTest : MonoBehaviour
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


    void Start()
    {
        centerPosition = uiCamera.ViewportToScreenPoint(Vector3.one * 0.5f);
    }

    void Update()
    {
        Application.targetFrameRate = targetFrameRate;
        if (Input.GetMouseButtonUp(0))
        {
            int index = Random.Range(0, 1500);
            DisplayDamage(index);
        }
    }
    public Material normalMaterial, criticalMaterial;
    void DisplayDamage(int _damage)
    {
        TextPooling instText = TryInstanceText();
        float alpha = (_damage > 1000) ? 1f : 0f;
        instText.critical.CrossFadeAlpha(alpha, 0f, false);
        instText.damageText.fontMaterial = (_damage > 1000) ? criticalMaterial : normalMaterial;
        instText.damageText.text = _damage.ToString();
        StartCoroutine(DamageTextAction(instText));
        FollowUI(instText);
    }

    TextPooling TryInstanceText()
    {
        if (textPoolingQueue == null)
            textPoolingQueue = new Queue<TextPooling>();

        if (textPoolingQueue.Count > 0)
        {
            return textPoolingQueue.Dequeue();
        }
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

    void FollowUI(TextPooling _instText)
    {
        Vector2 screenPosition = uiCamera.ViewportToScreenPoint(Input.mousePosition);
        Vector2 followPosition = uiCamera.ScreenToViewportPoint(screenPosition);
        followPosition -= centerPosition;

        Vector2 randomCircle = Random.insideUnitCircle * hitRadius;
        Vector2 randomPos = new Vector3(randomCircle.x, randomCircle.y);

        _instText.followUI.anchoredPosition = followPosition + randomPos;
    }

    IEnumerator DamageTextAction(TextPooling _instText)
    {
        _instText.canvasGroup.alpha = 1f;
        _instText.followUI.localScale = Vector3.one;

        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime / shakeDuration;

            float curveTime = sizeCurve.Evaluate(normalize);
            _instText.damageText.rectTransform.localScale = Vector3.Lerp(prevSize, nextSize, curveTime);

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
                    _instText.followUI.localScale = Vector3.Lerp(_instText.followUI.localScale, lastScale, normalize);
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
