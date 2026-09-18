using UnityEngine;

//Data container for weapon and bullet parameters. 
//See ShipConfig.cs for detailed rationale on ScriptableObject data architecture.
[CreateAssetMenu(fileName = "NewWeaponConfig", menuName = "Asteroids/Config/Weapon Config")]
public class WeaponConfig : ScriptableObject
{
    [Header("Fire Settings")]
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletLifetime = 3f;
    [SerializeField] private int damage = 1;

    [Header("Visuals & Audio")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject impactFXPrefab;
    [SerializeField] private AudioClip shootAudioClip;

    public float FireRate => fireRate;
    public float BulletSpeed => bulletSpeed;
    public float BulletLifetime => bulletLifetime;
    public int Damage => damage;

    public GameObject BulletPrefab => bulletPrefab;
    public GameObject MuzzleFlashPrefab => muzzleFlashPrefab;
    public GameObject ImpactFXPrefab => impactFXPrefab;
    public AudioClip ShootAudioClip => shootAudioClip;
}