using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Platformer.KeyBinding;

namespace Platformer.UI
{
    /// <summary>
    /// Procedural visual for the key-remap grid: lays out one KeyButtonView per cell of a
    /// KeyboardLayoutSO, moves a cursor around it, and reflects KeyBindingManager state
    /// (bound action label, blocked keys dimmed, the key picked as a swap source
    /// highlighted). Cell positions default to a uniform grid but honour a cell's custom
    /// rect override so buttons can be hand-aligned to a hand-drawn keyboard background.
    ///
    /// Blocked and empty/off-grid cells are treated as obstacles: <see cref="Move"/> simply
    /// refuses to step onto them, same as hitting a wall - the caller does not need any
    /// special-case handling for "no valid move".
    /// </summary>
    public class KeyboardGridUI : MonoBehaviour
    {
        [SerializeField] private KeyboardLayoutSO layout;
        [SerializeField] private Transform uiParent;
        [SerializeField] private Sprite handDrawnKeyboardBackground;
        [SerializeField] private Vector2 cellSize = new Vector2(56f, 56f);
        [SerializeField] private Vector2 cellSpacing = new Vector2(6f, 6f);
        [SerializeField] private Vector2 panelSize = new Vector2(760f, 340f);

        private GameObject root;
        private Image keyboardHighlight;
        private readonly List<KeyButtonView> cells = new List<KeyButtonView>();
        private int cursorRow;
        private int cursorCol;

        public KeyCode CursorKey => IsValidCell(cursorRow, cursorCol) ? layout.GetKey(cursorRow, cursorCol) : KeyCode.None;

        private void Awake()
        {
            if (layout == null) layout = KeyboardLayoutSO.CreateDefaultQwerty();
            BuildUI();
        }

        private void OnEnable()
        {
            var bindings = KeyBindingManager.Instance;
            if (bindings == null) return;
            bindings.OnBindingsChanged += RefreshLabels;
            bindings.OnKeyBlockedChanged += HandleKeyBlockedChanged;
        }

        private void OnDisable()
        {
            var bindings = KeyBindingManager.Instance;
            if (bindings == null) return;
            bindings.OnBindingsChanged -= RefreshLabels;
            bindings.OnKeyBlockedChanged -= HandleKeyBlockedChanged;
        }

        private void HandleKeyBlockedChanged(KeyCode key, bool blocked) => RefreshLabels();

        public void SetVisible(bool visible)
        {
            root.SetActive(visible);
            if (visible) RefreshLabels();
        }

        public void SetKeyboardHighlighted(bool highlighted)
        {
            keyboardHighlight.enabled = highlighted;
        }

        /// <summary>Places the cursor on the first key that currently has a usable, unblocked action bound to it.</summary>
        public void FocusFirstUsableCell()
        {
            var bindings = KeyBindingManager.Instance;
            for (int r = 0; r < layout.RowCount; r++)
            {
                for (int c = 0; c < layout.ColumnCount(r); c++)
                {
                    var key = layout.GetKey(r, c);
                    if (key == KeyCode.None) continue;
                    if (bindings.IsKeyBlocked(key)) continue;
                    if (!bindings.TryGetAction(key, out _)) continue;

                    cursorRow = r;
                    cursorCol = c;
                    UpdateCursorVisual();
                    return;
                }
            }
        }

        /// <summary>Steps the cursor one cell in the given direction; no-ops if that cell is off-grid, empty or blocked.</summary>
        public void Move(int deltaColumn, int deltaRow)
        {
            int r = cursorRow + deltaRow;
            int c = cursorCol + deltaColumn;
            if (!IsValidCell(r, c)) return;

            var key = layout.GetKey(r, c);
            if (KeyBindingManager.Instance.IsKeyBlocked(key)) return;

            cursorRow = r;
            cursorCol = c;
            UpdateCursorVisual();
        }

        public void SetSourceSelection(KeyCode key)
        {
            foreach (var cell in cells) cell.SetSourceSelected(cell.Key == key);
        }

        public void ClearSourceSelection()
        {
            foreach (var cell in cells) cell.SetSourceSelected(false);
        }

        public void RefreshLabels()
        {
            var bindings = KeyBindingManager.Instance;
            if (bindings == null) return;

            foreach (var cell in cells)
            {
                bool blocked = bindings.IsKeyBlocked(cell.Key);
                bool bound = bindings.TryGetAction(cell.Key, out var action);
                cell.SetState(blocked, bound ? action.ToString() : null);
            }
            UpdateCursorVisual();
        }

        private bool IsValidCell(int row, int column)
        {
            if (layout == null) return false;
            if (row < 0 || row >= layout.RowCount) return false;
            if (column < 0 || column >= layout.ColumnCount(row)) return false;
            return layout.GetKey(row, column) != KeyCode.None;
        }

        private void UpdateCursorVisual()
        {
            foreach (var cell in cells) cell.SetCursor(false);
            foreach (var cell in cells)
            {
                if (cell.Row == cursorRow && cell.Column == cursorCol)
                {
                    cell.SetCursor(true);
                    break;
                }
            }
        }

        private void BuildUI()
        {
            var canvas = uiParent != null ? uiParent : FindObjectOfType<Canvas>()?.transform;

            root = new GameObject("KeyRemapPanel", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(canvas, false);

            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = panelSize;
            rootRect.anchoredPosition = Vector2.zero;

            var background = root.GetComponent<Image>();
            background.sprite = handDrawnKeyboardBackground;
            background.color = handDrawnKeyboardBackground != null ? Color.white : new Color(0.12f, 0.12f, 0.14f, 0.95f);

            var highlightGO = new GameObject("KeyboardHighlight", typeof(RectTransform), typeof(Image));
            highlightGO.transform.SetParent(root.transform, false);
            keyboardHighlight = highlightGO.GetComponent<Image>();
            keyboardHighlight.color = new Color(1f, 0.85f, 0.2f, 0.35f);
            keyboardHighlight.raycastTarget = false;
            var highlightRect = highlightGO.GetComponent<RectTransform>();
            highlightRect.anchorMin = Vector2.zero;
            highlightRect.anchorMax = Vector2.one;
            highlightRect.offsetMin = Vector2.zero;
            highlightRect.offsetMax = Vector2.zero;

            bool overlaysArt = handDrawnKeyboardBackground != null;
            for (int r = 0; r < layout.RowCount; r++)
            {
                for (int c = 0; c < layout.ColumnCount(r); c++)
                {
                    var key = layout.GetKey(r, c);
                    if (key == KeyCode.None) continue;

                    Vector2 position;
                    Vector2 size;
                    if (layout.TryGetCustomRect(r, c, out var customRect))
                    {
                        position = customRect.position;
                        size = customRect.size;
                    }
                    else
                    {
                        position = DefaultCellPosition(r, c);
                        size = cellSize;
                    }

                    var cell = KeyButtonView.Create(root.transform, r, c, key, position, size, overlaysArt);
                    cells.Add(cell);
                }
            }
        }

        private Vector2 DefaultCellPosition(int row, int column)
        {
            float x = column * (cellSize.x + cellSpacing.x) - panelSize.x / 2f + cellSize.x / 2f + 20f;
            float y = -row * (cellSize.y + cellSpacing.y) + panelSize.y / 2f - cellSize.y / 2f - 20f;
            return new Vector2(x, y);
        }
    }
}
