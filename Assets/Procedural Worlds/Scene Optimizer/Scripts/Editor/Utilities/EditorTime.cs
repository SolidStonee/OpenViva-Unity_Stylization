using UnityEditor;
namespace ProceduralWorlds.SceneOptimizer
{
    [InitializeOnLoad]
    public static class PWEditorTime
    {
        public static float deltaTime = 0f;
        private static float lastTimeSinceStartup = 0f;
        static PWEditorTime()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }
        public static void Update()
        {
            if (lastTimeSinceStartup == 0f)
                lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
            deltaTime = (float)EditorApplication.timeSinceStartup - lastTimeSinceStartup;
            lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
        }
    }
}