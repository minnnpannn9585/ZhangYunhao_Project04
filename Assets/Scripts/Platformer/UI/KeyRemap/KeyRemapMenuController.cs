using UnityEngine;
using Platformer.Core;
using Platformer.KeyBinding;

namespace Platformer.UI
{
    /// <summary>
    /// Drives the key-allocation puzzle screen end to end:
    /// Closed -&gt; press open key -&gt; whole keyboard highlighted -&gt; Enter dives in -&gt;
    /// arrows+Enter pick the key to move (the "source") -&gt; arrows+Enter pick where it goes
    /// (the "target") -&gt; back to picking another source key. Escape steps back one level at
    /// a time; pressing it from the top-level keyboard-selected state closes the menu.
    ///
    /// Menu navigation (open key, arrows, Enter, Escape) always reads fixed physical
    /// KeyCodes directly - never through KeyBindingManager - so remapping or blocking a key
    /// can never lock the player out of the remap screen itself.
    /// </summary>
    [RequireComponent(typeof(KeyboardGridUI))]
    public class KeyRemapMenuController : SingletonBehaviour<KeyRemapMenuController>
    {
        private enum MenuState
        {
            Closed,
            KeyboardSelected,
            SelectingSourceKey,
            SelectingTargetKey,
        }

        [SerializeField] private KeyCode openMenuKey = KeyCode.Tab;
        [SerializeField] private KeyCode confirmKey = KeyCode.Return;
        [SerializeField] private KeyCode confirmKeyAlt = KeyCode.KeypadEnter;
        [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

        private KeyboardGridUI grid;
        private MenuState state = MenuState.Closed;
        private KeyCode selectedSourceKey = KeyCode.None;

        public bool IsOpen => state != MenuState.Closed;

        protected override void Awake()
        {
            base.Awake();
            grid = GetComponent<KeyboardGridUI>();
            grid.SetVisible(false);
            grid.SetKeyboardHighlighted(false);
        }

        private void Update()
        {
            if (state == MenuState.Closed)
            {
                if (Input.GetKeyDown(openMenuKey)) Open();
                return;
            }

            if (Input.GetKeyDown(cancelKey))
            {
                HandleCancel();
                return;
            }

            switch (state)
            {
                case MenuState.KeyboardSelected:
                    if (IsConfirmDown()) EnterGrid();
                    break;

                case MenuState.SelectingSourceKey:
                    HandleGridNavigation();
                    if (IsConfirmDown()) TrySelectSource();
                    break;

                case MenuState.SelectingTargetKey:
                    HandleGridNavigation();
                    if (IsConfirmDown()) TrySelectTarget();
                    break;
            }
        }

        private bool IsConfirmDown() => Input.GetKeyDown(confirmKey) || Input.GetKeyDown(confirmKeyAlt);

        private void HandleGridNavigation()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) grid.Move(-1, 0);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) grid.Move(1, 0);
            else if (Input.GetKeyDown(KeyCode.UpArrow)) grid.Move(0, -1);
            else if (Input.GetKeyDown(KeyCode.DownArrow)) grid.Move(0, 1);
        }

        private void Open()
        {
            state = MenuState.KeyboardSelected;
            grid.SetVisible(true);
            grid.SetKeyboardHighlighted(true);
            GameplayInputGate.Push();
        }

        private void EnterGrid()
        {
            state = MenuState.SelectingSourceKey;
            grid.SetKeyboardHighlighted(false);
            grid.ClearSourceSelection();
            grid.FocusFirstUsableCell();
        }

        private void TrySelectSource()
        {
            var bindings = KeyBindingManager.Instance;
            var key = grid.CursorKey;
            if (key == KeyCode.None || bindings.IsKeyBlocked(key)) return;
            if (!bindings.TryGetAction(key, out _)) return;

            selectedSourceKey = key;
            grid.SetSourceSelection(key);
            state = MenuState.SelectingTargetKey;
        }

        private void TrySelectTarget()
        {
            var targetKey = grid.CursorKey;
            if (targetKey == KeyCode.None) return;

            KeyBindingManager.Instance.TryRebind(selectedSourceKey, targetKey);
            grid.RefreshLabels();

            grid.ClearSourceSelection();
            selectedSourceKey = KeyCode.None;
            state = MenuState.SelectingSourceKey;
        }

        private void HandleCancel()
        {
            switch (state)
            {
                case MenuState.SelectingTargetKey:
                    grid.ClearSourceSelection();
                    selectedSourceKey = KeyCode.None;
                    state = MenuState.SelectingSourceKey;
                    break;

                case MenuState.SelectingSourceKey:
                    state = MenuState.KeyboardSelected;
                    grid.SetKeyboardHighlighted(true);
                    break;

                case MenuState.KeyboardSelected:
                    Close();
                    break;
            }
        }

        private void Close()
        {
            state = MenuState.Closed;
            grid.SetVisible(false);
            grid.SetKeyboardHighlighted(false);
            grid.ClearSourceSelection();
            selectedSourceKey = KeyCode.None;
            GameplayInputGate.Pop();
        }
    }
}
