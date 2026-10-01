using System;
using UnityEngine;

namespace Asteroids.Pooling
{
    public class PooledObject : MonoBehaviour
    {
        private IPoolable[] poolables;

        public string PoolKey { get; set; }

        // Looked up once and reused, so spawning and returning don't allocate an array each time.
        public IPoolable[] Poolables => poolables ??= GetComponents<IPoolable>();
        public event Action<PooledObject> OnReturnRequested;

        public void ReturnToPool()
        {
            OnReturnRequested?.Invoke(this);
        }
    }
}