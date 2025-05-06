using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class Pone_Utility
{
    // ���� �迭 ���ϱ�
    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        System.Random prng = new System.Random(seed);
        for (int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }
        return array;
    }

    public static List<T> ShuffleList<T>(List<T> list, int seed)
    {
        List<T> tempList = new List<T>();
        System.Random prng = new System.Random(seed);
        for (int i = 0; i < list.Count-1; i++)
        {
            int randomIndex = prng.Next(i, list.Count);
            T tempItem = list[randomIndex];
            list[randomIndex] = list[i];
            list[i] = tempItem;
        }
        return list;
    }

    // Ȯ�� ���ϱ�
    public static int Chance(float[] probs)
    {
        float total = 0;

        foreach (float elem in probs)
        {
            total += elem;
        }

        float randomPoint = Random.value * total;

        for (int i = 0; i < probs.Length; i++)
        {
            if (randomPoint < probs[i])
            {
                return i;
            }
            else
            {
                randomPoint -= probs[i];
            }
        }
        return probs.Length - 1;
    }

    // �׺�޽� ��ġ ����
    public static Vector3 NavmeshSamplePosition(Vector3 targetPos)
    {
        // �浹 ��ġ�� ���� ����� �׺�޽� ��ġ ����
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 100.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return targetPos;
    }

    // ����� ������Ʈ ã��
    public static GameObject FindClosest(Vector3 center, List<GameObject> objList)
    {
        GameObject closestObject = null;
        float closestDistSqr = Mathf.Infinity;

        for (int i = 0; i < objList.Count; i++)
        {
            Vector3 objectPos = objList[i].transform.position;
            float dist = (objectPos - center).sqrMagnitude;

            if (dist < closestDistSqr)
            {
                closestObject = objList[i].gameObject;
                closestDistSqr = dist;
            }
        }
        return closestObject;
    }

    // UIŬ�� üũ
    public static bool TryUIClick()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public static Color HexToColor(string hex)
    {
        hex = "#" + hex;
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }

    public static string ColorToHex(Color color)
    {
        return ColorUtility.ToHtmlStringRGBA(color);
    }

    public static float GetAngle(Vector2 start, Vector2 end)
    {
        Vector2 v2 = end - start;
        return Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
    }
}