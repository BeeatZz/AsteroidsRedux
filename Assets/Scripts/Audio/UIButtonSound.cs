using UnityEngine;
using UnityEngine.UI;

namespace Asteroids.Audio
{
    // Drop on any Button to play a click through UISoundPlayer.
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        [Tooltip("Optional. Leave empty to use UISoundPlayer's default click.")]
        [SerializeField] private AudioClip overrideClip;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(PlaySound);
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
