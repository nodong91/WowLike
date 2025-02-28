using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Data_Spawn))]
public class Data_Spawn_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
        fontStyle.fontSize = 15;
        fontStyle.normal.textColor = Color.yellow;

        Data_Spawn Inspector = target as Data_Spawn;
        if (GUILayout.Button("Update Data", fontStyle, GUILayout.Height(30f)))
        {
            Inspector.UpdateData();
            EditorUtility.SetDirty(Inspector);
        }
        GUILayout.Space(10f);
        base.OnInspectorGUI();
    }
}
#endif
[CreateAssetMenu(fileName = "Data_Spawn", menuName = "Scriptable Objects/Data_Spawn")]
public class Data_Spawn : ScriptableObject
{
    public Unit_AI.GroupType groupType;

#if UNITY_EDITOR
    public void UpdateData()
    {
        switch (groupType)
        {
            case Unit_AI.GroupType.None:

                break;

            case Unit_AI.GroupType.Unit:
                //몬스터 생성 데이터 = 스크립트오브젝트로 ???
                spawnUnitPoint = new List<SpawnUnitPoint>();
                //Selection.activeGameObject = unitSet;
                Unit_AI[] units = Selection.activeGameObject.GetComponentsInChildren<Unit_AI>();
                for (int i = 0; i < units.Length; i++)
                {
                    SpawnUnitPoint setPoint = new SpawnUnitPoint
                    {
                        unitID = units[i].unitID,
                        spawnPosition = units[i].transform.position,
                        spawnRotation = units[i].transform.rotation.eulerAngles,
                    };
                    spawnUnitPoint.Add(setPoint);
                }
                break;

            case Unit_AI.GroupType.Monster:

                spawnMonsterPoint = new List<SpawnUnitPoint>();
                Unit_AI[] monsters = Selection.activeGameObject.GetComponentsInChildren<Unit_AI>();
                for (int i = 0; i < monsters.Length; i++)
                {
                    SpawnUnitPoint setPoint = new SpawnUnitPoint
                    {
                        unitID = monsters[i].unitID,
                        spawnPosition = monsters[i].transform.position,
                        spawnRotation = monsters[i].transform.rotation.eulerAngles,
                    };
                    spawnMonsterPoint.Add(setPoint);
                }
                break;
        }
    }
#endif

    [System.Serializable]
    public struct SpawnUnitPoint
    {
        public string unitID;
        public Vector3 spawnPosition;
        public Vector3 spawnRotation;
    }
    public List<SpawnUnitPoint> spawnUnitPoint;
    public List<SpawnUnitPoint> spawnMonsterPoint;

    //void SpawnUnit()
    //{
    //    for (int i = 0; i < spawnUnitPoint.Count; i++)
    //    {
    //        Unit_AI unit = Singleton_Data.INSTANCE.Dict_Unit[spawnUnitPoint[i].unitID].unitProp;
    //        Unit_AI inst = Instantiate(unit, transform);
    //        inst.transform.position = spawnUnitPoint[i].spawnPosition;
    //        inst.transform.rotation = Quaternion.Euler(spawnUnitPoint[i].spawnRotation);

    //        inst.deadUnit += DeadUnit;// 죽음 카운트
    //        inst.unitList = UnitList;// 타겟을 찾기 위해
    //        inst.monsterList = MonsterList;// 타겟을 찾기 위해
    //        inst.SetUnitStruct("", Unit_AI.GroupType.Unit);

    //        unitDict[inst.transform] = inst;
    //    }

    //    for (int i = 0; i < spawnMonsterPoint.Count; i++)
    //    {
    //        Unit_AI unit = Singleton_Data.INSTANCE.Dict_Unit[spawnMonsterPoint[i].unitID].unitProp;
    //        Unit_AI inst = Instantiate(unit, transform);
    //        inst.transform.position = spawnMonsterPoint[i].spawnPosition;
    //        inst.transform.rotation = Quaternion.Euler(spawnMonsterPoint[i].spawnRotation);

    //        inst.deadUnit += DeadUnit;
    //        inst.unitList = UnitList;
    //        inst.monsterList = MonsterList;
    //        inst.SetUnitStruct("", Unit_AI.GroupType.Monster);

    //        unitDict[inst.transform] = inst;
    //    }
    //}
}
