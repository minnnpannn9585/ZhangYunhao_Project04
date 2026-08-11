using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// Generic base for the platformer's MonoBehaviour managers (key bindings, dialogue,
    /// key-remap UI). Keeps lookup/lifetime boilerplate in one place so each manager only
    /// implements its own behaviour.
    /// </summary>
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = (T)this;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
