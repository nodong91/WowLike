using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace P01
{
    public class Resource_Material : Resource_Texture
    {
        public List<Material> materials = new List<Material>();
        Vector2 scrollMaterial;
        bool sortMaterial;

        public enum SetShader
        {
            Character,
            Background
        }
        public SetShader setShader;
        Shader aaaa;

        public void DisplayMaterial()
        {
            if (materials?.Count <= 0)
                return;

            float setWidth = SetButtonWidth(4f);
            float setHight = 35;

            GUIStyle buttonText = new(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle centerText = new(GUI.skin.box)
            {
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft
            };
           
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal("box");
                if (SetButton("Material", setWidth, setHight))
                {
                    SortMaterial("Material");
                }
                if (SetButton("Shader", setWidth, setHight))
                {
                    SortMaterial("Shader");
                }
                if (SetButton("EnableGPUInstancing", setWidth, setHight))
                {
                    SortMaterial("EnableGPUInstancing");
                }
                EditorGUILayout.EndHorizontal();
                setHight = 20f;
                scrollMaterial = EditorGUILayout.BeginScrollView(scrollMaterial);
                for (int i = 0; i < materials.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    materials[i] = EditorGUILayout.ObjectField("", materials[i], typeof(Material), true, GUILayout.Width(setWidth), GUILayout.Height(setHight)) as Material;

                    string shaderName = materials[i].shader.name;
                    //centerText.normal.textColor = shaderName.Contains("Hidden") ? Color.red : Color.white;// 히든이면 빨간색으로

                    //GUILayout.Label(shaderName, centerText, GUILayout.Width(setWidth));
                    EditorGUILayout.ObjectField("", materials[i].shader, typeof(Shader), true, GUILayout.Width(setWidth), GUILayout.Height(setHight));
                    //setShader = (SetShader)EditorGUILayout.EnumPopup("", setShader, centerText, GUILayout.Width(setWidth), GUILayout.Height(setHight));
                    aaaa = EditorGUILayout.ObjectField("", aaaa, typeof(Shader), true, GUILayout.Width(setWidth), GUILayout.Height(setHight))as Shader;
                    //GUILayout.Label(matName, centerText, GUILayout.Width(setWidth));
                    buttonText.normal.textColor = materials[i].enableInstancing ? Color.green : Color.red;
                    if (GUILayout.Button(materials[i].enableInstancing.ToString(), buttonText, GUILayout.Width(setWidth), GUILayout.Height(setHight)))
                    {
                        materials[i].enableInstancing = !materials[i].enableInstancing;
                    }
                    //buttonText.normal.textColor = Color.yellow;
                    //if (GUILayout.Button("Remove", buttonText, GUILayout.Width(setWidth)))
                    //{
                    //    materials.Remove(materials[i]);
                    //    EditorGUILayout.EndHorizontal();
                    //    break;
                    //}
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        public void FindMaterial(string[] _paths)
        {
            materials.Clear();
            string[] assets = AssetDatabase.FindAssets("t: Material", _paths);
            // 데이터 추가
            for (int i = 0; i < assets.Length; i++)
            {
                var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(Material)) as Material;
                materials.Add(data);
            }
        }

        void SortMaterial(string _sortType)
        {
            sortMaterial = !sortMaterial;
            switch (_sortType)
            {
                case "Material":
                    materials.Sort(delegate (Material a, Material b)
                    {
                        if (sortMaterial == true)
                            return a.name.CompareTo(b.name);
                        else
                            return b.name.CompareTo(a.name);
                    });
                    break;

                case "Shader":
                    materials.Sort(delegate (Material a, Material b)
                    {
                        if (a.shader == b.shader)
                            return a.name.CompareTo(b.name);

                        if (sortMaterial == true)
                            return a.shader.name.CompareTo(b.shader.name);
                        else
                            return b.shader.name.CompareTo(a.shader.name);
                    });
                    break;

                case "EnableGPUInstancing":
                    materials.Sort(delegate (Material a, Material b)
                    {
                        if (a.enableInstancing == b.enableInstancing)
                            return a.name.CompareTo(b.name);

                        if (sortMaterial == true)
                            return a.enableInstancing.CompareTo(b.enableInstancing);
                        else
                            return b.enableInstancing.CompareTo(a.enableInstancing);
                    });
                    break;
            }
        }
    }
}
#endif