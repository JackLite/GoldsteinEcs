using System;
using UnityEngine;

namespace EcsCore
{
    /// <summary>
    /// Entry point for ecs world
    /// Processing Init, Run and RunPhysics
    /// Create ecs world and global modules
    /// </summary>
    public class EcsWorldMono : MonoBehaviour
    {
        public Action OnUpdate;
        public Action OnFixedUpdate;
        public Action OnDestroyed;

        private void Awake()
        {
            if (FindObjectOfType<EcsWorldMono>() != this)
            {
                DestroyImmediate(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }
    }
}