using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Unit_AI : MonoBehaviour
{
    public bool dummy;
    public string unitID;
    public NavMeshAgent agent;
    //public NavMeshObstacle obstacle;
    [SerializeField] Unit_AI target;
    Dictionary<Unit_AI, float> aggroDict = new Dictionary<Unit_AI, float>();
    Data_Manager.UnitStruct unitStruct;
    public Data_Manager.UnitStruct GetUnitStruct { get { return unitStruct; } }
    Data_Manager.UnitStruct.UnitStatus unitStatus;
    public Vector2Int[] synergy;

    public delegate List<Unit_AI> DeleUnitList();
    public DeleUnitList playerList, monsterList;

    public Unit_Animation unitAnimation;

    public enum State
    {
        None,
        Dead,
        Attack,
        Idle,
        Escape,
        Move,
        Patrol,
        Damage,
        End
    }
    public State state = State.None;
    float distance;
    Renderer[] renderers;
    public string[] lootings;
    Coroutine takeDamage, setRanderer;
    Coroutine aggroCoroutine;
    Coroutine stateAction;
    Coroutine coolingSkill;
    Coroutine holdAction, shakeUnit;

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

    public LayerMask targetMask, obstacleMask;

    public delegate void DeadUnit(Unit_AI _unit);
    public DeadUnit deadUnit;

    SkillStruct currentSkill;

    [System.Serializable]
    public class SkillStruct
    {
        [HideInInspector] public string skillID;
        public float startTime;
        public Data_Manager.SkillStruct skillStruct;
    }
    [SerializeField] List<SkillStruct> readySkills = new List<SkillStruct>();
    [SerializeField] List<SkillStruct> coolingSkills = new List<SkillStruct>();
    const float globalTime = 1f;
    public float GetUnitSize { get { return unitStruct.unitSize; } }
    public float healthPoint = 10f;

    public delegate void DeleUpdateHP(float _current, float _max, bool _shake);
    public DeleUpdateHP deleUpdateHP;
    public delegate void DeleUpdateAction(float _value);
    public DeleUpdateAction deleUpdateAction;

    public delegate void DeleDamage(Transform _target, string _damage);
    public DeleDamage deleDamage;

    public float GetDamage
    {
        get
        {
            float ap = unitStatus.AttackPower;
            float sp = unitStatus.SpellPower;
            float rp = unitStatus.RangePower;
            return currentSkill.skillStruct.GetDamage(ap, sp, rp);
        }
    }
    public Vector2 GetSkillRange { get { return currentSkill.skillStruct.range; } }

    Dictionary<string, Skill_Set> dictSkillSlot = new Dictionary<string, Skill_Set>();
    bool skillCasting;
    public SpriteRenderer castingImage;

    public void SetUnit(string _unitID, LayerMask _layerMask)// 첫세팅
    {
        unitID = _unitID;
        if (unitID == null || Singleton_Data.INSTANCE.Dict_Unit.ContainsKey(unitID) == false)
            return;

        Unit_Animation unit = Singleton_Data.INSTANCE.Dict_Unit[unitID].unitProp;
        unitAnimation = Instantiate(unit, this.transform);
        unitAnimation.SetAnimator();
        unitAnimation.attackEvent = Event_Attack;// 애니메이션 이벤트 받아올
        lootings = unitAnimation.GetComponent<Unit_Looting>().GetLooting(3);// 루팅 아이템

        Debug.Log("유닛 생성 : " + unitID);
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        //obstacle = GetComponentInChildren<NavMeshObstacle>();
        //obstacle.enabled = false;

        unitStruct = Singleton_Data.INSTANCE.Dict_Unit[unitID];
        unitStatus = unitStruct.TryStatus();// 스탯 속성
        synergy = unitStruct.synergy;
        ResetUnit();

        //castingImage.material = Instantiate(castingImage.material);
        gameObject.layer = _layerMask;

        AddSkill(unitStruct.defaultSkill01);
        AddSkill(unitStruct.defaultSkill02);

        healthPoint = unitStatus.Health;
        deleUpdateHP?.Invoke(healthPoint, unitStatus.Health, false);// 세팅

        renderers = GetComponentsInChildren<Renderer>();
    }

    void AddSkill(string _key)
    {
        if (Singleton_Data.INSTANCE.Dict_Skill.ContainsKey(_key))
        {
            SkillStruct skill = new SkillStruct
            {
                skillID = Singleton_Data.INSTANCE.Dict_Skill[_key].ID,
                startTime = 0,
                skillStruct = Singleton_Data.INSTANCE.Dict_Skill[_key]
            };
            readySkills.Add(skill);
        }
    }

    public void ResetUnit()// 첫소환 또는 부활 때 사용
    {
        healthPoint = unitStatus.Health;

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
        //StartCoroutine(DeadActing());
    }

#if UNITY_EDITOR
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

            if (target != null)
            {
                Handles.color = Gizmos.color = color;
                Handles.DrawLine(transform.position, target.transform.position);
                Gizmos.DrawSphere(targetPosition, 0.3f);
            }
        }
        string hp = healthPoint > 0 ? "00FF00" : "FFFFFF";
        hp = $"<color=#{hp}>{state}\n{healthPoint:N2}</color>";
        Handles.Label(transform.position, hp, fontStyle);
    }
