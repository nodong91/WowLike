using UnityEngine;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
using P01.Editor;
[CustomEditor(typeof(CT_ShaderControl))]
public class ShaderTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
        fontStyle.fontSize = 15;
        fontStyle.normal.textColor = Color.yellow;

        CT_ShaderControl Inspector = target as CT_ShaderControl;
        if (GUILayout.Button("SetRenderer", fontStyle, GUILayout.Height(30f)))
        {
            Inspector.SetData();
            EditorUtility.SetDirty(Inspector);
        }
        GUILayout.Space(10f);

        base.OnInspectorGUI();
    }
}
#endif

namespace P01.Editor
{
    [RequireComponent(typeof(PlayableDirector))]
    public class CT_ShaderControl : MonoBehaviour
    {
        public void SetData()
        {
            renderers = gameObject.GetComponentsInChildren<Renderer>();
        }

        public enum ShaderControlType
        {
            SetFloat,
            SetVector,
            SetColor,
        }
        //[System.Serializable]
        public class ControlClass
        {
            public ShaderControlType controlType;
            public string SetName;
            public float SetFloat;
            public Vector4 SetVector;
            public Color SetColor;
        }
        public Renderer[] renderers;

        public void ShaderControll(ControlClass _controlClass)
        {
            if (renderers == null || renderers.Length == 0)
                return;

            for (int i = 0; i < renderers.Length; i++)
            {
                bool isPlaying = (Application.isPlaying == true);
                Material[] materials = isPlaying ? renderers[i].materials : renderers[i].sharedMaterials;
                for (int j = 0; j < materials.Length; j++)
                {
                    switch (_controlClass.controlType)
                    {
                        case ShaderControlType.SetFloat:
                            materials[j].SetFloat(_controlClass.SetName, _controlClass.SetFloat);
                            break;

                        case ShaderControlType.SetVector:
                            materials[j].SetVector(_controlClass.SetName, _controlClass.SetVector);
                            break;

                        case ShaderControlType.SetColor:
                            materials[j].SetColor(_controlClass.SetName, _controlClass.SetColor);
                            break;
                    }
                }
            }
            Debug.LogWarning(_controlClass.SetName);
        }
    }
}