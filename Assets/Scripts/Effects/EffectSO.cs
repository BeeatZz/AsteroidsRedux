using System;
using UnityEngine;
using Asteroids.Pooling;
using Random = UnityEngine.Random;

namespace Asteroids.Effects
{
    /* Describes a one-shot effect (particles + sound) and knows how to spawn it.
     * Gameplay code only ever calls effect.Play(position), so swapping the prefab,
     * the clips or the tuning never requires touching the entity that triggered it.
     */
    [CreateAssetMenu(fileName = "NewEffect", menuName = "Asteroids/Effects/Effect")]
    public class EffectSO : ScriptableObject
    {
        [Header("Prefab")]
        [Tooltip("Pooled prefab with a PooledEffect component (particles and/or an AudioSource).")]
        [SerializeField] private GameObject prefab;

        [Header("Audio")]
        [Tooltip("One clip is picked at random each time the effect plays. Leave empty for a silent effect.")]
        [SerializeField] private AudioClip[] clips;

        [Range(0f, 1f)]
        [SerializeField] private float volume = 1f;

        [Tooltip("Random pitch range, so repeated sounds don't feel mechanical.")]
        [SerializeField] private Vector2 pitchRange = new(0.9f, 1.1f);

        [Tooltip("Max copies of this sound audible at once. Extra spawns still show particles but stay silent. 0 = unlimited.")]
        [SerializeField] private int maxConcurrentVoices = 4;

        // Runtime-only bookkeeping, never saved into the asset.
        [NonSerialized] private int activeVoices;

        private void OnEnable()
        {
            activeVoices = 0;
        }

        public void Play(Vector3 position)
        {
            Play(position, Quaternion.identity);
        }

        public void Play(Vector3 position, Quaternion rotation)
        {
            if (prefab == null || ObjectPool.Instance == null) return;

            GameObject instance = ObjectPool.Instance.Get(prefab, position, rotation);
            if (instance == null || !instance.TryGetComponent<PooledEffect>(out var effect))
            {
                Debug.LogWarning($"Effect '{name}' prefab is missing a PooledEffect component.", this);
                return;
            }

            AudioClip clip = TryClaimVoice() ? PickClip() : null;
            float pitch = Random.Range(pitchRange.x, pitchRange.y);

            effect.Play(this, clip, volume, pitch);
        }

        // Called by PooledEffect when an instance that was granted a voice goes silent.
        internal void ReleaseVoice()
        {
            activeVoices = Mathf.Max(0, activeVoices - 1);
        }

        private bool TryClaimVoice()
        {
            if (clips == null || clips.Length == 0) return false;
            if (maxConcurrentVoices > 0 && activeVoices >= maxConcurrentVoices) return false;

            activeVoices++;
            return true;
        }

        private AudioClip PickClip()
        {
            return clips[Random.Range(0, clips.Length)];
        }
    }
}