#endif

    public void SetStart()
    {
        StateMachine(State.Idle);
    }

    void StateMachine(State _state)
    {
        if (dummy == true || state == State.Dead)
            return;

        if (stateAction != null)
            StopCoroutine(stateAction);

        state = _state;
        switch (state)
        {
            case State.None:

                break;

            case State.Idle:
                State_Idle();
                break;

            case State.Move:// 공격할게 없을 때 배회
                State_Move();
                break;

            case State.Patrol:
                State_Patrol();
                break;

            case State.Escape:

                break;

            case State.Attack:
                State_Attack();
                break;

            case State.Damage:

                break;

            case State.Dead:
                DeadState();
                break;
        }
        if (agent != null)
            agent.avoidancePriority = (int)state;
    }

    void State_Idle()
    {
        stateAction = StartCoroutine(FindTarget());
    }

    IEnumerator FindTarget()
    {
        while (globalCooling == true)
            yield return null;

        List<Unit_AI> units = gameObject.layer == LayerMask.NameToLayer("Player") ? monsterList() : playerList();
        while (target == null || target.state == State.Dead)
        {
            float dist = float.MaxValue;
            for (int i = 0; i < units.Count; i++)
            {
                // 가까운 오브젝트 찾기
                distance = (units[i].transform.position - transform.position).magnitude;
                if (dist > distance)
                {
                    dist = distance;
                    target = units[i];
                }
            }
            yield return null;
        }

        if (readySkills?.Count > 0)// 준비된 스킬이 있다면
        {
            currentSkill = SelectSkill();// 스킬 선택
            StateMachine(State.Move);
        }
        else
        {
            StateMachine(State.Patrol);
        }
    }

    SkillStruct SelectSkill()
    {
        return readySkills[Random.Range(0, readySkills.Count)];
    }

    void State_Move()
    {
        agent.speed = unitStatus.MoveSpeed;
        stateAction = StartCoroutine(State_Moving());
    }

    IEnumerator State_Moving()
    {
        float deleyTime = 0.2f;
        bool moving = true;
        while (moving == true)
        {
            targetPosition = target.transform.position;
            Destination(targetPosition);
            Vector3 offset = (targetPosition - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(offset);

            float distance = (target.transform.position - transform.position).magnitude;
            float setDistance = target.GetUnitSize + GetUnitSize + GetSkillRange.y;
            if (distance < setDistance)
            {
                moving = false;
                StateMachine(State.Attack);
            }
            yield return new WaitForSeconds(deleyTime);
        }
    }

    void State_Patrol()
    {
        agent.speed = unitStatus.MoveSpeed * 0.3f;
        stateAction = StartCoroutine(State_Patroling());
    }

    IEnumerator State_Patroling()
    {
        // 랜덤 시간동안 이동
        float randomIndex = Random.Range(-1f, 1f) * randomValue * 0.5f;
        targetPosition = GetFrontPoint(target.transform.position, randomIndex);
        Destination(targetPosition);

        float randomTime = Random.Range(0.2f, 1f);
        bool moving = true;
        while (moving == true)
        {
            yield return new WaitForSeconds(randomTime);

            // 시간이 지나면 다시 스킬 찾기
            moving = false;
            StateMachine(State.Idle);
        }
    }

    void Destination(Vector3 _point)
    {
        //if (agent.enabled == true)
        agent.SetDestination(_point);
    }

    public delegate bool DeleTryPoint(Vector3 _target, float _unitSize);
    public DeleTryPoint tryPoint;// 다른 유닛이 없는 곳으로 포인팅

    Vector3 GetFrontPoint(Vector3 _from, float _addAngle)
    {
        float angle = GetAngle(_from, transform.position);
        float patrolRange = 3f;
        float setDistance = target.GetUnitSize + GetUnitSize + patrolRange;
        Vector3 dirFromAngle = DirFromAngle(_addAngle + angle, null);
        Vector3 tempTarget = target.transform.position + dirFromAngle * setDistance;

        NavMesh.SamplePosition(tempTarget, out NavMeshHit hit, setDistance, -1);
        tempTarget = hit.position;
        if (tryPoint(tempTarget, GetUnitSize) == true)
        {
            return tempTarget;
        }
        return Vector3.zero;
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

    float GetAngle(Vector3 from, Vector3 to)
    {
        Vector3 v = to - from;
        return Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;
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

    void State_Attack()
    {
        stateAction = StartCoroutine(State_Attacking());
    }

    IEnumerator State_Attacking()
    {
        Destination(transform.position);// 제자리에 정지
        float castingTime = currentSkill.skillStruct.castingTime;
        yield return StartCoroutine(SkillCasting(castingTime));// 캐스팅

        if (state != State.Dead)
        {
            distance = (target.transform.position - transform.position).magnitude;
            float unitAllSize = target.GetUnitSize + GetUnitSize;
            // 거리체크 (스킬 거리의 반정도는 멀어져도 OK)
            if (distance < GetSkillRange.y + (GetSkillRange.y * 0.5f) + unitAllSize && state != State.Dead)
            {
                transform.LookAt(target.transform.position);
                float actionTime = unitAnimation.PlayAnimation(3);// 애니메이션
                yield return new WaitForSeconds(actionTime);// 애니메이션 길이만큼 대기
            }
            StateMachine(State.Idle);
        }
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
        //deleUpdateAction(0f);
    }

    void Attacking()
    {
        string skillID = currentSkill.skillStruct.ID;
        if (dictSkillSlot.ContainsKey(skillID) == false)
        {
            // 스킬 이펙트 생성
            Data_Manager.SkillStruct skill = Singleton_Data.INSTANCE.Dict_Skill[skillID];
            //Debug.LogWarningFormat(skill.skillSet);
            Skill_Set slot = Singleton_Data.INSTANCE.Dict_SkillSet[skill.skillSet];
            Skill_Set inst = Instantiate(slot, transform);
            inst.gameObject.name = skillID;
            inst.SetSkillSlot(this, currentSkill.skillStruct);
            dictSkillSlot[skillID] = inst;
        }
        dictSkillSlot[skillID].PlayAction(target);

        readySkills.Remove(currentSkill);
        currentSkill.startTime = Time.time;
        coolingSkills.Add(currentSkill);
        CoolingSkill();

        StartCoroutine(GlobalCooling());
    }

    IEnumerator GlobalCooling()
    {
        globalCooling = true;
        yield return new WaitForSeconds(1f);
        globalCooling = false;
    }
    void Event_Attack()
    {
        Attacking();
    }

    void CoolingSkill()
    {
        if (coolingSkill != null)
            StopCoroutine(coolingSkill);
        coolingSkill = StartCoroutine(CoolingSkilling());
    }

    IEnumerator CoolingSkilling()
    {
        while (coolingSkills.Count > 0)
        {
            //List<SkillStruct> tempList = coolingSkills;
            for (int i = 0; i < coolingSkills.Count; i++)
            {
                float cooling = coolingSkills[i].startTime + coolingSkills[i].skillStruct.coolingTime;
                if (cooling < Time.time)
                {
                    readySkills.Add(coolingSkills[i]);
                    coolingSkills.Remove(coolingSkills[i]);
                    break;
                }
            }
            yield return null;
            //coolingSkills = tempList;
        }
    }

    public void TakeDamage(Unit_AI _from, Vector3 _center, float _damage, Data_Manager.SkillStruct _skillStruct)
    {
        if (state == State.Dead)
            return;

        Destination(transform.position);
        // _from 때린 유닛
        // _center 맞은 포인트

        Debug.Log($"{_from.gameObject.name} : {_damage}");
        //Vector3 hitPoint = transform.position;
        healthPoint -= _damage;
        deleUpdateHP?.Invoke(healthPoint, unitStatus.Health, true);// 데미지 바
        deleDamage?.Invoke(this.transform, _damage.ToString());// 데미지 폰트

        if (healthPoint <= 0)
        {
            // 죽음 액션
            StateMachine(State.Dead);
            SetRanderer();
            Debug.LogWarning("죽음 : " + gameObject.name);
            return;
        }

        // 데미지 액션
        SetRanderer();
        float animTime = unitAnimation.PlayAnimation(5);// 애니메이션

        if (holdAction != null)
            StopCoroutine(holdAction);
        holdAction = StartCoroutine(HoldAction(animTime));

        if (shakeUnit != null)// 캐릭터 흔들기
            StopCoroutine(shakeUnit);
        shakeUnit = StartCoroutine(ShakeUnit());

        // 어그로
        float aggro = _damage * _skillStruct.aggro;
        target = AddAggro(_from, aggro);// 어그로 추가

        // 군중 제어 Crowd Control
        if (takeDamage != null)
            StopCoroutine(takeDamage);
        switch (_skillStruct.ccType)
        {
            case Data_Manager.SkillStruct.CCType.Normal:

                break;

            case Data_Manager.SkillStruct.CCType.KnockBack:
                takeDamage = StartCoroutine(CCDamage(_center, 1f));
                break;
        }
    }

    IEnumerator HoldAction(float _hold)
    {
        StateMachine(State.None);
        yield return new WaitForSeconds(_hold);
        if (state != State.Dead)
            StateMachine(State.Idle);
    }

    IEnumerator ShakeUnit()
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 3f;
            Vector3 randomPosition = Random.insideUnitSphere * (1f - normalize) * 0.3f;
            unitAnimation.transform.localPosition = randomPosition;
            yield return null;
        }
        unitAnimation.transform.localPosition = Vector3.zero;
    }

    void DeadState()
    {
        unitAnimation.PlayAnimation(7);// 애니메이션
        deadUnit?.Invoke(this);
        //agent.enabled = false;
    }

    void SetRanderer()
    {
        switch (state)
        {
            case State.Dead:
                StartCoroutine(SetRenderer("_BlackNWhite", 1f));
                //setRanderer = StartCoroutine(SetRenderer("_BlackNWhite", 1f));
                break;

            default:
                if (setRanderer != null)
                    StopCoroutine(setRanderer);
                setRanderer = StartCoroutine(SetRenderer("_Damage", 3f));
                break;
        }
    }

    IEnumerator SetRenderer(string _renderName, float _speed)
    {
        float targetAmount = state == State.Dead ? 1f : 0f;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * _speed;
            for (int i = 0; i < renderers.Length; i++)
            {
                float value = Mathf.Lerp(1f - targetAmount, targetAmount, normalize);
                renderers[i].material.SetFloat(_renderName, value);
            }
            yield return null;
        }
    }

    // 어그로
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
        //StartCoroutine(DeadActing());
    }


    IEnumerator CCDamage(Vector3 _from, float _knockBack)
    {
        if (_knockBack > 0)
        {
            if (skillCasting == true)// 밀려나면 캐스팅 끊어짐
                skillCasting = false;

            Vector3 targetPoint = GetBackPoint(_from, 0f, _knockBack);
            if (float.IsInfinity(targetPoint.x))
                targetPoint = transform.position;

            float normalize = 0f;
            while (normalize < 1f)
            {
                normalize += Time.deltaTime * 3f;

                Vector3 position = Vector3.Lerp(transform.position, targetPoint, normalize);
                transform.position = position;
                Destination(position);
                yield return null;
            }
        }
    }

    public void BattleOver()
    {
        if (state == State.Dead)
            return;

        StateMachine(State.End);
        Destination(transform.position);// 제자리에 정지

        // 승리 포즈
        unitAnimation.PlayAnimation(4);// 애니메이션
    }
}
