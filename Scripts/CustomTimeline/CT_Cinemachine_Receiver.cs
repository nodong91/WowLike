using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

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