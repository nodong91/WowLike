using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class Unit_AI : MonoBehaviour
{
    public NavMeshAgent agent;
    Unit_AI target;
    Dictionary<Unit_AI, float> aggroDict = new Dictionary<Unit_AI, float>();
    public Data_Manager.UnitStruct unitStruct;
    public Data_Manager.UnitStruct.UnitAttributes unitAttributes;

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
    Renderer[] renderers;
    Coroutine stateMachine, takeDamage;
    [Header(" [ Status ]")]
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

    public float GetCoolTime { get { return currentSkill.coolingTime; } }
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

    public delegate void DeadUnit(Unit_AI _unit);
    public DeadUnit deadUnit;

    public Data_Manager.SkillStruct currentSkill;

    public class SkillStruct
    {
        public Data_Manager.SkillStruct skillStruct;
        public float coolTime;
    }
    public SkillStruct skill_01, skill_02;

    public void SetUnitStruct(Data_Manager.UnitStruct _unitStruct)
    {
        unitStruct = _unitStruct;
        unitAttributes = unitStruct.TryAttributes();
        skill_01 = new SkillStruct
        {
            skillStruct = Singleton_Data.INSTANCE.Dict_Skill[unitStruct.defaultSkill01],
            coolTime = 0
        };
        skill_02 = new SkillStruct
        {
            skillStruct = Singleton_Data.INSTANCE.Dict_Skill[unitStruct.defaultSkill02],
            coolTime = 0
        };
        agent.speed = unitAttributes.MoveSpeed;
        healthPoint = unitAttributes.Health;

        currentSkill = skill_01.skillStruct;
        renderers = GetComponentsInChildren<Renderer>();
    }

    public float GetUnitSize { get { return unitStruct.unitSize; } }
    public float healthPoint = 10f;
    public float GetDamage
    {
        get
        {
            float ap = unitAttributes.AttackPower;
            float sp = unitAttributes.SpellPower;
            float rp = unitAttributes.RangePower;
            return currentSkill.GetDamage(ap, sp, rp);
        }
    }
    public Vector2 GetSkillRange { get { return currentSkill.range; } }
    public void SetUnit()
    {
        agent.updateRotation = false;
        healthPoint = unitAttributes.Health;

        if (state == State.Dead)
        {
            Rebirth();
        }

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
                    break;

                case State.Damage:
                    DamageState();
                    yield return new WaitForSeconds(3f / 1f);// 애니메이션 길이?
                    if (state == State.Damage)
                    {
                        StateMachine(State.Idle);
                    }
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

            float unitAllSize = target.GetUnitSize + GetUnitSize;
            if (distance + 0.1f < GetSkillRange.x + unitAllSize && escapeCooling == false)
            {
                StateMachine(State.Escape);
                // 가까워 지면 도망
                float randomIndex = Random.Range(-1f, 1f) * randomValue * 0.5f;
                targetPosition = GetBackPoint(target.transform.position, randomIndex);
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
        targetPosition = GetFrontPoint(target.transform.position, 0f);
        Destination(targetPosition);

        //float remainingDistance = agent.remainingDistance;
        distance = (target.transform.position - transform.position).magnitude;
        float unitAllSize = target.GetUnitSize + GetUnitSize;
        if (distance < GetSkillRange.y + unitAllSize + 0.1f)
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
        targetPosition = GetFrontPoint(target.transform.position, randomIndex);
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
        float damage = GetDamage;
        target.TakeDamage(this, transform.position, damage, currentSkill);
    }

    void DamageState()
    {

    }

    void DeadState()
    {
        deadUnit?.Invoke(this);
        Destination(transform.position);
        StartCoroutine(DeadActing());
    }

    Vector3 GetFrontPoint(Vector3 _from, float _random)
    {
        float angle = GetAngle(_from, transform.position);
        float setDistance = target.GetUnitSize + GetUnitSize + GetSkillRange.y;
        Vector3 dirFromAngle = DirFromAngle(_random + angle, null);
        Vector3 targetPosition = target.transform.position + dirFromAngle * setDistance;

        NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, setDistance, -1);
        {
            return hit.position;
        }
    }

    Vector3 GetBackPoint(Vector3 _from, float _random, float _dist = 0)
    {
        float angle = GetAngle(_from, transform.position);
        float setDistance = _dist > 0 ? _dist : target.GetUnitSize + GetUnitSize + GetSkillRange.x;
        Vector3 dirFromAngle = DirFromAngle(_random + angle, null);
        Vector3 targetPosition = transform.position + dirFromAngle * setDistance;

        NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, setDistance, -1);
        {
            return hit.position;
        }
    }

    IEnumerator AttackCooling()
    {
        attackCooling = true;
        yield return new WaitForSeconds(GetCoolTime);
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

    public void TakeDamage(Unit_AI _from, Vector3 _center, float _damage, Data_Manager.SkillStruct _skillStruct)
    {
        Debug.Log($"{_from.gameObject.name} : {_damage}");
        healthPoint -= _damage;
        if (healthPoint <= 0)
        {
            StateMachine(State.Dead);
            return;
        }

        StateMachine(State.Damage);

        target = AddAggro(_from, _damage * _skillStruct.aggro);// 어그로 추가 후 타겟 변경

        if (takeDamage != null)
            StopCoroutine(takeDamage);
        switch (_skillStruct.ccType)
        {
            case Data_Manager.SkillStruct.CCType.Normal:
                takeDamage = StartCoroutine(TakeDamage());
                break;

            case Data_Manager.SkillStruct.CCType.KnockBack:
                takeDamage = StartCoroutine(CCDamage(_center));
                break;
        }
    }

    IEnumerator CCDamage(Vector3 _from)
    {
        Vector3 targetPoint = GetBackPoint(_from, 0f, 1f);
        if (float.IsInfinity(targetPoint.x))
            targetPoint = transform.position;

        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 3f;
            SetRanderer_Damage(normalize);

            Vector3 position = Vector3.Lerp(transform.position, targetPoint, normalize);
            transform.position = position;
            Destination(position);
            yield return null;
        }
    }

    IEnumerator TakeDamage()
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 3f;
            SetRanderer_Damage(normalize);
            yield return null;
        }
    }

    void SetRanderer_Damage(float _normalize)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.SetFloat("_Damage", 1f - _normalize);
        }
    }

    void SetRanderer_Dead(float _targetAmount, float _normalize)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            float deadAmount = Mathf.Lerp(1f - _targetAmount, _targetAmount, _normalize);
            renderers[i].material.SetFloat("_BlackNWhite", deadAmount);
        }
    }

    void Destination(Vector3 _point)
    {
        //agent.ResetPath();
        //agent.isStopped = true;
        agent.SetDestination(_point);
    }

    IEnumerator DeadActing()
    {
        float targetAmount = state == State.Dead ? 1f : 0f;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime;
            SetRanderer_Dead(targetAmount, normalize);
            yield return null;
        }
    }

    public Vector3 DirFromAngle(float _angleInDegrees, Transform _trans = null)
    {
        if (_trans != null)
        {
            // 로컬 기준
            _angleInDegrees += _trans.eulerAngles.y;
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
            float value = child.Value;

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
            Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360f, GetUnitSize);

            Handles.color = (groupType == GroupType.Unit) ? Color.red : Color.blue;
            Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360f, GetUnitSize + GetSkillRange.x);
            Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360f, GetUnitSize + GetSkillRange.y);

            Vector3 viewAngleA = DirFromAngle(viewAngle * 0.5f, transform);
            Vector3 viewAngleB = DirFromAngle(-viewAngle * 0.5f, transform);

            Handles.DrawLine(transform.position, transform.position + viewAngleA * (GetUnitSize + GetSkillRange.y));
            Handles.DrawLine(transform.position, transform.position + viewAngleB * (GetUnitSize + GetSkillRange.y));

            if (target != null)
            {
                Handles.color = Gizmos.color = color;
                Handles.DrawLine(transform.position, target.transform.position);
                string visibleTarget = VisibleTarget(target.transform) ? "0000FF" : "FF0000";
                visibleTarget = $"<color=#{visibleTarget}>{VisibleTarget(target.transform)}</color>";
                Vector3 textPosition = Vector3.Lerp(transform.position, target.transform.position, 0.5f);
                Handles.Label(textPosition, $"( {visibleTarget} : {CheckDistance():N2} )", fontStyle);
                Gizmos.DrawSphere(targetPosition, 0.3f);
            }
        }
        string hp = healthPoint > 0 ? "00FF00" : "FFFFFF";
        hp = $"<color=#{hp}>{state}\n{healthPoint:N2}</color>";
        Handles.Label(transform.position, hp, fontStyle);
    }
#endif
}
