using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace P01
{
    public class Resource_FBX : Resource_Material
    {
        public class FBXList
        {
            public ModelImporter importer;
            public GameObject data;
        }
        public List<FBXList> fbxs = new List<FBXList>();
        public Vector2 scrollFBX;
        bool sortFBX;

        //bool isReadable, constraints, animation;
        //ModelImporterMaterialImportMode materialMode;
        //ModelImporterAnimationType animMode;

        public void DisplayFBX()
        {
            if (fbxs.Count <= 0)
                return;

            GUIStyle centerText = new(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                fontSize = 15
            };

            GUIStyle buttonText = new(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.yellow },
                fontSize = 15,
                fontStyle = FontStyle.Bold,
            };

            float setWidth = SetButtonWidth(6f);
            float setHight = 30;
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal("box");
                if (SetButton("FBX", setWidth, setHight))
                {
                    SortFBX("FBX");
                }
                if (SetButton("Read/Write", setWidth, setHight))
                {
                    SortFBX("Read/Write");
                }
                if (SetButton("ImportConstraints", setWidth, setHight))
                {
                    SortFBX("ImportConstraints");
                }
                if (SetButton("ImportAnimation", setWidth, setHight))
                {
                    SortFBX("ImportAnimation");
                }
                if (SetButton("MaterialImportMode", setWidth, setHight))
                {
                    SortFBX("MaterialImportMode");
                }
                if (SetButton("AnimationType", setWidth, setHight))
                {
                    SortFBX("AnimationType");
                }
                EditorGUILayout.EndHorizontal();

                //EditorGUILayout.BeginHorizontal();
                //GUILayout.Label("FBX", centerText, GUILayout.Width(setWidth));
                //if (GUILayout.Button(isReadable.ToString(), GUILayout.Width(setWidth)))
                //{
                //    isReadable = !isReadable;
                //}
                //if (GUILayout.Button(constraints.ToString(), GUILayout.Width(setWidth)))
                //{
                //    constraints = !constraints;
                //}
                //if (GUILayout.Button(animation.ToString(), GUILayout.Width(setWidth)))
                //{
                //    animation = !animation;
                //}

                //materialMode = (ModelImporterMaterialImportMode)EditorGUILayout.EnumPopup("", materialMode, GUILayout.Width(setWidth));
                //animMode = (ModelImporterAnimationType)EditorGUILayout.EnumPopup("", animMode, GUILayout.Width(setWidth));
                //EditorGUILayout.EndHorizontal();
                setHight = 20f;
                scrollFBX = EditorGUILayout.BeginScrollView(scrollFBX);
                for (int i = 0; i < fbxs.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    fbxs[i].data = EditorGUILayout.ObjectField("", fbxs[i].data, typeof(GameObject), false, GUILayout.Width(setWidth), GUILayout.Height(setHight)) as GameObject;
                    buttonText.fontSize = 13;
                    buttonText.fontStyle = FontStyle.Normal;
                    buttonText.normal.textColor = fbxs[i].importer.isReadable ? Color.green : Color.red;
                    if (GUILayout.Button(fbxs[i].importer.isReadable.ToString(), buttonText, GUILayout.Width(setWidth), GUILayout.Height(setHight)))
                    {
                        fbxs[i].importer.isReadable = !fbxs[i].importer.isReadable;
                    }
                    buttonText.normal.textColor = fbxs[i].importer.importConstraints ? Color.green : Color.red;
                    if (GUILayout.Button(fbxs[i].importer.importConstraints.ToString(), buttonText, GUILayout.Width(setWidth), GUILayout.Height(setHight)))
                    {
                        fbxs[i].importer.importConstraints = !fbxs[i].importer.importConstraints;
                    }
                    //GUILayout.Label(fbxs[i].importer.importConstraints.ToString(), centerText, GUILayout.Width(setWidth));
                    buttonText.normal.textColor = fbxs[i].importer.importAnimation ? Color.green : Color.red;
                    if (GUILayout.Button(fbxs[i].importer.importAnimation.ToString(), buttonText, GUILayout.Width(setWidth), GUILayout.Height(setHight)))
                    {
                        fbxs[i].importer.importAnimation = !fbxs[i].importer.importAnimation;
                    }
                    //GUILayout.Label(fbxs[i].importer.importAnimation.ToString(), centerText, GUILayout.Width(setWidth));

                    buttonText.normal.textColor = fbxs[i].importer.materialImportMode == ModelImporterMaterialImportMode.None ? Color.red : Color.green;
                    fbxs[i].importer.materialImportMode = (ModelImporterMaterialImportMode)EditorGUILayout.EnumPopup("", fbxs[i].importer.materialImportMode, buttonText, GUILayout.Width(setWidth), GUILayout.Height(setHight));
                    //GUILayout.Label(fbxs[i].importer.materialImportMode.ToString(), centerText, GUILayout.Width(setWidth));
                    buttonText.normal.textColor = fbxs[i].importer.animationType == ModelImporterAnimationType.None ? Color.red : Color.green;
                    fbxs[i].importer.animationType = (ModelImporterAnimationType)EditorGUILayout.EnumPopup("", fbxs[i].importer.animationType, buttonText, GUILayout.Width(setWidth), GUILayout.Height(setHight));
                    //GUILayout.Label(fbxs[i].importer.animationType.ToString(), centerText, GUILayout.Width(setWidth));

                    //buttonText.normal.textColor = Color.yellow;
                    //if (GUILayout.Button("Remove", buttonText, GUILayout.Width(setWidth)))
                    //{
                    //    fbxs.Remove(fbxs[i]);
                    //    EditorGUILayout.EndHorizontal();
                    //    break;
                    //}
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
            //if (GUILayout.Button("Save Project", buttonText, GUILayout.Height(50f)))
            //{
            //    AssetDatabase.Refresh();
            //    //for(int i = 0; i< fbxs.Count; i++)
            //    //{
            //    //    EditorUtility.SetDirty(fbxs[i].data);
            //    //}
            //    AssetDatabase.SaveAssets();
            //}
        }

        public void FindFBX(string[] _paths)
        {
            fbxs.Clear();
            string[] assets = AssetDatabase.FindAssets("t: Model", _paths);
            // 데이터 추가
            for (int i = 0; i < assets.Length; i++)
            {
                var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(GameObject)) as GameObject;
                var sdata = new SerializedObject(data);
                ModelImporter importer = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(assets[i])) as ModelImporter;
                if (importer != null)
                {
                    FBXList fbxList = new FBXList
                    {
                        importer = importer,
                        data = data,
                    };
                    fbxs.Add(fbxList);
                }
            }
        }

        void SortFBX(string _sortType)
        {
            sortFBX = !sortFBX;
            switch (_sortType)
            {
                case "FBX":
                    fbxs.Sort(delegate (FBXList a, FBXList b)
                    {
                        if (sortFBX == true)
                            return a.data.name.CompareTo(b.data.name);
                        else
                            return b.data.name.CompareTo(a.data.name);
                    });
                    break;

                case "Read/Write":
                    fbxs.Sort(delegate (FBXList a, FBXList b)
                    {
                        if (a.importer.isReadable == b.importer.isReadable)
                            return a.data.name.CompareTo(b.data.name);

                        if (sortFBX == true)
                            return a.importer.isReadable.CompareTo(b.importer.isReadable);
                        else
                            return b.importer.isReadable.CompareTo(a.importer.isReadable);
                    });
                    break;

                case "ImportConstraints":
                    fbxs.Sort(delegate (FBXList a, FBXList b)
                    {
                        if (a.importer.importConstraints == b.importer.importConstraints)
                            return a.data.name.CompareTo(b.data.name);

                        if (sortFBX == true)
                            return a.importer.importConstraints.CompareTo(b.importer.importConstraints);
                        else
                            return b.importer.importConstraints.CompareTo(a.importer.importConstraints);
                    });
                    break;

                case "ImportAnimation":
                    fbxs.Sort(delegate (FBXList a, FBXList b)
                    {
                        if (a.importer.importAnimation == b.importer.importAnimation)
                            return a.data.name.CompareTo(b.data.name);

                        if (sortFBX == true)
                            return a.importer.importAnimation.CompareTo(b.importer.importAnimation);
                        else
                            return b.importer.importAnimation.CompareTo(a.importer.importAnimation);
                    });
                    break;

                case "MaterialImportMode":
                    fbxs.Sort(delegate (FBXList a, FBXList b)
                    {
                        if (a.importer.materialImportMode == b.importer.materialImportMode)
                            return a.data.name.CompareTo(b.data.name);

                        if (sortFBX == true)
                            return a.importer.materialImportMode.CompareTo(b.importer.materialImportMode);
                        else
                            return b.importer.materialImportMode.CompareTo(a.importer.materialImportMode);
                    });
                    break;

                case "AnimationType":
                    fbxs.Sort(delegate (FBXList a, FBXList b)
                    {
                        if (a.importer.animationType == b.importer.animationType)
                            return a.data.name.CompareTo(b.data.name);

                        if (sortFBX == true)
                            return a.importer.animationType.CompareTo(b.importer.animationType);
                        else
                            return b.importer.animationType.CompareTo(a.importer.animationType);
                    });
                    break;
            }
        }
    }
}
#endif