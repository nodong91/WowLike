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
        private void OnEnable()
        {
            targetObject = new SerializedObject(this);
            //ResourceList.Clear();
        }

        void OnGUI()
        {
            GUIStyle buttonStyle = new(GUI.skin.button)
            {
                fontSize = 13,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.MiddleCenter
            }; 

            GUILayout.BeginHorizontal("box");
            unitID = EditorGUILayout.TextField("Unit ID",unitID, buttonStyle);
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
            if(gameObjects?.Count > 0)
            {
                EditorGUILayout.LabelField("FBX", labelStyle);
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    EditorGUILayout.ObjectField("", gameObjects[i], typeof(Object), true);
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

            if (GUILayout.Button("폴더", buttonStyle))
            {

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
            gameObjects = new List<GameObject>();
            animationClips = new List<AnimationClip>();
            SetAnimationClip(paths);
            SetMaterial(paths);
            SetPrefab(paths);
        }
        List<Material> materials = new List<Material>();
        List<GameObject> gameObjects = new List<GameObject>();
        List<AnimationClip> animationClips = new List<AnimationClip>();
        UnityEditor.Animations.AnimatorController animator;
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
                    gameObjects.Add(temp);

                    GameObject prefab = InstancePrefab(temp, assetPath);
                    Data_Animation animationData = InstanceAnimationData(temp, assetPath);
                    SetUnit(prefab, animationData);

                    Selection.activeGameObject = prefab;
                }
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
            Animator tempAnimator = _unit.AddComponent<Animator>();
            tempAnimator.runtimeAnimatorController = animator;
            //tempAnimator.runtimeAnimatorController = animator;
            Unit_Animation tempUnit = _unit.AddComponent<Unit_Animation>();
            tempUnit.dataAnimation = _animationData;
            tempUnit.ID = unitID;
        }
    }
}
#endif