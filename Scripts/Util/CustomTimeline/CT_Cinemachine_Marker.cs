using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace P01.Editor
{
    public class CT_Cinemachine_Marker : Marker, INotification, INotificationOptionProvider
    {
        [SerializeField] CinemachineBlendDefinition.Styles brainStyles;
        [SerializeField] float brainTime;

        [Space(20)]
        [SerializeField] bool retroactive;
        [SerializeField] bool emitOnce;

        public PropertyName id => new PropertyName();
        public CinemachineBlendDefinition.Styles BrainStyles => brainStyles;
        public float BrainTime => brainTime;

        public NotificationFlags flags =>
            (retroactive ? NotificationFlags.Retroactive : default) |
             (emitOnce ? NotificationFlags.TriggerOnce : default);
    }
}