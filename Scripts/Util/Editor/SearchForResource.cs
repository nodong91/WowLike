
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
# if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;  // EditorSceneManager�� ����ϱ� ���� �߰�
namespace P01.Editor
{
    public class SearchForResource : EditorWindow
    {
        [MenuItem("Graphics Tool/04. Search For Resource")]
        public static void OpenWindow()
        {
            SearchForResource window = GetWindow<SearchForResource>("SearchForResource"); // Ŭ���� �̸��� ��ġ
            window.minSize = new Vector2(500f, 200f);
            window.Show();
        }

        private List<Object> registeredObjects = new List<Object>();
        private Vector2 scrollPosition;
        //private Object selectedObject;
        //private const float labelWidth = 200f;  // �� �̸��� ���� �ʺ�
        //private const float buttonWidth = 80f;  // ��ư�� ���� �ʺ�
        private const string RegisteredObjectsKey = "RegisteredObjects";  // EditorPrefs�� ������ Ű

        // ������ ��θ� ������ ����Ʈ
        private List<string> chaseDependencies = new List<string>();
        bool tutorialToggle;
        Object[] objects;

        string showFile = "";
        //Object source;

        private void OnEnable()
        {
            // EditorPrefs���� ��ϵ� ��ü ����Ʈ�� ����
            LoadRegisteredObjects();
        }

        //private void OnDisable()
        //{
        //    // EditorPrefs�� ��ϵ� ��ü ����Ʈ�� ����
        //    SaveRegisteredObjects();
        //}

        //[MenuItem("Tools/RemoveEditorPrefs")]
        static void DeleteAllExample()
        {
            if (EditorUtility.DisplayDialog("Delete all editor preferences.",
                "���� �����Ͱ� ���� �˴ϴ�.\n" +
                "���� �����Ͻð���??", "Yes", "No"))
            {
                Debug.Log("yes");
                EditorPrefs.DeleteKey(RegisteredObjectsKey);
            }
        }

        private void OnGUI()
        {
            //source = EditorGUILayout.ObjectField(source, typeof(Object), true)as Object;
            SetTutorial();
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold,
                fontSize = 17
            };
            objects = Selection.objects;

            float buttonNum = 4f;
            float width = (position.width - 7f * (buttonNum - 1f)) / buttonNum;
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add", buttonStyle, GUILayout.Width(width)))
                {
                    for (int i = 0; i < objects.Length; i++)
                    {
                        AddObjectToList(objects[i]);
                    }
                }
                if (GUILayout.Button("Save", buttonStyle, GUILayout.Width(width)))
                {
                    SaveRegisteredObjects();
                }
                if (GUILayout.Button("Load", buttonStyle, GUILayout.Width(width)))
                {
                    LoadRegisteredObjects();
                }
                GUI.color = Color.red;
                if (GUILayout.Button("Reset", buttonStyle))
                {
                    DeleteAllExample();
                    //registeredObjects.Clear();
                }
                GUILayout.EndHorizontal();
                SortCategory();
            }
            GUILayout.EndVertical();
            GUILayout.Space(5);

            //GUILayout.Label("Registered List:", EditorStyles.boldLabel);

            //// ��ϵ� ��ü ����Ʈ�� �з��Ͽ� ����
            //List<Object> sortedObjects = registeredObjects
            //    .OrderBy(obj => obj.GetType().Name)  // Ÿ�Ժ��� ����
            //    .ThenBy(obj => obj.name)  // �̸����� ����
            //    .ToList();
            // ��ϵ� ��ü ����Ʈ�� �з��Ͽ� ����
            List<Object> sortedObjects = registeredObjects
                .OrderByDescending(obj => obj.name)  // �̸����� ����
                .ToList();

