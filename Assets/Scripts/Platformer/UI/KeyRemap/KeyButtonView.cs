using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Platformer.UI
{
    /// <summary>
    /// Single key cell in the remap grid: shows the key's name and (if bound) the action it
    /// currently drives, and reflects blocked / cursor / picked-as-source visual states.
    /// Built entirely at runtime by KeyboardGridUI - no prefab required.
    /// </summary>
    public class KeyButtonView : MonoBehaviour
    {
        private static readonly Color NormalColor = new Color(0.85f, 0.85f, 0.85f, 0.9f);
        private static readonly Color BlockedColor = new Color(0.22f, 0.22f, 0.22f, 0.6f);
        private static readonly Color CursorColor = new Color(1f, 0.9f, 0.3f, 1f);
        private static readonly Color SourceColor = new Color(0.3f, 0.9f, 0.5f, 1f);
        private static readonly Color TransparentOverlay = new Color(1f, 1f, 1f, 0.08f);

        public int Row { get; private set; }
        public int Column { get; private set; }
        public KeyCode Key { get; private set; }

        private Image background;
        private TextMeshProUGUI keyLabel;
        private TextMeshProUGUI actionLabel;
        private bool isBlocked;
        private bool isSourceSelected;
        private bool overlaysHandDrawnArt;

        public static KeyButtonView Create(Transform parent, int row, int column, KeyCode key, Vector2 anchoredPosition, Vector2 size, bool overlaysHandDrawnArt)
        {
            var go = new GameObject($"Key_{key}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var view = go.AddComponent<KeyButtonView>();
            view.Row = row;
            view.Column = column;
            view.Key = key;
            view.overlaysHandDrawnArt = overlaysHandDrawnArt;
            view.background = go.GetComponent<Image>();

            view.keyLabel = CreateLabel(go.transform, key.ToString(), 14, new Vector2(0f, 8f));
            view.actionLabel = CreateLabel(go.transform, string.Empty, 9, new Vector2(0f, -12f));
            view.actionLabel.color = new Color(0.15f, 0.45f, 0.95f);

            view.ApplyRestColor();
            return view;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string text, int fontSize, Vector2 offset)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(70f, 20f);
            rect.anchoredPosition = offset;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;
            tmp.raycastTarget = false;
            return tmp;
        }

        public void SetState(bool blocked, string boundActionName)
        {
            isBlocked = blocked;
            actionLabel.text = string.IsNullOrEmpty(boundActionName) ? string.Empty : boundActionName;
            ApplyRestColor();
        }

        public void SetCursor(bool isCursor)
        {
            background.color = isCursor ? CursorColor : RestColor;
        }

        public void SetSourceSelected(bool selected)
        {
            isSourceSelected = selected;
            ApplyRestColor();
        }

        private Color RestColor
        {
            get
            {
                if (isBlocked) return BlockedColor;
                if (isSourceSelected) return SourceColor;
                return overlaysHandDrawnArt ? TransparentOverlay : NormalColor;
            }
        }

        private void ApplyRestColor()
        {
            background.color = RestColor;
            keyLabel.color = isBlocked ? new Color(0.55f, 0.55f, 0.55f) : Color.black;
        }
    }
}
