using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace P01.Editor
{
    public class CT_Cinemachie_Clip : PlayableAsset
    {
        public CinemachineBlendDefinition.Styles brainStyles = CinemachineBlendDefinition.Styles.Cut;
        public float FloatVal = 0;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<CT_Cinemachie_Behaviour>.Create(graph);
            CT_Cinemachie_Behaviour behaviour = playable.GetBehaviour();
            behaviour.brainStyles = brainStyles;
            behaviour.FloatVal = FloatVal;
            return playable;
        }
    }

    public class CT_Cinemachie_Behaviour : PlayableBehaviour // 트랙 내용 적용
    {
        public CinemachineBlendDefinition.Styles brainStyles = CinemachineBlendDefinition.Styles.Cut;
        public float FloatVal = 0;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            int inputCount = playable.GetInputCount();

            CinemachineBlendDefinition.Styles brainStyles = CinemachineBlendDefinition.Styles.Cut;
            float FloatVal = 0;

            CT_ShaderControl test = playerData as CT_ShaderControl;
            CT_ShaderControl.ControlClass controlClass = new()
            {
                //controlType = finalType,
                //SetName = finalName,
                //SetFloat = finalFloat,
                //SetVector = finalVector4,
                //SetColor = finalColor,
                //SetHDRColor = finalHDRColor,
            };
            test?.ShaderControll(controlClass);
        }
    }
}