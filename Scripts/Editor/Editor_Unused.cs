using P01.Editor;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;  // EditorSceneManager�� ����ϱ� ���� �߰�
namespace P01.Editor
{
    public class Editor_Unused : EditorWindow
    {
        [MenuItem("Graphics Tool/04. Unused files")]
        public static void OpenWindow()
        {
            Editor_Unused window = GetWindow<Editor_Unused>("Editor_Unused"); // Ŭ���� �̸��� ��ġ
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

                // objectList �� ������ ������ List�� ������
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
            if (GUILayout.Button("����", buttonText, GUILayout.Height(20f)))
            {
                tutorialToggle = !tutorialToggle;
            }

            if (tutorialToggle == true)
            {
                guiText.normal.textColor = Color.gray;
                GUILayout.Space(10f);
                GUILayout.Label(" 1. Type ����", guiText);
                GUILayout.Label(" 2. ���ҽ� ���� �߰�", guiText);
                GUILayout.Label(" 3. Find Ŭ��", guiText);
                GUILayout.Label(" 4. ������ ���� ��ư Ŭ��", guiText);
                GUILayout.Label(" 5. Save Project ��ư Ŭ��", guiText);

                GUILayout.Space(10f);
                guiText.normal.textColor = Color.yellow;
                GUILayout.Label(" �� �̸� ��ư Ŭ�� - ���� (���Է� �� ���� ����)", guiText);
            }
            EditorGUILayout.EndVertical();
        }

        void FindFiles()
        {
            if (ResourceList.Count == 0)
            {
                //props.Clear();
                //materials.Clear();
                Debug.LogError("DataFolders ���� �ʿ�");
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
            // ������ �߰�
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