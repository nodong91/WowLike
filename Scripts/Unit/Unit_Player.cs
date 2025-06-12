using System;
using System.Collections;
using UnityEngine;

public class Unit_Player : Unit_AI
{
    [Flags]
    public enum ControllDirection
    {
        None = 0, W = 1 << 0, A = 1 << 1, S = 1 << 2, D = 1 << 3
    }
    public ControllDirection controllDirection = ControllDirection.None;
    public Vector2Int dirction;
    public float currentSpeed;

    float moveSpeed = 0.05f;

    public void SetPlayer()
    {
        state = State.Idle;
        currentSpeed = moveSpeed;
        SetMouse();
        SetKeyCode();
    }

    void SetMouse()
    {
        Singleton_Controller.INSTANCE.key_MouseLeft += InputMousetLeft;
        //Singleton_Controller.INSTANCE.key_MouseRight += InputMouseRight;
        //Singleton_Controller.INSTANCE.key_MouseWheel += InputMouseWheel;
    }

    void RemoveMouse()
    {
        Singleton_Controller.INSTANCE.key_MouseLeft -= InputMousetLeft;
        //Singleton_Controller.INSTANCE.key_MouseRight += InputMouseRight;
        //Singleton_Controller.INSTANCE.key_MouseWheel += InputMouseWheel;
    }

    void InputMousetLeft(bool _input)
    {
        if (_input == true)
        {

        }
        else
        {

        }
        State_Attack(_input);
    }

    void SetKeyCode()
    {
        Singleton_Controller.INSTANCE.key_W = Direction_UP;
        Singleton_Controller.INSTANCE.key_A = Direction_Left;
        Singleton_Controller.INSTANCE.key_S = Direction_Down;
        Singleton_Controller.INSTANCE.key_D = Direction_Right;

        Singleton_Controller.INSTANCE.key_SpaceBar = Key_SpaceBar;
        Singleton_Controller.INSTANCE.key_1 = Key_1;
        Singleton_Controller.INSTANCE.key_2 = Key_2;
    }

    void RemoveKeyCode()
    {
        Singleton_Controller.INSTANCE.key_W -= Direction_UP;
        Singleton_Controller.INSTANCE.key_A -= Direction_Left;
        Singleton_Controller.INSTANCE.key_S -= Direction_Down;
        Singleton_Controller.INSTANCE.key_D -= Direction_Right;

        Singleton_Controller.INSTANCE.key_SpaceBar -= Key_SpaceBar;
        Singleton_Controller.INSTANCE.key_1 -= Key_1;
        Singleton_Controller.INSTANCE.key_2 -= Key_2;
    }

    void Direction_UP(bool _input)
    {
        if (_input == true)
        {
            controllDirection |= ControllDirection.W;
            dirction.y += 1;
        }
        else
        {
            controllDirection &= ~ControllDirection.W;
            dirction.y -= 1;
        }
        StateMove();
    }

    void Direction_Left(bool _input)
    {
        if (_input == true)
        {
            controllDirection |= ControllDirection.A;
            dirction.x -= 1;
        }
        else
        {
            controllDirection &= ~ControllDirection.A;
            dirction.x += 1;
        }
        StateMove();
    }

    void Direction_Down(bool _input)
    {
        if (_input == true)
        {
            controllDirection |= ControllDirection.S;
            dirction.y -= 1;
        }
        else
        {
            controllDirection &= ~ControllDirection.S;
            dirction.y += 1;
        }
        StateMove();
    }

    void Direction_Right(bool _input)
    {
        if (_input == true)
        {
            controllDirection |= ControllDirection.D;
            dirction.x += 1;
        }
        else
        {
            controllDirection &= ~ControllDirection.D;
            dirction.x -= 1;
        }
        StateMove();
    }

    void Key_1(bool _input)
    {
        if (_input == true)
            QuickSlot(0);
    }

    void Key_2(bool _input)
    {
        if (_input == true)
            QuickSlot(1);
    }

    public void QuickSlot(int _index)
    {
        Game_Manager.current.QuickSlotAction(_index);
    }

    void Key_SpaceBar(bool _input)
    {
        // 방어 스킬
        // 방패를 가진 무기는 방패막기
        // 양손검은 패링
        // 한손검은 구르기
        // 무기 특징
        if (_input == true)
        {
            StateEscape();
        }
    }

