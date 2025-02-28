using System.Collections.Generic;
using UnityEngine;

public class Unit_AI_Manager : MonoBehaviour
{
    public List<Unit_AI> units = new List<Unit_AI>();
    public List<Unit_AI> monsters = new List<Unit_AI>();

    public List<Unit_AI> UnitList()
    {
        return units;
    }
    public List<Unit_AI> MonsterList()
    {
        return monsters;
    }

    Dictionary<Transform, Unit_AI> unitDict = new Dictionary<Transform, Unit_AI>();
    public Dictionary<Transform, Unit_AI> GetUnitDict { get { return unitDict; } }
    public Unit_AI selectUnit;
    public void SetTimeScale(float _timeScale)
    {
        timeScale = _timeScale;
        Time.timeScale = timeScale;
    }
    public float timeScale = 1f;

    public static Unit_AI_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Time.timeScale = timeScale;
        SpawnUnit();



        //for (int i = 0; i < units.Count; i++)
        //{
        //    units[i].deadUnit += DeadUnit;
        //    units[i].unitList = UnitList;
        //    units[i].monsterList = MonsterList;
        //    units[i].SetUnitStruct("", Unit_AI.GroupType.Unit);

        //    unitDict[units[i].transform] = units[i];
        //}

        //for (int i = 0; i < monsters.Count; i++)
        //{
        //    monsters[i].deadUnit += DeadMonster;
        //    monsters[i].unitList = UnitList;
        //    monsters[i].monsterList = MonsterList;
        //    monsters[i].SetUnitStruct("", Unit_AI.GroupType.Monster);

        //    unitDict[monsters[i].transform] = monsters[i];
        //}
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartBattle();
        }

        if (Input.GetMouseButtonDown(0))
        {
            RayCasting(true);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            RayCasting(false);
        }
    }

    void StartBattle()
    {
        units.Clear();
        monsters.Clear();
        foreach (var child in unitDict)
        {
            switch (child.Value.groupType)
            {
                case Unit_AI.GroupType.Unit:
                    units.Add(child.Value);
                    break;

                case Unit_AI.GroupType.Monster:
                    monsters.Add(child.Value);
                    break;
            }
            child.Value.SetUnit();
        }
    }

    void RayCasting(bool _input)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //int layerMask = 1 << 8;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Debug.LogWarning("히트!! " + hit.collider.gameObject.name);
            Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red, 0.3f);

            if (_input == true)
            {
                if (unitDict.ContainsKey(hit.transform))
                {
                    selectUnit = unitDict[hit.transform];
                }
                else
                {
                    selectUnit = null;
                }
            }
            else if (selectUnit != null)
            {
                Vector3 targetPosition = hit.point;
                selectUnit.CommendMoveing(targetPosition);
            }
        }
    }

    void DeadUnit(Unit_AI _unit)
    {
        units.Remove(_unit);
        if (units.Count == 0)
        {
            for (int i = 0; i < monsters.Count; i++)
            {
                monsters[i].BattleOver();
            }
            Debug.LogWarning($"승리 : monsters {monsters.Count:D2}");
        }
    }

    void DeadMonster(Unit_AI _unit)
    {
        monsters.Remove(_unit);
        if (monsters.Count == 0)
        {
            for (int i = 0; i < units.Count; i++)
            {
                units[i].BattleOver();
            }
            Debug.LogWarning($"승리 : units {units.Count:D2}");
        }
    }




















    //public void UpdateData()
    //{
    //    // 몬스터 생성 데이터 = 스크립트오브젝트로???
    //    spawnUnitPoint = new List<SpawnUnitPoint>();
    //    for (int i = 0; i < units.Count; i++)
    //    {
    //        SpawnUnitPoint setPoint = new SpawnUnitPoint
    //        {
    //            unitID = units[i].unitID,
    //            spawnPosition = units[i].transform.position,
    //            spawnRotation = units[i].transform.rotation.eulerAngles,
    //        };
    //        spawnUnitPoint.Add(setPoint);
    //    }

    //    spawnMonsterPoint = new List<SpawnUnitPoint>();
    //    for (int i = 0; i < monsters.Count; i++)
    //    {
    //        SpawnUnitPoint setPoint = new SpawnUnitPoint
    //        {
    //            unitID = monsters[i].unitID,
    //            spawnPosition = monsters[i].transform.position,
    //            spawnRotation = monsters[i].transform.rotation.eulerAngles,
    //        };
    //        spawnMonsterPoint.Add(setPoint);
    //    }
    //}

    //public List<SpawnUnitPoint> spawnUnitPoint;
    //public List<SpawnUnitPoint> spawnMonsterPoint;

    public Data_Spawn spawnData;
    void SpawnUnit()
    {
        for (int i = 0; i < spawnData.spawnUnitPoint.Count; i++)
        {
            string unitID = spawnData.spawnUnitPoint[i].unitID;
            Unit_AI unit = Singleton_Data.INSTANCE.Dict_Unit[unitID].unitProp;
            Unit_AI inst = Instantiate(unit, transform);
            inst.transform.position = spawnData.spawnUnitPoint[i].spawnPosition;
            inst.transform.rotation = Quaternion.Euler(spawnData.spawnUnitPoint[i].spawnRotation);

            inst.deadUnit += DeadUnit;// 죽음 카운트
            inst.unitList = UnitList;// 타겟을 찾기 위해
            inst.monsterList = MonsterList;// 타겟을 찾기 위해
            inst.SetUnitStruct(unitID, Unit_AI.GroupType.Unit);

            unitDict[inst.transform] = inst;
        }

        for (int i = 0; i < spawnData.spawnMonsterPoint.Count; i++)
        {
            string unitID = spawnData.spawnMonsterPoint[i].unitID;
            Unit_AI unit = Singleton_Data.INSTANCE.Dict_Unit[unitID].unitProp;
            Unit_AI inst = Instantiate(unit, transform);
            inst.transform.position = spawnData.spawnMonsterPoint[i].spawnPosition;
            inst.transform.rotation = Quaternion.Euler(spawnData.spawnMonsterPoint[i].spawnRotation);

            inst.deadUnit += DeadUnit;
            inst.unitList = UnitList;
            inst.monsterList = MonsterList;
            inst.SetUnitStruct(unitID, Unit_AI.GroupType.Monster);

            unitDict[inst.transform] = inst;
        }
    }
}
