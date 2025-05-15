using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class RewardAction : MonoBehaviour
{
    public enum RewardType
    {
        Train,
        Up,
        Num,
        Boom,
        Parabola,
    }
    public RewardType rewardType;
    public RewardMaterial reward;
    public GameObject target;
    public RectTransform parent;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(SetRewardType());
        }
    }
    Queue<RewardMaterial> instQueue = new Queue<RewardMaterial>();
    RewardMaterial InstanceRewardMaterial()
    {
        if (instQueue.Count > 0)
            return instQueue.Dequeue();
        RewardMaterial inst = Instantiate(reward, parent);
        inst.movingOver += MovingOver;
        return inst;
    }

    IEnumerator SetRewardType()
    {
        switch (rewardType)
        {
            case RewardType.Train:
                for (int i = 0; i < 10; i++)
                {
                    RewardMaterial inst = InstanceRewardMaterial();
                    inst.Reward_Train(Input.mousePosition, target);
                    yield return new WaitForSeconds(0.1f);
                }
                break;

            case RewardType.Up:
                {
                    RewardMaterial inst = InstanceRewardMaterial();
                    int randomIndex = Random.Range(0, 1000);
                    inst.Reward_Up(Input.mousePosition, randomIndex);
                }
                break;

            case RewardType.Num:
                {
                    RewardMaterial inst = InstanceRewardMaterial();
                    int randomIndex = Random.Range(0, 1000);
                    inst.Reward_Num(Input.mousePosition, randomIndex);
                }
                break;

            case RewardType.Boom:
                for (int i = 0; i < 10; i++)
                {
                    RewardMaterial inst = InstanceRewardMaterial();
                    inst.Reward_Boom(Input.mousePosition, target);
                }
                break;

            case RewardType.Parabola:
                //for (int i = 0; i < 10; i++)
                //{
                //    RewardMaterial inst = InstanceRewardMaterial();
                //    inst.Reward_Parabola(Input.mousePosition, target, parabolaSpeed, firingAngle);
                //    yield return new WaitForSeconds(0.1f);
                //}
                break;
        }
    }
    //public float parabolaSpeed = 1000f;
    //public float firingAngle = 45f;
    void MovingOver(RewardMaterial _reward)
    {
        instQueue.Enqueue(_reward);
    }
}