    void Update()
    {
        if (state == State.Move)
            Moving();

        if (state != State.None)
        {
            RotateMousePosition();
        }
    }

    public override void StateMachine(State _state)
    {
        state = _state;

        if (stateAction != null)
            StopCoroutine(stateAction);

        switch (state)
        {
            case State.None:
                //controllDirection = ControllDirection.None;
                break;
            case State.Dead:
                RemoveKeyCode();
                DeadState();
                StateMachine(State.None);
                break;
            case State.Action:
                break;
            case State.Idle:
                if (controllDirection != ControllDirection.None)
                    StateMachine(State.Move);
                break;
            case State.Escape:
                break;
            case State.Move:
                break;
            case State.Patrol:
                break;
            case State.Damage:
                break;
            case State.End:
                break;
        }
    }

    void StateMove()
    {
        if (state == State.Idle)
        {
            StateMachine(State.Move);
        }
        if (state == State.Move && controllDirection == ControllDirection.None)
        {
            StateMachine(State.Idle);
        }
    }

    void StateEscape()
    {
        stateAction = StartCoroutine(MoveEscape());
    }

    IEnumerator MoveEscape()// 탈출 (회피)
    {
        StateMachine(State.Escape);
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 5f;
            Moving();
            currentSpeed = Mathf.Lerp(0.3f, moveSpeed, normalize);
            yield return null;

            float actionTime = unitAnimation.PlayAnimation(4);// 애니메이션
            //yield return new WaitForSeconds(actionTime);// 애니메이션 길이만큼 대기
        }
        StateMachine(State.Idle);
        currentSpeed = moveSpeed;
    }

    void Moving()
    {
        Camera_Manager.current.transform.position = transform.position;
        Vector3 dir = new Vector3(dirction.x, 0f, dirction.y);
        Vector3 target = transform.position + Camera_Manager.current.transform.TransformDirection(dir).normalized;
        transform.position = Vector3.Lerp(transform.position, target, currentSpeed);
        if (state == State.None && controllDirection != ControllDirection.None)
        {
            RotateDirection(target);
        }
    }

    void RotateDirection(Vector3 _target)
    {
        Vector3 offset = (_target - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(offset), currentSpeed * 5f);
    }

    void RotateMousePosition()
    {
        Vector3 playerPosition = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 mousePosition = Input.mousePosition;

        Vector3 uiOffset = (mousePosition - playerPosition).normalized;
        Vector3 dir = new Vector3(uiOffset.x, 0f, uiOffset.y);

        Vector3 target = transform.position + Camera_Manager.current.transform.TransformDirection(dir).normalized;
        Vector3 offset = (target - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(offset), currentSpeed * 5f);
    }










    Coroutine stateAction;
    void State_Attack(bool _input)
    {
        if (_input == true)
        {
            if (state == State.Idle || state == State.Move)
            {
                StateMachine(State.Action);
                stateAction = StartCoroutine(State_Attacking());
            }
        }
        else
        {
            //skillCasting = false;
            //StateMachine(State.Idle);
        }
    }

    IEnumerator State_Attacking()
    {
        string key = GetUnitStruct.defaultSkill01;// 스킬 선택
        if (Singleton_Data.INSTANCE.Dict_Skill.ContainsKey(key))
        {
            SkillStruct skill = new SkillStruct
            {
                skillID = Singleton_Data.INSTANCE.Dict_Skill[key].ID,
                startTime = 0,
                skillStruct = Singleton_Data.INSTANCE.Dict_Skill[key]
            };
            currentSkill = skill;
        }
        float castingTime = currentSkill.skillStruct.castingTime;
        yield return StartCoroutine(SkillCasting(castingTime));// 캐스팅

        float actionTime = unitAnimation.PlayAnimation(3);// 애니메이션
        yield return new WaitForSeconds(actionTime);// 애니메이션 길이만큼 대기

        StateMachine(State.Idle);
    }

    IEnumerator SkillCasting(float _castingTime)
    {
        float casting = 0f;
        skillCasting = true;
        while (skillCasting == true)
        {
            casting += Time.deltaTime;

            deleUpdateAction(casting / _castingTime);
            if (casting > _castingTime)
            {
                skillCasting = false;
            }
            yield return null;
        }
        deleUpdateAction(0f);
    }
}