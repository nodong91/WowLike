using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class Unit_AI : MonoBehaviour
{
    public NavMeshAgent agent;
    Unit_AI target;
    Dictionary<Unit_AI, float> aggroDict = new Dictionary<Unit_AI, float>();

    public delegate List<Unit_AI> UNITLIST();
    public UNITLIST unitList;
    public UNITLIST monsterList;

    public enum State
    {
        None,
        Dead,
        Attack,
        Idle,
        Escape,
        Move,
        Chase,
        Damage,
    }
    public State state = State.None;
    float distance;
    public Renderer[] renderers;
    Unit_AI takeTarget;
    Coroutine stateMachine, takeDamage;
    [Header(" [ Status ]")]
    public float unitSize;
    public float healthPoint = 10f;
    public float damage = 1f;
    public float moveSpeed;
    public Vector2 viewRadius;
    public float viewAngle;

    public enum JobType
    {
        Fighter,
        Ranger,
        Sorcerer
    }
    public JobType jobType = JobType.Fighter;
    Vector3 targetPosition;
    public float randomValue = 45f;

    public float attackCoolTime = 1.5f;
    public bool attackCooling = false;
    float escapeCoolTime = 3f;
    public bool escapeCooling = false;

    Coroutine aggroCoroutine;
    public LayerMask targetMask, obstacleMask;

    public enum GroupType
    {
        Unit,
        Monster,
    }
    public GroupType groupType;








    public void SetUnit()
    {
        agent.updateRotation = false;
        agent.speed = moveSpeed;

        unitList = Unit_AI_Manager.instance.UnitList;
        monsterList = Unit_AI_Manager.instance.MonsterList;
    }

    public void ResetBattle()
    {
        if (state == State.Dead)
        {
            Rebirth();
        }
        healthPoint = 10f;
        target = null;
        attackCooling = false;
        escapeCooling = false;
        aggroDict.Clear();

        StateMachine(State.Idle);
        StartBattle();
    }

    void Rebirth()
    {
        state = State.None;
        StartCoroutine(DeadActing());
    }

    public void CommendMoveing(Vector3 _point)
    {
        target = null;
        StateMachine(State.None);
        Destination(_point);
    }

    void StateMachine(State _state)
    {
        state = _state;
        agent.avoidancePriority = (int)_state;
        //StartBattle();
    }

    void StartBattle()
    {
        if (stateMachine != null)
            StopCoroutine(stateMachine);
        stateMachine = StartCoroutine(StateMachine());
    }

    IEnumerator StateMachine()
    {
        while (state != State.Dead)
        {
            Debug.LogWarning(gameObject.name);
            switch (state)
            {
                case State.None:

                    break;

                case State.Idle:
                    IdleState();
                    break;

                case State.Move:// 공격할게 없을 때 배회
                    MoveState();
                    float randomTime = Random.Range(1f, 3f);
                    yield return new WaitForSeconds(randomTime);
                    if (state == State.Move)
                    {
                        StateMachine(State.Idle);
                    }
                    //else
                    //{
                    //    Debug.LogError("State.Move" + state);
                    //}
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
                    if (state == State.Attack)
                    {
                        StateMachine(State.Idle);
                    }
                    //else
                    //{
                    //    Debug.LogError("State.Attack" + state);
                    //}
                    break;

                case State.Damage:
                    DamageState();
                    yield return new WaitForSeconds(3f / 1f);// 애니메이션 길이?
                    if (state == State.Damage)
                    {
                        StateMachine(State.Idle);
                    }
                    //else
                    //{
                    //    Debug.LogError("State.Damage" + state);
                    //}
                    break;

                case State.Dead:

                    break;
            }
            yield return null;
            if (target != null)
            {
                transform.LookAt(target.transform.position);
            }
        }
        DeadState();
    }

    void IdleState()
    {
        if (target == null || target.state == State.Dead) // 타겟이 없는 경우
        {
            float dist = float.MaxValue;
            List<Unit_AI> units = groupType == GroupType.Unit ? monsterList() : unitList();
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
                float randomIndex = Random.Range(-1f, 1f) * randomValue * 0.5f;
                targetPosition = GetBackPoint(target.transform, randomIndex);
                Destination(targetPosition);

                StartCoroutine(EscapeCooling());// 계속 도망만 치면 한대도 못때림
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
        targetPosition = GetFrontPoint(target.transform, 0f);
        Destination(targetPosition);

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
        targetPosition = GetFrontPoint(target.transform, randomIndex);
        Destination(targetPosition);
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
        Destination(transform.position);
        target.TakeDamage(this, damage);
    }

    void DamageState()
    {
        Vector3 targetPoint = GetBackPoint(takeTarget.transform, 0f, 1f);
        if (float.IsInfinity(targetPoint.x))
            targetPoint = transform.position;

        if (takeDamage != null)
            StopCoroutine(takeDamage);
        takeDamage = StartCoroutine(TakeDamage(targetPoint));
        //KnockBack();
    }

    Vector3 GetFrontPoint(Transform _from, float _random)
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

    Vector3 GetBackPoint(Transform _from, float _random, float _dist = 0)
    {
        float angle = GetAngle(_from.position, transform.position);
        float setDistance = _dist > 0 ? _dist : target.unitSize + unitSize + viewRadius.x;
        Vector3 dirFromAngle = DirFromAngle(_random + angle, true);
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
        healthPoint -= _damage;
        if (healthPoint <= 0)
        {
            StateMachine(State.Dead);
            return;
        }

        takeTarget = _from;
        StateMachine(State.Damage);

        float aggro = 0f;
        switch (_from.jobType)
        {
            case JobType.Fighter:
                aggro = 1.0f;
                break;

            case JobType.Ranger:
                aggro = 0.5f;
                break;

            case JobType.Sorcerer:
                aggro = 0.3f;
                break;
        }
        target = AddAggro(_from, _damage * aggro);
    }

    IEnumerator TakeDamage(Vector3 _knockBack)
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 3f;

            Vector3 position = Vector3.Lerp(transform.position, _knockBack, normalize);
            transform.position = position;
            Destination(position);

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetFloat("_Damage", 1f - normalize);
            }
            yield return null;
        }
    }

    void Destination(Vector3 _point)
    {
        //agent.ResetPath();
        //agent.isStopped = true;
        agent.SetDestination(_point);
    }

    void DeadState()
    {
        Destination(transform.position);
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
        float targetAmount = state == State.Dead ? 1f : 0f;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime;
            for (int i = 0; i < renderers.Length; i++)
            {
                float deadAmount = Mathf.Lerp(1f - targetAmount, targetAmount, normalize);
                renderers[i].material.SetFloat("_BlackNWhite", deadAmount);
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
                return true;
            }
        }
        return false;
    }

    public float CheckDistance()
    {
        if (target != null)
        {
            float targetDistance = Vector3.Distance(target.transform.position, transform.position);
            return targetDistance;
        }
        return 0f;
    }

    public void BattleOver()
    {
        Destination(transform.position);
        StopAllCoroutines();
        Debug.LogWarning("BattleOver : " + gameObject.name);
    }
















    Unit_AI AddAggro(Unit_AI _unit, float _aggro)
    {
        if (aggroDict.ContainsKey(_unit) == true)
        {
            aggroDict[_unit] += _aggro;
        }
        else
        {
            aggroDict[_unit] = _aggro;
        }
        //if (aggroCoroutine == null)
        //    aggroCoroutine = StartCoroutine(DecreaseAggro());
        return _unit;
    }

    void SetAggro()
    {
        testAggro.Clear();
        tempDict.Clear();
        foreach (var child in aggroDict)
        {
            Unit_AI unit = child.Key;
            float value = child.Value ;

            if (value > 0f)
            {
                tempDict[unit] = value;
                testAggro.Add(value);
            }
        }
        aggroDict = tempDict;
    }
    Dictionary<Unit_AI, float> tempDict = new Dictionary<Unit_AI, float>();
    public List<float> testAggro = new List<float>();

    IEnumerator DecreaseAggro()
    {
        float value = 1f;
        while (state != State.Dead)
        {
            SetAggro();
            yield return new WaitForSeconds(value);
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color color = Color.green;
        GUIStyle fontStyle = new()
        {
            fontSize = 20,
            normal = { textColor = color },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
        };

        if (state != State.Dead)
        {
            Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360f, unitSize);

            Handles.color = (groupType == GroupType.Unit) ? Color.red : Color.blue;
            Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360f, unitSize + viewRadius.x);
            Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360f, unitSize + viewRadius.y);

            Vector3 viewAngleA = DirFromAngle(viewAngle * 0.5f, false);
            Vector3 viewAngleB = DirFromAngle(-viewAngle * 0.5f, false);

            Handles.DrawLine(transform.position, transform.position + viewAngleA * (unitSize + viewRadius.y));
            Handles.DrawLine(transform.position, transform.position + viewAngleB * (unitSize + viewRadius.y));

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
        }
        fontStyle.fontSize = 30;
        string hp = healthPoint > 0 ? "00FF00" : "FFFFFF";
        hp = $"<color=#{hp}>{state} : {healthPoint}</color>";
        Handles.Label(transform.position, hp, fontStyle);
    }
#endif
}
