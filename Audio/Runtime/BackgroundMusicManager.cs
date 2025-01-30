using UnityEngine;

namespace MGDK.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class BackgroundMusicManager : MonoBehaviour
    {
        [SerializeField] private Sound backgroundMusic;
        private AudioSource audioSource;
        private int currentTrackIndex = 0;
        private static BackgroundMusicManager instance;

        public static BackgroundMusicManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<BackgroundMusicManager>();
                    if (instance == null)
                    {
                        GameObject managerObject = new GameObject("BackgroundMusicManager");
                        instance = managerObject.AddComponent<BackgroundMusicManager>();
                        DontDestroyOnLoad(managerObject);
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
            DontDestroyOnLoad(gameObject); // Keeps the manager between scenes

            audioSource = GetComponent<AudioSource>();
            audioSource.loop = false; // Make sure the audio source doesn't loop
            PlayNextTrack();
        }

        private void Update()
        {
            if (!audioSource.isPlaying)
            {
                PlayNextTrack();
            }
        }

        private void PlayNextTrack()
        {
            if (backgroundMusic == null || backgroundMusic.soundClips.Length == 0)
            {
                Debug.LogWarning("No background music clips assigned.");
                return;
            }

            SoundClip currentTrack = backgroundMusic.soundClips[currentTrackIndex];
            audioSource.clip = currentTrack.clip;
            audioSource.volume = currentTrack.volume;
            audioSource.pitch = currentTrack.pitch;
            audioSource.outputAudioMixerGroup = backgroundMusic.mixer;
            audioSource.Play();

            currentTrackIndex = (currentTrackIndex + 1) % backgroundMusic.soundClips.Length;
        }
    }
}