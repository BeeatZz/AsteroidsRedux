using System;
using UnityEngine;
using UnityEngine.Audio;
using Asteroids.Audio;

namespace Asteroids.Managers
{
    /* Owns the player's volume settings: converts slider values to mixer decibels,
     * saves them to PlayerPrefs and re-applies them whenever the scene loads.
     */
    public class AudioSettingsManager : MonoBehaviour
    {
        // The mixer's floor. Anything at or below this is effectively silent.
        private const float MinDecibels = -80f;
        private const float MinLinearVolume = 0.0001f; // Log10 of this is -4, i.e. -80 dB
        private const string PrefsKeyPrefix = "Volume_";

        public static AudioSettingsManager Instance { get; private set; }

        [Header("Mixer")]
        [Tooltip("Must expose MasterVol, MusicVol, SFXVol and UIVol parameters.")]
        [SerializeField] private AudioMixer mixer;

        [Header("Defaults (0-1)")]
        [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.8f;

        private readonly float[] volumes = new float[Enum.GetValues(typeof(AudioChannel)).Length];

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            foreach (AudioChannel channel in Enum.GetValues(typeof(AudioChannel)))
            {
                volumes[(int)channel] = PlayerPrefs.GetFloat(GetPrefsKey(channel), defaultVolume);
            }
        }

        // AudioMixer.SetFloat is ignored if called during Awake, so the saved values are pushed here.
        private void Start()
        {
            foreach (AudioChannel channel in Enum.GetValues(typeof(AudioChannel)))
            {
                ApplyToMixer(channel);
            }
        }

        public float GetVolume(AudioChannel channel)
        {
            return volumes[(int)channel];
        }

        // Linear 0-1 value, as a slider provides it.
        public void SetVolume(AudioChannel channel, float linearVolume)
        {
            volumes[(int)channel] = Mathf.Clamp01(linearVolume);
            PlayerPrefs.SetFloat(GetPrefsKey(channel), volumes[(int)channel]);
            ApplyToMixer(channel);
        }

        // PlayerPrefs are written to disk on quit anyway; call this when a settings screen closes
        // so changes survive a crash too.
        public void SaveSettings()
        {
            PlayerPrefs.Save();
        }

        private void ApplyToMixer(AudioChannel channel)
        {
            if (mixer == null) return;

            string parameter = GetMixerParameter(channel);
            if (!mixer.SetFloat(parameter, LinearToDecibels(volumes[(int)channel])))
            {
                Debug.LogWarning($"AudioMixer '{mixer.name}' has no exposed parameter '{parameter}'.", this);
            }
        }

        // Perceived loudness is logarithmic, so a linear slider mapped straight to dB feels wrong:
        // almost all of the change would bunch up at the bottom of the slider.
        private static float LinearToDecibels(float linear)
        {
            return linear <= MinLinearVolume ? MinDecibels : Mathf.Log10(linear) * 20f;
        }

        private static string GetMixerParameter(AudioChannel channel) => $"{channel}Vol";

        private static string GetPrefsKey(AudioChannel channel) => PrefsKeyPrefix + channel;
    }
}
