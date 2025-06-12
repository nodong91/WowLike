using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Unit_Looting))]
public class Unit_Animation : MonoBehaviour
{
    public string ID;
    public Data_Animation dataAnimation;
    private List<Data_Animation.AniClipClass> animationDatas;

    public enum AnimationType
    {
        Idle,
        Walk,
        Run,
        Skill,
        Dodge,
        DamageSmall,
        DamageBig,
        Die// ������ Ÿ��
    }
    private AnimationType animationType;
    protected AnimatorOverrideController animatorOverrideController;
    protected AnimationClipOverrides clipOverrides;

    private bool actionBool;
    private Animator animator;

    public void SetAnimator()
    {
        if (dataAnimation == null)
            return;

        animationDatas = dataAnimation.AnimationDatas;

        animator = GetComponent<Animator>();
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);
        SetOverrides();
    }

    void SetOverrides()
    {
        // �⺻ �ִϸ��̼� ���� (Key - �ִϸ��̼� �̸�)
        clipOverrides["Idle"] = TryAnimationClip((int)AnimationType.Idle);// ���� �̸��� �ִϸ��̼� ��ġ �ʿ�
        clipOverrides["Walk"] = TryAnimationClip((int)AnimationType.Walk);
        clipOverrides["Run"] = TryAnimationClip((int)AnimationType.Run);
        // �ִϸ����� �������̵�
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    AnimationClip TryAnimationClip(int _index)
    {
        AnimationClip clip = null;
        if (animationDatas[_index].animationClips.Count > 0)
        {
            clip = animationDatas[_index].animationClips[0];
        }
        return clip;
    }

    public float PlayAnimation(int _index, float _animatorSpeed = 1.0f)
    {
        animator.speed = _animatorSpeed;
        animationType = (AnimationType)_index;
        // �ִϸ��̼� Ŭ�� ����
        AnimationClip aniClip = animationDatas[_index].animationClips[Random.Range(0, animationDatas[_index].animationClips.Count)];
        if (animationDatas[_index].animationClips.Count > 0) // �ִϸ��̼� Ŭ���� �ִٸ�
        {
            //MoveAnimation(0f);// �̵� ����
            string actionState = actionBool ? "01" : "02";// �ΰ��� �ִϸ��̼��� ����� ���
            actionBool = !actionBool;
            // �ִϸ��̼� Ŭ�� �������̵� (Ŭ�� �̸��� Ű�� ���)
            clipOverrides["Action" + actionState] = aniClip;
            animatorOverrideController.ApplyOverrides(clipOverrides);

            switch (animationDatas[_index].playType)
            {
                case Data_Animation.AniClipClass.PlayType.Trigger:
                    // ���� �ִϸ��̼�
                    animator.SetTrigger("T_Action" + actionState);// Ʈ���� �Ķ��Ÿ T_Action �ʿ�
                    animator.SetBool("B_Hold", false);// B_Hold �Ķ��Ÿ �ʿ�
                    break;

                case Data_Animation.AniClipClass.PlayType.Single:
                    // ���� �ȵ�
                    animator.Play("S_Action" + actionState, -1, 0);// �ִϸ��̼� ������Ʈ S_Action01,02 �ʿ�
                    animator.SetBool("B_Hold", false);
                    break;

                case Data_Animation.AniClipClass.PlayType.Once:
                    // ���� ���� �ִϸ��̼� �ǰ� ������ �����ӿ��� ����(���� ���� �ִϸ��̼�)
                    animator.Play("S_Action" + actionState, -1, 0);// �ִϸ��̼� ������Ʈ
                    animator.SetBool("B_Hold", true);
                    break;

                case Data_Animation.AniClipClass.PlayType.Loop:
                    // ���� �ִϸ��̼�
                    animator.SetTrigger("T_Action" + actionState);// Ʈ���� �Ķ��Ÿ
                    animator.SetBool("B_Hold", true);
                    break;
            }
            //Debug.LogWarning($"{gameObject.name} - ({aniClip.name}) : {animationDatas[_index].playType} {animator.GetBool("B_Hold")}");
        }
        return aniClip.length * _animatorSpeed;
    }

    public void MoveAnimation(float _moveSpeed, float _moveX, float _moveY)
    {
        animator.SetFloat("F_MoveSpeed", _moveSpeed);
        animator.SetFloat("F_Move_X", _moveX);
        animator.SetFloat("F_Move_Y", _moveY);
    }
    public delegate void DeleEvent_Action();
    public DeleEvent_Action deleEvent_Action;
    public delegate void DeleEvent_FX(string _id);
    public DeleEvent_FX fxEvent;

    public void Event_Attack()
    {
        deleEvent_Action?.Invoke();
    }

    public void Event_FX(string _id)
    {
        fxEvent?.Invoke(_id);
    }
}