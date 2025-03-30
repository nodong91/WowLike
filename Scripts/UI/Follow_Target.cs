using System.Collections;
using UnityEngine;

//[ExecuteInEditMode]
public class Follow_Target : MonoBehaviour
{
    public enum FollowType
    {
        Overlay,
        Camera,
        HP
    }
    public FollowType followType;
    private Transform followTarget;
    public Vector3 followOffset;
    public RectTransform shackRect;




    //public TMPro.TMP_Text tmpText;
    //Material tmpMaterial;


    //[ColorUsage(true, true)]
    //public Color _FaceColor;
    //[Range(0f, 60f)]
    //public float _FramePerSecond;
    //public float _LineSpeed;
    //public float _LineSize;
    //[Range(0f, 360f)]
    //public float _LineRotate;
    //[Range(0f, 1f)]
    //public float _LineFraction;
    //public Vector2 _LineDistortion;
    //public float _WaveSpeed;
    //public float _WaveSize;
    //[Range(0f, 10f)]
    //public float _WaveDistortion;

    //void Start()
    //{
    //    tmpMaterial = tmpText.fontMaterial;
    //}

    //void Update()
    //{
    //    if (tmpMaterial != null)
    //    {
    //        tmpMaterial.SetColor("_FaceColor", _FaceColor);
    //        tmpMaterial.SetFloat("_FramePerSecond", _FramePerSecond);
    //        tmpMaterial.SetFloat("_LineSpeed", _LineSpeed);
    //        tmpMaterial.SetFloat("_LineSize", _LineSize);
    //        tmpMaterial.SetFloat("_LineRotate", _LineRotate);
    //        tmpMaterial.SetFloat("_LineFraction", _LineFraction);
    //        tmpMaterial.SetVector("_LineDistortion", _LineDistortion);
    //        tmpMaterial.SetFloat("_WaveSpeed", _WaveSpeed);
    //        tmpMaterial.SetFloat("_WaveSize", _WaveSize);
    //        tmpMaterial.SetFloat("_WaveDistortion", _WaveDistortion);
    //    }
    //}

    public float duration;
    public AnimationCurve durationCurve;
    Coroutine shakining;
    public Vector2 shakSize;
    Vector2 prevPosition;

    public void ShackStart()
    {
        if (shackRect == null)
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

            Vector2 randomCircle = Random.insideUnitCircle * shakSize * curveTime;
            Vector2 randomPos = new Vector3(randomCircle.x, randomCircle.y);
            shackRect.anchoredPosition = prevPosition + randomPos;
            //tmpText.alpha = curveTime;
            //tmpMaterial.SetFloat("_LineFraction", curveTime);
            yield return null;
        }
    }
}
