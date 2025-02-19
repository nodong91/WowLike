using P01.Editor;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;  // EditorSceneManager를 사용하기 위해 추가
namespace P01.Editor
{
    public class Editor_Unused : EditorWindow
    {
        [MenuItem("Graphics Tool/04. Unused files")]
        public static void OpenWindow()
        {
            Editor_Unused window = GetWindow<Editor_Unused>("Editor_Unused"); // 클래스 이름과 일치
            window.minSize = new Vector2(500f, 200f);
            window.Show();
        }

        bool tutorialToggle;
        GUIStyle guiText, buttonText;

        void OnGUI()
        {
            guiText = new()
            {
                fontSize = 13,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.MiddleLeft
            };
            Tutorial();

            if (targetObject != null)
            {
                targetObject.Update();

                // objectList 는 위에서 선언한 List의 변수명
                SerializedProperty testProp = targetObject.FindProperty("ResourceList");
                EditorGUILayout.PropertyField(testProp, new GUIContent(testProp.displayName));

                targetObject.ApplyModifiedProperties();
            }

        }

        void Tutorial()
        {
            buttonText = new(GUI.skin.button)
            {
                fontSize = 13,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("사용법", buttonText, GUILayout.Height(20f)))
            {
                tutorialToggle = !tutorialToggle;
            }

            if (tutorialToggle == true)
            {
                guiText.normal.textColor = Color.gray;
                GUILayout.Space(10f);
                GUILayout.Label(" 1. Type 선택", guiText);
                GUILayout.Label(" 2. 리소스 폴더 추가", guiText);
                GUILayout.Label(" 3. Find 클릭", guiText);
                GUILayout.Label(" 4. 변경할 내용 버튼 클릭", guiText);
                GUILayout.Label(" 5. Save Project 버튼 클릭", guiText);

                GUILayout.Space(10f);
                guiText.normal.textColor = Color.yellow;
                GUILayout.Label(" ※ 이름 버튼 클릭 - 정렬 (재입력 시 역순 정렬)", guiText);
            }
            EditorGUILayout.EndVertical();
        }

        void FindFiles()
        {
            if (ResourceList.Count == 0)
            {
                //props.Clear();
                //materials.Clear();
                Debug.LogError("DataFolders 폴더 필요");
                return;
            }
            string[] paths = new string[ResourceList.Count];
            for (int i = 0; i < ResourceList.Count; i++)
            {
                paths[i] = AssetDatabase.GetAssetPath(ResourceList[i]);
                Debug.LogWarning("File paths : " + paths[i]);
            }
        }

        List<Texture2D> images = new List<Texture2D>();

        public void FindImage(string[] _paths)
        {
            images.Clear();
            string[] assets = AssetDatabase.FindAssets("t:Prefab", _paths);
            // 데이터 추가
            for (int i = 0; i < assets.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(GameObject)) as GameObject;
                string assetPath = AssetDatabase.GetAssetPath(prefab);
                Image[] findImage = prefab.GetComponentsInChildren<Image>();
                //TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                for (int j = 0; j < findImage.Length; j++)
                {
                    Sprite sprite = findImage[j].sprite;
                    //RendererList list = new RendererList
                    //{
                    //    path = assetPath,
                    //    gameObject = prefab,
                    //    renderer = findRenderer[j],
                    //};
                    //renderers.Add(list);
                }
            }
            Debug.LogWarning($"{_paths.Length} : {assets.Length} >> {images.Count}");
        }



        SerializedObject targetObject;
        [SerializeField] private List<Object> ResourceList = new List<Object>();

        private void OnEnable()
        {
            targetObject = new SerializedObject(this);
            ResourceList.Clear();
        }
    }
}
#endif