using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;



#if UNITY_EDITOR
//using UnityEditor;

namespace P01
{
    public class Editor_TimelinePlayer : EditorWindow
    {
        [MenuItem("Graphics Tool/TimelinePlayer")]
        public static void OpenWindow()
        {
            Editor_TimelinePlayer window = EditorWindow.GetWindow<Editor_TimelinePlayer>("Editor_TimelinePlayer");
            window.minSize = new Vector2(500f, 200f);
            window.Show();
        }

        PlayableDirector[] playableDirectors;
        bool tutorialToggle;
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
                Camera main = Camera.main;
                if (main != null)
                {
                    main.enabled = true;
                    main.gameObject.SetActive(true);
                }

                playableDirectors = Object.FindObjectsByType<PlayableDirector>(FindObjectsSortMode.InstanceID);
                for (int i = 0; i < playableDirectors.Length; i++)
                {
                    EditorGUILayout.ObjectField(playableDirectors[i].gameObject, typeof(GameObject), true);
                }

                if (GUILayout.Button("연출 시작", buttonText, GUILayout.Height(30f)))
                {
                    PlayTimeline();
                }
            }
            else
            {
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
                GUILayout.Label($" 1. '▶' 버튼 눌러 주세요.", guiText);
                GUILayout.Label($" 2. '연출 시작' 버튼 눌러 주세요.", guiText);

                GUILayout.Space(10f);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif