using System;
using UnityEngine;

namespace EcsCore
{
    /// <summary>
    /// Entry point for ecs world
    /// Processing Init, Run, RunLate and RunPhysics
    /// Create ecs world and global modules
    /// </summary>
    public class EcsWorldMono : MonoBehaviour
    {
        public Action onUpdate;
        public Action onFixedUpdate;
        public Action onLateUpdate;
        public Action onDestroyed;

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
            onUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            onFixedUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            onLateUpdate?.Invoke();
        }
        
        private void OnDestroy()
        {
            onDestroyed?.Invoke();
        }
    }
}