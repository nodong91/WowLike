using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Unit_Player : Unit_AI
{
    [Flags]
    public enum ControllDirection
    {
        None = 0, W = 1 << 0, A = 1 << 1, S = 1 << 2, D = 1 << 3
    }
    public ControllDirection controllDirection = ControllDirection.None;

    public void SetPlayer()
    {
        state = State.Idle;
        currentSpeed = moveSpeed;
        SetKeyCode();
    }

    void SetKeyCode()
    {
        Singleton_Controller.INSTANCE.key_W = Direction_UP;
        Singleton_Controller.INSTANCE.key_A = Direction_Left;
        Singleton_Controller.INSTANCE.key_S = Direction_Down;
        Singleton_Controller.INSTANCE.key_D = Direction_Right;

        Singleton_Controller.INSTANCE.key_SpaceBar = Key_SpaceBar;
    }

    void RemoveKeyCode()
    {
        Singleton_Controller.INSTANCE.key_W -= Direction_UP;
        Singleton_Controller.INSTANCE.key_A -= Direction_Left;
        Singleton_Controller.INSTANCE.key_S -= Direction_Down;
        Singleton_Controller.INSTANCE.key_D -= Direction_Right;

        Singleton_Controller.INSTANCE.key_SpaceBar -= Key_SpaceBar;
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
    public Vector2Int dirction;

    void Key_SpaceBar(bool _input)
    {
        if (_input == true)
        {
            StateEscape();
        }
    }
    public float currentSpeed;
    void Update()
    {
        if (controllDirection != ControllDirection.None)
        {
            Moving();
        }
        RotateMousePosition();
    }

    public override void StateMachine(State _state)
    {
        state = _state;
        switch (state)
        {
            case State.None:
                break;
            case State.Dead:
                RemoveKeyCode();
                DeadState();
                controllDirection = ControllDirection.None;
                break;
            case State.Attack:
                break;
            case State.Idle:
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
        if (state != State.None)
        {
            if (controllDirection == ControllDirection.None)
            {
                state = State.Idle;
            }
            else
            {
                state = State.Move;
            }
        }
    }
    void Moving()
    {
        if (state == State.None)
            return;

        Camera_Manager.current.transform.position = transform.position;
        Vector3 dir = new Vector3(dirction.x, 0f, dirction.y);
        Vector3 target = transform.position + Camera_Manager.current.transform.TransformDirection(dir).normalized;
        transform.position = Vector3.Lerp(transform.position, target, currentSpeed);
        Vector3 offset = (target - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(offset), currentSpeed * 5f);
    }

    Coroutine moveEscape;
    void StateEscape()
    {
        if (moveEscape != null)
            StopCoroutine(moveEscape);
        moveEscape = StartCoroutine(MoveEscape());
    }
    float moveSpeed = 0.05f;
    IEnumerator MoveEscape()
    {
        state = State.Escape;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 5f;
            Moving();
            currentSpeed = Mathf.Lerp(0.3f, moveSpeed, normalize);
            yield return null;
        }
        state = State.Idle;
        currentSpeed = moveSpeed;
    }

    public RectTransform test1,test2;
    void RotateMousePosition()
    {
        Vector3 playerPosition = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 mousePosition = Input.mousePosition;
        test1.position = playerPosition;
        test2.position = mousePosition;
    }
}