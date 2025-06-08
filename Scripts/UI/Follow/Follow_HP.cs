using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Follow_HP : Follow_Target
{
    [Header(" [ Follow_HP ]")]

    public Slider hpSlider;
    public Slider actionSlider;
    CanvasGroup actionGroup;
    public Image sliderImage;
    //[SerializeField] 
    Unit_AI unit;
    public Unit_AI SetUnit { set { unit = value; } }
    //public TMPro.TMP_Text m_Text;
    public float maxHP;
    public void SetFollowUnit(Unit_AI _unit)
    {
        unit = _unit;
        maxHP = _unit.healthPoint;
        SetFollowCamera();
        SetAction(0f);
    }

    public override void SetFollowCamera()
    {
        base.SetFollowCamera();
        actionGroup = actionSlider.GetComponent<CanvasGroup>();
    }

    public void SetHP(float _current, float _max, bool _shake)
    {
        hpSlider.value = _current / maxHP;
        //m_Text.text = ((int)Mathf.Clamp(_current, 0, _max)).ToString();
        if (_current <= 0)
            mainCanvas.alpha = 0f;
        if (_shake == true)
            ShakeStart();
    }

    public void SetAction(float _value)
    {
        actionSlider.value = _value;
        if (_value > 0f)
        {
            actionGroup.alpha = 1f;
        }
        else
        {
            if (actionComplate != null)
                StopCoroutine(actionComplate);
            actionComplate = StartCoroutine(ActionComplate(0f));
        }
    }
    Coroutine actionComplate;
    IEnumerator ActionComplate(float _target)
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 3f;
            actionGroup.alpha = Mathf.Clamp(actionGroup.alpha, _target, normalize);
            yield return null;
        }
    }
}
