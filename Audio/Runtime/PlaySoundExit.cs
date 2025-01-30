using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MGDK.Audio
{
    public class PlaySoundExit : StateMachineBehaviour
    {
        [SerializeField] private Sound sound;
        [SerializeField, Range(0, 1)] private float volume = 1;
        [SerializeField, Range(-3, 3)] private float pitch = 1;
        [SerializeField, Range(0, 1)] private float volumeVarience = 0;
        [SerializeField, Range(0, 6)] private float pitchVarience = 0;
        [SerializeField] private bool usePosition = false;
        [SerializeField] private bool useOwnPosition = false;
        [SerializeField] private Transform positionTransform;
        [SerializeField] private Vector3 customPosition;

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Vector3 finalPosition = Vector3.zero;

            if (usePosition)
            {
                if (useOwnPosition)
                {
                    finalPosition = animator.gameObject.transform.position;
                }
                else
                {
                    if (positionTransform != null)
                    {
                        finalPosition = positionTransform.position;
                    }
                    else
                    {
                        finalPosition = customPosition;
                    }
                }
            }

            if (usePosition)
            {
                SoundManager.PlaySoundAtPosition(sound.soundName, finalPosition, volume, pitch, volumeVarience, pitchVarience);
            }
            else
            {
                SoundManager.PlaySound(sound.soundName, volume, pitch, volumeVarience, pitchVarience);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PlaySoundExit))]
    public class PlaySoundExitEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var soundProperty = serializedObject.FindProperty("sound");
            EditorGUILayout.PropertyField(soundProperty);

            var usePositionProperty = serializedObject.FindProperty("usePosition");
            EditorGUILayout.PropertyField(usePositionProperty);

            if (usePositionProperty.boolValue)
            {
                var useOwnPositionProperty = serializedObject.FindProperty("useOwnPosition");
                EditorGUILayout.PropertyField(useOwnPositionProperty);

                if (!useOwnPositionProperty.boolValue)
                {
                    var positionTransformProperty = serializedObject.FindProperty("positionTransform");
                    EditorGUILayout.PropertyField(positionTransformProperty);

                    var customPositionProperty = serializedObject.FindProperty("customPosition");
                    EditorGUILayout.PropertyField(customPositionProperty);
                }
            }

            var volumeProperty = serializedObject.FindProperty("volume");
            EditorGUILayout.PropertyField(volumeProperty);

            var pitchProperty = serializedObject.FindProperty("pitch");
            EditorGUILayout.PropertyField(pitchProperty);

            var volumeVarienceProperty = serializedObject.FindProperty("volumeVarience");
            EditorGUILayout.PropertyField(volumeVarienceProperty);

            var pitchVarienceProperty = serializedObject.FindProperty("pitchVarience");
            EditorGUILayout.PropertyField(pitchVarienceProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
