using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Asteroids.Audio
{
    // Drop on any Button to play a click, and a hover sound when the pointer moves onto it,
    // through UISoundPlayer.
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour, IPointerEnterHandler
    {
        [Tooltip("Optional. Leave empty to use UISoundPlayer's default click.")]
        [SerializeField] private AudioClip overrideClip;
        [Tooltip("Plays UISoundPlayer's hover clip when the pointer moves onto the button.")]
        [SerializeField] private bool playHoverSound = true;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(PlaySound);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Not while dragging a held press back onto the button, nor on a disabled button.
            if (!playHoverSound || eventData.dragging || !button.IsInteractable()) return;
            if (UISoundPlayer.Instance == null) return;

            UISoundPlayer.Instance.PlayHover();
        }

        private void PlaySound()
        {
            if (UISoundPlayer.Instance == null) return;

            if (overrideClip != null)
            {
                UISoundPlayer.Instance.Play(overrideClip);
            }
            else
            {
                UISoundPlayer.Instance.PlayClick();
            }
        }
    }
}
