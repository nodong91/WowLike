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
    //public NavMeshObstacle obstacle;
    [SerializeField] Unit_AI target;
    Dictionary<Unit_AI, float> aggroDict = new Dictionary<Unit_AI, float>();
    Data_Manager.UnitStruct unitStruct;
    public Data_Manager.UnitStruct GetUnitStruct { get { return unitStruct; } }
    Data_Manager.UnitStruct.UnitAttributes unitAttributes;

    public delegate List<Unit_AI> DeleUnitList();
    public DeleUnitList playerList, monsterList;
    public bool dummy;

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
    Coroutine stateMachine, takeDamage, setRanderer;

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

    public delegate void DeleDamage(Vector3 _point, string _damage);
    public DeleDamage deleDamage;

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

    Dictionary<string, Skill_Set> dictSkillSlot = new Dictionary<string, Skill_Set>();
    bool skillCasting;
    public SpriteRenderer castingImage;

    public void SetUnit(string _unitID, LayerMask _layerMask)// 첫세팅
    {
        unitID = _unitID;
        Debug.LogWarning("생성 : " + unitID);
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        unitAnimation.attackEvent = Event_Attack;// 애니메이션 이벤트 받아올
        //obstacle = GetComponentInChildren<NavMeshObstacle>();
        //obstacle.enabled = false;

        unitStruct = Singleton_Data.INSTANCE.Dict_Unit[unitID];
        unitAttributes = unitStruct.TryAttributes();
        ResetUnit();

        //castingImage.material = Instantiate(castingImage.material);
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
        healthPoint = unitAttributes.Health;
        deleUpdateHP?.Invoke(healthPoint, unitAttributes.Health, false);// 세팅

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






















    //void Rebirth()
    //{
    //    state = State.None;
    //    StartCoroutine(DeadActing());
    //}

    ////public void CommendMoveing(Vector3 _point)
    ////{
    ////    target = null;
    ////    StateMachine(State.None);
    ////    Destination(_point);
    ////}

    //public void StateMachine(State _state)
    //{
    //    state = _state;
    //    agent.avoidancePriority = (int)_state;

    //    if (stateMachine != null)
    //        StopCoroutine(stateMachine);

    //    switch (state)
    //    {
    //        case State.None:

    //            break;

    //        case State.Idle:
    //            stateMachine = StartCoroutine(IdleState());
    //            break;

    //        case State.Move:// �����Ұ� ���� �� ��ȸ
    //            stateMachine = StartCoroutine(MoveState());
    //            break;

    //        case State.Chase:
    //            stateMachine = StartCoroutine(ChaseState());
    //            break;

    //        case State.Escape:
    //            stateMachine = StartCoroutine(EscapeState());
    //            break;

    //        case State.Attack:
    //            Debug.LogError($"Hit Unit : adsffffffffffffffffffffffffffffffffffffffffffffaWef");
    //            stateMachine = StartCoroutine(AttackState());
    //            break;

    //        case State.Damage:
    //            stateMachine = StartCoroutine(DamageState());
    //            break;

    //        case State.Dead:
    //            DeadState();
    //            break;
    //    }
    //}

    //IEnumerator IdleState()
    //{
    //    while (true)
    //    {
    //        if (target == null || target.state == State.Dead) // Ÿ���� ���� ���
    //        {
    //            float dist = float.MaxValue;
    //            List<Unit_AI> units = gameObject.layer == LayerMask.NameToLayer("Player") ? monsterList() : playerList();
    //            for (int i = 0; i < units.Count; i++)
    //            {
    //                Unit_AI unit = units[i];
    //                distance = (unit.transform.position - transform.position).magnitude;
    //                if (dist > distance)
    //                {
    //                    dist = distance;
    //                    target = unit;
    //                }
    //            }
    //        }
    //        else if (readySkills?.Count > 0)// �غ�� ��ų�� �ִٸ�
    //        {
    //            currentSkill = SelectSkill();// ��ų ����
    //            distance = (target.transform.position - transform.position).magnitude;
    //            float unitAllSize = target.GetUnitSize + GetUnitSize;
    //            if (distance + 0.1f < GetSkillRange.x + unitAllSize && escapeCooling == false)
    //            {
    //                // ����� ���� ����
    //                float randomIndex = Random.Range(-1f, 1f) * randomValue * 0.5f;
    //                targetPosition = GetBackPoint(target.transform.position, randomIndex);
    //                Destination(targetPosition);

    //                StartCoroutine(EscapeCooling());// ��� ������ ġ�� �Ѵ뵵 ������
    //                StateMachine(State.Escape);
    //            }
    //            else
    //            {
    //                Debug.LogError($"Hit Unit : adsffffffffffffffffffffffffffffffffffffffffffffaWef");
    //                // ����
    //                StateMachine(State.Chase);
    //            }
    //        }
    //        else// �غ�� ��ų�� ����
    //        {
    //            StateMachine(State.Move);// Ȥ�� ��ȸ ���⿡ ����??
    //                                     //StateMachine(State.Escape);
    //                                     // ��ų�� ��� ����
    //            float randomIndex = Random.Range(-1f, 1f) * randomValue * 0.5f;
    //            targetPosition = GetBackPoint(target.transform.position, randomIndex);
    //            Destination(targetPosition);
    //        }
    //        yield return null;
    //    }
    //}

    //SkillStruct SelectSkill()
    //{
    //    return readySkills[Random.Range(0, readySkills.Count)];
    //}

    //IEnumerator ChaseState()
    //{
    //    while (true)
    //    {
    //        // �ָ� ����
    //        targetPosition = GetFrontPoint(target.transform.position, 0f);
    //        Destination(targetPosition);
    //        yield return null;

    //        //float remainingDistance = agent.remainingDistance;
    //        distance = (target.transform.position - transform.position).magnitude;
    //        float unitAllSize = target.GetUnitSize + GetUnitSize;
    //        if (distance < GetSkillRange.y + unitAllSize + 0.1f)// �����Ÿ� ������ ������
    //        {
    //            if (globalCooling == false)
    //            {
    //                StateMachine(State.Attack);
    //            }
    //            else
    //            {
    //                //StateMachine(State.Move);
    //            }
    //        }
    //    }
    //}

    //IEnumerator MoveState()// ��ȸ
    //{
    //    float randomIndex = Random.Range(-1f, 1f) * randomValue * 0.5f;
    //    targetPosition = GetFrontPoint(target.transform.position, randomIndex);
    //    Destination(targetPosition);

    //    float randomTime = Random.Range(1f, 3f);
    //    yield return new WaitForSeconds(randomTime);
    //    if (state == State.Move)
    //    {
    //        StateMachine(State.Idle);
    //    }
    //}

    //IEnumerator EscapeState()
    //{
    //    while (true)
    //    {
    //        float remainingDistance = agent.remainingDistance;
    //        Debug.LogWarning("remainingDistance : " + remainingDistance);
    //        if (remainingDistance == 0f)
    //        {
    //            StateMachine(State.Idle);
    //        }
    //        yield return null;
    //    }
    //}

    //Dictionary<string, Skill_Set> dictSkillSlot = new Dictionary<string, Skill_Set>();
    //bool skillCastring;
    //public Image image;
    //IEnumerator SkillCasting(float _castingTime)
    //{
    //    Destination(transform.position);// ���ڸ��� ����

    //    skillCastring = true;
    //    image.CrossFadeAlpha(1f, 0.5f, false);
    //    float casting = 0f;
    //    while (skillCastring == true)
    //    {
    //        casting += Time.deltaTime;
    //        image.material.SetFloat("_FillAmount", casting / _castingTime);// UI_Filled ���̴�
    //        if (casting > _castingTime)
    //        {
    //            skillCastring = false;
    //        }
    //        yield return null;
    //    }
    //    image.CrossFadeAlpha(0f, 0.5f, false);
    //}

    //IEnumerator AttackState()
    //{
    //    float castingTime = currentSkill.skillStruct.castingTime;
    //    yield return StartCoroutine(SkillCasting(castingTime));
    //    // �۷ι� �� ����
    //    StartCoroutine(GlobalCooling());// �۷ι� ��Ÿ��? 
    //    if (state == State.Attack)
    //    {
    //        StateMachine(State.Idle);
    //    }
    //}

    //void ActionSkill()
    //{
    //    unitAnimation.PlayAnimation(3);// �ִϸ��̼�
    //    StartCoroutine(SkillCooling(currentSkill));// ��ų ��Ÿ�� ����

    //    string skillID = currentSkill.skillStruct.ID;
    //    if (dictSkillSlot.ContainsKey(skillID) == false)
    //    {
    //        Data_Manager.SkillStruct skill = Singleton_Data.INSTANCE.Dict_Skill[skillID];
    //        Debug.LogWarningFormat(skill.skillSet);
    //        Skill_Set slot = Singleton_Data.INSTANCE.Dict_SkillSet[skill.skillSet];
    //        Skill_Set inst = Instantiate(slot, transform);
    //        inst.gameObject.name = skillID;
    //        inst.SetSkillSlot(this, currentSkill.skillStruct);
    //        dictSkillSlot[skillID] = inst;
    //    }
    //    dictSkillSlot[skillID].SetAction(target);
    //}

    //IEnumerator SkillCooling(SkillStruct _skillStruct)
    //{
    //    readySkills.Remove(_skillStruct);
    //    float coolTime = _skillStruct.skillStruct.coolingTime;
    //    //Debug.LogWarning($"{_skillStruct.skillStruct.ID} : {coolTime}");
    //    yield return new WaitForSeconds(coolTime);

    //    readySkills.Add(_skillStruct);
    //}

    //IEnumerator DamageState()
    //{
    //    unitAnimation.PlayAnimation(5);// �ִϸ��̼�

    //    yield return new WaitForSeconds(3f / 1f);// �ִϸ��̼� ����?
    //    if (state == State.Damage)
    //    {
    //        StateMachine(State.Idle);
    //    }
    //}

    //void DeadState()
    //{
    //    unitAnimation.PlayAnimation(7);// �ִϸ��̼�

    //    deadUnit?.Invoke(this);
    //    Destination(transform.position);
    //    StartCoroutine(DeadActing());
    //}

    //Vector3 GetFrontPoint(Vector3 _from, float _random)
    //{
    //    float angle = GetAngle(_from, transform.position);
    //    float setDistance = target.GetUnitSize + GetUnitSize + GetSkillRange.y;
    //    Vector3 dirFromAngle = DirFromAngle(_random + angle, null);
    //    Vector3 targetPosition = target.transform.position + dirFromAngle * setDistance;

    //    NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, setDistance, -1);
    //    {
    //        return hit.position;
    //    }
    //}

    //Vector3 GetBackPoint(Vector3 _from, float _random, float _dist = 0)
    //{
    //    float angle = GetAngle(_from, transform.position);
    //    float setDistance = _dist > 0 ? _dist : target.GetUnitSize + GetUnitSize + GetSkillRange.x;
    //    Vector3 dirFromAngle = DirFromAngle(_random + angle, null);
    //    Vector3 targetPosition = transform.position + dirFromAngle * setDistance;

    //    NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, setDistance, -1);
    //    {
    //        return hit.position;
    //    }
    //}

    //IEnumerator GlobalCooling()
    //{
    //    globalCooling = true;
    //    yield return new WaitForSeconds(globalTime);
    //    globalCooling = false;
    //}

    //IEnumerator EscapeCooling()
    //{
    //    escapeCooling = true;
    //    yield return new WaitForSeconds(escapeCoolTime);
    //    escapeCooling = false;
    //}

    //float GetAngle(Vector3 from, Vector3 to)
    //{
    //    Vector3 v = to - from;
    //    return Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;
    //}

    //public void TakeDamage(Unit_AI _from, Vector3 _center, float _damage, Data_Manager.SkillStruct _skillStruct)
    //{
    //    // _from ���� ����
    //    // _center ���� ����Ʈ
    //    Debug.Log($"{_from.gameObject.name} : {_damage}");
    //    Vector3 hitPoint = transform.position;
    //    deleDamage?.Invoke(hitPoint, _damage.ToString());// ������ ��Ʈ
    //    healthPoint -= _damage;
    //    deleUpdateHP?.Invoke(healthPoint, unitAttributes.Health);
    //    if (healthPoint <= 0)
    //    {
    //        StateMachine(State.Dead);
    //        return;
    //    }

    //    //StateMachine(State.Damage);
    //    //float aggro = _damage * _skillStruct.aggro;
    //    //target = AddAggro(_from, aggro);// ��׷� �߰� �� Ÿ�� ����

    //    //if (takeDamage != null)
    //    //    StopCoroutine(takeDamage);
    //    //switch (_skillStruct.ccType)
    //    //{
    //    //    case Data_Manager.SkillStruct.CCType.Normal:
    //    //        //takeDamage = StartCoroutine(CCDamage(_center, 0f));
    //    //        break;

    //    //    case Data_Manager.SkillStruct.CCType.KnockBack:
    //    //        takeDamage = StartCoroutine(CCDamage(_center, 1f));
    //    //        break;
    //    //}


    //}

    //IEnumerator CCDamage(Vector3 _from, float _knockBack)
    //{
    //    if (_knockBack > 0)
    //    {
    //        if (skillCastring == true)// ĳ���� ���̰� �и��� ���
    //            skillCastring = false;

    //        Vector3 targetPoint = GetBackPoint(_from, 0f, _knockBack);
    //        if (float.IsInfinity(targetPoint.x))
    //            targetPoint = transform.position;

    //        float normalize = 0f;
    //        while (normalize < 1f)
    //        {
    //            normalize += Time.deltaTime * 3f;
    //            SetRanderer_Damage(normalize);

    //            Vector3 position = Vector3.Lerp(transform.position, targetPoint, normalize);
    //            transform.position = position;
    //            Destination(position);
    //            yield return null;
    //        }
    //    }
    //}

    //IEnumerator TakeDamage()
    //{
    //    float normalize = 0f;
    //    while (normalize < 1f)
    //    {
    //        normalize += Time.deltaTime * 3f;
    //        SetRanderer_Damage(normalize);
    //        yield return null;
    //    }
    //}

    //void SetRanderer_Damage(float _normalize)
    //{
    //    for (int i = 0; i < renderers.Length; i++)
    //    {
    //        renderers[i].material.SetFloat("_Damage", 1f - _normalize);
    //    }
    //}

    //void SetRanderer_Dead(float _targetAmount, float _normalize)
    //{
    //    for (int i = 0; i < renderers.Length; i++)
    //    {
    //        float deadAmount = Mathf.Lerp(1f - _targetAmount, _targetAmount, _normalize);
    //        renderers[i].material.SetFloat("_BlackNWhite", deadAmount);
    //    }
    //}

    //void Destination(Vector3 _point)
    //{
    //    //agent.ResetPath();
    //    //agent.isStopped = true;
    //    agent.SetDestination(_point);

    //    //bool stop = (_point == transform.position);
    //    //if(stop == true)
    //    //{
    //    //    //agent.SetDestination(_point);
    //    //    agent.enabled = false;
    //    //    obstacle.enabled = true;
    //    //}
    //    //else
    //    //{
    //    //    obstacle.enabled = false;
    //    //    agent.enabled = true;
    //    //    agent.SetDestination(_point);
    //    //}
    //}

    //IEnumerator DeadActing()
    //{
    //    float targetAmount = state == State.Dead ? 1f : 0f;
    //    float normalize = 0f;
    //    while (normalize < 1f)
    //    {
    //        normalize += Time.deltaTime;
    //        SetRanderer_Dead(targetAmount, normalize);
    //        yield return null;
    //    }
    //}

    //public Vector3 DirFromAngle(float _angleInDegrees, Transform _trans = null)
    //{
    //    if (_trans != null)
    //    {
    //        // ���� ����
    //        _angleInDegrees += _trans.eulerAngles.y;
    //    }
    //    return new Vector3(Mathf.Sin(_angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(_angleInDegrees * Mathf.Deg2Rad));
    //}

    ////bool VisibleTarget(Transform _target)// ���̴��� Ȯ��
    ////{
    ////    Vector3 dirToTarget = (_target.position - transform.position).normalized;
    ////    if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle * 0.5f)// �ޱ� �ȿ� ���� �Ǵ���
    ////    {
    ////        float dstToTarget = (transform.position - _target.position).magnitude;
    ////        if (Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask) == false)
    ////        {
    ////            return true;
    ////        }
    ////    }
    ////    return false;
    ////}

    //public float CheckDistance()
    //{
    //    if (target != null)
    //    {
    //        float targetDistance = Vector3.Distance(target.transform.position, transform.position);
    //        return targetDistance;
    //    }
    //    return 0f;
    //}

    //public void BattleOver()
    //{
    //    Destination(transform.position);
    //    StopAllCoroutines();
    //}


























































    Coroutine stateAction;
    public void StateMachineTest(State _state)
    {
        if (dummy == true || _state == State.End)
            return;

        if (stateAction != null)
            StopCoroutine(stateAction);

        state = _state;
        agent.avoidancePriority = (int)_state;

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
            StateMachineTest(State.Move);
        }
        else
        {
            StateMachineTest(State.Patrol);
        }
    }

    SkillStruct SelectSkill()
    {
        return readySkills[Random.Range(0, readySkills.Count)];
    }

    void State_Move()
    {
        agent.speed = unitAttributes.MoveSpeed;
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
            yield return new WaitForSeconds(deleyTime);

            float distance = (target.transform.position - transform.position).magnitude;
            float setDistance = target.GetUnitSize + GetUnitSize + GetSkillRange.y;
            if (distance < setDistance)
            {
                moving = false;
                StateMachineTest(State.Attack);
            }
        }
    }

    void State_Patrol()
    {
        agent.speed = unitAttributes.MoveSpeed * 0.3f;
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
            StateMachineTest(State.Idle);
        }
    }

    void Destination(Vector3 _point)
    {
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
            StateMachineTest(State.Idle);
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
    Coroutine collingSkill;

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
        if (collingSkill != null)
            StopCoroutine(collingSkill);
        collingSkill = StartCoroutine(CoolingSkilling());
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

        //StopAllCoroutines();
        Destination(transform.position);
        // _from 때린 유닛
        // _center 맞은 포인트

        Debug.Log($"{_from.gameObject.name} : {_damage}");
        Vector3 hitPoint = transform.position;
        healthPoint -= _damage;
        deleUpdateHP?.Invoke(healthPoint, unitAttributes.Health, true);// 데미지 바
        deleDamage?.Invoke(hitPoint, _damage.ToString());// 데미지 폰트
        if (healthPoint <= 0)
        {
            // 죽음 액션
            StateMachineTest(State.Dead);
            SetRanderer();
            return;
        }

        // 데미지 액션
        SetRanderer();
        float animTime = unitAnimation.PlayAnimation(5);// 애니메이션

        if (holdAction != null)
            StopCoroutine(holdAction);
        holdAction = StartCoroutine(HoldAction(animTime));

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
    Coroutine holdAction;
    IEnumerator HoldAction(float _hold)
    {
        StateMachineTest(State.None);
        yield return new WaitForSeconds(_hold);
        StateMachineTest(State.Idle);
    }

    void DeadState()
    {
        unitAnimation.PlayAnimation(7);// 애니메이션

        deadUnit?.Invoke(this);
        StopAllCoroutines();

        agent.enabled = false;
    }

    void SetRanderer()
    {
        if (setRanderer != null)
            StopCoroutine(setRanderer);

        switch (state)
        {
            case State.Dead:
                setRanderer = StartCoroutine(SetRenderer("_BlackNWhite", 1f));
                break;

            default:
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

    public void BattleOver()
    {
        if (state == State.Dead)
            return;

        StopAllCoroutines();
        StateMachineTest(State.End);
        Destination(transform.position);

        // 승리 포즈
        unitAnimation.PlayAnimation(4);// 애니메이션
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


}
