using UnityEngine;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
namespace P01.Editor
{
    public class Editor_CreateUnitPrefab : EditorWindow
    {
        [MenuItem("Graphics Tool/11. CreateUnitPrefab")]
        public static void OpenWindow()
        {
            Editor_CreateUnitPrefab window = EditorWindow.GetWindow<Editor_CreateUnitPrefab>("Editor_CreateUnitPrefab");
            window.minSize = new Vector2(500f, 200f);
            window.Show();
        }

        SerializedObject targetObject;
        [SerializeField] private List<Object> ResourceList = new List<Object>();
        Vector2 scrollPos;
        string unitID;
        List<Material> materials = new List<Material>();
        List<ObjectSetting> objectSetting = new List<ObjectSetting>();
        List<AnimationClip> animationClips = new List<AnimationClip>();
        UnityEditor.Animations.AnimatorController animator;

        private void OnEnable()
        {
            targetObject = new SerializedObject(this);
            //ResourceList.Clear();
        }

        void OnGUI()
        {
            //GUIStyle fontStyle = new("box")
            //{
            //    fontSize = 13,
            //    normal = { textColor = Color.cyan },
            //    alignment = TextAnchor.MiddleCenter
            //};

            GUIStyle buttonStyle = new(GUI.skin.button)
            {
                fontSize = 13,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField("FBX를 프리팹으로 만들어주는 툴");
            EditorGUILayout.LabelField("1. 해당 유닛 ID 입력");
            EditorGUILayout.LabelField("2. FBX가 들어있는 폴더 추가");
            EditorGUILayout.LabelField("3. Find File 클릭");
            EditorGUILayout.LabelField("4. 내용 확인 후 저장 클릭");

            GUILayout.BeginHorizontal("box");
            unitID = EditorGUILayout.TextField("Unit ID", unitID);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            if (targetObject != null)
            {
                targetObject.Update();

                // objectList 는 위에서 선언한 List의 변수명
                SerializedProperty testProp = targetObject.FindProperty("ResourceList");
                EditorGUILayout.PropertyField(testProp, new GUIContent(testProp.displayName));

                targetObject.ApplyModifiedProperties();

                if (GUILayout.Button("Find File", buttonStyle))
                {
                    //tutorialToggle = !tutorialToggle;
                    FindFile();
                }
            }
            GUILayout.EndHorizontal();
            GUIStyle labelStyle = new()
            {
                fontSize = 13,
                normal = { textColor = Color.cyan },
                alignment = TextAnchor.MiddleLeft
            };
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            if (objectSetting?.Count > 0)
            {
                EditorGUILayout.LabelField("FBX", labelStyle);
                for (int i = 0; i < objectSetting.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField("", objectSetting[i].unitObject, typeof(Object), true);
                    if (GUILayout.Button("Remove", buttonStyle))
                    {
                        objectSetting.Remove(objectSetting[i]);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            if (materials?.Count > 0)
            {
                EditorGUILayout.LabelField("MATERIAL", labelStyle);
                for (int i = 0; i < materials.Count; i++)
                {
                    EditorGUILayout.ObjectField("", materials[i], typeof(Object), true);
                }
            }
            if (animationClips?.Count > 0)
            {
                EditorGUILayout.LabelField("ANIMATION CLIP", labelStyle);
                for (int i = 0; i < animationClips.Count; i++)
                {
                    EditorGUILayout.ObjectField("", animationClips[i], typeof(Object), true);
                }
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("저장", buttonStyle))
            {
                Test();
            }
        }

        void FindFile()
        {
            if (ResourceList.Count == 0)
            {
                Debug.LogError("DataFolders 폴더 필요");
                return;
            }

            string[] paths = new string[ResourceList.Count];
            for (int i = 0; i < ResourceList.Count; i++)
            {
                paths[i] = AssetDatabase.GetAssetPath(ResourceList[i]);
                Debug.LogWarning("File paths : " + paths[i]);
            }

            materials = new List<Material>();
            objectSetting = new List<ObjectSetting>();
            animationClips = new List<AnimationClip>();
            SetAnimationClip(paths);
            SetMaterial(paths);
            SetPrefab(paths);
        }

        void SetAnimationClip(string[] _paths)
        {
            string[] assets = AssetDatabase.FindAssets("t: AnimatorController", _paths);

            for (int i = 0; i < assets.Length; i++)
            {
                var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(Object));
                if (data as UnityEditor.Animations.AnimatorController) animator = data as UnityEditor.Animations.AnimatorController;
            }

            assets = AssetDatabase.FindAssets("t: AnimationClip", _paths);
            for (int i = 0; i < assets.Length; i++)
            {
                var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(Object));
                if (data as AnimationClip) animationClips.Add(data as AnimationClip);
            }
        }

        void SetMaterial(string[] _paths)
        {
            string[] assets = AssetDatabase.FindAssets("t: Material", _paths);
            for (int i = 0; i < assets.Length; i++)
            {
                var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(Object));
                if (data as Material) materials.Add(data as Material);
            }
        }
        void SetPrefab(string[] _paths)
        {
            string[] assets = AssetDatabase.FindAssets("t: Model", _paths);
            for (int i = 0; i < assets.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assets[i]);
                // 폴더 위치 찾기
                string[] tempPath = assetPath.Split('/');
                assetPath = "";
                for (int p = 0; p < tempPath.Length - 1; p++)
                {
                    if (p > 0)
                        assetPath += "/";
                    assetPath += tempPath[p];
                }
                Debug.LogWarning(assetPath);
                var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(Object));
                GameObject temp = data as GameObject;
                if (temp != null)
                {
                    ObjectSetting setting = new ObjectSetting
                    {
                        unitObject = temp,
                        unitPath = assetPath,
                    };
                    objectSetting.Add(setting);

                }
            }
        }
        public class ObjectSetting
        {
            public GameObject unitObject;
            public string unitPath;
        }

