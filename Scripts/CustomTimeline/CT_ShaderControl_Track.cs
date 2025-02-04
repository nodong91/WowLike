using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace P01.Editor
{
    [TrackBindingType(typeof(CT_ShaderControl))]
    [TrackClipType(typeof(CT_ShaderControl_Clip))]
    public class CT_ShaderControl_Track : TrackAsset // 트랙 만들기
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<CT_ShaderControl_Mixer>.Create(graph, inputCount);
            return mixer;
        }
    }
}
