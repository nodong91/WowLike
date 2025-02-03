using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackBindingType(typeof(ShaderTest))]
[TrackClipType(typeof(CT_Clip))]
public class CT_Track : TrackAsset // 트랙 만들기
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var mixer = ScriptPlayable<CT_Mixer>.Create(graph, inputCount);
        return mixer;
    }
}
