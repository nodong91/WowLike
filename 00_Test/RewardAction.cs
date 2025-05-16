using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

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
        int randomIndex = Random.Range(0, 1000);
        switch (rewardType)
        {
            case RewardType.Train:
                scoreCurve = squashCurve;
                for (int i = 0; i < 10; i++)
                {
                    RewardMaterial inst = InstanceRewardMaterial();
                    inst.Reward_Train(Input.mousePosition, target, randomIndex);
                    yield return new WaitForSeconds(0.1f);
                }
                break;

            case RewardType.Up:
                scoreCurve = defaultCurve;
                {
                    RewardMaterial inst = InstanceRewardMaterial();
                    inst.Reward_Up(Input.mousePosition, randomIndex);
                }
                break;

            case RewardType.Num:
                scoreCurve = defaultCurve;
                {
                    RewardMaterial inst = InstanceRewardMaterial();
                    inst.Reward_Num(Input.mousePosition, randomIndex);
                }
                break;

            case RewardType.Boom:
                scoreCurve = squashCurve;
                for (int i = 0; i < 10; i++)
                {
                    RewardMaterial inst = InstanceRewardMaterial();
                    inst.Reward_Boom(Input.mousePosition, target, randomIndex);
                }
                break;

            case RewardType.Parabola:
                scoreCurve = squashCurve;
                {
                    RewardMaterial inst = InstanceRewardMaterial();
                    inst.Reward_Scale(Input.mousePosition, target, randomIndex);
                }
                break;
        }
    }

    void MovingOver(RewardMaterial _reward, int _index)
    {
        instQueue.Enqueue(_reward);
        if (scoring != null)
            StopCoroutine(scoring);
        scoring = StartCoroutine(SetScore(_index));
    }

    IEnumerator SetScore(int _addAmount)
    {
        int prevAmount = score;
        score += _addAmount;
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime;
            float index = Mathf.Lerp(prevAmount, score, normalize);
            scoreText.text = Mathf.RoundToInt(index).ToString();

            float curve = scoreCurve.Evaluate(normalize);
            scoreBoard.transform.localScale = new Vector3(1f, 1f, 1f) * curve;
            yield return null;
        }
    }
    public GameObject scoreBoard;
    public AnimationCurve squashCurve, defaultCurve;
    AnimationCurve scoreCurve;
    public TMPro.TMP_Text scoreText;
    int score;
    Coroutine scoring;
}