            GUIStyle fontStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
                //fontStyle = FontStyle.Bold,
                fontSize = 15
            };

            float windowWidth = position.width;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(windowWidth), GUILayout.Height(position.height - 100));
            for (int i = sortedObjects.Count - 1; i >= 0; i--)
            {
                string fileExtension = GetFileExtension(sortedObjects[i]);
                if (fileExtension == showFile || showFile == "")
                {
                    GUILayout.BeginHorizontal("box");
                    {
                        GUILayout.Label($"{sortedObjects[i].name} <color=#808080>({fileExtension})</color>", fontStyle);
                        GUILayout.FlexibleSpace();// ���������
                        GUI.color = Color.cyan;
                        if (sortedObjects[i] is SceneAsset)
                        {
                            if (GUILayout.Button("Open", GUILayout.Width(windowWidth * 0.1f)))
                            {
                                OpenSceneWithPrompt(sortedObjects[i]);
                            }
                        }

                        GUI.color = Color.green;
                        if (GUILayout.Button("Search", GUILayout.Width(windowWidth * 0.1f)))
                        {
                            EditorGUIUtility.PingObject(sortedObjects[i]);
                        }

                        GUI.color = Color.yellow;
                        if (GUILayout.Button("Chase", GUILayout.Width(windowWidth * 0.1f)))
                        {
                            OpenChaseWindow(sortedObjects[i]);
                        }

                        GUI.color = Color.red;
                        if (GUILayout.Button("Remove", GUILayout.Width(windowWidth * 0.1f)))
                        {
                            RemoveObjectFromList(sortedObjects[i]);
                            GUILayout.EndHorizontal();
                            continue;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUI.color = Color.white;  // ���� �������� ����
            }
            //GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        void SortCategory()
        {
            GUI.color = Color.white;
            float buttonNum = 9f;
            float width = (position.width - 7f * (buttonNum - 1f)) / buttonNum;
            GUILayout.BeginHorizontal();
            GUI.color = showFile == "" ? Color.yellow : Color.white;
            if (GUILayout.Button("All", GUILayout.Width(width)))
            {
                showFile = "";
            }
            GUI.color = showFile == "unity" ? Color.yellow : Color.white;
            if (GUILayout.Button("Scene", GUILayout.Width(width)))
            {
                showFile = "unity";
            }
            GUI.color = showFile == "asset" ? Color.yellow : Color.white;
            if (GUILayout.Button("Asset", GUILayout.Width(width)))
            {
                showFile = "asset";
            }
            GUI.color = showFile == "prefab" ? Color.yellow : Color.white;
            if (GUILayout.Button("Prefab", GUILayout.Width(width)))
            {
                showFile = "prefab";
            }
            GUI.color = showFile == "cs" ? Color.yellow : Color.white;
            if (GUILayout.Button("Script", GUILayout.Width(width)))
            {
                showFile = "cs";
            }
            GUI.color = showFile == "mat" ? Color.yellow : Color.white;
            if (GUILayout.Button("Material", GUILayout.Width(width)))
            {
                showFile = "mat";
            }
            GUI.color = showFile == "png" ? Color.yellow : Color.white;
            if (GUILayout.Button("Png", GUILayout.Width(width)))
            {
                showFile = "png";
            }
            GUI.color = showFile == "shadergraph" ? Color.yellow : Color.white;
            if (GUILayout.Button("Shader", GUILayout.Width(width)))
            {
                showFile = "shadergraph";
            }
            GUI.color = showFile == "anim" ? Color.yellow : Color.white;
            if (GUILayout.Button("Anim"))
            {
                showFile = "anim";
            }
            GUILayout.EndHorizontal();
        }

        void SetTutorial()
        {
            GUIStyle buttonText = new(GUI.skin.button)
            {
                fontSize = 13,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle guiText = new()
            {
                fontSize = 13,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.MiddleLeft
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
                GUILayout.Label(" 1. �������� �ϱ� ���� ������Ʈ�� Project���� ����", guiText);
                GUILayout.Label(" 2. Add - ������ ������Ʈ ������", guiText);
                GUILayout.Label(" 3. Save - ������ ����", guiText);
                GUILayout.Label(" 4. Load - ����� ����Ʈ �ҷ�����", guiText);
                GUILayout.Label(" 5. Reset - ��� ������ ����", guiText);
                GUILayout.Space(10f);
                GUILayout.Label(" Registered List", guiText);
                GUILayout.Label(" 1. Open - Scene �̵�", guiText);
                GUILayout.Label(" 2. Search - ������Ʈ ����", guiText);
                GUILayout.Label(" 3. Chase - ��ũ�� ������Ʈ ������", guiText);
                GUILayout.Label(" 4. Remove - ����Ʈ���� ����", guiText);

                GUILayout.Space(10f);
            }
            EditorGUILayout.EndVertical();
        }

        private void AddObjectToList(Object obj)
        {
            if (obj != null)
            {
                if (!registeredObjects.Contains(obj))
                {
                    registeredObjects.Add(obj);
                    Debug.Log($"{obj.name} has been added to the list.");
                }
                else
                {
                    EditorUtility.DisplayDialog("�ߺ��� ���ҽ�", "����Ʈ�� ��ϵǾ� �ֽ��ϴ�.", "Ȯ��");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "No object selected. Please select a resource.", "OK");
            }
        }

        private void RemoveObjectFromList(Object obj)
        {
            Debug.Log($"{obj.name} has been removed from the list.");
            registeredObjects.Remove(obj);
        }

        private void OpenSceneWithPrompt(Object sceneObject)
        {
            if (EditorSceneManager.GetActiveScene().isDirty)  // EditorSceneManager ���
            {
                if (EditorUtility.DisplayDialog("�� ����", "���� ���� �����Ͻðڽ��ϱ�?", "��", "�ƴϿ�"))
                {
                    EditorSceneManager.SaveOpenScenes();  // EditorSceneManager ���
                }
            }

            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneObject));  // EditorSceneManager ���
        }

        private string GetFileExtension(Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            return System.IO.Path.GetExtension(path).TrimStart('.').ToLower();
        }

        // Chase ��ư�� ������ �� ���ο� â�� ���� ��θ� ��ư���� ǥ��
        private void OpenChaseWindow(Object obj)
        {
            // �� â ����
            ChaseWindow.ShowWindow(obj);
        }

        private void SaveRegisteredObjects()
        {
            List<string> paths = new List<string>();
            foreach (var obj in registeredObjects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path))
                {
                    paths.Add(path);
                }
            }
            string warning = "���� ���� ���� ���� �˴ϴ�.";
            if (EditorUtility.DisplayDialog("����", warning, "�׷�", "�ƴϿ�") == true)
            {
                EditorPrefs.SetString(RegisteredObjectsKey, string.Join(";", paths));
            }
        }

        private void LoadRegisteredObjects()
        {
            registeredObjects.Clear();
            string savedData = EditorPrefs.GetString(RegisteredObjectsKey, "");
            if (!string.IsNullOrEmpty(savedData))
            {
                string[] paths = savedData.Split(';');
                foreach (var path in paths)
                {
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if (obj != null)
                    {
                        registeredObjects.Add(obj);
                    }
                }
            }
        }
    }

    // ���ο� â�� ��� ��θ� ��ư���� ǥ���ϴ� Ŭ����
    public class ChaseWindow : EditorWindow
    {
        private static List<string> chaseDependencies = new List<string>();
        private Vector2 scrollPos;

        public static void ShowWindow(Object obj)
        {
            // �� â�� ���� ������ ��� ��������
            ChaseWindow window = GetWindow<ChaseWindow>($"{obj.name} Usage");
            window.GetDependencies(obj);
            window.Show();
        }

        private void GetDependencies(Object obj)
        {
            // ������ ��θ� ������ ����Ʈ�� ����
            string assetPath = AssetDatabase.GetAssetPath(obj);
            string[] dependencies = AssetDatabase.GetDependencies(assetPath);
            chaseDependencies = dependencies.ToList();
            if (chaseDependencies.Contains(assetPath))
            {
                chaseDependencies.Remove(assetPath);
            }
        }

        private void OnGUI()
        {
            //GUILayout.Label("Chase Results:", EditorStyles.boldLabel);
            SortCategory();
            GUILayout.Space(5);

            GUI.color = Color.white;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - 50));

            GUIStyle buttonText = new(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Normal,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft
            };

            // ��θ� ��ư���� ǥ��
            foreach (var dependency in chaseDependencies)
            {
                string[] fileExtension = dependency.ToString().Split('.');
                string[] fileName = dependency.ToString().Split('/');

                if (fileExtension[^1].Equals(showFile) || showFile == "")
                {
                    if (GUILayout.Button(fileName[^1], buttonText))
                    {
                        Object targetObject = AssetDatabase.LoadAssetAtPath<Object>(dependency);
                        if (targetObject != null)
                        {
                            EditorGUIUtility.PingObject(targetObject);  // ��� ����
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Error", $"Could not find object at path: {dependency}", "OK");
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }

        string showFile = "";
        void SortCategory()
        {
            GUI.color = Color.white;
            float buttonNum = 9f;
            float width = (position.width - 7f * (buttonNum - 1f)) / buttonNum;
            GUILayout.BeginHorizontal();
            GUI.color = showFile == "" ? Color.yellow : Color.white;
            if (GUILayout.Button("All", GUILayout.Width(width)))
            {
                showFile = "";
            }
            GUI.color = showFile == "unity" ? Color.yellow : Color.white;
            if (GUILayout.Button("Scene", GUILayout.Width(width)))
            {
                showFile = "unity";
            }
            GUI.color = showFile == "asset" ? Color.yellow : Color.white;
            if (GUILayout.Button("Asset", GUILayout.Width(width)))
            {
                showFile = "asset";
            }
            GUI.color = showFile == "prefab" ? Color.yellow : Color.white;
            if (GUILayout.Button("Prefab", GUILayout.Width(width)))
            {
                showFile = "prefab";
            }
            GUI.color = showFile == "cs" ? Color.yellow : Color.white;
            if (GUILayout.Button("Script", GUILayout.Width(width)))
            {
                showFile = "cs";
            }
            GUI.color = showFile == "mat" ? Color.yellow : Color.white;
            if (GUILayout.Button("Material", GUILayout.Width(width)))
            {
                showFile = "mat";
            }
            GUI.color = showFile == "png" ? Color.yellow : Color.white;
            if (GUILayout.Button("Png", GUILayout.Width(width)))
            {
                showFile = "png";
            }
            GUI.color = showFile == "shadergraph" ? Color.yellow : Color.white;
            if (GUILayout.Button("Shader", GUILayout.Width(width)))
            {
                showFile = "shadergraph";
            }
            GUI.color = showFile == "anim" ? Color.yellow : Color.white;
            if (GUILayout.Button("Anim"))
            {
                showFile = "anim";
            }
            GUILayout.EndHorizontal();
        }
    }
}
#endif