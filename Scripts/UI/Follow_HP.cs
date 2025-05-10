using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Follow_HP : Follow_Target
{
    [SerializeField] Unit_AI unit;
    public Unit_AI SetUnit { set { unit = value; } }
    //public TMPro.TMP_Text m_Text;

    public Slider hpSlider, actionSlider;
    CanvasGroup actionGroup;
    public Image sliderImage;

    public void SetFollowUnit(Unit_AI _unit)
    {
        unit = _unit;
        SetFollow();
    }

    public override void SetFollow()
    {
        base.SetFollow();
        actionGroup = actionSlider.GetComponent<CanvasGroup>();
        //actionGroup.alpha = 0f;
    }

    public void SetHP(float _current, float _max, bool _shake)
    {
        hpSlider.value = _current / _max;
        //m_Text.text = ((int)Mathf.Clamp(_current, 0, _max)).ToString();
        if (_shake == true)
            ShakeStart();
    }

    public void SetAction(float _value)
    {
        actionSlider.value = _value;

        //if (actionComplate != null)
        //    StopCoroutine(actionComplate);

        //if (_value == 0f)
        //{
        //    actionGroup.alpha = 0f;
        //    //actionComplate = StartCoroutine(ActionComplate());
        //}
    }
    Coroutine actionComplate;
    IEnumerator ActionComplate()
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime;
            actionGroup.alpha = Mathf.Clamp(1f, 0f, normalize);
            yield return null;
        }
    }
}
