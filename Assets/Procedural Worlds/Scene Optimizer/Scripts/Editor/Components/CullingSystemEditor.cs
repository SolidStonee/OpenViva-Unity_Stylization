using UnityEditor;
using UnityEngine;
namespace ProceduralWorlds.SceneOptimizer
{
    [CustomEditor(typeof(CullingSystem))]
    public class CullingSystemEditor : SceneOptimizerBaseEditor
    {
        private CullingSystem m_cullingSystem;
        private Vector2 scrollPos;
        public override void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this);
            m_cullingSystem = target as CullingSystem;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            m_editorUtils.GUIHeader();
            m_editorUtils.GUINewsHeader();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, Styles.panel);
            {
                EditorGUI.BeginChangeCheck();
                m_editorUtils.Panel("CullingSettings", CullingSettingsPanel);
                if (EditorGUI.EndChangeCheck())
                {
                    UnityEditor.EditorUtility.SetDirty(m_cullingSystem);
                }
            }
            EditorGUILayout.EndScrollView();
        }
        public void CullingSettingsPanel(bool helpEnabled)
        {
            m_cullingSystem.MainCamera = (Camera)m_editorUtils.ObjectField("MainCamera", m_cullingSystem.MainCamera, typeof(Camera), true, helpEnabled);
            m_cullingSystem.SunLight = (Light)m_editorUtils.ObjectField("SunLight", m_cullingSystem.SunLight, typeof(Light), true, helpEnabled);
            m_cullingSystem.EnableLayerCulling = m_editorUtils.Toggle("EnableLayerCulling", m_cullingSystem.EnableLayerCulling);
            m_cullingSystem.AdditionalCullingDistance = m_editorUtils.FloatField("AdditionalCullingDistance", m_cullingSystem.AdditionalCullingDistance, helpEnabled);
            m_editorUtils.Heading("ObjectLayerCullingDistance");
            m_editorUtils.InlineHelp("ObjectLayerCullingDistance", helpEnabled);
            EditorGUI.indentLevel++;
            {
                float[] objectLayerCullingDistances = m_cullingSystem.ObjectLayerCullingDistances;
                for (int i = 0; i < objectLayerCullingDistances.Length; i++)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        objectLayerCullingDistances[i] = EditorGUILayout.FloatField(string.Format("[{0}] {1}", i, layerName), objectLayerCullingDistances[i]);
                    }
                }
            }
            EditorGUI.indentLevel--;
            m_editorUtils.Heading("ShadowLayerCullingDistance");
            m_editorUtils.InlineHelp("ShadowLayerCullingDistance", helpEnabled);
            EditorGUI.indentLevel++;
            {
                float[] shadowLayerCullingDistances = m_cullingSystem.ShadowLayerCullingDistances;
                for (int i = 0; i < shadowLayerCullingDistances.Length; i++)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        shadowLayerCullingDistances[i] = EditorGUILayout.FloatField(string.Format("[{0}] {1}", i, layerName), shadowLayerCullingDistances[i]);
                    }
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}