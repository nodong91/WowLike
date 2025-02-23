using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;



#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AI;
using static Unit_AI;
using static UnityEngine.Rendering.DebugUI;

public class Unit_AI : MonoBehaviour
{
    public NavMeshAgent agent;
    Unit_AI target;

    public enum State
    {
        None,
        Idle,
        Move,
        Chase,
        Escape,
        Attack,
        Damage,
        Dead
    }
    public State state = State.None;
    float distance;
    Coroutine stateMachine;
    public Renderer[] renderers;
    Unit_AI takeTarget;
    Coroutine takeDamage;
    [Header(" [ Status ]")]
    public float unitSize;
    public float healthPoint = 10f;
    public float moveSpeed;
    public Vector2 viewRadius;
    public float viewAngle;


    public enum JobType
    {
        Fighter,
        Ranger
    }
    public JobType jobType = JobType.Fighter;
    Vector3 targetPosition;
    public float attackCoolTime = 1.5f;
    public bool attackCooling = false;
    public float randomValue = 45f;
    float escapeCoolTime = 3f;
    public bool escapeCooling = false;
    Dictionary<Unit_AI, float> aggroDict = new Dictionary<Unit_AI, float>();
    Coroutine aggroCoroutine;
    public LayerMask targetMask, obstacleMask;

    public enum GroupType
    {
        Unit,
        Monster,
    }
    public GroupType groupType;

    public void ResetBattle()
    {
        healthPoint = 10f;
        SetUnit();
    }

    public void SetUnit()
    {
        agent.updateRotation = false;
        agent.speed = moveSpeed;
        StateMachine(State.Idle);
        stateMachine = StartCoroutine(StateMachine());
    }

    void StateMachine(State _state)
    {
        if (state != State.Dead)
        {
            state = _state;
            agent.avoidancePriority = 50 - (int)_state;
        }
    }

    IEnumerator StateMachine()
    {
        while (state != State.Dead)
        {
            switch (state)
            {
                case State.None:

                    break;

                case State.Idle:
                    IdleState();
                    break;

                case State.Move:// 공격할게 없을 때 배회
                    MoveState();
                    float randomTime = Random.Range(0f, 3f);
                    yield return new WaitForSeconds(randomTime);
                    StateMachine(State.Idle);
                    break;

                case State.Chase:
                    ChaseState();
                    break;

                case State.Escape:
                    EscapeState();
                    break;

                case State.Attack:
                    AttackState();
                    yield return new WaitForSeconds(1f);// 글로벌 쿨타임? 애니메이션 길이?
                    StateMachine(State.Idle);
                    break;

                case State.Damage:
                    DamageState();
                    yield return new WaitForSeconds(0.5f);// 애니메이션 길이?
                    StateMachine(State.Idle);
                    break;

                case State.Dead:

                    break;
            }
            yield return null;
            transform.LookAt(target.transform.position);
        }
        DeadState();
    }

    void IdleState()
    {
        agent.isStopped = false;
        if (target == null || target.state == State.Dead) // 타겟이 없는 경우
        {
            float dist = float.MaxValue;
            List<Unit_AI> units = groupType == GroupType.Unit ? Unit_AI_Manager.instance.monsters : Unit_AI_Manager.instance.units;
            for (int i = 0; i < units.Count; i++)
            {
                Unit_AI unit = units[i];
                distance = (unit.transform.position - transform.position).magnitude;
                if (dist > distance)
                {
                    dist = distance;
                    target = unit;
                }
            }
        }
        else
        {
            distance = (target.transform.position - transform.position).magnitude;

            float unitAllSize = target.unitSize + unitSize;
            if (distance + 0.1f < viewRadius.x + unitAllSize && escapeCooling == false)
            {
                StateMachine(State.Escape);
                // 가까워 지면 도망
                targetPosition = GetEscapePoint(target.transform);
                agent.SetDestination(targetPosition);

                StartCoroutine(EscapeCooling());
            }
            else
            {
                StateMachine(State.Chase);
            }
        }
    }

