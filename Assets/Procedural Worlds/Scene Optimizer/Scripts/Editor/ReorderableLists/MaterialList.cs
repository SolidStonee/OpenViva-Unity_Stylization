using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
namespace ProceduralWorlds.SceneOptimizer
{
    public class MaterialList : ReorderableListEditor<MaterialEntry>
    {
        private Tools m_sceneOptimization;
        private OptimizeCommand m_currentCommand;
        public Tools SceneOptimization
        {
            get => m_sceneOptimization;
            set => m_sceneOptimization = value;
        }
        public OptimizeCommand CurrentCommand
        {
            get => m_currentCommand;
            set
            {
                m_currentCommand = value;
                Create(m_currentCommand.MaterialFilter, false, true, true, true, true);
            }
        }
        
        private List<MaterialEntry> duplicates = new List<MaterialEntry>();
        public bool HasDuplicates()
        {
            ScanForDuplicates();
            return duplicates.Count > 0;
        }
        private void ScanForDuplicates()
        {
            duplicates.Clear();
            List<string> names = new List<string>();
            Dictionary<Material, MaterialEntry> uniqueEntries = new Dictionary<Material, MaterialEntry>();
            foreach (MaterialEntry entry in m_reorderableList.list)
            {
                Material material = entry.Material;
                if (material == null)
                    continue;
                if (uniqueEntries.ContainsKey(material))
                {
                    // Duplicate found!
                    MaterialEntry uniqueMaterial = uniqueEntries[material];
                    if (uniqueMaterial == null)
                        continue;
                    if (!duplicates.Contains(uniqueMaterial))
                        duplicates.Add(uniqueMaterial);
                    if (!duplicates.Contains(entry))
                        duplicates.Add(entry);
                }
                else
                {
                    uniqueEntries.Add(material, entry);
                }
            }
        }
        protected override void DrawListHeader(Rect rect)
        {
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.LabelField(rect, ("Material Entries"));
            EditorGUI.indentLevel = oldIndent;
            ScanForDuplicates();
        }
        protected override void DrawListElement(Rect rect, MaterialEntry entry, bool isFocused)
        {
            if (isFocused)
            {
                // if (m_selectedExtension != entry.Extension)
                // {
                //     DeselectAllExtensionEntries();
                //     entry.IsSelected = true;
                //     SelectExtensionEntry(entry);
                // }
            }
            // Spawner Object
            EditorGUI.BeginChangeCheck();
            {
                Color oldColor = GUI.color;
                bool hasDiplicates = duplicates.Contains(entry);
                if (hasDiplicates)
                    GUI.color = SceneOptimizerEditor.ERROR_COLOR;
                int oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                EditorGUI.LabelField(new Rect(rect.x, rect.y + 1f, rect.width * 0.18f, EditorGUIUtility.singleLineHeight), "Active");
                entry.Enabled = EditorGUI.Toggle(new Rect(rect.x + rect.width * 0.18f, rect.y, rect.width * 0.1f, EditorGUIUtility.singleLineHeight), entry.Enabled);
                entry.Material = (Material)EditorGUI.ObjectField(new Rect(rect.x + rect.width * 0.4f, rect.y + 1f, rect.width * 0.6f, EditorGUIUtility.singleLineHeight), entry.Material, typeof(Material), false);
                EditorGUI.indentLevel = oldIndent;
                GUI.color = oldColor;
            }
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }
        protected override void OnAddDropdownCallback(Rect rect, ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add New Material"), false, AddNewMaterial);
            menu.AddItem(new GUIContent("Scan For Materials"), false, ScanForMaterials);
            menu.ShowAsContext();
        }
        private void AddNewMaterial()
        {
            m_currentCommand?.AddNewMaterial();
        }
        private void ScanForMaterials()
        {
            if (m_currentCommand == null)
                return;
            OptimizeCall optimizeCall = new OptimizeCall();
            optimizeCall.rootObjects = SceneOptimization.GetAllRoots();
            m_currentCommand.ScanForMaterials(optimizeCall);
        }
    }
}