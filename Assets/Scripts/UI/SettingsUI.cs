using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Asteroids.Audio;
using Asteroids.Managers;

namespace Asteroids.UI
{
    /* Volume settings screen. It doesn't know who opened it: callers pass an onClosed callback,
     * so the same panel can be reused from the pause menu now and a main menu later.
     */
    public class SettingsUI : MonoBehaviour
    {
        [Serializable]
        private struct VolumeSlider
        {
            public AudioChannel channel;
            public Slider slider;
            [Tooltip("Optional percentage readout.")]
            public TextMeshProUGUI valueLabel;
        }

        [Header("UI Elements")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button backButton;
        [SerializeField] private List<VolumeSlider> volumeSliders = new();

        private Action onClosed;

        public bool IsOpen => settingsPanel != null && settingsPanel.activeSelf;

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(Close);
            }

            foreach (var binding in volumeSliders)
            {
                if (binding.slider == null) continue;

                binding.slider.minValue = 0f;
                binding.slider.maxValue = 1f;

                // Copied locally so each listener keeps its own binding.
                VolumeSlider captured = binding;
                binding.slider.onValueChanged.AddListener(value => HandleSliderChanged(captured, value));
            }
        }

        public void Open(Action onClosedCallback = null)
        {
            onClosed = onClosedCallback;
            RefreshSliders();

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        public void Close()
        {
            if (!IsOpen) return;

            settingsPanel.SetActive(false);
            AudioSettingsManager.Instance?.SaveSettings();

            // Cleared before invoking so a callback that reopens the panel isn't wiped out.
            Action callback = onClosed;
            onClosed = null;
            callback?.Invoke();
        }

        // Pulls the current values in without firing onValueChanged, so opening the panel
        // doesn't write every setting straight back out.
        private void RefreshSliders()
        {
            if (AudioSettingsManager.Instance == null) return;

            foreach (var binding in volumeSliders)
            {
                if (binding.slider == null) continue;

                float volume = AudioSettingsManager.Instance.GetVolume(binding.channel);
                binding.slider.SetValueWithoutNotify(volume);
                UpdateLabel(binding, volume);
            }
        }

        private void HandleSliderChanged(VolumeSlider binding, float value)
        {
            AudioSettingsManager.Instance?.SetVolume(binding.channel, value);
            UpdateLabel(binding, value);
        }

        private static void UpdateLabel(VolumeSlider binding, float value)
        {
            if (binding.valueLabel != null)
            {
                binding.valueLabel.text = $"{Mathf.RoundToInt(value * 100f)}%";
            }
        }
    }
}
