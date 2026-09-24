using System.Collections.Generic;
using UnityEngine;

namespace Asteroids.Pooling
{
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        private readonly Dictionary<string, Queue<PooledObject>> poolDictionary = new();
        private readonly Dictionary<string, GameObject> prefabLookup = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Prewarm(GameObject prefab, int initialSize)
        {
            if (prefab == null) return;
            string key = GetPoolKey(prefab);

            if (!poolDictionary.ContainsKey(key))
            {
                poolDictionary[key] = new Queue<PooledObject>();
                prefabLookup[key] = prefab;
            }

            for (int i = 0; i < initialSize; i++)
            {
                CreateNewInstance(key, prefab);
            }
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

            foreach (var poolable in obj.GetComponents<IPoolable>())
            {
                poolable.OnSpawnFromPool();
            }

            return obj.gameObject;
        }

        public void ReturnToPool(PooledObject obj)
        {
            foreach (var poolable in obj.GetComponents<IPoolable>())
            {
                poolable.OnReturnToPool();
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
            return pooledObj;
        }
    }
}