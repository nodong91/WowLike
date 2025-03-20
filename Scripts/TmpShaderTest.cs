using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TmpShaderTest : MonoBehaviour
{
    public TMPro.TMP_Text tmpText;
    public Material tmpMaterial;
    public float test;
    public AnimationCurve aniCurve;

    void Start()
    {
        tmpMaterial = tmpText.fontMaterial;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (glitching != null)
                StopCoroutine(glitching);
            glitching = StartCoroutine(Glitching());
        }
    }

    public float duration;
    Coroutine glitching;
    public Vector2 fffff;
    public Transform parent;

    IEnumerator Glitching()
    {
        InstanceText();
        Vector3 prevPosition = tmpText.transform.localPosition;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime / duration;
            float curveTime = aniCurve.Evaluate(normalize);
            for (int i = 0; i < instText.Count; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * fffff * curveTime;
                Vector3 fffffsdf = new Vector3(randomCircle.x, randomCircle.y, 0f);
                instText[i].transform.localPosition = prevPosition + fffffsdf;
                instText[i].alpha = curveTime;
            }
            tmpText.alpha = 1f - curveTime;
            tmpMaterial.SetFloat("_LineFraction", curveTime);
            yield return null;
        }
        tmpText.alpha = 1f;
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
