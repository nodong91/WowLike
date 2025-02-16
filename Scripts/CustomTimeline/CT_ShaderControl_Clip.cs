using UnityEngine;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
using P01.Editor;
[CustomEditor(typeof(CT_ShaderControl_Clip))]
public class CT_ShaderControl_ClipEditor : Editor
{
    Vector2 scrollPosition;
    public override void OnInspectorGUI()
    {
        CT_ShaderControl_Clip Inspector = target as CT_ShaderControl_Clip;
        Inspector.controlType = (CT_ShaderControl_Receiver.ShaderControlType)EditorGUILayout.EnumPopup("Control Type", Inspector.controlType);

        switch (Inspector.controlType)
        {
            case CT_ShaderControl_Receiver.ShaderControlType.SetFloat:
                Inspector.FloatVal = EditorGUILayout.FloatField("Float", Inspector.FloatVal);
                break;

            case CT_ShaderControl_Receiver.ShaderControlType.SetVector:
                Inspector.VectorVal = EditorGUILayout.Vector4Field("Vector", Inspector.VectorVal);
                break;

            case CT_ShaderControl_Receiver.ShaderControlType.SetColor:
                Inspector.ColorVal = EditorGUILayout.ColorField(new GUIContent("Color"), Inspector.ColorVal);
                break;

            case CT_ShaderControl_Receiver.ShaderControlType.SetHDRColor:
                Inspector.HDRColorVal = EditorGUILayout.ColorField(new GUIContent("HDR Color"), Inspector.HDRColorVal, true, true, true);
                break;
        }

        GUIStyle fontStyle = new()
        {
            fontSize = 15,
            normal = { textColor = Color.yellow },
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
        };

        EditorGUILayout.BeginVertical("box");
        Inspector.NameVal = EditorGUILayout.TextField("Shader Property", Inspector.NameVal);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        switch (Inspector.controlType)
        {
            case CT_ShaderControl_Receiver.ShaderControlType.SetFloat:
                SetButton("_Damage", Inspector);
                SetButton("_Specular_Strength", Inspector);
                SetButton("_Rimlight_Strangth", Inspector); 
                SetButton("_PortalAmount", Inspector);
                break;

            case CT_ShaderControl_Receiver.ShaderControlType.SetVector:
                Inspector.VectorVal = EditorGUILayout.Vector4Field("Vector", Inspector.VectorVal);
                break;
                
            case CT_ShaderControl_Receiver.ShaderControlType.SetColor:
                SetButton("_MainColor", Inspector);
                break;

            case CT_ShaderControl_Receiver.ShaderControlType.SetHDRColor:
                SetButton("_EmissionColor", Inspector);
                SetButton("_DamageColor", Inspector);
                break;
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        //base.OnInspectorGUI();
    }

    void SetButton(string _name, CT_ShaderControl_Clip Inspector)
    {
        GUIStyle fontStyle = new(GUI.skin.button)
        {
            fontSize = 12,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter
        };

        if (GUILayout.Button(_name, fontStyle, GUILayout.Height(30f)))
        {
            Inspector.NameVal = _name;
        }
    }
}
#endif

namespace P01.Editor
{
    public class CT_ShaderControl_Clip : PlayableAsset // 트랙에 표시
    {
        public CT_ShaderControl_Receiver.ShaderControlType controlType = CT_ShaderControl_Receiver.ShaderControlType.SetFloat;
        public string NameVal = "";
        public float FloatVal = 0;
        public Vector4 VectorVal = Vector4.zero;
        public Color ColorVal = default;
        [ColorUsage(true, true)]
        public Color HDRColorVal = default;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<CT_ShaderControl_Behaviour>.Create(graph);
            CT_ShaderControl_Behaviour behaviour = playable.GetBehaviour();
            behaviour.controlType = controlType;
            behaviour.NameVal = NameVal;
            behaviour.FloatVal = FloatVal;
            behaviour.VectorVal = VectorVal;
            behaviour.ColorVal = ColorVal;
            behaviour.HDRColorVal = HDRColorVal;
            return playable;
        }
    }

    public class CT_ShaderControl_Behaviour : PlayableBehaviour // 트랙 내용 적용
    {
        public CT_ShaderControl_Receiver.ShaderControlType controlType = CT_ShaderControl_Receiver.ShaderControlType.SetFloat;
        public string NameVal = "";
        public float FloatVal = 0;
        public Vector4 VectorVal = Vector4.zero;
        public Color ColorVal = default;
        [ColorUsage(true, true)]
        public Color HDRColorVal = default;
    }

    public class CT_ShaderControl_Mixer : PlayableBehaviour // 트랙 믹서
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            int inputCount = playable.GetInputCount();

            CT_ShaderControl_Receiver.ShaderControlType finalType = CT_ShaderControl_Receiver.ShaderControlType.SetFloat;
            string finalName = "";
            float finalFloat = 0;
            Vector4 finalVector4 = Vector4.zero;
            Color finalColor = default;
            Color finalHDRColor = default;

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
                finalHDRColor += behaviour.HDRColorVal * weight;

                totalWeight += weight;
            }

            if (totalWeight < 0.5f)
                return;

            CT_ShaderControl_Receiver test = playerData as CT_ShaderControl_Receiver;
            CT_ShaderControl_Receiver.ControlClass controlClass = new()
            {
                controlType = finalType,
                SetName = finalName,
                SetFloat = finalFloat,
                SetVector = finalVector4,
                SetColor = finalColor,
                SetHDRColor = finalHDRColor,
            };
            test?.ShaderControll(controlClass);
        }
    }
}