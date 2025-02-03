using UnityEngine;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(CT_Clip))]
public class CT_ClipEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
        //fontStyle.fontSize = 15;
        //fontStyle.normal.textColor = Color.yellow;

        CT_Clip Inspector = target as CT_Clip;
        //if (GUILayout.Button("SetRenderer", fontStyle, GUILayout.Height(30f)))
        //{
        //    //Inspector.SetData();
        //    EditorUtility.SetDirty(Inspector);
        //}
        Inspector.NameVal = EditorGUILayout.TextField("Name", Inspector.NameVal);
        Inspector.controlType = (ShaderTest.ShaderControlType)EditorGUILayout.EnumPopup("Control Type", Inspector.controlType);

        switch (Inspector.controlType)
        {
            case ShaderTest.ShaderControlType.SetFloat:
                Inspector.FloatVal = EditorGUILayout.FloatField("Float", Inspector.FloatVal);
                break;

            case ShaderTest.ShaderControlType.SetVector:
                Inspector.VectorVal = EditorGUILayout.Vector4Field("Vector", Inspector.VectorVal);
                break;

            case ShaderTest.ShaderControlType.SetColor:
                Inspector.ColorVal = EditorGUILayout.ColorField("Color", Inspector.ColorVal);
                //Inspector.ewrewr = EditorGUILayout.ColorField("ewrewrColor", Inspector.ewrewr, true, true, true,);
                break;
        }
        //GUILayout.Space(10f);

        base.OnInspectorGUI();
    }
}
#endif

public class CT_Clip : PlayableAsset // 트랙에 표시
{
    public ShaderTest.ShaderControlType controlType;
    public string NameVal = "";
    public float FloatVal = 0;
    public Vector4 VectorVal = Vector4.zero;
    [ColorUsage(true, true)]
    public Color ColorVal = Color.black;
    public Color ewrewr;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<CT_Behaviour>.Create(graph);
        CT_Behaviour behaviour = playable.GetBehaviour();
        behaviour.controlType = controlType;
        behaviour.NameVal = NameVal;
        behaviour.FloatVal = FloatVal;
        behaviour.VectorVal = VectorVal;
        behaviour.ColorVal = ColorVal;
        return playable;
    }
}

public class CT_Behaviour : PlayableBehaviour // 트랙 내용
{
    public ShaderTest.ShaderControlType controlType;
    public string NameVal = "";
    public float FloatVal = 0;
    public Vector4 VectorVal = Vector4.zero;
    public Color ColorVal = Color.black;
}

public class CT_Mixer : PlayableBehaviour // 트랙 믹서
{
    //public string ShaderVarName;
    //public Color subColor;
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        int inputCount = playable.GetInputCount();

        ShaderTest.ShaderControlType finalType = ShaderTest.ShaderControlType.SetFloat;
        string finalName = "";
        float finalFloat = 0;
        Vector4 finalVector4 = Vector4.zero;
        Color finalColor = Color.black;
        float totalWeight = 0;
        for (int index = 0; index < inputCount; index++)
        {
            float weight = playable.GetInputWeight(index);
            var inputPlayable = (ScriptPlayable<CT_Behaviour>)playable.GetInput(index);
            var behaviour = inputPlayable.GetBehaviour();
            finalType = behaviour.controlType;
            finalName = behaviour.NameVal;
            finalFloat += behaviour.FloatVal * weight;
            finalVector4 += behaviour.VectorVal * weight;
            finalColor += behaviour.ColorVal * weight;

            totalWeight += weight;
        }

        if (totalWeight < 0.5f)
            return;

        ShaderTest test = playerData as ShaderTest;
        ShaderTest.ControlClass controlClass = new()
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