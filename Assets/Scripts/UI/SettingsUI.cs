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
            [Tooltip("Optional mute button. On = sound playing, off = muted.")]
            public SpriteToggleButton muteToggle;
        }

        [Header("UI Elements")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button backButton;
        [SerializeField] private List<VolumeSlider> volumeSliders = new();

        [Header("Muted Look")]
        [Tooltip("Brightness a muted channel's slider is drawn at (1 = unchanged, 0 = black). Alpha is kept.")]
        [Range(0f, 1f)]
        [SerializeField] private float mutedBrightness = 0.45f;

        private Action onClosed;

        // Each slider graphic's colour as set up in the editor, so dimming can always be undone exactly.
        private readonly Dictionary<Graphic, Color> originalColors = new();

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

                foreach (var graphic in binding.slider.GetComponentsInChildren<Graphic>(true))
                {
                    originalColors[graphic] = graphic.color;
                }

                // Copied locally so each listener keeps its own binding.
                VolumeSlider captured = binding;
                binding.slider.onValueChanged.AddListener(value => HandleSliderChanged(captured, value));

                if (binding.muteToggle != null)
                {
                    binding.muteToggle.OnValueChanged += isOn => HandleMuteToggled(captured, isOn);
                }
            }
        }

        // AudioSettingsManager loads the saved values in its Awake, which has run by now.
        private void Start()
        {
            RefreshSliders();
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

                bool isMuted = AudioSettingsManager.Instance.IsMuted(binding.channel);
                if (binding.muteToggle != null)
                {
                    binding.muteToggle.SetIsOnWithoutNotify(!isMuted);
                }

                SetSliderDimmed(binding, isMuted);
            }
        }

        private void HandleSliderChanged(VolumeSlider binding, float value)
        {
            AudioSettingsManager.Instance?.SetVolume(binding.channel, value);
            UpdateLabel(binding, value);

            // Dragging the slider of a muted channel means the player wants to hear it again.
            if (binding.muteToggle != null && !binding.muteToggle.IsOn)
            {
                binding.muteToggle.IsOn = true;
            }
        }

        private void HandleMuteToggled(VolumeSlider binding, bool isOn)
        {
            AudioSettingsManager.Instance?.SetMuted(binding.channel, !isOn);
            SetSliderDimmed(binding, !isOn);
        }

        // Scales RGB only, so the slider reads as darker rather than see-through. The Slider's own
        // colour tint (hover/press) multiplies on top of this, so the two don't fight.
        private void SetSliderDimmed(VolumeSlider binding, bool dimmed)
        {
            if (binding.slider == null) return;

            foreach (var graphic in binding.slider.GetComponentsInChildren<Graphic>(true))
            {
                if (!originalColors.TryGetValue(graphic, out Color color)) continue;

                if (dimmed)
                {
                    color.r *= mutedBrightness;
                    color.g *= mutedBrightness;
                    color.b *= mutedBrightness;
                }

                graphic.color = color;
            }
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
