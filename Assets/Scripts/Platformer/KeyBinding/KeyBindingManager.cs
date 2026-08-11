using System;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Core;

namespace Platformer.KeyBinding
{
    /// <summary>
    /// Central authority for the key-allocation puzzle mechanic: which physical KeyCode
    /// currently drives each <see cref="GameAction"/>, and which physical keys are
    /// temporarily unusable ("blocked"). Gameplay code never reads Input.GetKey directly -
    /// it goes through IsActionPressed/-Down/-Up, so a rebound or blocked key is honoured
    /// everywhere automatically with no per-feature special-casing.
    ///
    /// Runs its own initialization defensively (see EnsureInitialized) so it behaves
    /// correctly no matter what order Unity happens to call Awake() on scene objects in.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class KeyBindingManager : SingletonBehaviour<KeyBindingManager>
    {
        [Serializable]
        public struct DefaultBinding
        {
            public GameAction action;
            public KeyCode key;
        }

        [SerializeField]
        private List<DefaultBinding> defaultBindings = new List<DefaultBinding>
        {
            new DefaultBinding { action = GameAction.MoveLeft, key = KeyCode.A },
            new DefaultBinding { action = GameAction.MoveRight, key = KeyCode.D },
            new DefaultBinding { action = GameAction.Jump, key = KeyCode.Space },
            new DefaultBinding { action = GameAction.Interact, key = KeyCode.F },
        };

        private readonly Dictionary<GameAction, KeyCode> actionToKey = new Dictionary<GameAction, KeyCode>();
        private readonly Dictionary<KeyCode, GameAction> keyToAction = new Dictionary<KeyCode, GameAction>();
        private readonly HashSet<KeyCode> blockedKeys = new HashSet<KeyCode>();

        /// <summary>Raised whenever two actions swap keys.</summary>
        public event Action OnBindingsChanged;

        /// <summary>Raised whenever a key becomes blocked or unblocked. Args: key, isBlocked.</summary>
        public event Action<KeyCode, bool> OnKeyBlockedChanged;

        protected override void Awake()
        {
            base.Awake();
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (actionToKey.Count > 0) return;
            foreach (var binding in defaultBindings)
            {
                actionToKey[binding.action] = binding.key;
                keyToAction[binding.key] = binding.action;
            }
        }

        public KeyCode GetKey(GameAction action)
        {
            EnsureInitialized();
            return actionToKey.TryGetValue(action, out var key) ? key : KeyCode.None;
        }

        public bool TryGetAction(KeyCode key, out GameAction action)
        {
            EnsureInitialized();
            return keyToAction.TryGetValue(key, out action);
        }

        public bool IsKeyBlocked(KeyCode key) => blockedKeys.Contains(key);

        /// <summary>An action is usable when it has a key and that key isn't blocked.</summary>
        public bool IsActionUsable(GameAction action)
        {
            var key = GetKey(action);
            return key != KeyCode.None && !IsKeyBlocked(key);
        }

        public bool IsActionPressed(GameAction action) => IsActionUsable(action) && Input.GetKey(GetKey(action));
        public bool IsActionDown(GameAction action) => IsActionUsable(action) && Input.GetKeyDown(GetKey(action));
        public bool IsActionUp(GameAction action) => IsActionUsable(action) && Input.GetKeyUp(GetKey(action));

        /// <summary>
        /// Marks a physical key as usable/unusable. Blocking a key that currently drives an
        /// action does not clear the binding - the action simply stops responding until the
        /// player reallocates it to a free key, which is the intended puzzle pressure.
        /// </summary>
        public void SetKeyBlocked(KeyCode key, bool blocked)
        {
            if (key == KeyCode.None) return;
            bool wasBlocked = blockedKeys.Contains(key);
            if (blocked == wasBlocked) return;

            if (blocked) blockedKeys.Add(key);
            else blockedKeys.Remove(key);

            OnKeyBlockedChanged?.Invoke(key, blocked);
        }

        /// <summary>
        /// Moves the action bound to <paramref name="sourceKey"/> onto
        /// <paramref name="targetKey"/>. If the target already drives another action, the
        /// two actions simply swap keys; otherwise the action moves and the source key is
        /// freed up. Fails (returns false, no state change) if either key is blocked, the
        /// keys are identical, or the source key has no action bound to it.
        /// </summary>
        public bool TryRebind(KeyCode sourceKey, KeyCode targetKey)
        {
            EnsureInitialized();

            if (sourceKey == targetKey) return false;
            if (IsKeyBlocked(sourceKey) || IsKeyBlocked(targetKey)) return false;
            if (!keyToAction.TryGetValue(sourceKey, out var sourceAction)) return false;

            bool targetHadAction = keyToAction.TryGetValue(targetKey, out var targetAction);

            keyToAction.Remove(sourceKey);
            keyToAction[targetKey] = sourceAction;
            actionToKey[sourceAction] = targetKey;

            if (targetHadAction)
            {
                keyToAction[sourceKey] = targetAction;
                actionToKey[targetAction] = sourceKey;
            }

            OnBindingsChanged?.Invoke();
            return true;
        }

        public IEnumerable<KeyValuePair<GameAction, KeyCode>> AllBindings
        {
            get
            {
                EnsureInitialized();
                return actionToKey;
            }
        }
    }
}
