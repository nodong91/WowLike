using UnityEngine;
using UnityEngine.Playables;
using System;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ShaderTest))]
public class ShaderTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
        fontStyle.fontSize = 15;
        fontStyle.normal.textColor = Color.yellow;

        ShaderTest Inspector = target as ShaderTest;
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
[RequireComponent(typeof(PlayableDirector))]
public class ShaderTest : MonoBehaviour
{
    public void SetData()
    {
        renderers = gameObject.GetComponentsInChildren<Renderer>();
    }

    //[Flags]
    public enum ShaderControlType
    {
        //None = 1 << 0,
        SetFloat = 1 << 0,
        SetVector = 1 << 1,
        SetColor = 1 << 2,
    }
    //public ShaderControlType controlType;
    public class ControlClass
    {
        public ShaderControlType controlType;
        public string SetName;
        public float SetFloat;
        public Vector4 SetVector;
        public Color SetColor;
    }
    public ControlClass controlClass;
    public Renderer[] renderers;
    //public const string strMainColor = "_MainColor";

    public void ShaderControll(ControlClass controlClass)
    {
        if (renderers == null || renderers.Length == 0)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            bool isPlaying = (Application.isPlaying == true);
            Material[] materials = isPlaying ? renderers[i].materials : renderers[i].sharedMaterials;
            for (int j = 0; j < materials.Length; j++)
            {
                //switch (controlType)
                //{
                //    case ShaderControlType.SetFloat:
                //        materials[j].SetFloat(controlClass.SetName, controlClass.SetFloat);
                //        break;

                //    case ShaderControlType.SetVector:
                //        materials[j].SetVector(controlClass.SetName, controlClass.SetVector);
                //        break;

                //    case ShaderControlType.SetColor:
                //        materials[j].SetColor(controlClass.SetName, controlClass.SetColor);
                //        break;
                //}

                if ((controlClass.controlType & ShaderControlType.SetFloat) != 0)
                {
                    materials[j].SetFloat(controlClass.SetName, controlClass.SetFloat);
                }
                if ((controlClass.controlType & ShaderControlType.SetVector) != 0)
                {
                    materials[j].SetVector(controlClass.SetName, controlClass.SetVector);
                }
                if ((controlClass.controlType & ShaderControlType.SetColor) != 0)
                {
                    materials[j].SetColor(controlClass.SetName, controlClass.SetColor);
                }
            }
        }
    }
}
