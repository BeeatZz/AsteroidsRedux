using UnityEngine;
using Asteroids.Pooling;
using Asteroids.Combat;

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

        private void Start()
        {
            if (weaponConfig != null && weaponConfig.BulletPrefab != null && ObjectPool.Instance != null)
            {
                ObjectPool.Instance.Prewarm(weaponConfig.BulletPrefab, 15);
            }
        }

        private void Update()
        {
            if (Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space))
            {
                TryShoot();
            }
        }

        private void TryShoot()
        {
            if (weaponConfig == null || Time.time < nextFireTime) return;

            nextFireTime = Time.time + weaponConfig.FireRate;

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Quaternion spawnRot = firePoint != null ? firePoint.rotation : transform.rotation;

            GameObject bulletObj = ObjectPool.Instance.Get(weaponConfig.BulletPrefab, spawnPos, spawnRot);

            if (bulletObj.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.Initialize(weaponConfig.BulletSpeed, weaponConfig.BulletLifetime);
            }

            if (audioSource != null && weaponConfig.ShootAudioClip != null)
            {
                audioSource.PlayOneShot(weaponConfig.ShootAudioClip);
            }
        }
    }
}