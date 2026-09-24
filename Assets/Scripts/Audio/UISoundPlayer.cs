using UnityEngine;

namespace Asteroids.Audio
{
    /* Single shared source for menu sounds. Route its AudioSource to the mixer's UI group.
     * It ignores AudioListener.pause, since menus are mostly used while the game is paused.
     */
    [RequireComponent(typeof(AudioSource))]
    public class UISoundPlayer : MonoBehaviour
    {
        public static UISoundPlayer Instance { get; private set; }

        [Header("Default Clips")]
        [SerializeField] private AudioClip clickClip;

        private AudioSource audioSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.ignoreListenerPause = true;
        }

        public void PlayClick()
        {
            Play(clickClip);
        }

        public void Play(AudioClip clip)
        {
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
