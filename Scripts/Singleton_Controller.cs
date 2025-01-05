using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton_Controller : MonoSingleton<Singleton_Controller>
{
    public delegate void Holder(bool _input);
    Dictionary<KeyCode, Holder> keyCodeSets;

    private void Update()
    {
        foreach (var dic in keyCodeSets)
        {
            if (Input.GetKeyUp(dic.Key))
            {
                dic.Value(false);
            }
            else if (Input.GetKeyDown(dic.Key))
            {
                dic.Value(true);
                Debug.LogWarning(dic.Key);
            }
        }
        Mouse_Wheel(Input.GetAxis("Mouse ScrollWheel"));
    }

    public void SetController()
    {
        keyCodeSets = new Dictionary<KeyCode, Holder>
        {
            { KeyCode.Q, Key_Q },
            { KeyCode.W, Key_W },
            { KeyCode.E, Key_E },
            { KeyCode.R, Key_R },
            { KeyCode.A, Key_A },
            { KeyCode.S, Key_S },
            { KeyCode.D, Key_D },
            { KeyCode.F, Key_F },
            { KeyCode.Tab, Key_Tab },
            { KeyCode.Escape, Key_Esc },
            { KeyCode.LeftShift, Key_LeftShift },
            { KeyCode.Space, Key_SpaceBar },
            //{ KeyCode.Alpha1, QuickSlot_01 },
            //{ KeyCode.Alpha2, QuickSlot_02 },
            //{ KeyCode.Alpha3, QuickSlot_03 },
            //{ KeyCode.Alpha4, QuickSlot_04 },
            //{ KeyCode.Alpha5, QuickSlot_05 },
            //{ KeyCode.Alpha6, QuickSlot_06 },
            //{ KeyCode.Alpha7, QuickSlot_07 },
            //{ KeyCode.Alpha8, QuickSlot_08 },
            { KeyCode.Mouse0, Mouse_Left },
            { KeyCode.Mouse1, Mouse_Right }
        };
    }
    public delegate void Key_Bool(bool _input);
    public Key_Bool key_Q;
    public Key_Bool key_W;
    public Key_Bool key_E;
    public Key_Bool key_R;
    public Key_Bool key_A;
    public Key_Bool key_S;
    public Key_Bool key_D;
    public Key_Bool key_F;
    public Key_Bool key_Tab;
    public Key_Bool key_Esc;
    public Key_Bool key_LeftShift;
    public Key_Bool key_SpaceBar;
    public Key_Bool key_MouseLeft;
    public Key_Bool key_MouseRight;
    public Key_Bool key_MouseWheel;

    void Key_Q(bool _input) { key_Q?.Invoke(_input); }

    void Key_W(bool _input) { key_W?.Invoke(_input); }

    void Key_E(bool _input) { key_E?.Invoke(_input); }

    void Key_R(bool _input) { key_R?.Invoke(_input); }

    void Key_A(bool _input) { key_A?.Invoke(_input); }

    void Key_S(bool _input) { key_S?.Invoke(_input); }

    void Key_D(bool _input) { key_D?.Invoke(_input); }

    void Key_F(bool _input) { key_F?.Invoke(_input); }

    void Key_Tab(bool _input) { key_Tab?.Invoke(_input); }

    void Key_Esc(bool _input) { key_Esc?.Invoke(_input); }

    void Key_LeftShift(bool _input) { key_LeftShift?.Invoke(_input); }

    void Key_SpaceBar(bool _input) { key_SpaceBar?.Invoke(_input); }

    void Mouse_Left(bool _input) { key_MouseLeft?.Invoke(_input); }

    void Mouse_Right(bool _input) { key_MouseRight?.Invoke(_input); }

    void Mouse_Wheel(float _value)
    {
        if (_value > 0)
        {
            key_MouseWheel?.Invoke(true);
        }
        else if (_value < 0)
        {
            key_MouseWheel?.Invoke(false);
        }
    }
}
