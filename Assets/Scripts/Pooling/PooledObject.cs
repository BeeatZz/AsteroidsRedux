using System;
using UnityEngine;

namespace Asteroids.Pooling
{
    public class PooledObject : MonoBehaviour
    {
        public string PoolKey { get; set; }
        public event Action<PooledObject> OnReturnRequested;

        public void ReturnToPool()
        {
            OnReturnRequested?.Invoke(this);
        }
    }
}