using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
using P01.Editor;
[CustomEditor(typeof(CT_Cinemachine_Receiver))]
public class CT_Cinemachine_Receiver_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        GUIStyle fontStyle = new GUIStyle()
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.yellow},
            alignment = TextAnchor.MiddleLeft,
        };
        GUILayout.Label("CinemachineBrain의", fontStyle);
        GUILayout.Label("Brain Styles을", fontStyle);
        GUILayout.Label("마커로 컨트롤", fontStyle);

        CT_Cinemachine_Receiver Inspector = target as CT_Cinemachine_Receiver;
        base.OnInspectorGUI();
    }
}
#endif
namespace P01.Editor
{
    public class CT_Cinemachine_Receiver : MonoBehaviour, INotificationReceiver
    {
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is CT_Cinemachine_Marker cinemachineMarker)
            {
                Camera mainCam = Camera.main;
                CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
                brain.DefaultBlend.Style = cinemachineMarker.BrainStyles;
                brain.DefaultBlend.Time = cinemachineMarker.BrainTime;
                Debug.LogWarning($"{brain.DefaultBlend.Style}  {brain.DefaultBlend.Time}");
            }
        }
    }
}