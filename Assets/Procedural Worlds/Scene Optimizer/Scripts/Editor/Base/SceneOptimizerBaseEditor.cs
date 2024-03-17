using PWCommon5;
using UnityEditor;
using UnityEngine;
namespace ProceduralWorlds.SceneOptimizer
{
    public class SceneOptimizerBaseEditor : Editor, IPWEditor
    {
        #region Variables
        private PWStyles m_styles;
        protected EditorUtils m_editorUtils;
        protected bool m_inited = false;
        #endregion
        #region Properties
        protected PWStyles Styles => m_styles;
        public Rect position { get; set; }
        public bool maximized { get; set; }
        public bool PositionChecked { get; set; }
        #endregion
        #region Methods
        protected void Initialize()
        {
            // Initialize GUI
            if (m_styles == null || m_inited == false)
            {
                m_styles?.Dispose();
                m_styles = new PWStyles();
                m_inited = true;
            }
            // Initialize Editor Utils (if it exists)
            m_editorUtils?.Initialize();
        }
        protected virtual void OnDestroy() => m_styles?.Dispose();
        public override void OnInspectorGUI() => Initialize();
        public virtual void OnEnable()
        {
        }
        public virtual void OnSceneGUI()
        {
        }
        public virtual void OnFocus()
        {
        }
        public virtual void OnLostFocus()
        {
        }
        #endregion
    }
}