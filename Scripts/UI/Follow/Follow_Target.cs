using System.Collections;
using UnityEngine;

//[ExecuteInEditMode]
public class Follow_Target : MonoBehaviour
{
    public enum FollowType
    {
        Overlay,
        Camera
    }
    public FollowType followType;
    public Vector3 followOffset;
    public CanvasGroup mainCanvas;
    private RectTransform mainRect;

    public float duration;
    public AnimationCurve durationCurve;
    Coroutine shakining;
    public Vector2 shakeSize;
    Vector2 prevPosition;

    public virtual void SetFollowCamera()
    {
        //followType = FollowType.Camera;
        mainRect = mainCanvas.GetComponent<RectTransform>();
    }

    public void ShakeStart()
    {
        if (mainCanvas == null)
            return;

        if (shakining != null)
            StopCoroutine(shakining);
        shakining = StartCoroutine(Shaking());
    }

    IEnumerator Shaking()
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime / duration;
            float curveTime = durationCurve.Evaluate(normalize);

            Vector2 randomCircle = Random.insideUnitCircle * shakeSize * curveTime;
            Vector2 randomPos = new Vector3(randomCircle.x, randomCircle.y);
            mainRect.anchoredPosition = prevPosition + randomPos;
            yield return null;
        }
    }
}
