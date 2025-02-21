using System.Collections;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AI;

public class Unit_AI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Unit_AI target;
    public Transform dummy;
    public float maxDistance;
    public float unitSize;

    public enum State
    {
        None,
        Idle,
        Move,
        RunAway,
        Attack,
        Damage,
        Dead
    }
    public State state = State.None;
    public float distance;
    Coroutine stateMachine;

    public Renderer[] renderers;

    void Start()
    {
        StateMachine(State.None);
    }

    void StateMachine(State _state)
    {
        state = _state;
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
                    distance = (target.transform.position - transform.position).magnitude;
                    if (distance > viewRadius)
                    {
                        // 멀면 이동
                        Vector3 targetPosition = MoveState(target.transform);
                        agent.SetDestination(targetPosition);
                        Debug.LogWarning($" Distance : {distance}");
                        yield return null;

                        StateMachine(State.Move);
                    }
                    else
                    {
                        // 가까우면 공격
                        StateMachine(State.Attack);
                    }
                    break;

                case State.Move:
                    float remainingDistance = agent.remainingDistance;
                    distance = (target.transform.position - transform.position).magnitude;
                    if (remainingDistance == 0f || distance < viewRadius)
                    {
                        StateMachine(State.Idle);
                    }
                    break;

                case State.RunAway:
                    remainingDistance = agent.remainingDistance;
                    if (remainingDistance == 0f)
                    {
                        state = State.Idle;
                    }
                    break;

                case State.Attack:
                    agent.ResetPath();
                    target.TakeDamage(1f, transform);
                    Debug.LogWarning("State.Attack");
                    yield return new WaitForSeconds(1f);
                    StateMachine(State.Idle);
                    break;

                case State.Damage:

                    break;

                case State.Dead:

                    break;
            }
            yield return null;
        }
    }

    public float frontValue = 45f;
    public float backValue = 90f;
    Vector3 MoveState(Transform _from)
    {
        float angle = GetAngle(_from.position, transform.position);
        float randomIndex = Random.Range(-1f, 1f) * frontValue;
        Vector3 dirFromAngle = DirFromAngle(randomIndex + angle, false);
        float unitDistance = target.unitSize + unitSize;
        Vector3 targetPosition = _from.transform.position + dirFromAngle * unitDistance;

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, maxDistance, -1))
        {
            dummy.position = hit.position;
            return hit.position;
        }
        return default;
    }

    float runAwayCoolTime = 3f;
    bool runAwayCool;

    IEnumerator RunAwayCooling()
    {
        runAwayCool = true;
        yield return new WaitForSeconds(runAwayCoolTime);
        runAwayCool = false;
    }

    void RunAwayState(Transform _from)
    {
        float angle = GetAngle(_from.position, transform.position);
        float randomIndex = Random.Range(-1f, 1f) * backValue;
        Vector3 dirFromAngle = DirFromAngle(randomIndex + angle, false);
        Vector3 targetPosition = transform.position + dirFromAngle * viewRadius;
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, maxDistance, -1))
        {
            dummy.position = hit.position;
            agent.SetDestination(hit.position);
        }
        StateMachine(State.RunAway);
    }

    public static float GetAngle(Vector3 from, Vector3 to)
    {
        Vector3 v = to - from;
        return Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;
    }


    Coroutine takeDamage;
    public void TakeDamage(float _damage, Transform _from)
    {
        StateMachine(State.Damage);

        if (takeDamage != null)
            StopCoroutine(takeDamage);
        takeDamage = StartCoroutine(TakeDamage(_from));
    }
    public bool run;
    IEnumerator TakeDamage(Transform _from)
    {
        if (run == true && state != State.RunAway && runAwayCool == false)
        {
            RunAwayState(_from);
            StartCoroutine(RunAwayCooling());
            yield return null;
        }

        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime;
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetFloat("_Damage", 1f - normalize);
            }
            yield return null;
        }
    }











    public float viewRadius;
    public float viewAngle;
    public LayerMask targetMask, obstacleMask;

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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
        Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360f, viewRadius);

        Vector3 viewAngleA = DirFromAngle(viewAngle * 0.5f, false);
        Vector3 viewAngleB = DirFromAngle(-viewAngle * 0.5f, false);

        Handles.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Handles.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

        if (target != null)
        {
            Color color = Color.green;
            GUIStyle fontStyle = new()
            {
                fontSize = 50,
                normal = { textColor = color },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };

            Handles.color = Gizmos.color = color;
            Handles.DrawLine(transform.position, target.transform.position);
            string tempColor = VisibleTarget(target.transform) ? "0000FF" : "FF0000";
            tempColor = $"<color=#{tempColor}>{VisibleTarget(target.transform)}</color>";
            Handles.Label(target.transform.position, $"{tempColor} : {CheckDistance().ToString("N2")}", fontStyle);
            Gizmos.DrawSphere(target.transform.position, 0.3f);
        }
    }
#endif
}
