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

    public Dictionary<Transform, Unit_AI> unitDict = new Dictionary<Transform, Unit_AI>();

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
            units[i].SetUnit();
            unitDict[units[i].transform] = units[i];
        }
        for (int i = 0; i < monsters.Count; i++)
        {
            monsters[i].SetUnit();
            unitDict[monsters[i].transform] = monsters[i];
        }
    }

    private void Update()
    {
        Time.timeScale = timeScale;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.LogWarning(unitDict.Count);
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
                child.Value.ResetBattle();
            }
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
    public Unit_AI selectUnit;
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

    public void DeadUnit(Unit_AI _unit)
    {
        units.Remove(_unit);
        if (units.Count == 0)
        {
            for (int i = 0; i < monsters.Count; i++)
            {
                monsters[i].BattleOver();
            }
        }
    }

    public void DeadMonster(Unit_AI _unit)
    {
        monsters.Remove(_unit);
        if (monsters.Count == 0)
        {
            for (int i = 0; i < units.Count; i++)
            {
                units[i].BattleOver();
            }
        }
    }
}
