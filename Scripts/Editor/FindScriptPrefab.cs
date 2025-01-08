using System.Collections.Generic;
using UnityEngine;
using System.Reflection;



#if UNITY_EDITOR
using UnityEditor;
using System.Linq;

namespace P01
{
    public class FindScriptPrefab : EditorWindow
    {
        [MenuItem("Graphics Tool/FindScriptPrefab")]
        public static void OpenWindow()
        {
            FindScriptPrefab window = EditorWindow.GetWindow<FindScriptPrefab>("FindScriptPrefab");
            window.minSize = new Vector2(500f, 200f);
            window.Show();
        }

        const string one = "Scene Prefab(Script)";
        const string two = "Scene Prefab(Missing)";
        const string three = "Project Prefab(Missing)";
        const string four = "Selected Prefab(Missing)";
        const string five = "Scene Prefab(String)";

        public struct MissingPrefab
        {
            public string missingName;
            public GameObject prefab;
        }
        List<MissingPrefab> findMissing = new List<MissingPrefab>();
        Vector2 scrollBehaviour;
        bool sortBehaviour, tutorialToggle;

        void OnGUI()
        {
            SetTutorial();
            GUIStyle buttonText = new(GUI.skin.button)
            {
                fontSize = 13,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.MiddleCenter
            };

            float amount = 5f;
            float width = (position.width - 5f * (amount - 1f)) / amount;

            GUI.color = Color.green;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(five, buttonText, GUILayout.Width(width), GUILayout.Height(30f)))
            {
                FindSceneObject();
            }

            if (GUILayout.Button(one, buttonText, GUILayout.Width(width), GUILayout.Height(30f)))
            {
                FindSceneScript();
            }

            GUI.color = Color.yellow;
            if (GUILayout.Button(two, buttonText, GUILayout.Width(width), GUILayout.Height(30f)))
            {
                FindScene();
            }

            if (GUILayout.Button(three, buttonText, GUILayout.Width(width), GUILayout.Height(30f)))
            {
                FindMissing();
            }

            if (GUILayout.Button(four, buttonText, GUILayout.Width(width), GUILayout.Height(30f)))
            {
                FindChild();
            }
            EditorGUILayout.EndHorizontal();

            DisplayFindMissing();
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
                GUILayout.Label($" {five} - Scene���� �յ� ���� ������Ʈ ������", guiText);
                GUILayout.Label($" {one} - ��ũ��Ʈ�� ���� ������ ������", guiText);
                GUILayout.Label($" {two} - Scene���� ��ũ��Ʈ�� ���� ������ ������", guiText);
                GUILayout.Label($" {three} - Project���� ��ũ��Ʈ�� ���� ������ ������", guiText);
                GUILayout.Label($" {four} - ������ ����(Prefab) ���� �˻�", guiText);

                GUILayout.Space(10f);
            }
            EditorGUILayout.EndVertical();
        }

        public void DisplayFindMissing()
        {
            if (findMissing?.Count <= 0)
                return;

            float amount = 2f;
            float setWidth = (position.width - 5f * (amount - 1f)) / amount;

            string temp = "";
            scrollBehaviour = EditorGUILayout.BeginScrollView(scrollBehaviour);
            for (int i = 0; i < findMissing.Count; i++)
            {
                EditorGUILayout.BeginHorizontal("box");
                {
                    if (findMissing[i].prefab != null)
                    {
                        if (temp != findMissing[i].missingName)
                        {
                            temp = findMissing[i].missingName;
                            GUI.color = GUI.color == Color.yellow ? Color.green : Color.yellow;
                        }
                        GUILayout.Label(findMissing[i].missingName, GUILayout.Width(setWidth));
                        EditorGUILayout.ObjectField(findMissing[i].prefab, typeof(GameObject), true);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            GUI.color = Color.white;
            EditorGUILayout.EndScrollView();
        }

        void FindMissing()
        {
            findMissing.Clear();
            string[] prefabPaths = AssetDatabase.GetAllAssetPaths().Where(path
                => path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)).ToArray();
            foreach (string prefabPath in prefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                    foreach (Component component in prefab.GetComponentsInChildren<Component>())
                    {
                        string name = prefabPath;
                        if (component == null)
                        {
                            MissingPrefab missing = new MissingPrefab
                            {
                                prefab = prefab,
                                missingName = name,
                            };
                            findMissing.Add(missing);
                            break;
                        }
                    }
            }
        }

        void FindChild()
        {
            findMissing.Clear();

            //Object[] objects = Selection.objects;
            GameObject[] objects = Selection.gameObjects;
            foreach (GameObject obj in objects)
            {
                foreach (Transform trans in obj.GetComponentsInChildren<Transform>(true))
                {
                    foreach (Component component in trans.GetComponents<Component>())
                    {
                        if (component == null)
                        {
                            MissingPrefab missing = new MissingPrefab
                            {
                                prefab = trans.gameObject,
                                missingName = trans.gameObject.name,
                            };
                            findMissing.Add(missing);
                            break;
                        }
                    }
                }
            }
        }

        void FindScene()
        {
            findMissing.Clear();
            GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.InstanceID);
            foreach (GameObject obj in objects)
            {
                foreach (Component component in obj.GetComponents<Component>())
                {
                    if (component == null)
                    {
                        MissingPrefab missing = new MissingPrefab
                        {
                            prefab = obj.gameObject,
                            missingName = obj.gameObject.name,
                        };
                        findMissing.Add(missing);
                        break;
                    }
                }
            }
        }

        void FindSceneScript()
        {
            findMissing.Clear();
            GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.InstanceID);
            foreach (GameObject obj in objects)
            {
                foreach (MonoBehaviour component in obj.GetComponents<MonoBehaviour>())
                {
                    if (component != null)
                    {
                        string scriptName = component.GetType().ToString();
                        // ����Ƽ �⺻ ��ũ��Ʈ ����
                        if (scriptName.Contains("Unity") == false && scriptName.Contains("TMPro") == false)
                        {
                            MissingPrefab missing = new MissingPrefab
                            {
                                prefab = obj.gameObject,
                                missingName = scriptName,
                            };
                            findMissing.Add(missing);
                            break;
                        }
                    }
                }
            }

            findMissing.Sort(delegate (MissingPrefab a, MissingPrefab b)
            {
                return a.missingName.CompareTo(b.missingName);
            });
        }

        void FindSceneObject()
        {
            findMissing.Clear();
            GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.InstanceID);
            foreach (GameObject obj in objects)
            {
                string name = obj.name;
                if (name.Length > 0)
                {
                    if (name.StartsWith(" ") || name.EndsWith(" "))
                    {
                        MissingPrefab missing = new MissingPrefab
                        {
                            prefab = obj.gameObject,
                            missingName = name,
                        };
                        findMissing.Add(missing);
                    }
                }
            }

            findMissing.Sort(delegate (MissingPrefab a, MissingPrefab b)
            {
                return a.missingName.CompareTo(b.missingName);
            });
        }
    }
}
#endif