using UnityEngine;
using Asteroids.Pooling;

namespace Asteroids.Effects
{
    /* Lives on every effect prefab. Plays its particles and sound once, then returns
     * itself to the pool when the longest of them has finished.
     */
    [RequireComponent(typeof(PooledObject))]
    public class PooledEffect : MonoBehaviour, IPoolable
    {
        [Tooltip("Optional. Leave empty for a particles-only effect.")]
        [SerializeField] private AudioSource audioSource;

        [Tooltip("Seconds before returning to the pool. 0 = work it out from the particles and the clip.")]
        [SerializeField] private float lifetimeOverride;

        private PooledObject pooledObject;
        private ParticleSystem[] particleSystems;

        private EffectSO voiceOwner;
        private float remainingLifetime;

        private void Awake()
        {
            pooledObject = GetComponent<PooledObject>();
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource != null)
            {
                audioSource.playOnAwake = false;
                audioSource.loop = false;
            }
        }

        // Called by EffectSO right after the pool hands this instance out.
        public void Play(EffectSO owner, AudioClip clip, float volume, float pitch)
        {
            foreach (var ps in particleSystems)
            {
                ps.Clear(false);
                ps.Play(false);
            }

            float clipDuration = 0f;
            if (audioSource != null && clip != null)
            {
                // Remembered so the voice is handed back even if this instance is destroyed early
                // (e.g. the scene reloads mid-explosion).
                voiceOwner = owner;

                audioSource.clip = clip;
                audioSource.volume = volume;
                audioSource.pitch = pitch;
                audioSource.Play();

                clipDuration = clip.length / Mathf.Max(Mathf.Abs(pitch), 0.01f);
            }
            else if (owner != null && clip != null)
            {
                // A clip was assigned but the prefab can't play it; give the voice straight back.
                owner.ReleaseVoice();
            }

            remainingLifetime = lifetimeOverride > 0f
                ? lifetimeOverride
                : Mathf.Max(GetParticleDuration(), clipDuration);
        }

        public void OnSpawnFromPool() { }

        public void OnReturnToPool()
        {
            foreach (var ps in particleSystems)
            {
                ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }

            ReleaseVoice();
        }

        private void OnDisable()
        {
            ReleaseVoice();
        }

        private void Update()
        {
            // Scaled time on purpose: particles freeze while paused, so the lifetime should too.
            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f)
            {
                pooledObject.ReturnToPool();
            }
        }

        private void ReleaseVoice()
        {
            if (voiceOwner == null) return;

            voiceOwner.ReleaseVoice();
            voiceOwner = null;
        }

        private float GetParticleDuration()
        {
            float longest = 0f;
            foreach (var ps in particleSystems)
            {
                var main = ps.main;

                // A looping system never finishes on its own; one cycle is the best guess.
                // Use lifetimeOverride for anything that needs to run longer.
                float duration = main.duration + (main.loop ? 0f : main.startLifetime.constantMax);
                longest = Mathf.Max(longest, main.startDelay.constantMax + duration);
            }

            return longest;
        }
    }
}