        void Test()
        {
            for (int i = 0; i < objectSetting.Count; i++)
            {
                GameObject temp = objectSetting[i].unitObject;
                GameObject prefab = InstancePrefab(temp, objectSetting[i].unitPath);
                Data_Animation animationData = InstanceAnimationData(temp, objectSetting[i].unitPath);
                SetUnit(prefab, animationData);

                Selection.activeGameObject = prefab;
            }
        }

        GameObject InstancePrefab(GameObject _object, string _path)
        {
            if (!Directory.Exists(_path + "/Prefabs"))
                AssetDatabase.CreateFolder(_path, "Prefabs");

            string localPath = _path + "/Prefabs/" + _object.name + ".prefab";
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(_object);
            instance.name = _object.name;  // '(Clone)' 접미사 제거
            instance.transform.SetParent(null);  // 부모 설정 방지

            GameObject prefabVariant = PrefabUtility.SaveAsPrefabAsset(instance, localPath);
            EditorUtility.DisplayDialog("완료", $"{localPath} \n\n프리팹 생성 성공!", "확인");
            DestroyImmediate(instance);  // 임시 인스턴스 삭제

            return prefabVariant;
        }

        Data_Animation InstanceAnimationData(GameObject _object, string _path)
        {
            // 애니메이션 데이터 세팅
            Data_Animation animationData = ScriptableObject.CreateInstance<Data_Animation>();
            animationData.SetAnimationClip(animationClips);
            string localPath = _path + "/Prefabs/" + _object.name + ".asset";
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);// 같은거 있는지 체크
            AssetDatabase.CreateAsset(animationData, localPath);
            AssetDatabase.SaveAssets();

            return animationData;
        }

        void SetUnit(GameObject _unit, Data_Animation _animationData)
        {
            Animator tempAnimator = _unit.GetComponent<Animator>();
            if (tempAnimator == null)
                tempAnimator = _unit.AddComponent<Animator>();
            tempAnimator.runtimeAnimatorController = animator;
            //tempAnimator.runtimeAnimatorController = animator;
            Unit_Animation tempUnit = _unit.AddComponent<Unit_Animation>();
            tempUnit.dataAnimation = _animationData;
            tempUnit.ID = unitID;
        }
    }
}
#endif