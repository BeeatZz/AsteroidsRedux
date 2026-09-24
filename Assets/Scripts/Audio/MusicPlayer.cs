using UnityEngine;

namespace Asteroids.Audio
{
    // Plays a looping background track. Route its AudioSource to the mixer's Music group.
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip musicClip;

        [Tooltip("Keep the music going on the pause menu. PauseManager pauses every other sound.")]
        [SerializeField] private bool playWhilePaused = true;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.ignoreListenerPause = playWhilePaused;
        }

        private void Start()
        {
            if (musicClip != null)
            {
                Play(musicClip);
            }
        }

        public void Play(AudioClip clip)
        {
            if (clip == null) return;
            if (audioSource.clip == clip && audioSource.isPlaying) return;

            audioSource.clip = clip;
            audioSource.Play();
        }

        public void Stop()
        {
            audioSource.Stop();
        }
    }
}
