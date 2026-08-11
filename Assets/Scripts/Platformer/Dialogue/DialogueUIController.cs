using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Platformer.Core;

namespace Platformer.Dialogue
{
    /// <summary>
    /// Owns the on-screen dialogue box and builds it at runtime so no hand-authored prefab
    /// is required. Call Advance(data) once per Interact press: the first call opens the
    /// box and shows line 0, later calls step through the remaining lines, and the call
    /// after the last line closes the box again.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class DialogueUIController : SingletonBehaviour<DialogueUIController>
    {
        [SerializeField] private Transform uiParent;
        [SerializeField] private Color boxColor = new Color(0f, 0f, 0f, 0.75f);
        [SerializeField] private Color textColor = Color.white;

        private GameObject panel;
        private TextMeshProUGUI speakerText;
        private TextMeshProUGUI lineText;

        private DialogueData current;
        private int lineIndex;

        protected override void Awake()
        {
            base.Awake();
            BuildUI();
            panel.SetActive(false);
        }

        /// <summary>Shows the next line of <paramref name="data"/>, opening or closing the box as needed.</summary>
        public void Advance(DialogueData data)
        {
            if (data == null || data.LineCount == 0) return;

            if (current != data)
            {
                current = data;
                lineIndex = 0;
                Open();
            }
            else
            {
                lineIndex++;
                if (lineIndex >= current.LineCount)
                {
                    Close();
                    return;
                }
            }

            speakerText.text = current.SpeakerName;
            lineText.text = current.GetLine(lineIndex);
        }

        /// <summary>Force-ends the conversation if it's the one currently showing (e.g. player walked away).</summary>
        public void EndConversation(DialogueData data)
        {
            if (current == data) Close();
        }

        private void Open()
        {
            panel.SetActive(true);
            GameplayInputGate.Push();
        }

        private void Close()
        {
            if (panel != null) panel.SetActive(false);
            if (current != null) GameplayInputGate.Pop();
            current = null;
            lineIndex = 0;
        }

        private void BuildUI()
        {
            var canvas = uiParent != null ? uiParent : FindObjectOfType<Canvas>()?.transform;

            panel = new GameObject("DialoguePanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas, false);

            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.sizeDelta = new Vector2(900f, 160f);
            panelRect.anchoredPosition = new Vector2(0f, 40f);
            panel.GetComponent<Image>().color = boxColor;

            speakerText = CreateText("Speaker", panel.transform, 22, TextAlignmentOptions.TopLeft);
            speakerText.fontStyle = FontStyles.Bold;
            var speakerRect = speakerText.rectTransform;
            speakerRect.anchorMin = new Vector2(0f, 1f);
            speakerRect.anchorMax = new Vector2(1f, 1f);
            speakerRect.pivot = new Vector2(0.5f, 1f);
            speakerRect.sizeDelta = new Vector2(-40f, 32f);
            speakerRect.anchoredPosition = new Vector2(0f, -10f);

            lineText = CreateText("Line", panel.transform, 24, TextAlignmentOptions.TopLeft);
            var lineRect = lineText.rectTransform;
            lineRect.anchorMin = Vector2.zero;
            lineRect.anchorMax = Vector2.one;
            lineRect.offsetMin = new Vector2(20f, 12f);
            lineRect.offsetMax = new Vector2(-20f, -44f);
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, int fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.color = textColor;
            text.alignment = alignment;
            return text;
        }
    }
}
