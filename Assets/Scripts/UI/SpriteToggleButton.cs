using System;
using UnityEngine;
using UnityEngine.UI;

namespace Asteroids.UI
{
    /* A Button with an on/off state shown by swapping sprites: the button's own background
     * and an icon inside it. Works like a Slider/Toggle from the outside (IsOn, OnValueChanged,
     * SetIsOnWithoutNotify), so screens like SettingsUI can drive it the same way.
     * The Button's own transition (e.g. Color Tint) still handles hover/press on top.
     */
    [RequireComponent(typeof(Button))]
    public class SpriteToggleButton : MonoBehaviour
    {
        [SerializeField] private bool isOn = true;

        [Header("Background (the Button's Image)")]
        [SerializeField] private Image background;
        [SerializeField] private Sprite backgroundOn;
        [SerializeField] private Sprite backgroundOff;

        [Header("Icon (child Image)")]
        [SerializeField] private Image icon;
        [SerializeField] private Sprite iconOn;
        [SerializeField] private Sprite iconOff;

        public event Action<bool> OnValueChanged;

        public bool IsOn
        {
            get => isOn;
            set
            {
                if (isOn == value) return;
                SetIsOnWithoutNotify(value);
                OnValueChanged?.Invoke(isOn);
            }
        }

        private void Reset()
        {
            background = GetComponent<Image>();

            foreach (Transform child in transform)
            {
                if (child.TryGetComponent(out Image image))
                {
                    icon = image;
                    break;
                }
            }

            // Start from whatever the button currently shows as its "on" look.
            if (background != null) backgroundOn = background.sprite;
            if (icon != null) iconOn = icon.sprite;
        }

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Toggle);
            Refresh();
        }

        // Lets the on/off look be previewed in the editor by ticking Is On.
        private void OnValidate()
        {
            Refresh();
        }

        public void Toggle()
        {
            IsOn = !IsOn;
        }

        // Updates the visuals without raising OnValueChanged, for loading a saved state into the UI.
        public void SetIsOnWithoutNotify(bool value)
        {
            isOn = value;
            Refresh();
        }

        private void Refresh()
        {
            Apply(background, isOn ? backgroundOn : backgroundOff);
            Apply(icon, isOn ? iconOn : iconOff);
        }

        // A missing sprite leaves the image as it is, so a half-set-up button doesn't go blank.
        private static void Apply(Image image, Sprite sprite)
        {
            if (image != null && sprite != null)
            {
                image.sprite = sprite;
            }
        }
    }
}
