using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Unit_AI : MonoBehaviour
{
    public string unitID = "U10010";
    public NavMeshAgent agent;
    //public NavMeshObstacle obstacle;
    Unit_AI target;
    Dictionary<Unit_AI, float> aggroDict = new Dictionary<Unit_AI, float>();
    Data_Manager.UnitStruct unitStruct;
    public Data_Manager.UnitStruct GetUnitStruct { get { return unitStruct; } }
    Data_Manager.UnitStruct.UnitAttributes unitAttributes;

    public delegate List<Unit_AI> DeleUnitList();
    public DeleUnitList playerList;
    public DeleUnitList monsterList;

    public Unit_Animation unitAnimation;

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
    Vector3 targetPosition;
    const float randomValue = 90f;

    //public enum JobType
    //{
    //    Fighter,
    //    Ranger,
    //    Sorcerer
    //}
    //public JobType jobType = JobType.Fighter;

    public float GetCoolTime { get { return currentSkill.skillStruct.coolingTime; } }
    public bool globalCooling = false;
    float escapeCoolTime = 3f;
    public bool escapeCooling = false;

    Coroutine aggroCoroutine;
    public LayerMask targetMask, obstacleMask;

    public delegate void DeadUnit(Unit_AI _unit);
    public DeadUnit deadUnit;

    SkillStruct currentSkill;

    [System.Serializable]
    public class SkillStruct
    {
        [HideInInspector] public string skillID;
        public Data_Manager.SkillStruct skillStruct;
    }
    List<SkillStruct> readySkills = new List<SkillStruct>();
    const float globalTime = 1f;
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

    public void SetUnit(string _unitID, LayerMask _layerMask)// 첫세팅
    {
        unitID = _unitID;
        Debug.LogWarning("생성 : " + unitID);
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        //obstacle = GetComponentInChildren<NavMeshObstacle>();
        //obstacle.enabled = false;

        unitStruct = Singleton_Data.INSTANCE.Dict_Unit[unitID];
        unitAttributes = unitStruct.TryAttributes();
        ResetUnit();

        image.material = Instantiate(image.material);
        gameObject.layer = _layerMask;

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
        deleUpdateHP?.Invoke(healthPoint, unitAttributes.Health);

        renderers = GetComponentsInChildren<Renderer>();
    }

    public void ResetUnit()// 첫소환 또는 부활 때 사용
    {
        healthPoint = unitAttributes.Health;

        if (state == State.Dead)
        {
            Rebirth();
        }

        target = null;
        globalCooling = false;
        escapeCooling = false;
        aggroDict.Clear();
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

    public void StateMachine(State _state)
    {
        state = _state;
        agent.avoidancePriority = (int)_state;
        StartBattle();
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
                    SkillState();
                    float castingTime = currentSkill.skillStruct.castingTime;
                    yield return StartCoroutine(SkillCasting(castingTime));
                    // 글로벌 쿨링 시작
                    StartCoroutine(GlobalCooling());// 글로벌 쿨타임? 
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
            List<Unit_AI> units = gameObject.layer == LayerMask.NameToLayer("Player") ? monsterList() : playerList();
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
        else if (readySkills?.Count > 0)// 준비된 스킬이 있다면
        {
            currentSkill = SelectSkill();// 스킬 선택

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
                // 추적
                StateMachine(State.Chase);
            }
        }
        else// 준비된 스킬이 없음
        {
            StateMachine(State.Move);// 혹은 배회 성향에 따라??
            //StateMachine(State.Escape);
            // 스킬이 없어서 도망
            float randomIndex = Random.Range(-1f, 1f) * randomValue * 0.5f;
            targetPosition = GetBackPoint(target.transform.position, randomIndex);
            Destination(targetPosition);
        }
    }

    SkillStruct SelectSkill()
    {
        return readySkills[Random.Range(0, readySkills.Count)];
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
            if (globalCooling == false)
            {
                StateMachine(State.Attack);
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

    //public Skill_Set skillSet;
    Dictionary<string, Skill_Set> dictSkillSlot = new Dictionary<string, Skill_Set>();
    bool skillCastring;
    public Image image;
    IEnumerator SkillCasting(float _castingTime)
    {
        Destination(transform.position);// 제자리에 정지

        skillCastring = true;
        image.CrossFadeAlpha(1f, 0.5f, false);
        float casting = 0f;
        while (skillCastring == true)
        {
            casting += Time.deltaTime;
            image.material.SetFloat("_FillAmount", casting / _castingTime);// UI_Filled 쉐이더
            if (casting > _castingTime)
            {
                skillCastring = false;

                distance = (target.transform.position - transform.position).magnitude;
                float unitAllSize = target.GetUnitSize + GetUnitSize;
                // 거리체크 (스킬 거리의 반정도는 멀어져도 OK)
                if (distance < GetSkillRange.y + (GetSkillRange.y * 0.5f) + unitAllSize && state != State.Dead)
                {
                    ActionSkill();
                }
            }
            yield return null;
        }
        image.CrossFadeAlpha(0f, 0.5f, false);
    }

    void SkillState()
    {
        //StartCoroutine(SkillCasting(currentSkill.skillStruct.castingTime));
    }

    void ActionSkill()
    {
        unitAnimation.PlayAnimation(3);// 애니메이션
        StartCoroutine(SkillCooling(currentSkill));// 스킬 쿨타임 시작

        string skillID = currentSkill.skillStruct.ID;
        if (dictSkillSlot.ContainsKey(skillID) == false)
        {
            Data_Manager.SkillStruct skill = Singleton_Data.INSTANCE.Dict_Skill[skillID];
            Debug.LogWarningFormat(skill.skillSet);
            Skill_Set slot = Singleton_Data.INSTANCE.Dict_SkillSet[skill.skillSet];
            Skill_Set inst = Instantiate(slot, transform);
            inst.gameObject.name = skillID;
            inst.SetSkillSlot(this, currentSkill.skillStruct);
            dictSkillSlot[skillID] = inst;
        }
        dictSkillSlot[skillID].SetAction(target);
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
        unitAnimation.PlayAnimation(5);// 애니메이션
    }

    void DeadState()
    {
        unitAnimation.PlayAnimation(7);// 애니메이션

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

    IEnumerator GlobalCooling()
    {
        globalCooling = true;
        yield return new WaitForSeconds(globalTime);
        globalCooling = false;
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

    public delegate void DeleUpdateHP(float _current, float _max);
    public DeleUpdateHP deleUpdateHP;
    public void TakeDamage(Unit_AI _from, Vector3 _center, float _damage, Data_Manager.SkillStruct _skillStruct)
    {
        // _from 때린 유닛
        // _center 맞은 포인트
        Debug.Log($"{_from.gameObject.name} : {_damage}");
        healthPoint -= _damage;
        deleUpdateHP?.Invoke(healthPoint, unitAttributes.Health);
        if (healthPoint <= 0)
        {
            StateMachine(State.Dead);
            return;
        }

        StateMachine(State.Damage);
        float aggro = _damage * _skillStruct.aggro;
        target = AddAggro(_from, aggro);// 어그로 추가 후 타겟 변경

        if (takeDamage != null)
            StopCoroutine(takeDamage);
        switch (_skillStruct.ccType)
        {
            case Data_Manager.SkillStruct.CCType.Normal:
                //takeDamage = StartCoroutine(CCDamage(_center, 0f));
                break;

            case Data_Manager.SkillStruct.CCType.KnockBack:
                takeDamage = StartCoroutine(CCDamage(_center, 1f));
                break;
        }


    }

    IEnumerator CCDamage(Vector3 _from, float _knockBack)
    {
        if (_knockBack > 0)
        {
            if (skillCastring == true)// 캐스팅 중이고 밀리면 취소
                skillCastring = false;

            Vector3 targetPoint = GetBackPoint(_from, 0f, _knockBack);
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

        //bool stop = (_point == transform.position);
        //if(stop == true)
        //{
        //    //agent.SetDestination(_point);
        //    agent.enabled = false;
        //    obstacle.enabled = true;
        //}
        //else
        //{
        //    obstacle.enabled = false;
        //    agent.enabled = true;
        //    agent.SetDestination(_point);
        //}
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

    //bool VisibleTarget(Transform _target)// 보이는지 확인
    //{
    //    Vector3 dirToTarget = (_target.position - transform.position).normalized;
    //    if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle * 0.5f)// 앵글 안에 포함 되는지
    //    {
    //        float dstToTarget = (transform.position - _target.position).magnitude;
    //        if (Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask) == false)
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

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
        //testAggro.Clear();// 보는 용도 나중에 삭제
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
    //public float viewAngle;
    private void OnDrawGizmos()
    {
        if (currentSkill == null)
            return;

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

            Handles.color = gameObject.layer == LayerMask.NameToLayer("Player") ? Color.red : Color.blue;
            Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360f, GetUnitSize + GetSkillRange.x);
            Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360f, GetUnitSize + GetSkillRange.y);

            //Vector3 viewAngleA = DirFromAngle(viewAngle * 0.5f, transform);
            //Vector3 viewAngleB = DirFromAngle(-viewAngle * 0.5f, transform);

            //Handles.DrawLine(transform.position, transform.position + viewAngleA * (GetUnitSize + GetSkillRange.y));
            //Handles.DrawLine(transform.position, transform.position + viewAngleB * (GetUnitSize + GetSkillRange.y));

            if (target != null)
            {
                Handles.color = Gizmos.color = color;
                Handles.DrawLine(transform.position, target.transform.position);
                //string visibleTarget = VisibleTarget(target.transform) ? "0000FF" : "FF0000";
                //visibleTarget = $"<color=#{visibleTarget}>{VisibleTarget(target.transform)}</color>";
                //Vector3 textPosition = Vector3.Lerp(transform.position, target.transform.position, 0.5f);
                //Handles.Label(textPosition, $"{visibleTarget} : {CheckDistance():N2}", fontStyle);
                Gizmos.DrawSphere(targetPosition, 0.3f);
            }
        }
        string hp = healthPoint > 0 ? "00FF00" : "FFFFFF";
        hp = $"<color=#{hp}>{state}\n{healthPoint:N2}</color>";
        Handles.Label(transform.position, hp, fontStyle);
    }
#endif
}
