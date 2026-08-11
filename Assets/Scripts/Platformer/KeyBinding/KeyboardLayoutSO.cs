using System;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.KeyBinding
{
    /// <summary>
    /// Describes the physical key grid shown in the key-remap UI: which KeyCode sits at
    /// each row/column, matching (roughly) a real keyboard layout. KeyCode.None marks an
    /// empty cell - not a real key, skipped entirely by navigation.
    ///
    /// A cell can optionally carry a custom pixel rect (<see cref="KeyCell.overrideRect"/>)
    /// so its on-screen position/size can be hand-tuned to line up with a hand-drawn
    /// keyboard background instead of the default uniform grid. Create a custom asset via
    /// the asset menu below; if none is assigned, <see cref="CreateDefaultQwerty"/> is used
    /// as a runtime fallback so the game still works without one.
    /// </summary>
    [CreateAssetMenu(fileName = "KeyboardLayout", menuName = "Platformer/Key Binding/Keyboard Layout")]
    public class KeyboardLayoutSO : ScriptableObject
    {
        [Serializable]
        public struct KeyCell
        {
            public KeyCode key;
            public bool overrideRect;
            public Rect customRect;
        }

        [Serializable]
        public class Row
        {
            public KeyCell[] cells;
        }

        [SerializeField] private List<Row> rows = new List<Row>();

        public int RowCount => rows.Count;

        public int ColumnCount(int row) => rows[row].cells.Length;

        public KeyCode GetKey(int row, int column)
        {
            if (!IsValid(row, column)) return KeyCode.None;
            return rows[row].cells[column].key;
        }

        public bool TryGetCustomRect(int row, int column, out Rect rect)
        {
            rect = default;
            if (!IsValid(row, column)) return false;
            var cell = rows[row].cells[column];
            if (!cell.overrideRect) return false;
            rect = cell.customRect;
            return true;
        }

        private bool IsValid(int row, int column)
        {
            if (row < 0 || row >= rows.Count) return false;
            var cells = rows[row].cells;
            return cells != null && column >= 0 && column < cells.Length;
        }

        /// <summary>
        /// Runtime fallback layout covering letters, digits and the most common extra keys,
        /// used whenever no hand-authored KeyboardLayoutSO asset is assigned.
        /// </summary>
        public static KeyboardLayoutSO CreateDefaultQwerty()
        {
            var layout = CreateInstance<KeyboardLayoutSO>();

            KeyCell C(KeyCode key) => new KeyCell { key = key };

            layout.rows = new List<Row>
            {
                new Row { cells = new[]
                {
                    C(KeyCode.Alpha1), C(KeyCode.Alpha2), C(KeyCode.Alpha3), C(KeyCode.Alpha4), C(KeyCode.Alpha5),
                    C(KeyCode.Alpha6), C(KeyCode.Alpha7), C(KeyCode.Alpha8), C(KeyCode.Alpha9), C(KeyCode.Alpha0),
                    C(KeyCode.Backspace),
                }},
                new Row { cells = new[]
                {
                    C(KeyCode.Tab), C(KeyCode.Q), C(KeyCode.W), C(KeyCode.E), C(KeyCode.R), C(KeyCode.T),
                    C(KeyCode.Y), C(KeyCode.U), C(KeyCode.I), C(KeyCode.O), C(KeyCode.P),
                }},
                new Row { cells = new[]
                {
                    C(KeyCode.A), C(KeyCode.S), C(KeyCode.D), C(KeyCode.F), C(KeyCode.G), C(KeyCode.H),
                    C(KeyCode.J), C(KeyCode.K), C(KeyCode.L), C(KeyCode.Semicolon),
                }},
                new Row { cells = new[]
                {
                    C(KeyCode.LeftShift), C(KeyCode.Z), C(KeyCode.X), C(KeyCode.C), C(KeyCode.V), C(KeyCode.B),
                    C(KeyCode.N), C(KeyCode.M), C(KeyCode.Comma), C(KeyCode.Period), C(KeyCode.Slash),
                }},
                new Row { cells = new[]
                {
                    C(KeyCode.LeftControl), C(KeyCode.LeftAlt), C(KeyCode.Space), C(KeyCode.RightAlt),
                    C(KeyCode.UpArrow), C(KeyCode.LeftArrow), C(KeyCode.DownArrow), C(KeyCode.RightArrow),
                }},
            };
            return layout;
        }
    }
}
