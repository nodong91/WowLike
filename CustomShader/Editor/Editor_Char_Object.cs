using UnityEditor;
using UnityEngine;
//P01.Editor.Editor_Char_Object
namespace P01.Editor
{
    public class Editor_Char_Object : ShaderGUI
    {
        Vector2 scrollPosition;
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            GUILayout.Label("특수 효과 글자", EditorStyles.boldLabel);
            EditorGUILayout.Space(10f);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10f);
            materialEditor.RenderQueueField();
            materialEditor.DoubleSidedGIField();
            //base.OnGUI(materialEditor, properties);
        }
    }
}