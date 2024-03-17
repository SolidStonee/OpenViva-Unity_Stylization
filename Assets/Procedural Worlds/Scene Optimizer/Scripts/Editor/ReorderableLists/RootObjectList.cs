using UnityEditor;
using UnityEngine;
namespace ProceduralWorlds.SceneOptimizer
{
    public class RootObjectList : ReorderableListEditor<GameObjectEntry>
    {
        public Tools m_tools;
        protected override void DrawListHeader(Rect rect)
        {
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.LabelField(rect, ("Root GameObjects"));
            EditorGUI.indentLevel = oldIndent;
        }
        protected override void DrawListElement(Rect rect, GameObjectEntry entry, bool isFocused)
        {
            // Spawner Object
            EditorGUI.BeginChangeCheck();
            {
                GameObject gameObject = entry.GameObject;
                bool hasDuplicates = m_tools.RootGameObjects.IsDuplicate(entry);
                bool isNull = gameObject == null;
                bool isSceneNull = entry.IsNullScene();
                Color oldColor = GUI.color;
                if (isNull)
                    GUI.color = SceneOptimizerEditor.WARNING_COLOR;
                if (hasDuplicates)
                    GUI.color = SceneOptimizerEditor.ERROR_COLOR;
                if (m_tools.SaveToDisk && isSceneNull)
                    GUI.color = SceneOptimizerEditor.WARNING_COLOR;
                int oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                EditorGUI.LabelField(new Rect(rect.x, rect.y + 1f, rect.width * 0.18f, EditorGUIUtility.singleLineHeight), "Active");
                entry.Enabled = EditorGUI.Toggle(new Rect(rect.x + rect.width * 0.18f, rect.y, rect.width * 0.1f, EditorGUIUtility.singleLineHeight), entry.Enabled);
                entry.GameObject = (GameObject)EditorGUI.ObjectField(new Rect(rect.x + rect.width * 0.4f, rect.y + 1f, rect.width * 0.6f, EditorGUIUtility.singleLineHeight), entry.GameObject, typeof(GameObject), true);
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