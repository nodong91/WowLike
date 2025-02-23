using System.Collections.Generic;
using UnityEngine;

public class Unit_AI_Manager : MonoBehaviour
{
    public List<Unit_AI> units = new List<Unit_AI>();
    public List<Unit_AI> monsters = new List<Unit_AI>();

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
        }
        for (int i = 0; i < monsters.Count; i++)
        {
            monsters[i].SetUnit();
        }
    }

    private void Update()
    {
        Time.timeScale = timeScale;
        if (Input.GetKeyDown(KeyCode.A))
        {
            for (int i = 0; i < units.Count; i++)
            {
                units[i].ResetBattle();
            }
            for (int i = 0; i < monsters.Count; i++)
            {
                monsters[i].ResetBattle();
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
