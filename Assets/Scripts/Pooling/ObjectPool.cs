using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Asteroids.Pooling
{
    public class ObjectPool : MonoBehaviour
    {
        [System.Serializable]
        private struct PoolEntry
        {
            public GameObject Prefab;
            public int Count;
        }

        public static ObjectPool Instance { get; private set; }

        [SerializeField] private List<PoolEntry> prewarmOnStart = new();
        [Tooltip("How many instances the prewarm creates per frame. It's spread out so a loading screen " +
                 "can keep animating while the pool fills; lower is smoother, higher is quicker.")]
        [Min(1)]
        [SerializeField] private int prewarmPerFrame = 8;

        private readonly Dictionary<string, Queue<PooledObject>> poolDictionary = new();
        private readonly Dictionary<string, GameObject> prefabLookup = new();
        private readonly Dictionary<string, int> totalCounts = new();

        // True once everything in Prewarm On Start exists. The loading screen waits for this.
        public bool IsReady { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private IEnumerator Start()
        {
            // A duplicate that removed itself in Awake mustn't fill a second pool.
            if (Instance != this) yield break;

            int created = 0;
            foreach (var entry in prewarmOnStart)
            {
                if (entry.Prefab == null) continue;

                string key = EnsurePool(entry.Prefab);
                while (totalCounts[key] < entry.Count)
                {
                    CreateNewInstance(key, entry.Prefab);

                    if (++created % prewarmPerFrame == 0)
                    {
                        yield return null;
                    }
                }
            }

            IsReady = true;
        }

        // Tops the pool up until at least targetSize instances exist (active + pooled),
        // so repeated calls for the same prefab don't stack.
        public void Prewarm(GameObject prefab, int targetSize)
        {
            if (prefab == null) return;
            string key = EnsurePool(prefab);

            for (int i = totalCounts[key]; i < targetSize; i++)
            {
                CreateNewInstance(key, prefab);
            }
        }

        private string EnsurePool(GameObject prefab)
        {
            string key = GetPoolKey(prefab);

            if (!poolDictionary.ContainsKey(key))
            {
                poolDictionary[key] = new Queue<PooledObject>();
                prefabLookup[key] = prefab;
                totalCounts[key] = 0;
            }

            return key;
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;
            string key = GetPoolKey(prefab);

            if (!poolDictionary.ContainsKey(key))
            {
                Prewarm(prefab, 5);
            }

            if (poolDictionary[key].Count == 0)
            {
                CreateNewInstance(key, prefabLookup[key]);
            }

            PooledObject obj = poolDictionary[key].Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.gameObject.SetActive(true);

            IPoolable[] poolables = obj.Poolables;
            for (int i = 0; i < poolables.Length; i++)
            {
                poolables[i].OnSpawnFromPool();
            }

            return obj.gameObject;
        }

        public void ReturnToPool(PooledObject obj)
        {
            // Guards against double returns (e.g. a bullet overlapping two asteroids in the
            // same physics step), which would enqueue the same instance twice.
            if (!obj.gameObject.activeSelf) return;

            IPoolable[] poolables = obj.Poolables;
            for (int i = 0; i < poolables.Length; i++)
            {
                poolables[i].OnReturnToPool();
            }

            obj.gameObject.SetActive(false);
            poolDictionary[obj.PoolKey].Enqueue(obj);
        }

        // Keyed by instance ID rather than prefab.name to avoid pool collisions between
        // different prefab assets that happen to share a display name.
        private string GetPoolKey(GameObject prefab)
        {
            return prefab.GetInstanceID().ToString();
        }

        private PooledObject CreateNewInstance(string key, GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, transform);
            instance.SetActive(false);

            PooledObject pooledObj = instance.GetComponent<PooledObject>();
            if (pooledObj == null)
            {
                pooledObj = instance.AddComponent<PooledObject>();
            }

            pooledObj.PoolKey = key;
            pooledObj.OnReturnRequested += ReturnToPool;

            poolDictionary[key].Enqueue(pooledObj);
            totalCounts[key]++;
            return pooledObj;
        }
    }
}