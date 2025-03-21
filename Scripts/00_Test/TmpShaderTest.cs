using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TmpShaderTest : MonoBehaviour
{
    public TMPro.TMP_Text tmpText;
    Material tmpMaterial;
    public AnimationCurve aniCurve;
    public DialogTest dialogTest;
    public string dialogID;

    public float duration;
    Coroutine glitching;
    public Vector2 glitchSize;
    public Transform parent;
    Vector2 prevPosition;

    void Start()
    {
        tmpMaterial = tmpText.fontMaterial;
        prevPosition = tmpText.rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //InstanceText();
            //GlitchStart();
            dialogTest.SetDialog(dialogID);
        }
    }

    void GlitchStart()
    {
        if (glitching != null)
            StopCoroutine(glitching);
        glitching = StartCoroutine(Shaking());
    }

    IEnumerator Glitching()
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime / duration;
            float curveTime = aniCurve.Evaluate(normalize);
            for (int i = 0; i < instText.Count; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * glitchSize * curveTime;
                Vector2 randomPos = new Vector3(randomCircle.x, randomCircle.y);
                instText[i].rectTransform.anchoredPosition = prevPosition + randomPos;
                instText[i].alpha = curveTime;
            }
            tmpText.alpha = 1f - curveTime;
            tmpMaterial.SetFloat("_LineFraction", curveTime);
            yield return null;
        }
        tmpText.alpha = 1f;
    }

    IEnumerator Shaking()
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime / duration;
            float curveTime = aniCurve.Evaluate(normalize);

            Vector2 randomCircle = Random.insideUnitCircle * glitchSize * curveTime;
            Vector2 randomPos = new Vector3(randomCircle.x, randomCircle.y);
            tmpText.rectTransform.anchoredPosition = prevPosition + randomPos;
            tmpText.alpha = curveTime;
            tmpMaterial.SetFloat("_LineFraction", curveTime);
            yield return null;
        }
    }

    List<TMPro.TMP_Text> instText = new List<TMPro.TMP_Text>();
    void InstanceText()
    {
        for (int i = 0; i < instText.Count; i++)
        {
            Destroy(instText[i].gameObject);
        }
        instText.Clear();
        TMPro.TMP_Text inst = Instantiate(tmpText, parent);
        inst.color = Color.red;
        instText.Add(inst);
        inst = Instantiate(tmpText, parent);
        inst.color = Color.green;
        instText.Add(inst);
        inst = Instantiate(tmpText, parent);
        inst.color = Color.blue;
        instText.Add(inst);
    }
}
