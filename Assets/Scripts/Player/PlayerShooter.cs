using UnityEngine;
using UnityEngine.InputSystem;
using Asteroids.Pooling;
using Asteroids.Combat;
using Asteroids.ScriptableObjects;

namespace Asteroids.Player
{
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Data Config")]
        [SerializeField] private WeaponConfig weaponConfig;

        [Header("Spawn Points")]
        [SerializeField] private Transform firePoint;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        private float nextFireTime;
        private PlayerHyperspace hyperspace;

        private void Awake()
        {
            hyperspace = GetComponent<PlayerHyperspace>();
        }

        private void Update()
        {
            bool firePressed = (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
                || (Mouse.current != null && Mouse.current.leftButton.isPressed);

            if (firePressed)
            {
                TryShoot();
            }
        }

        private void TryShoot()
        {
            // Update still runs while Time.timeScale is 0 (paused/game over), and Time.time freezes
            // with it, so the cooldown check alone can't block fire input during a pause.
            if (Time.timeScale <= 0f || weaponConfig == null || Time.time < nextFireTime) return;
            if (hyperspace != null && hyperspace.IsInHyperspace) return;

            nextFireTime = Time.time + weaponConfig.FireRate;

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Quaternion spawnRot = firePoint != null ? firePoint.rotation : transform.rotation;

            GameObject bulletObj = ObjectPool.Instance.Get(weaponConfig.BulletPrefab, spawnPos, spawnRot);

            if (bulletObj.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.Initialize(weaponConfig.BulletSpeed, weaponConfig.BulletLifetime, weaponConfig.Damage);
            }

            if (audioSource != null && weaponConfig.ShootAudioClip != null)
            {
                audioSource.PlayOneShot(weaponConfig.ShootAudioClip);
            }
        }
    }
}