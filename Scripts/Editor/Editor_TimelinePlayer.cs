using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;
using System.Collections.Generic;


#if UNITY_EDITOR
//using UnityEditor;

namespace P01.Editor
{
    public class Editor_TimelinePlayer : EditorWindow
    {
        [MenuItem("Graphics Tool/06. TimelinePlayer")]
        public static void OpenWindow()
        {
            Editor_TimelinePlayer window = EditorWindow.GetWindow<Editor_TimelinePlayer>("Editor_TimelinePlayer");
            window.minSize = new Vector2(500f, 200f);
            window.Show();
        }

        PlayableDirector[] playableDirectors;
        Dictionary<GameObject, CinemachineCamera> cinemachineCameras = new Dictionary<GameObject, CinemachineCamera>();
        bool tutorialToggle;

        public class ObjectStruct
        {
            public bool structEnabled;
            public GameObject structObject;
        }
        public List<ObjectStruct> objectStruct = new List<ObjectStruct>();
        public bool toggleObject;
        public Vector2 scrollPos;
        private void OnGUI()
        {
            SetTutorial();
            GUIStyle fontText = new()
            {
                fontSize = 13,
                normal = { textColor = Color.red },
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle buttonText = new(GUI.skin.button)
            {
                fontSize = 13,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.MiddleCenter
            };

            float amount = 5f;
            float width = (position.width - 5f * (amount - 1f)) / amount;

            if (Application.isPlaying == true)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    mainCamera.enabled = true;
                    mainCamera.gameObject.SetActive(true);
                }

                if (GUILayout.Button("연출 시작", buttonText, GUILayout.Height(30f)))
                {
                    PlayTimeline();
                }
            }
            else
            {
                if (GUILayout.Button("미리 세팅", buttonText, GUILayout.Height(30f)))
                {
                    // 타임라인 세팅
                    playableDirectors = Object.FindObjectsByType<PlayableDirector>(FindObjectsSortMode.None);
                    objectStruct = new List<ObjectStruct>();
                    Data_Manager[] enabledObject = Object.FindObjectsByType<Data_Manager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    for (int i = 0; i < enabledObject.Length; i++)
                    {
                        ObjectStruct newStruct = new ObjectStruct
                        {
                            structEnabled = enabledObject[i].gameObject.activeSelf,
                            structObject = enabledObject[i].gameObject
                        };
                        objectStruct.Add(newStruct);
                    }

                    CinemachineCamera[] child = Object.FindObjectsByType<CinemachineCamera>
                        (FindObjectsInactive.Include, FindObjectsSortMode.None);
                    for (int c = 0; c < child.Length; c++)
                    {
                        cinemachineCameras[child[c].gameObject] = child[c];
                        ObjectStruct newStruct = new ObjectStruct
                        {
                            structEnabled = child[c].gameObject.activeSelf,
                            structObject = child[c].gameObject
                        };
                        objectStruct.Add(newStruct);
                    }
                }
                if (cinemachineCameras.Count > 0)
                {
                    GUILayout.Label($"타임라인 오브젝트 :");
                    GUILayout.Space(10);
                    for (int i = 0; i < playableDirectors.Length; i++)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(playableDirectors[i].gameObject, typeof(GameObject), true);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.Space(10);

                    if (GUILayout.Button("시네머신 카메라", buttonText, GUILayout.Height(30f)))
                    {
                        toggleObject = !toggleObject;
                    }

                    if (toggleObject)
                    {
                        scrollPos = GUILayout.BeginScrollView(scrollPos);
                        for (int i = 0; i < objectStruct.Count; i++)
                        {
                            GUILayout.BeginHorizontal("box");
                            objectStruct[i].structEnabled = EditorGUILayout.Toggle(objectStruct[i].structEnabled);
                            objectStruct[i].structObject = EditorGUILayout.ObjectField(objectStruct[i].structObject, typeof(GameObject), true) as GameObject;
                            objectStruct[i].structObject.SetActive(objectStruct[i].structEnabled);
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.Space(10);
                }

                GUILayout.Label("플레이 버튼 눌러 주세요~~", fontText);
            }
        }

        void PlayTimeline()
        {
            for (int i = 0; i < playableDirectors.Length; i++)
            {
                playableDirectors[i].Stop();
                playableDirectors[i].Play();
            }
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
            if (GUILayout.Button("사용법", buttonText, GUILayout.Height(20f)))
            {
                tutorialToggle = !tutorialToggle;
            }

            if (tutorialToggle == true)
            {
                guiText.normal.textColor = Color.gray;
                GUILayout.Space(10f);
                GUILayout.Label($" 0. '미리 세팅' 버튼 눌러 주세요.", guiText);
                GUILayout.Label($" 시네머신 카메라 = 씬 내에서 모든 카메라 찾기", guiText);
                GUILayout.Label($" 우선 순위 높은 카메라를 못찾는 경우가 있음", guiText);
                GUILayout.Label($" 토글 스위치로 온오프", guiText);
                GUILayout.Space(5f);

                GUILayout.Label($" 1. '▶' 버튼 눌러 주세요.", guiText);
                GUILayout.Label($" 2. '연출 시작' 버튼 눌러 주세요.", guiText);

                GUILayout.Space(10f);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif