using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.SceneOptimizer
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SceneData))]
    public class SceneDataEditor : SceneOptimizerBaseEditor
    {
        public override void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "SceneOptimizerEditor");
        }

        public override void OnInspectorGUI()
        {
            Initialize();

            List<SceneData> sceneDatas = new List<SceneData>();
            foreach (var target in targets)
            {
                sceneDatas.Add(target as SceneData);
            }

            if (GUILayout.Button("Show Original"))
            {
                foreach (var sceneData in sceneDatas)
                    sceneData.ShowOriginal();
            }

            if (GUILayout.Button("Show Optimized"))
            {
                foreach (var sceneData in sceneDatas)
                    sceneData.ShowOptimized();
            }
        }
    }
}