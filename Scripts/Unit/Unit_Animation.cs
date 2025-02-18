using System.Collections.Generic;
using UnityEngine;
public class Unit_Animation : MonoBehaviour
{
    public Data_Animation dataAnimation;
    public List<Data_Animation.AniClipClass> AnimationDatas;

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

    private Animator animator;
    private bool actionBool;

    private void Start()
    {
        SetAnimator();
    }

    public void SetAnimator()
    {
        AnimationDatas = dataAnimation.AnimationDatas;

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
        if (AnimationDatas[_index].animationClips.Count > 0)
        {
            clip = AnimationDatas[_index].animationClips[0];
        }
        return clip;
    }

    public float PlayAnimation(int _index, float _animatorSpeed = 1.0f)
    {
        animator.speed = _animatorSpeed;
        animationType = (AnimationType)_index;
        // �ִϸ��̼� Ŭ�� ����
        AnimationClip aniClip = AnimationDatas[_index].animationClips[Random.Range(0, AnimationDatas[_index].animationClips.Count)];
        if (AnimationDatas[_index].animationClips.Count > 0) // �ִϸ��̼� Ŭ���� �ִٸ�
        {
            //MoveAnimation(0f);// �̵� ����
            string actionState = actionBool ? "01" : "02";// �ΰ��� �ִϸ��̼��� ����� ���
            actionBool = !actionBool;
            // �ִϸ��̼� Ŭ�� �������̵� (Ŭ�� �̸��� Ű�� ���)
            clipOverrides["Action" + actionState] = aniClip;
            animatorOverrideController.ApplyOverrides(clipOverrides);

            animator.Play("S_Action" + actionState, -1, 0);// �ִϸ��̼� ������Ʈ
            //animator.SetBool("B_Hold", true);// B_Hold �Ķ��Ÿ �ʿ�

            //Debug.LogWarning($"{aniClip.name}    {"Action" + actionState}       {"S_Action" + actionState}");
            //switch (AnimationDatas[_index].playType)
            //{
            //    case AniClipClass.PlayType.Trigger:
            //        // ���� �ִϸ��̼�
            //        animator.SetTrigger("T_Action" + actionState);// Ʈ���� �Ķ��Ÿ T_Action �ʿ�
            //        animator.SetBool("B_Hold", false);// B_Hold �Ķ��Ÿ �ʿ�
            //        break;

            //    case AniClipClass.PlayType.Single:
            //        // ���� �ȵ�
            //        animator.Play("Action State" + actionState, -1, 0);// �ִϸ��̼� ������Ʈ Action State �ʿ�
            //        animator.SetBool("B_Hold", false);
            //        break;

            //    case AniClipClass.PlayType.Once:
            //        // ���� ���� �ִϸ��̼� �ǰ� ������ �����ӿ��� ����(���� ���� �ִϸ��̼�)
            //        animator.Play("Action State" + actionState, -1, 0);// �ִϸ��̼� ������Ʈ
            //        animator.SetBool("B_Hold", true);
            //        break;

            //    case AniClipClass.PlayType.Loop:
            //        // ���� �ִϸ��̼�
            //        animator.SetTrigger("T_Action" + actionState);// Ʈ���� �Ķ��Ÿ
            //        animator.SetBool("B_Hold", true);
            //        break;
            //}
        }
        return aniClip.length * _animatorSpeed;
    }

    public void MoveAnimation(float _moveSpeed, float _moveX, float _moveY)
    {
        animator.SetFloat("F_MoveSpeed", _moveSpeed);
        animator.SetFloat("F_Move_X", _moveX);
        animator.SetFloat("F_Move_Y", _moveY);
    }
}