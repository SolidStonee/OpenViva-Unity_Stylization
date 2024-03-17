using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
namespace ProceduralWorlds.SceneOptimizer
{
    public class OptimizeCommandList : ReorderableListEditor<OptimizeCommand>
    {
        private static Color[] m_defaultColors =
        {
            Color.red,
            Color.green,
            Color.blue
        };
        private static int m_nextColor = -1;
        public static int NextColor
        {
            get => m_nextColor;
            set
            {
                if (value < 0)
                    value = m_defaultColors.Length - 1;
                if (value >= m_defaultColors.Length)
                    value = 0;
                m_nextColor = value;
            }
        }
        public static Color GetNextColor()
        {
            NextColor++;
            if (NextColor >= m_defaultColors.Length)
                NextColor = 0;
            return m_defaultColors[NextColor];
        }
        private List<OptimizeCommand> duplicates = new List<OptimizeCommand>();
        public bool HasDuplicates()
        {
            ScanForDuplicates();
            return duplicates.Count > 0;
        }
        private void ScanForDuplicates()
        {
            duplicates.Clear();
            List<string> names = new List<string>();
            Dictionary<string, OptimizeCommand> uniqueNames = new Dictionary<string, OptimizeCommand>();
            foreach (OptimizeCommand command in m_reorderableList.list)
            {
                string name = command.Name;
                if (uniqueNames.ContainsKey(name))
                {
                    // Duplicate found!
                    OptimizeCommand a = uniqueNames[name];
                    if (!duplicates.Contains(a))
                        duplicates.Add(a);
                    if (!duplicates.Contains(command))
                        duplicates.Add(command);
                }
                else
                {
                    uniqueNames.Add(name, command);
                }
            }
        }
        protected override void DrawListHeader(Rect rect)
        {
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.LabelField(rect, "OptimizeCommands");
            EditorGUI.indentLevel = oldIndent;
            ScanForDuplicates();
        }
        protected override void AddListEntry(ReorderableList reorderableList)
        {
            OptimizeCommand newCommand = new OptimizeCommand();
            newCommand.VisualizationColor = GetNextColor();
            reorderableList.list.Add(newCommand);
            reorderableList.index = reorderableList.list.Count - 1;
        }
        protected override void OnRemoveListEntry(ReorderableList reorderableList)
        {
            base.OnRemoveListEntry(reorderableList);
            NextColor--;
        }
        protected override void DrawListElement(Rect rect, OptimizeCommand entry, bool isFocused)
        {
            // Spawner Object
            EditorGUI.BeginChangeCheck();
            {
                Color oldColor = GUI.color;
                if (duplicates.Contains(entry))
                    GUI.color = SceneOptimizerEditor.ERROR_COLOR;
                int oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                float rectX = rect.x;
                EditorGUILayout.BeginHorizontal();
                EditorGUI.LabelField(new Rect(rectX, rect.y + 1f, rect.width * 0.2f, EditorGUIUtility.singleLineHeight), "Active");
                rectX += rect.width * 0.18f;
                entry.Enabled = EditorGUI.Toggle(new Rect(rectX, rect.y, rect.width * 0.1f, EditorGUIUtility.singleLineHeight), entry.Enabled);
                rectX += rect.width * 0.1f;
                entry.Name = EditorGUI.TextField(new Rect(rectX, rect.y + 1f, rect.width * 0.7f, EditorGUIUtility.singleLineHeight), entry.Name);
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel = oldIndent;
                GUI.color = oldColor;
            }
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }
    }
}