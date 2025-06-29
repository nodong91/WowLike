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
        Die// 마지막 타입
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
        // 기본 애니메이션 세팅 (Key - 애니메이션 이름)
        clipOverrides["Idle"] = TryAnimationClip((int)AnimationType.Idle);// 각각 이름의 애니메이션 배치 필요
        clipOverrides["Walk"] = TryAnimationClip((int)AnimationType.Walk);
        clipOverrides["Run"] = TryAnimationClip((int)AnimationType.Run);
        // 애니메이터 오버라이드
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
        // 애니메이션 클립 랜덤
        AnimationClip aniClip = animationDatas[_index].animationClips[Random.Range(0, animationDatas[_index].animationClips.Count)];
        if (animationDatas[_index].animationClips.Count > 0) // 애니메이션 클립이 있다면
        {
            //MoveAnimation(0f);// 이동 정지
            string actionState = actionBool ? "01" : "02";// 두개의 애니메이션을 교대로 출력
            actionBool = !actionBool;
            // 애니메이션 클립 오버라이드 (클립 이름을 키로 사용)
            clipOverrides["Action" + actionState] = aniClip;
            animatorOverrideController.ApplyOverrides(clipOverrides);

            switch (animationDatas[_index].playType)
            {
                case Data_Animation.AniClipClass.PlayType.Trigger:
                    // 블랜딩 애니메이션
                    animator.SetTrigger("T_Action" + actionState);// 트리거 파라메타 T_Action 필요
                    animator.SetBool("B_Hold", false);// B_Hold 파라메타 필요
                    break;

                case Data_Animation.AniClipClass.PlayType.Single:
                    // 블랜딩 안됨
                    animator.Play("S_Action" + actionState, -1, 0);// 애니메이션 스테이트 S_Action01,02 필요
                    animator.SetBool("B_Hold", false);
                    break;

                case Data_Animation.AniClipClass.PlayType.Once:
                    // 블랜딩 없이 애니메이션 되고 마지막 프레임에서 정지(죽음 같은 애니메이션)
                    animator.Play("S_Action" + actionState, -1, 0);// 애니메이션 스테이트
                    animator.SetBool("B_Hold", true);
                    break;

                case Data_Animation.AniClipClass.PlayType.Loop:
                    // 루프 애니메이션
                    animator.SetTrigger("T_Action" + actionState);// 트리거 파라메타
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