using UnityEditor;
using UnityEngine;
namespace ProceduralWorlds.SceneOptimizer
{
    public static class MenuItemEditor
    {
        [MenuItem("Window/Procedural Worlds/Scene Optimizer/Main Window...", priority = 41)]
        public static void OpenMainWindow()
        {
            SceneOptimizerEditorWindow win = SceneOptimizerEditorWindow.GetWindow<SceneOptimizerEditorWindow>();
            win.titleContent = new GUIContent(SceneOptimizerEditorWindow.WINDOW_TITLE);
            win.minSize = new Vector2(300f, 300f);
            win.Show();
        }
        [MenuItem("Window/Procedural Worlds/Scene Optimizer/Cleanup")]
        public static void Cleanup()
        {
            SceneOptimizer.Cleanup();
        }
    }
}