    void ChaseState()
    {
        // 멀면 추적
        targetPosition = GetMovePoint(target.transform, 0f);
        agent.SetDestination(targetPosition);

        //float remainingDistance = agent.remainingDistance;
        distance = (target.transform.position - transform.position).magnitude;
        float unitAllSize = target.unitSize + unitSize;
        if (distance < viewRadius.y + unitAllSize + 0.1f)
        {
            if (attackCooling == false)
            {
                StateMachine(State.Attack);
                StartCoroutine(AttackCooling());
            }
            else
            {

                StateMachine(State.Move);
            }
        }
    }

    void MoveState()// 배회
    {
        float randomIndex = Random.Range(-1f, 1f) * randomValue * 0.5f;
        targetPosition = GetMovePoint(target.transform, randomIndex);
        agent.SetDestination(targetPosition);
    }

    void EscapeState()
    {
        float remainingDistance = agent.remainingDistance;
        if (remainingDistance == 0f)
        {
            StateMachine(State.Idle);
        }
    }

    void AttackState()
    {
        StopMove();
        target.TakeDamage(this, 1f);
    }

    void DamageState()
    {
        StopMove();
    }

    Vector3 GetMovePoint(Transform _from, float _random)
    {
        float angle = GetAngle(_from.position, transform.position);
        float setDistance = target.unitSize + unitSize + viewRadius.y;
        Vector3 dirFromAngle = DirFromAngle(_random + angle, true);
        Vector3 targetPosition = target.transform.position + dirFromAngle * setDistance;

        NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, setDistance, -1);
        {
            return hit.position;
        }
    }

    Vector3 GetEscapePoint(Transform _from)
    {
        float angle = GetAngle(_from.position, transform.position);
        float randomIndex = Random.Range(-1f, 1f) * randomValue * 0.5f;
        float setDistance = target.unitSize + unitSize + viewRadius.x;
        Vector3 dirFromAngle = DirFromAngle(randomIndex + angle, true);
        Vector3 targetPosition = transform.position + dirFromAngle * setDistance;

        NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, setDistance, -1);
        {
            return hit.position;
        }
    }

    IEnumerator AttackCooling()
    {
        attackCooling = true;
        yield return new WaitForSeconds(attackCoolTime);
        attackCooling = false;
    }

    IEnumerator EscapeCooling()
    {
        escapeCooling = true;
        yield return new WaitForSeconds(escapeCoolTime);
        escapeCooling = false;
    }

    float GetAngle(Vector3 from, Vector3 to)
    {
        Vector3 v = to - from;
        return Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;
    }


    public void TakeDamage(Unit_AI _from, float _damage)
    {
        StateMachine(State.Damage);

        healthPoint -= _damage;
        if (healthPoint <= 0)
        {
            StateMachine(State.Dead);
            return;
        }

        takeTarget = _from;
        float aggro = 0f;
        switch (_from.jobType)
        {
            case JobType.Fighter:
                aggro = 1.0f;
                break;

            case JobType.Ranger:
                aggro = 0.9f;
                break;
        }
        target = AddAggro(_from, _damage * aggro);

        if (takeDamage != null)
            StopCoroutine(takeDamage);
        takeDamage = StartCoroutine(TakeDamage());
    }

    IEnumerator TakeDamage()
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 3f;
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetFloat("_Damage", 1f - normalize);
            }
            yield return null;
        }
    }

    void StopMove()
    {
        agent.ResetPath();
        agent.isStopped = true;
    }

    void DeadState()
    {
        Debug.LogWarning("DeadState");
        StopMove();
        switch (groupType)
        {
            case GroupType.Unit:
                Unit_AI_Manager.instance.DeadUnit(this);
                break;

            case GroupType.Monster:
                Unit_AI_Manager.instance.DeadMonster(this);
                break;
        }
        StartCoroutine(DeadActing());
    }

    IEnumerator DeadActing()
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime;
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetFloat("_BlackNWhite", normalize);
            }
            yield return null;
        }
    }








    public Vector3 DirFromAngle(float _angleInDegrees, bool _angleIsGlobal)
    {
        if (_angleIsGlobal == false)
        {
            _angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(_angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(_angleInDegrees * Mathf.Deg2Rad));
    }

    bool VisibleTarget(Transform _target)// 보이는지 확인
    {
        Vector3 dirToTarget = (_target.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle * 0.5f)// 앵글 안에 포함 되는지
        {
            float dstToTarget = (transform.position - _target.position).magnitude;
            if (Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask) == false)
            {
                //visibleTargets.Add(_target);
                return true;
            }
        }
        return false;
    }

    public float CheckDistance()
    {
        //Transform target = Game_Manager.instance.GetTarget;
        if (target != null)
        {
            float targetDistance = Vector3.Distance(target.transform.position, transform.position);
            //UI_Manager.instance.CheckDistance(targetDistance);
            //UI_InvenSlot[] quickSlots = UI_Manager.instance.GetInventory.GetQuickSlot;
            //for (int i = 0; i < quickSlots.Length; i++)
            //{
            //    //Skill_Slot[] slotArray = UI_Manager.instance.slotArray;
            //    quickSlots[i].InDistance(quickSlots[i].skillStruct.distance > targetDistance);
            //}
            return targetDistance;
        }
        return 0f;
    }



    public void BattleOver()
    {
        StopMove();
        StopAllCoroutines();
        Debug.LogWarning("BattleOver : " + gameObject.name);
    }
















    Unit_AI AddAggro(Unit_AI _unit, float _aggro)
    {
        if (aggroDict.ContainsKey(_unit) == true)
            aggroDict[_unit] += _aggro;
        else
            aggroDict[_unit] = _aggro;

        float value = 0f;
        foreach (var child in aggroDict)
        {
            if (child.Value > value)
            {
                value = child.Value;
                _unit = child.Key;
            }
        }

        //if (aggroCoroutine != null)
        //    StopCoroutine(aggroCoroutine);
        //aggroCoroutine = StartCoroutine(DecreaseAggro());
        return _unit;
    }

    IEnumerator DecreaseAggro()
    {
        float value = 1f;
        while (aggroDict.Count > 0 && state != State.Dead)
        {
            foreach (var child in aggroDict)
            {
                if (child.Value > 0)
                {
                    float aggro = child.Value - value;
                    aggroDict[child.Key] = aggro;
                }
                else
                {
                    aggroDict.Remove(child.Key);
                }
            }
            yield return new WaitForSeconds(value);
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360f, unitSize);

        Handles.color = (groupType == GroupType.Unit) ? Color.red : Color.blue;
        Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360f, unitSize + viewRadius.x);
        Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360f, unitSize + viewRadius.y);

        Vector3 viewAngleA = DirFromAngle(viewAngle * 0.5f, false);
        Vector3 viewAngleB = DirFromAngle(-viewAngle * 0.5f, false);

        Handles.DrawLine(transform.position, transform.position + viewAngleA * (unitSize + viewRadius.y));
        Handles.DrawLine(transform.position, transform.position + viewAngleB * (unitSize + viewRadius.y));

        Color color = Color.green;
        GUIStyle fontStyle = new()
        {
            fontSize = 20,
            normal = { textColor = color },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
        };

        if (target != null)
        {
            Handles.color = Gizmos.color = color;
            Handles.DrawLine(transform.position, target.transform.position);
            string visibleTarget = VisibleTarget(target.transform) ? "0000FF" : "FF0000";
            visibleTarget = $"<color=#{visibleTarget}>{VisibleTarget(target.transform)}</color>";
            Vector3 textPosition = Vector3.Lerp(transform.position, target.transform.position, 0.5f);
            Handles.Label(textPosition, $"( {visibleTarget} : {CheckDistance().ToString("N2")} )", fontStyle);
            Gizmos.DrawSphere(targetPosition, 0.3f);
        }

        fontStyle.fontSize = 30;
        string hp = healthPoint > 0 ? "00FF00" : "FFFFFF";
        hp = $"<color=#{hp}>{state} : {healthPoint}</color>";
        Handles.Label(transform.position, hp, fontStyle);
    }
#endif
}
