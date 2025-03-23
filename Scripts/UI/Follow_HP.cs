using UnityEngine;
using UnityEngine.UI;

public class Follow_HP : Follow_Target
{
    public Unit_AI unit;
    public Unit_AI SetUnit { set { unit = value; } }
    public TMPro.TMP_Text m_Text;

    public Slider hpSlider;

    public void SetHP(float _current, float _max)
    {
        hpSlider.value = _current / _max;
        m_Text.text = ((int)Mathf.Clamp(_current, 0, _max)).ToString();
        ShackStart();
    }
}
