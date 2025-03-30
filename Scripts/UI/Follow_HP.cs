using UnityEngine;
using UnityEngine.UI;
using static Data_Manager.UnitStruct;

public class Follow_HP : Follow_Target
{
    [SerializeField] Unit_AI unit;
    public Unit_AI SetUnit { set { unit = value; } }
    public TMPro.TMP_Text m_Text;

    public Slider hpSlider;
    public Image sliderImage;

    public void SetFollowUnit(Unit_AI _unit)
    {
        unit = _unit;
        followType = FollowType.Camera;
        followOffset = new Vector3(0f, 1f, 0f);
    }

    public void SetHP(float _current, float _max)
    {
        hpSlider.value = _current / _max;
        m_Text.text = ((int)Mathf.Clamp(_current, 0, _max)).ToString();
        ShackStart();
    }
}
