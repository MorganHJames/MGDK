using UnityEngine;
using System.Collections.Generic;
using MGDK.Util;

namespace MGDK.Audio
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private Sound[] soundList;
        private static SoundManager instance;
        private Dictionary<string, Sound> soundDictionary;
        private static ObjectPooler objectPooler;

        public static SoundManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SoundManager>();
                    if (instance == null)
                    {
                        GameObject soundManagerObject = new GameObject("SoundManager");
                        instance = soundManagerObject.AddComponent<SoundManager>();
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject); // Ensures only one instance exists
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject); // Keeps the SoundManager between scenes

            objectPooler = ObjectPooler.SharedInstance;
            InitializeSoundDictionary();
        }

        private void InitializeSoundDictionary()
        {
            soundDictionary = new Dictionary<string, Sound>();
            foreach (Sound sound in soundList)
            {
                if (!soundDictionary.ContainsKey(sound.soundName))
                {
                    soundDictionary[sound.soundName] = sound;
                }
            }
        }

        public static void PlaySound(string soundName, float volume = 1, float pitch = 1, float volumeVariance = 0, float pitchVariance = 0)
        {
            if (instance.soundDictionary.TryGetValue(soundName, out Sound sound))
            {
                SoundClip[] soundClips = sound.soundClips;
                SoundClip randomSoundClip = soundClips[Random.Range(0, soundClips.Length)];

                // Get object from object pool
                GameObject audioObject = objectPooler.GetPooledObject(0);
                if (audioObject != null)
                {
                    AudioSource audioSource = audioObject.GetComponent<AudioSource>();

                    // Apply volume variance
                    float finalVolume = volume + Random.Range(-volumeVariance, volumeVariance);
                    finalVolume *= randomSoundClip.volume;
                    finalVolume = Mathf.Clamp(finalVolume, 0, 1); // Ensure volume is within range

                    // Apply pitch variance
                    float finalPitch = pitch + Random.Range(-pitchVariance, pitchVariance);
                    finalPitch *= randomSoundClip.pitch;
                    finalPitch = Mathf.Clamp(finalPitch, -3, 3); // Ensure pitch is within range

                    audioSource.outputAudioMixerGroup = sound.mixer;
                    audioSource.pitch = finalPitch;
                    audioSource.volume = finalVolume;
                    audioSource.clip = randomSoundClip.clip;
                    audioSource.spatialBlend = 0;
                    audioObject.SetActive(true);
                    audioSource.Play();

                    // Call a static method to disable the audio object after clip ends
                    DisableAudioObject(audioObject, randomSoundClip.clip.length);
                }
            }
            else
            {
                Debug.LogWarning($"Sound {soundName} not found in the dictionary.");
            }
        }


        public static void PlaySoundAtPosition(string soundName, Vector3 position, float volume = 1, float pitch = 1, float volumeVariance = 0, float pitchVariance = 0)
        {
            if (instance.soundDictionary.TryGetValue(soundName, out Sound sound))
            {
                SoundClip[] soundClips = sound.soundClips;
                SoundClip randomSoundClip = soundClips[Random.Range(0, soundClips.Length)];

                // Get object from object pool
                GameObject audioObject = objectPooler.GetPooledObject(0);
                if (audioObject != null)
                {
                    audioObject.transform.position = position;
                    AudioSource audioSource = audioObject.GetComponent<AudioSource>();

                    // Apply volume variance
                    float finalVolume = volume + Random.Range(-volumeVariance, volumeVariance);
                    finalVolume *= randomSoundClip.volume;
                    finalVolume = Mathf.Clamp(finalVolume, 0, 1); // Ensure volume is within range

                    // Apply pitch variance
                    float finalPitch = pitch + Random.Range(-pitchVariance, pitchVariance);
                    finalPitch *= randomSoundClip.pitch;
                    finalPitch = Mathf.Clamp(finalPitch, -3, 3); // Ensure pitch is within range

                    audioSource.outputAudioMixerGroup = sound.mixer;
                    audioSource.pitch = finalPitch;
                    audioSource.volume = finalVolume;
                    audioSource.clip = randomSoundClip.clip;
                    audioSource.spatialBlend = 1;
                    audioObject.SetActive(true);
                    audioSource.Play();

                    // Call a static method to disable the audio object after clip ends
                    DisableAudioObject(audioObject, randomSoundClip.clip.length);
                }
            }
            else
            {
                Debug.LogWarning($"Sound {soundName} not found in the dictionary.");
            }
        }

        private static void DisableAudioObject(GameObject audioObject, float delay)
        {
            instance.StartCoroutine(instance.DisableAudioObjectCoroutine(audioObject, delay));
        }

        private System.Collections.IEnumerator DisableAudioObjectCoroutine(GameObject audioObject, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (audioObject != null)
            {
                audioObject.SetActive(false);
            }
        }
    }
}
