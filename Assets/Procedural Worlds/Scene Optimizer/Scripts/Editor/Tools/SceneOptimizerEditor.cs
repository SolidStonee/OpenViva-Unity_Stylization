using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
namespace ProceduralWorlds.SceneOptimizer
{
    [CustomEditor(typeof(SceneOptimizer))]
    public class SceneOptimizerEditor : SceneOptimizerBaseEditor
    {
        public const string OBJECT_LAYER_CULLING = "Object Layer Culling";
        public const string SHADOW_LAYER_CULLING = "Shadow Layer Culling";
        public static readonly Color WARNING_COLOR = new Color(1f, 0.8f, 0.34f);
        public static readonly Color ERROR_COLOR = new Color(1f, 0.52f, 0.41f);
        private static Color ACTION_BUTTON_COLOR = new Color(0.4666667f, 0.6666667f, 0.2352941f);
        private static Color ACTION_BUTTON_PRO_COLOR = new Color(0.2117647f, 0.3176471f, 0.09019608f);
        public static Color ActionButtonColor => EditorGUIUtility.isProSkin ? ACTION_BUTTON_PRO_COLOR : ACTION_BUTTON_COLOR;
        private static bool m_generalSettingsPanel = false;
        private static bool m_keyBindingsPanel = false;
        private static bool m_sceneOptimizationPanel = true;
        [NonSerialized] private int m_selectedOptimizeCommand = -1;
        private SceneOptimizer m_sceneOptimizer;
        private Tools m_tools;
        private RootObjectList m_rootObjectList = new RootObjectList();
        private OptimizeCommandList m_optimizeCommandList = new OptimizeCommandList();
        private MaterialList m_materialEntries = new MaterialList();
        private List<MaterialEntry> m_filteredEntries = new List<MaterialEntry>();
        private Vector2 scrollPos;
        private bool m_infoPresent = false;
        private bool m_warningsPresent = false;
        private bool m_errorsPresent = false;
        // private bool m_canRevert = false;
        public OptimizeCommandList optimizeCommandList => m_optimizeCommandList;
        public Tools tools => m_sceneOptimizer.Tools;
        public List<OptimizeCommand> optimizeCommands => tools.OptimizeCommands;
        public OptimizeCommand optimizeCommand
        {
            get
            {
                OptimizeCommand result = null;
                if (m_selectedOptimizeCommand >= 0 && m_selectedOptimizeCommand < optimizeCommands.Count)
                    result = optimizeCommands[m_selectedOptimizeCommand];
                return result;
            }
        }
        public override void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this);
            m_sceneOptimizer = target as SceneOptimizer;
            if (m_sceneOptimizer == null)
                return;
            m_tools = m_sceneOptimizer.Tools;
            m_rootObjectList.m_tools = m_sceneOptimizer.Tools;
            m_rootObjectList.Create(tools.RootGameObjects);
            m_rootObjectList.OnChanged = MarkDirty;
            m_optimizeCommandList.Create(optimizeCommands);
            m_optimizeCommandList.OnChanged = MarkDirty;
            m_materialEntries.Create(m_filteredEntries, false, true, true, true, true);
            m_materialEntries.OnChanged = MarkDirty;
            m_materialEntries.SceneOptimization = tools;
            m_optimizeCommandList.OnSelectionChangedEvent -= OnSelectionChanged;
            m_optimizeCommandList.OnSelectionChangedEvent += OnSelectionChanged;
        }
        private void MarkDirty()
        {
            EditorUtility.SetDirty(m_sceneOptimizer);
        }
        private void OnSelectionChanged(int index)
        {
            List<OptimizeCommand> optimizeCommands = m_tools.OptimizeCommands;
            if (index >= 0 && index < optimizeCommands.Count)
            {
                m_selectedOptimizeCommand = index;
                OptimizeCommand command = optimizeCommands[index];
                if (command != null)
                {
                    m_materialEntries.CurrentCommand = command;
                }
            }
            GUI.changed = true;
        }
        /// <summary>
        /// Handle drop area for new objects
        /// </summary>
        public bool DrawRootGameObjectGUI()
        {
            // Ok - set up for drag and drop
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drop GameObjects", Styles.dropPanel);
            if (evt.type == EventType.DragPerform || evt.type == EventType.DragUpdated)
            {
                if (!dropArea.Contains(evt.mousePosition))
                    return false;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    //Handle game objects / prefabs
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject go)
                        {
                            tools.RootGameObjects.Add(new GameObjectEntry
                            {
                                Enabled = true,
                                GameObject = go
                            });
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            m_editorUtils.GUIHeader();
            m_editorUtils.GUINewsHeader();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, Styles.panel);
            {
                EditorGUI.BeginChangeCheck();
                m_generalSettingsPanel = m_editorUtils.Panel("Settings", SettingsPanel, m_generalSettingsPanel);
                m_keyBindingsPanel = m_editorUtils.Panel("KeyBindings", KeyBindingsPanel, m_keyBindingsPanel);
                m_sceneOptimizationPanel = m_editorUtils.Panel("SceneOptimization", SceneOptimizationPanel, m_sceneOptimizationPanel);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(m_sceneOptimizer);
                }
            }
            EditorGUILayout.EndScrollView();
            m_editorUtils.GUINewsFooter();
        }
        public void PerformSceneOptimization(bool useGaia = false, bool recordUndo = true)
        {
            bool perform = true;
            if (tools.ShowWarning)
            {
                perform = false;
                int option = EditorUtility.DisplayDialogComplex("Scene Optimizer - Warning",
                    "Warning: The scene optimizer will change your scene while combining meshes. " +
                    "This can be undone, but it is still a good idea to back up the project / use source " +
                    "control so you can undo the changes if you need to.",
                    "Proceed",
                    "Cancel",
                    "Proceed and never show again");
                switch (option)
                {
                    // Proceed.
                    case 0:
                        perform = true;
                        break;
                    // Cancel.
                    case 1:
                        break;
                    // Proceed and never show again.
                    case 2:
                        perform = true;
                        tools.ShowWarning = false;
                        EditorUtility.SetDirty(m_sceneOptimizer);
                        break;
                    default:
                        Debug.LogError("Unrecognized option.");
                        break;
                }
            }
            if (perform)
            {
                int undoGroup = -1;
                if (recordUndo)
                {
                    Undo.SetCurrentGroupName("Scene Optimization");
                    undoGroup = Undo.GetCurrentGroup();
                }
                OptimizeCall optimizeCall = new OptimizeCall();
                optimizeCall.rootObjects = tools.GetAllRoots();
                if (recordUndo)
                {
                    foreach (GameObjectEntry entry in optimizeCall.rootObjects)
                    {
                        if (entry == null)
                            continue;
                        GameObject gameObject = entry.GameObject;
                        if (gameObject == null)
                            continue;
                        if (gameObject.IsTerrain())
                            continue;
                        Undo.RegisterFullObjectHierarchyUndo(gameObject, "transform selected objects");
                    }
                }
                m_sceneOptimizer.ProcessSceneOptimization(optimizeCall, useGaia, recordUndo);
                if (recordUndo)
                {
                    Undo.CollapseUndoOperations(undoGroup);
                }
            }
        }
        public override void OnSceneGUI()
        {
            base.OnSceneGUI();
            if (m_sceneOptimizer == null)
                return;
            Event e = Event.current;
            if (e == null)
                return;
            if (e.control)
            {
                Settings settings = m_sceneOptimizer.Settings;
                KeyBindings keyBindings = m_sceneOptimizer.KeyBindings;
                Tools sceneOptimization = m_sceneOptimizer.Tools;
                bool process = false;
                bool raiseOrLower = false;
                if (e.type == EventType.KeyDown)
                {
                    if (e.keyCode == keyBindings.SnapToGroundKey)
                    {
                        settings.SnapToGround = true;
                        settings.AlignToGround = false;
                        settings.MoveUp = false;
                        settings.MoveDown = false;
                        process = true;
                    }
                    else if (e.keyCode == keyBindings.AlignToGroundKey)
                    {
                        settings.SnapToGround = false;
                        settings.AlignToGround = true;
                        settings.MoveUp = false;
                        settings.MoveDown = false;
                        process = true;
                    }
                    else if (e.keyCode == keyBindings.AlignAndSnapToGroundKey)
                    {
                        settings.SnapToGround = true;
                        settings.AlignToGround = true;
                        settings.MoveUp = false;
                        settings.MoveDown = false;
                        process = true;
                    }
                    else if (e.keyCode == keyBindings.RaiseFromGroundKey || e.keyCode == KeyCode.KeypadPlus)
                    {
                        settings.MoveUp = true;
                        settings.MoveDown = false;
                        raiseOrLower = true;
                    }
                    else if (e.keyCode == keyBindings.LowerInGroundKey || e.keyCode == KeyCode.KeypadMinus)
                    {
                        settings.MoveDown = true;
                        settings.MoveUp = false;
                        raiseOrLower = true;
                    }
                    else if (e.keyCode == keyBindings.OptimizeKey)
                    {
                        PerformSceneOptimization();
                        e.Use();
                    }
                    if (process)
                    {
                        Undo.SetCurrentGroupName("Processed Selection");
                        int group = Undo.GetCurrentGroup();
                        foreach (GameObject gameObject in Selection.gameObjects)
                            Undo.RegisterFullObjectHierarchyUndo(gameObject, "transform selected objects");
                        m_sceneOptimizer.ProcessSelectedObjects(Selection.gameObjects);
                        Undo.CollapseUndoOperations(group);
                        e.Use();
                    }
                    else if (raiseOrLower)
                    {
                        Undo.SetCurrentGroupName("Raised or Lowered");
                        int group = Undo.GetCurrentGroup();
                        foreach (GameObject gameObject in Selection.gameObjects)
                            Undo.RegisterFullObjectHierarchyUndo(gameObject, "Raised or Lowered");
                        m_sceneOptimizer.RaiseOrLower(Selection.gameObjects);
                        Undo.CollapseUndoOperations(group);
                        e.Use();
                    }
                }
            }
        }
        private void SettingsPanel(bool helpEnabled)
        {
            Settings settings = m_sceneOptimizer.Settings;
            settings.SnapMode = (Constants.SnapMode)m_editorUtils.EnumPopup("SettingsSnapMode", settings.SnapMode, helpEnabled);
            settings.OffsetCheck = m_editorUtils.FloatField("SettingsOffsetCheck", settings.OffsetCheck, helpEnabled);
            settings.DistanceCheck = m_editorUtils.FloatField("SettingsDistanceCheck", settings.DistanceCheck, helpEnabled);
            settings.RaiseAndLowerAmount = m_editorUtils.FloatField("SettingsRaiseAndLowerAmount", settings.RaiseAndLowerAmount, helpEnabled);
        }
        private void KeyBindingsPanel(bool helpEnabled)
        {
            KeyBindings keyBindings = m_sceneOptimizer.KeyBindings;
            GUI.enabled = false;
            SceneOptimizer.m_firstKey = (KeyCode)m_editorUtils.EnumPopup("KeyBindingsHoldDownKey", SceneOptimizer.m_firstKey, helpEnabled);
            GUI.enabled = true;
            keyBindings.SnapToGroundKey = (KeyCode)m_editorUtils.EnumPopup("KeyBindingsSnapToGroundKey", keyBindings.SnapToGroundKey, helpEnabled);
            keyBindings.AlignToGroundKey = (KeyCode)m_editorUtils.EnumPopup("KeyBindingsAlignToSlopeKey", keyBindings.AlignToGroundKey, helpEnabled);
            keyBindings.AlignAndSnapToGroundKey = (KeyCode)m_editorUtils.EnumPopup("KeyBindingsSnapAndAlignToGroundKey", keyBindings.AlignAndSnapToGroundKey, helpEnabled);
            keyBindings.RaiseFromGroundKey = (KeyCode)m_editorUtils.EnumPopup("KeyBindingsRaiseFromGroundKey", keyBindings.RaiseFromGroundKey, helpEnabled);
            keyBindings.LowerInGroundKey = (KeyCode)m_editorUtils.EnumPopup("KeyBindingsLowerInGroundKey", keyBindings.LowerInGroundKey, helpEnabled);
            keyBindings.OptimizeKey = (KeyCode)m_editorUtils.EnumPopup("KeyBindingsOptimizeKey", keyBindings.OptimizeKey, helpEnabled);
        }
        private bool IsStandalonePlatform()
        {
            BuildTarget windows = BuildTarget.StandaloneWindows;
            BuildTarget windows64 = BuildTarget.StandaloneWindows64;
            BuildTarget mac = BuildTarget.StandaloneOSX;
            BuildTarget linux = BuildTarget.StandaloneLinux64;
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            return target == windows || target == windows64 || target == mac || target == linux;
        }
        private void OptimizeCommandPanel(OptimizeCommand command, bool helpEnabled)
        {
            #region Filters
            m_editorUtils.Heading("Filters");
            m_editorUtils.InlineHelp("Filters", helpEnabled);
            EditorGUI.indentLevel++;
            command.ObjectFilterMode = (Constants.ObjectFilterMode)m_editorUtils.EnumPopup("ObjectFilterMode", command.ObjectFilterMode, helpEnabled);
            command.SourceMeshLayers = m_editorUtils.LayerMaskField("SourceMeshLayers", command.SourceMeshLayers, helpEnabled);
            if (command.UseLargeRanges)
            {
                command.MinObjectSize = m_editorUtils.FloatField("MinObjectSize", command.MinObjectSize, helpEnabled);
                command.MaxObjectSize = m_editorUtils.FloatField("MaxObjectSize", command.MaxObjectSize, helpEnabled);
            }
            else
            {
                float minObjectSize = command.MinObjectSize;
                float maxObjectSize = command.MaxObjectSize;
                m_editorUtils.MinMaxSliderWithFields("ObjectSizeRange", ref minObjectSize, ref maxObjectSize, 0, 1024, helpEnabled);
                if (Math.Abs(command.MaxObjectSize - minObjectSize) > 0.01f || Math.Abs(command.MaxObjectSize - maxObjectSize) > 0.01f)
                {
                    command.MinObjectSize = minObjectSize;
                    command.MaxObjectSize = maxObjectSize;
                    GUI.changed = true;
                }
            }
            command.FilterLightmaps = m_editorUtils.Toggle("FilterLightmaps", command.FilterLightmaps, helpEnabled);
            command.FilterMaterials = m_editorUtils.Toggle("FilterMaterials", command.FilterMaterials, helpEnabled);
            if (command.FilterMaterials)
            {
                m_materialEntries.DrawList();
                m_editorUtils.InlineHelp("MaterialEntries", helpEnabled);
                if (m_editorUtils.Button("ClearAllMaterials", helpEnabled))
                {
                    command.ClearAllMaterials();
                }
            }
            EditorGUI.indentLevel--;
            #endregion
            #region Spatial Parition
            m_editorUtils.Heading("SpatialPartition");
            m_editorUtils.InlineHelp("SpatialPartition", helpEnabled);
            EditorGUI.indentLevel++;
            if (command.UseLargeRanges)
            {
                command.CellSize = m_editorUtils.Vector3Field("CellSize", command.CellSize, helpEnabled);
                command.CellOffset = m_editorUtils.Vector3Field("CellOffset", command.CellOffset, helpEnabled);
            }
            else
            {
                Vector3 cellSize = command.CellSize;
                Vector3 offset = command.CellOffset;
                EditorGUI.BeginChangeCheck();
                {
                    cellSize.x = cellSize.y = cellSize.z = m_editorUtils.FloatField("CellSize", cellSize.x, helpEnabled);
                    offset.x = offset.y = offset.z = m_editorUtils.FloatField("CellOffset", offset.x, helpEnabled);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    command.CellSize = cellSize;
                    command.CellOffset = offset;
                }
            }
            EditorGUI.indentLevel--;
            #endregion
            #region General Settings
            m_editorUtils.Heading("Settings");
            m_editorUtils.InlineHelp("Settings", helpEnabled);
            EditorGUI.indentLevel++;
            {
                command.IsStatic = m_editorUtils.Toggle("IsStatic", command.IsStatic, helpEnabled);
                command.MeshFormat = (IndexFormat)m_editorUtils.EnumPopup("MeshFormat", command.MeshFormat, helpEnabled);
                bool performanceIssues = command.MeshFormat == IndexFormat.UInt32 && !IsStandalonePlatform();
                if (performanceIssues)
                {
                    EditorGUILayout.HelpBox("UInt32 can cause performance issues in the current platform. Consider switching to UInt16.", MessageType.Warning);
                    m_warningsPresent |= performanceIssues;
                }
                command.VisualizationColor = m_editorUtils.ColorField("VisualizationColor", command.VisualizationColor, helpEnabled);
                command.DisableRenderers = m_editorUtils.Toggle("DisableRenderers", command.DisableRenderers, helpEnabled);
                bool backupNotification = command.DisableRenderers;
                if (backupNotification)
                {
                    EditorGUILayout.HelpBox("This setting will modify the original objects. You may want to make a backup of the original objects before combining.", MessageType.Info);
                    m_infoPresent |= backupNotification;
                }
                command.UseLargeRanges = m_editorUtils.Toggle("UseLargeRanges", command.UseLargeRanges, helpEnabled);
            }
            EditorGUI.indentLevel--;
            #endregion
            #region Layer Culling
            m_editorUtils.Heading("LayerCulling");
            m_editorUtils.InlineHelp("LayerCulling", helpEnabled);
            EditorGUI.indentLevel++;
            {
                command.AddLayerCulling = m_editorUtils.Toggle("AddLayerCulling", command.AddLayerCulling, helpEnabled);
                if (command.AddLayerCulling)
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        EditorGUI.indentLevel++;
                        {
                            command.TargetMeshLayer = m_editorUtils.LayerField("TargetMeshLayer", command.TargetMeshLayer, helpEnabled);
                            GUI.SetNextControlName(OBJECT_LAYER_CULLING);
                            command.ObjectlayerCullingDistance = m_editorUtils.FloatField("ObjectDistance", command.ObjectlayerCullingDistance, helpEnabled);
                            GUI.SetNextControlName(SHADOW_LAYER_CULLING);
                            command.ShadowLayerCullingDistance = m_editorUtils.FloatField("ShadowDistance", command.ShadowLayerCullingDistance, helpEnabled);
                            command.ObjectVisualizationColor = m_editorUtils.ColorField("ObjectVizColor", command.ObjectVisualizationColor, helpEnabled);
                            command.ShadowVisualizationColor = m_editorUtils.ColorField("ShadowVizColor", command.ShadowVisualizationColor, helpEnabled);
                        }
                        EditorGUI.indentLevel--;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        CullingSystemInternal cullingSystem = m_sceneOptimizer.GetCullingSystem();
                        if (cullingSystem != null)
                        {
                            cullingSystem.SetObjectCullingDistance(command.TargetMeshLayer, command.ObjectlayerCullingDistance);
                            cullingSystem.SetShadowCullingDistance(command.TargetMeshLayer, command.ShadowLayerCullingDistance);
                        }
                    }
                }
            }
            EditorGUI.indentLevel--;
            #endregion
            #region Collisions
            m_editorUtils.Heading("Collisions");
            m_editorUtils.InlineHelp("Collisions", helpEnabled);
            EditorGUI.indentLevel++;
            {
                command.MergeColliders = m_editorUtils.Toggle("MergeColliders", command.MergeColliders, helpEnabled);
                command.AddColliders = m_editorUtils.Toggle("AddColliders", command.AddColliders, helpEnabled);
                if (command.AddColliders)
                {
                    EditorGUI.indentLevel++;
                    {
                        command.AddColliderLayer = m_editorUtils.LayerField("AddColliderLayer", command.AddColliderLayer);
                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
            #endregion
            #region Lod Groups
            m_editorUtils.Heading("LodGroups");
            m_editorUtils.InlineHelp("LodGroups", helpEnabled);
            EditorGUI.indentLevel++;
            if (command.UseLargeRanges)
                command.LodSizeMultiplier = m_editorUtils.FloatField("LodSizeMultiplier", command.LodSizeMultiplier, helpEnabled);
            else
                command.LodSizeMultiplier = m_editorUtils.Slider("LodSizeMultiplier", command.LodSizeMultiplier, 0f, 1f, helpEnabled);
            command.AddLodGroup = m_editorUtils.Toggle("AddLodGroup", command.AddLodGroup, helpEnabled);
            if (command.AddLodGroup)
            {
                EditorGUI.indentLevel++;
                if (command.UseLargeRanges)
                    command.LodCullPercentage = m_editorUtils.FloatField("LodCullPercentage", command.LodCullPercentage, helpEnabled);
                else
                    command.LodCullPercentage = m_editorUtils.Slider("LodCullPercentage", command.LodCullPercentage, 99f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            #endregion
            EditorGUILayout.Space(6f);
        }
        private void SceneOptimizationPanel(bool helpEnabled)
        {
            m_infoPresent = false;
            m_warningsPresent = false;
            m_errorsPresent = false;
            Tools tools = m_sceneOptimizer.Tools;
            List<GameObjectEntry> rootObjects = tools.RootGameObjects;
            m_editorUtils.Heading("TitleOriginalGameObjects");
            m_editorUtils.InlineHelp("TitleOriginalGameObjects", helpEnabled);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                DrawRootGameObjectGUI();
                m_rootObjectList.DrawList();
                m_editorUtils.InlineHelp("RootGameObjects", helpEnabled);
                #region Warnings, Errors and Info
                if (tools.SaveToDisk)
                {
                    bool hasNullScenes = rootObjects.HasNullScenes();
                    if (hasNullScenes)
                    {
                        EditorGUILayout.HelpBox("You have root entries that do not have a saved scene! These objects will be ignored until you save their respective scene.", MessageType.Warning);
                        m_warningsPresent |= hasNullScenes;
                    }
                }
                bool hasDuplicates = rootObjects.HasDuplicates();
                if (hasDuplicates)
                {
                    EditorGUILayout.HelpBox("You have root entries with the same GameObjects!", MessageType.Error);
                    m_errorsPresent |= hasDuplicates;
                }
                bool nullRoots = rootObjects.HasNullRoots();
                if (nullRoots)
                {
                    EditorGUILayout.HelpBox("You have some null root objects!", MessageType.Warning);
                    m_warningsPresent |= nullRoots;
                }
                bool emptyRoots = m_rootObjectList.IsEmpty;
                if (emptyRoots)
                {
                    EditorGUILayout.HelpBox("You need to provide Root GameObjects that contain Meshes in order to optimize.", MessageType.Info);
                    m_infoPresent |= emptyRoots;
                }
                #endregion
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6f);
            m_editorUtils.Heading("TitleOptimizationSettings");
            m_editorUtils.InlineHelp("TitleOptimizationSettings", helpEnabled);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                bool hasDuplicates = m_optimizeCommandList.HasDuplicates();
                m_optimizeCommandList.DrawList();
                if (hasDuplicates)
                {
                    EditorGUILayout.HelpBox("You cannot have Optimize Commands with the same names!", MessageType.Error);
                    m_errorsPresent |= hasDuplicates;
                }
                m_editorUtils.InlineHelp("OptimizeCommands", helpEnabled);
                OptimizeCommand selected = optimizeCommand;
                if (selected != null)
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        OptimizeCommandPanel(selected, helpEnabled);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(m_sceneOptimizer);
                        SceneView.RepaintAll();
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6f);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.Space(3f);
                tools.SaveToDisk = m_editorUtils.Toggle("SaveToDisk", tools.SaveToDisk, helpEnabled);
                bool oldEnabled = GUI.enabled;
                if (tools.DebugPerformance)
                    GUI.enabled = false;
                tools.ChildUnderRoots = m_editorUtils.Toggle("ChildUnderRoots", tools.ChildUnderRoots, helpEnabled);
                GUI.enabled = oldEnabled;
                tools.DebugPerformance = m_editorUtils.Toggle("DebugPerformance", tools.DebugPerformance, helpEnabled);
                EditorGUILayout.Space(3f);
                if (m_errorsPresent)
                {
                    EditorGUILayout.HelpBox("You need to fix all Errors above before Combining Meshes!", MessageType.Error);
                }
                if (m_warningsPresent)
                {
                    EditorGUILayout.HelpBox("Be sure to address any warnings before Combining Meshes.", MessageType.Warning);
                }
                EditorGUILayout.BeginHorizontal();
                {
                    if (m_editorUtils.Button("ResetToDefaults", GUILayout.Height(30f)))
                    {
                        if (EditorUtility.DisplayDialog("Reset to Defaults", "Are you sure you want to reset all of the Scene Optimization settings?", "Yes", "No"))
                            tools.ResetToDefaults();
                    }
                    oldEnabled = GUI.enabled;
                    if (m_errorsPresent || m_rootObjectList.IsEmpty)
                        GUI.enabled = false;
                    Color oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = ActionButtonColor;
                    if (m_editorUtils.Button("ActionButton", GUILayout.Height(30f)))
                        PerformSceneOptimization();
                    GUI.backgroundColor = oldColor;
                    GUI.enabled = oldEnabled;
                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("ResetToDefaults", helpEnabled);
                m_editorUtils.InlineHelp("ActionButton", helpEnabled);
                if (m_sceneOptimizer.IsGaiaTerrainLoadedScene())
                {
                    oldEnabled = GUI.enabled;
                    if (m_errorsPresent)
                        GUI.enabled = false;
                    Color oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = ActionButtonColor;
                    if (GUILayout.Button("Optimize Scene: Gaia Loaded Terrains", GUILayout.Height(30f)))
                    {
                        PerformSceneOptimization(true, false);
                    }
                    GUI.backgroundColor = oldColor;
                    GUI.enabled = oldEnabled;
                }
                // if (m_canRevert)
                // {
                //     if (m_editorUtils.Button("Revert Scene", GUILayout.Height(30f)))
                //     {
                //         if (EditorUtility.DisplayDialog("", "Are you sure you want to reset all of the Scene Optimization settings?", "Yes", "No"))
                //         {
                //             SceneOptimizer.RevertOptimization();
                //             m_canRevert = false;
                //         }
                //     }
                // }
                EditorGUILayout.Space(3f);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6f);
        }
    }
}