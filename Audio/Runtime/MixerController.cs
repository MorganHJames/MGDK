using UnityEngine;
using UnityEngine.Audio;

namespace MGDK.Audio
{
    public class MixerController : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private UnityEngine.UI.Slider masterVolumeSlider;
        [SerializeField] private UnityEngine.UI.Slider musicVolumeSlider;
        [SerializeField] private UnityEngine.UI.Slider effectsVolumeSlider;
        [SerializeField] private UnityEngine.UI.Slider ambienceVolumeSlider;
        [SerializeField] private UnityEngine.UI.Slider dialogueVolumeSlider;

        private const string MASTER_VOLUME_KEY = "MasterVolume";
        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string EFFECTS_VOLUME_KEY = "EffectsVolume";
        private const string AMBIENCE_VOLUME_KEY = "AmbienceVolume";
        private const string DIALOGUE_VOLUME_KEY = "DialogueVolume";

        private void Start()
        {
            // Load saved volume values and apply to the audio mixer
            LoadAudioSettings();

            // Optionally set the sliders' values to the saved ones
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
            if (effectsVolumeSlider != null)
                effectsVolumeSlider.value = PlayerPrefs.GetFloat(EFFECTS_VOLUME_KEY, 1f);
            if (ambienceVolumeSlider != null)
                ambienceVolumeSlider.value = PlayerPrefs.GetFloat(AMBIENCE_VOLUME_KEY, 1f);
            if (dialogueVolumeSlider != null)
                dialogueVolumeSlider.value = PlayerPrefs.GetFloat(DIALOGUE_VOLUME_KEY, 1f);
        }

        // Called by sliders to change volume levels
        public void SetMasterVolume(float sliderValue)
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, sliderValue); // Save value
            PlayerPrefs.Save();
        }

        public void SetMusicVolume(float sliderValue)
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, sliderValue); // Save value
            PlayerPrefs.Save();
        }

        public void SetEffectsVolume(float sliderValue)
        {
            audioMixer.SetFloat("EffectsVolume", Mathf.Log10(sliderValue) * 20);
            PlayerPrefs.SetFloat(EFFECTS_VOLUME_KEY, sliderValue); // Save value
            PlayerPrefs.Save();
        }

        public void SetAmbienceVolume(float sliderValue)
        {
            audioMixer.SetFloat("AmbienceVolume", Mathf.Log10(sliderValue) * 20);
            PlayerPrefs.SetFloat(AMBIENCE_VOLUME_KEY, sliderValue); // Save value
            PlayerPrefs.Save();
        }

        public void SetDialogueVolume(float sliderValue)
        {
            audioMixer.SetFloat("DialogueVolume", Mathf.Log10(sliderValue) * 20);
            PlayerPrefs.SetFloat(DIALOGUE_VOLUME_KEY, sliderValue); // Save value
            PlayerPrefs.Save();
        }

        // Load saved settings (on Start)
        private void LoadAudioSettings()
        {
            // Load values from PlayerPrefs, default to 1f (max volume) if not found
            if (audioMixer != null)
            {
                float masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
                float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
                float effectsVolume = PlayerPrefs.GetFloat(EFFECTS_VOLUME_KEY, 1f);
                float ambienceVolume = PlayerPrefs.GetFloat(AMBIENCE_VOLUME_KEY, 1f);
                float dialogueVolume = PlayerPrefs.GetFloat(DIALOGUE_VOLUME_KEY, 1f);

                audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
                audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
                audioMixer.SetFloat("EffectsVolume", Mathf.Log10(effectsVolume) * 20);
                audioMixer.SetFloat("AmbienceVolume", Mathf.Log10(ambienceVolume) * 20);
                audioMixer.SetFloat("DialogueVolume", Mathf.Log10(dialogueVolume) * 20);
            }
        }
    }
}
