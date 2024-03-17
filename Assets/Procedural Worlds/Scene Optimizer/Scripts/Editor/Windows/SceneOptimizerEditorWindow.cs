using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace ProceduralWorlds.SceneOptimizer
{
    public class SceneOptimizerEditorWindow : EditorWindow
    {
        public const string WINDOW_TITLE = "Scene Optimizer";
        #region Variables
        protected bool m_inited = false;
        public SceneOptimizer m_sceneOptimizer;
        public SceneOptimizerEditor m_editor;
        private Dictionary<int, Transform> uniqueTransforms = new Dictionary<int, Transform>();
        private SpatialHashing<Transform> uniqueSpatialHash = null;
        #endregion
        #region Properties
        public SceneOptimizer SceneOptimizer
        {
            get
            {
                if (m_sceneOptimizer == null)
                {
                    m_sceneOptimizer = Resources.Load<SceneOptimizer>(WINDOW_TITLE);
                }
                return m_sceneOptimizer;
            }
        }
        public SceneOptimizerEditor ToolsEditor
        {
            get
            {
                if (m_editor == null)
                {
                    m_editor = Editor.CreateEditor(SceneOptimizer) as SceneOptimizerEditor;
                }
                return m_editor;
            }
        }
        #endregion
        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.hierarchyWindowItemOnGUI -= DetectInput;
        }
        private void OnFocus()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.hierarchyWindowItemOnGUI -= DetectInput;
            EditorApplication.hierarchyWindowItemOnGUI += DetectInput;
            ToolsEditor.OnFocus();
        }
        private void OnLostFocus()
        {
            ToolsEditor.OnLostFocus();
        }
        private void OnValidate()
        {
            RefreshTransforms();
        }
        private void OnSelectionChange()
        {
            RefreshTransforms();
        }
        private void RefreshTransforms()
        {
            OptimizeCommand command = ToolsEditor.optimizeCommand;
            if (command != null)
            {
                Tools sceneOptimization = ToolsEditor.tools;
                List<GameObjectEntry> entries = sceneOptimization.GetAllRoots();
                List<GameObjectEntry> filteredEntries = new List<GameObjectEntry>();
                foreach (GameObjectEntry entry in entries)
                {
                    GameObject gameObject = entry.GameObject;
                    if (gameObject == null)
                        continue;
                    foreach (GameObject selected in Selection.gameObjects)
                    {
                        if (selected.IsChildOf(gameObject))
                        {
                            filteredEntries.Add(new GameObjectEntry(selected)
                            {
                                Enabled = entry.Enabled,
                                DisableChildren = entry.DisableChildren
                            });
                        }
                    }
                }
                uniqueTransforms = PWUtility.GetUniqueTransforms(command, filteredEntries);
                uniqueSpatialHash = new SpatialHashing<Transform>(command.CellSize, command.CellOffset);
                foreach (KeyValuePair<int, Transform> pair in uniqueTransforms)
                {
                    Transform transform = pair.Value;
                    Vector3 position = transform.position;
                    uniqueSpatialHash.Insert(position, transform);
                }
            }
        }
        private void DetectInput(int instanceID, Rect selectionRect)
        {
            if (instanceID == Selection.activeInstanceID)
            {
                ToolsEditor.OnSceneGUI();
            }
        }
        private void DrawHandles(OptimizeCommand command)
        {
            if (!command.Enabled)
                return;
            if (uniqueSpatialHash != null)
            {
                Vector3 cellSize = command.CellSize;
                Vector3 offset = command.CellOffset;
                Handles.color = command.VisualizationColor;
                foreach (KeyValuePair<Vector3Int, List<Transform>> pair in uniqueSpatialHash.chunks)
                {
                    Vector3Int key = pair.Key;
                    Vector3 center = Vector3.Scale(key, cellSize);
                    center += offset;
                    Handles.DrawWireCube(center, cellSize);
                }
                Bounds bounds = uniqueSpatialHash.GetBounds();
                Handles.color = Color.yellow;
                Handles.DrawWireCube(bounds.center, bounds.size);
                float objectLayerCullingDistance = command.ObjectlayerCullingDistance;
                float shadowLayerCullingDistance = command.ShadowLayerCullingDistance;
                Color objectColor = command.ObjectVisualizationColor;
                Color shadowColor = command.ShadowVisualizationColor;
                PWEditorUtility.RenderSceneCullingCamera(objectLayerCullingDistance, shadowLayerCullingDistance, objectColor, shadowColor);
            }
        }
        private void OnSceneGUI(SceneView sceneView)
        {
            ToolsEditor.OnSceneGUI();
            // Draw Handles
            OptimizeCommand selected = ToolsEditor.optimizeCommand;
            if (selected != null)
            {
                DrawHandles(selected);
                HandleUtility.Repaint();
            }
        }
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            {
                ToolsEditor.OnInspectorGUI();
            }
            if (EditorGUI.EndChangeCheck())
            {
                RefreshTransforms();
            }
        }
    }
}