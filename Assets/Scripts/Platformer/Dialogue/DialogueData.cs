using UnityEngine;

namespace Platformer.Dialogue
{
    /// <summary>
    /// A short sequence of lines shown one at a time. Each Interact press while talking
    /// advances to the next line; interacting again after the last line ends the
    /// conversation. Author instances via Assets > Create > Platformer > Dialogue.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueData", menuName = "Platformer/Dialogue/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [SerializeField] private string speakerName;
        [SerializeField, TextArea(2, 4)] private string[] lines;

        public string SpeakerName => speakerName;
        public int LineCount => lines?.Length ?? 0;

        public string GetLine(int index)
        {
            if (lines == null || index < 0 || index >= lines.Length) return string.Empty;
            return lines[index];
        }
    }
}
