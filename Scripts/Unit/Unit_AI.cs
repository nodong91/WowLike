using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Unit_AI : MonoBehaviour
{
    public string unitID = "U10010";
    public NavMeshAgent agent;
    Unit_AI target;
    Dictionary<Unit_AI, float> aggroDict = new Dictionary<Unit_AI, float>();
    public Data_Manager.UnitStruct unitStruct;
    public Data_Manager.UnitStruct.UnitAttributes unitAttributes;

    public delegate List<Unit_AI> DeleUnitList();
    public DeleUnitList unitList;
    public DeleUnitList monsterList;

    public enum State
    {
        None,
        Dead,
        Skill,
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

    //public enum JobType
    //{
    //    Fighter,
    //    Ranger,
    //    Sorcerer
    //}
    //public JobType jobType = JobType.Fighter;
    Vector3 targetPosition;
    public float randomValue = 45f;

    public float GetCoolTime { get { return currentSkill.skillStruct.coolingTime; } }
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

    public SkillStruct currentSkill;
    [System.Serializable]
    public class SkillStruct
    {
        [HideInInspector] public string skillID;
        public Data_Manager.SkillStruct skillStruct;
    }
    public List<SkillStruct> readySkills = new List<SkillStruct>();
    public void SetUnitStruct()
    {
        unitStruct = Singleton_Data.INSTANCE.Dict_Unit[unitID];
        //unitStruct = _unitStruct;
        unitAttributes = unitStruct.TryAttributes();
        SkillStruct skill_01 = new SkillStruct
        {
            skillID = Singleton_Data.INSTANCE.Dict_Skill[unitStruct.defaultSkill01].ID,
            skillStruct = Singleton_Data.INSTANCE.Dict_Skill[unitStruct.defaultSkill01]
        };
        readySkills.Add(skill_01);
        SkillStruct skill_02 = new SkillStruct
        {
            skillID = Singleton_Data.INSTANCE.Dict_Skill[unitStruct.defaultSkill02].ID,
            skillStruct = Singleton_Data.INSTANCE.Dict_Skill[unitStruct.defaultSkill02]
        };
        readySkills.Add(skill_02);
        agent.speed = unitAttributes.MoveSpeed;
        healthPoint = unitAttributes.Health;

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
            return currentSkill.skillStruct.GetDamage(ap, sp, rp);
        }
    }
    public Vector2 GetSkillRange { get { return currentSkill.skillStruct.range; } }
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

                case State.Move:// �����Ұ� ���� �� ��ȸ
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

                case State.Skill:
                    SkillState();
                    yield return new WaitForSeconds(1f);// �۷ι� ��Ÿ��? �ִϸ��̼� ����?
                    if (state == State.Skill)
                    {
                        StateMachine(State.Idle);
                    }
                    break;

                case State.Damage:
                    DamageState();
                    yield return new WaitForSeconds(3f / 1f);// �ִϸ��̼� ����?
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
        if (target == null || target.state == State.Dead) // Ÿ���� ���� ���
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
        else if (readySkills?.Count > 0)
        {
            currentSkill = SelectSkill();// ��ų ����

            distance = (target.transform.position - transform.position).magnitude;

            float unitAllSize = target.GetUnitSize + GetUnitSize;
            if (distance + 0.1f < GetSkillRange.x + unitAllSize && escapeCooling == false)
            {
                StateMachine(State.Escape);
                // ����� ���� ����
                float randomIndex = Random.Range(-1f, 1f) * randomValue * 0.5f;
                targetPosition = GetBackPoint(target.transform.position, randomIndex);
                Destination(targetPosition);

                StartCoroutine(EscapeCooling());// ��� ������ ġ�� �Ѵ뵵 ������
            }
            else
            {
                // ����
                StateMachine(State.Chase);
            }
        }
        else
        {
            StateMachine(State.Escape);
            // ��ų�� ��� ����
            float randomIndex = Random.Range(-1f, 1f) * randomValue * 0.5f;
            targetPosition = GetBackPoint(target.transform.position, randomIndex);
            Destination(targetPosition);


            //StateMachine(State.Move);// Ȥ�� ��ȸ ���⿡ ����??
        }
    }

    SkillStruct SelectSkill()
    {
        return readySkills[Random.Range(0, readySkills.Count)];
    }

    void ChaseState()
    {
        // �ָ� ����
        targetPosition = GetFrontPoint(target.transform.position, 0f);
        Destination(targetPosition);

        //float remainingDistance = agent.remainingDistance;
        distance = (target.transform.position - transform.position).magnitude;
        float unitAllSize = target.GetUnitSize + GetUnitSize;
        if (distance < GetSkillRange.y + unitAllSize + 0.1f)
        {
            if (attackCooling == false)
            {
                StateMachine(State.Skill);
                StartCoroutine(AttackCooling());
            }
            else
            {
                StateMachine(State.Move);
            }
        }
    }

    void MoveState()// ��ȸ
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

    public Skill_Slot skillSlot;
    Dictionary<string, Skill_Slot> dictSkillSlot = new Dictionary<string, Skill_Slot>();
    Coroutine skillCastring;
    IEnumerator SkillCasting()
    {
        // ĳ���� �ϴ� ���� ������?? �з�����?? ��ų Ǯ����
        // ĳ���� �ϴ� ���� Ÿ���� �ָ� �̵� �ϰ� �Ǹ�???
        float castingTime = 3f;
        float casting = 0f;
        if (casting < 1f)
        {
            casting += Time.deltaTime / castingTime;
            yield return null;
        }
        if(cnlth == true)
        {
            cnlth = false;
            Debug.LogWarning("��Ұ� �ƴµ� �����!!!");
        }
        ActionSkill();
    }

    void SkillState()
    {
        if (currentSkill.skillStruct.castingTime > 0)
        {
            skillCastring = StartCoroutine(SkillCasting());
        }
        else
        {
            // ��� ���
            ActionSkill();
        }
    }

    void ActionSkill()
    {
        Destination(transform.position);// ���ڸ��� ����
        StartCoroutine(SkillCooling(currentSkill));// ��ų ��Ÿ�� ����

        string skillID = currentSkill.skillStruct.ID;
        if (dictSkillSlot.ContainsKey(skillID) == false)
        {
            Skill_Slot slot = Instantiate(skillSlot, transform);
            slot.gameObject.name = skillID;
            slot.fromUnit = this;
            dictSkillSlot[skillID] = slot;
        }
        dictSkillSlot[skillID].SetFromUnit(target);
    }

    IEnumerator SkillCooling(SkillStruct _skillStruct)
    {
        readySkills.Remove(_skillStruct);
        float coolTime = _skillStruct.skillStruct.coolingTime;
        //Debug.LogWarning($"{_skillStruct.skillStruct.ID} : {coolTime}");
        yield return new WaitForSeconds(coolTime);

        readySkills.Add(_skillStruct);
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
        float aggro = _damage * _skillStruct.aggro;
        target = AddAggro(_from, aggro);// ��׷� �߰� �� Ÿ�� ����

        if (takeDamage != null)
            StopCoroutine(takeDamage);
        switch (_skillStruct.ccType)
        {
            case Data_Manager.SkillStruct.CCType.Normal:
                takeDamage = StartCoroutine(TakeDamage());
                break;

            case Data_Manager.SkillStruct.CCType.KnockBack:
                takeDamage = StartCoroutine(CCDamage(_center));

                if (skillCastring != null)// ĳ���� ���̶�� ���
                {
                    cnlth = true;
                    StopCoroutine(skillCastring);
                }
                break;
        }
    }
    bool cnlth;
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
            // ���� ����
            _angleInDegrees += _trans.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(_angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(_angleInDegrees * Mathf.Deg2Rad));
    }

    bool VisibleTarget(Transform _target)// ���̴��� Ȯ��
    {
        Vector3 dirToTarget = (_target.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle * 0.5f)// �ޱ� �ȿ� ���� �Ǵ���
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

        Unit_AI tempUnit = _unit;
        foreach (var child in aggroDict)
        {
            Unit_AI unit = child.Key;
            float value = child.Value;

            if (value > aggroDict[tempUnit])
            {
                tempUnit = unit;
            }
        }

        if (aggroCoroutine != null)
            StopCoroutine(aggroCoroutine);
        aggroCoroutine = StartCoroutine(DecreaseAggro());
        return tempUnit;
    }

    void SetAggro()
    {
        //testAggro.Clear();// ���� �뵵 ���߿� ����
        Dictionary<Unit_AI, float> tempDict = new Dictionary<Unit_AI, float>();
        foreach (var child in aggroDict)
        {
            Unit_AI unit = child.Key;
            float value = child.Value - 1f;
            if (value > 0f)
            {
                tempDict[unit] = value;
                //TestAggro test = new TestAggro
                //{
                //    name = unit.gameObject.name,
                //    Unit = unit,
                //    Value = value,
                //};
                //testAggro.Add(test);
            }
        }
        aggroDict = tempDict;
    }
    public List<TestAggro> testAggro = new List<TestAggro>();
    [System.Serializable]
    public class TestAggro
    {
        public string name;
        public Unit_AI Unit;
        public float Value;
    }

    IEnumerator DecreaseAggro()
    {
        float dalay = 0.5f;
        while (aggroDict.Count > 0 && state != State.Dead)
        {
            Dictionary<Unit_AI, float> tempDict = new Dictionary<Unit_AI, float>();
            testAggro.Clear();
            foreach (var child in aggroDict)
            {
                Unit_AI unit = child.Key;
                float value = child.Value - dalay;
                if (value > 0f)
                {
                    tempDict[unit] = value;
                    TestAggro test = new TestAggro
                    {
                        name = unit.gameObject.name,
                        Unit = unit,
                        Value = value,
                    };
                    testAggro.Add(test);
                }
            }
            aggroDict = tempDict;
            yield return new WaitForSeconds(dalay);
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
                Handles.Label(textPosition, $"{visibleTarget} : {CheckDistance():N2}", fontStyle);
                Gizmos.DrawSphere(targetPosition, 0.3f);
            }
        }
        string hp = healthPoint > 0 ? "00FF00" : "FFFFFF";
        hp = $"<color=#{hp}>{state}\n{healthPoint:N2}</color>";
        Handles.Label(transform.position, hp, fontStyle);
    }
#endif
}
