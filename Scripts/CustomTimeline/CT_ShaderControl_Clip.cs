using UnityEngine;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(CT_ShaderControl_Clip))]
public class CT_ShaderControl_ClipEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
        //fontStyle.fontSize = 15;
        //fontStyle.normal.textColor = Color.yellow;

        CT_ShaderControl_Clip Inspector = target as CT_ShaderControl_Clip;
        //if (GUILayout.Button("SetRenderer", fontStyle, GUILayout.Height(30f)))
        //{
        //    //Inspector.SetData();
        //    EditorUtility.SetDirty(Inspector);
        //}
        Inspector.NameVal = EditorGUILayout.TextField("Name", Inspector.NameVal);
        Inspector.controlType = (CT_ShaderControl.ShaderControlType)EditorGUILayout.EnumPopup("Control Type", Inspector.controlType);

        switch (Inspector.controlType)
        {
            case CT_ShaderControl.ShaderControlType.SetFloat:
                Inspector.FloatVal = EditorGUILayout.FloatField("Float", Inspector.FloatVal);
                break;

            case CT_ShaderControl.ShaderControlType.SetVector:
                Inspector.VectorVal = EditorGUILayout.Vector4Field("Vector", Inspector.VectorVal);
                break;

            case CT_ShaderControl.ShaderControlType.SetColor:
                //Inspector.ColorVal = EditorGUILayout.ColorField("Color", Inspector.ColorVal);
                Inspector.ColorVal = EditorGUILayout.ColorField(new GUIContent("Color"), Inspector.ColorVal, true, true, true);
                break;
        }
        //GUILayout.Space(10f);

        //base.OnInspectorGUI();
    }
}
#endif

public class CT_ShaderControl_Clip : PlayableAsset // 트랙에 표시
{
    public CT_ShaderControl.ShaderControlType controlType;
    public string NameVal = "";
    public float FloatVal = 0;
    public Vector4 VectorVal = Vector4.zero;
    [ColorUsage(true, true)]
    public Color ColorVal = Color.black;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<CT_ShaderControl_Behaviour>.Create(graph);
        CT_ShaderControl_Behaviour behaviour = playable.GetBehaviour();
        behaviour.controlType = controlType;
        behaviour.NameVal = NameVal;
        behaviour.FloatVal = FloatVal;
        behaviour.VectorVal = VectorVal;
        behaviour.ColorVal = ColorVal;
        return playable;
    }
}

public class CT_ShaderControl_Behaviour : PlayableBehaviour // 트랙 내용
{
    public CT_ShaderControl.ShaderControlType controlType;
    public string NameVal = "";
    public float FloatVal = 0;
    public Vector4 VectorVal = Vector4.zero;
    public Color ColorVal = Color.black;
}

public class CT_ShaderControl_Mixer : PlayableBehaviour // 트랙 믹서
{
    //public string ShaderVarName;
    //public Color subColor;
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        int inputCount = playable.GetInputCount();

        CT_ShaderControl.ShaderControlType finalType = CT_ShaderControl.ShaderControlType.SetFloat;
        string finalName = "";
        float finalFloat = 0;
        Vector4 finalVector4 = Vector4.zero;
        Color finalColor = Color.black;
        float totalWeight = 0;
        for (int index = 0; index < inputCount; index++)
        {
            float weight = playable.GetInputWeight(index);
            var inputPlayable = (ScriptPlayable<CT_ShaderControl_Behaviour>)playable.GetInput(index);
            var behaviour = inputPlayable.GetBehaviour();
            if (behaviour == null)
                continue;
            finalType = behaviour.controlType;
            finalName = behaviour.NameVal;
            finalFloat += behaviour.FloatVal * weight;
            finalVector4 += behaviour.VectorVal * weight;
            finalColor += behaviour.ColorVal * weight;

            totalWeight += weight;
        }

        if (totalWeight < 0.5f)
            return;

        CT_ShaderControl test = playerData as CT_ShaderControl;
        CT_ShaderControl.ControlClass controlClass = new()
        {
            controlType = finalType,
            SetName = finalName,
            SetFloat = finalFloat,
            SetVector = finalVector4,
            SetColor = finalColor,
        };
        test?.ShaderControll(controlClass);
        Debug.LogWarning(controlClass.SetName);
    }
}