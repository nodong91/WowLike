using UnityEngine;

public class Unit_Generator : MonoBehaviour
{
    //public GameObject buffTest;
    //public Queue<GameObject> buffQueue = new Queue<GameObject>();
    //public List<GameObject> currentBuff = new List<GameObject>();

    //public void OnBuff(Node _node)
    //{
    //    BuffClear();
    //    if (_node.nodeType == Node.NodeType.Player && _node.onObject != null)
    //    {
    //        Unit_AI unit = Game_Manager.current.GetUnitDict[_node.onObject];
    //        if (unit != null)
    //        {
    //            for (int i = 0; i < unit.synergy.Length; i++)
    //            {
    //                Vector2Int grid = _node.grid + unit.synergy[i];
    //                Node synergy = Game_Manager.current.GetMapGenerator.nodeMap[grid.x, grid.y];
    //                //GameObject instBuff = TryBuff();
    //                //instBuff.SetActive(true);
    //                //currentBuff.Add(instBuff);
    //                //instBuff.transform.position = synergy.worldPosition;

    //                instUIManager.followManager.AddFollowVector(synergy.worldPosition);// Å×½ºÆ®
    //            }
    //        }
    //    }
    //}

    //public void BuffClear()
    //{
    //    for (int i = 0; i < currentBuff.Count; i++)
    //    {
    //        buffQueue.Enqueue(currentBuff[i]);
    //        currentBuff[i].SetActive(false);
    //    }
    //    currentBuff.Clear();
    //}

    //GameObject TryBuff()
    //{
    //    if (buffQueue.Count > 0)
    //    {
    //        return buffQueue.Dequeue();
    //    }
    //    GameObject inst = Instantiate(buffTest);
    //    return inst;
    //}
}
