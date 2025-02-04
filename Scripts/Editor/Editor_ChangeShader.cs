using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif
namespace P01.Editor
{
    public class Editor_ChangeShader : EditorWindow
    {
        [MenuItem("Graphics Tool/07. ChangeShader")]
        public static void OpenWindow()
        {
            Editor_ChangeShader window = EditorWindow.GetWindow<Editor_ChangeShader>("Editor_ChangeShader");
            window.minSize = new Vector2(500f, 200f);
            window.Show();
        }
        Material material;
        Shader changeShader;
        void OnGUI()
        {
            float setWidth = DivideWigth(4f);
            float setHight = 20;
            GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
            fontStyle.fontSize = 15;
            fontStyle.normal.textColor = Color.yellow;

            EditorGUILayout.BeginHorizontal();
            material = Selection.activeObject as Material;
            EditorGUILayout.ObjectField("", material, typeof(Material), true, GUILayout.Width(setWidth), GUILayout.Height(setHight)) ;
            changeShader = EditorGUILayout.ObjectField("", changeShader, typeof(Shader), true, GUILayout.Width(setWidth), GUILayout.Height(setHight)) as Shader;

            if (GUILayout.Button("Combine", fontStyle, GUILayout.Height(setHight)))
            {
                CheckShaderKeywordState();
                material.shader = changeShader;
                EditorUtility.SetDirty(material);
            }
            EditorGUILayout.EndHorizontal();
        }

        float DivideWigth(float _value)
        {
            float setWidth = (position.width - 5f * (_value - 1f)) / _value;
            return setWidth;
        }

        void CheckShaderKeywordState()
        {
            // Get the instance of the Shader class that the material uses
            var shader = material.shader;

            // Get all the local keywords that affect the Shader
            var keywordSpace = shader.keywordSpace;

            // Iterate over the local keywords
            foreach (var localKeyword in keywordSpace.keywords)
            {
                // If the local keyword is overridable (i.e., it was declared with a global scope),
                // and a global keyword with the same name exists and is enabled,
                // then Unity uses the global keyword state
                if (localKeyword.isOverridable && Shader.IsKeywordEnabled(localKeyword.name))
                {
                    Debug.Log("Local keyword with name of " + localKeyword.name + " is overridden by a global keyword, and is enabled");
                }
                // Otherwise, Unity uses the local keyword state
                else
                {
                    var state = material.IsKeywordEnabled(localKeyword) ? "enabled" : "disabled";
                    Debug.Log("Local keyword with name of " + localKeyword.name + " is " + state);
                }
            }
        }
    }
}