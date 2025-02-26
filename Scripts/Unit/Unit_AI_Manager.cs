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
    public float timeScale = 1f;

    public static Unit_AI_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Time.timeScale = timeScale;
        for (int i = 0; i < units.Count; i++)
        {
            units[i].deadUnit += DeadUnit;
            units[i].unitList = UnitList;
            units[i].monsterList = MonsterList;

            unitDict[units[i].transform] = units[i];
            //if (i < 2)
            //    units[i].SetUnitStruct(Singleton_Data.INSTANCE.Dict_Unit["U10011"]);
            //else
            //    units[i].SetUnitStruct(Singleton_Data.INSTANCE.Dict_Unit["U10010"]);
            units[i].SetUnitStruct();
        }

        for (int i = 0; i < monsters.Count; i++)
        {
            monsters[i].deadUnit += DeadMonster;
            monsters[i].unitList = UnitList;
            monsters[i].monsterList = MonsterList;

            unitDict[monsters[i].transform] = monsters[i];
            //if (i < 2)
            //    monsters[i].SetUnitStruct(Singleton_Data.INSTANCE.Dict_Unit["U10011"]);
            //else
            //    monsters[i].SetUnitStruct(Singleton_Data.INSTANCE.Dict_Unit["U10010"]);
            monsters[i].SetUnitStruct();
        }
    }


    private void Update()
    {
        Time.timeScale = timeScale;
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
            Debug.LogWarning("È÷Æ®!! " + hit.collider.gameObject.name);
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
            Debug.LogWarning($"½Â¸® : monsters {monsters.Count:D2}");
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
            Debug.LogWarning($"½Â¸® : units {units.Count:D2}");
        }
    }
}
