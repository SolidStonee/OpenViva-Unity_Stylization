using UnityEditor;
using UnityEditor.SceneManagement;

namespace viva
{
    public class SceneLoaderEditor
    {
#if UNITY_EDITOR
        [MenuItem("Scenes/BETA")]
        public static void LoadBETA() { OpenScene("Assets/scenes/BETA.unity"); }
        [MenuItem("Scenes/canyon")]
        public static void Loadcanyon() { OpenScene("Assets/scenes/canyon.unity"); }
        [MenuItem("Scenes/Home")]
        public static void LoadHome() { OpenScene("Assets/scenes/Home.unity"); }
        private static void OpenScene(string scenePath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
#endif
    }
}