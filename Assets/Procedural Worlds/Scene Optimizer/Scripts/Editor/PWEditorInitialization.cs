using UnityEditor;
namespace ProceduralWorlds.SceneOptimizer
{
    [InitializeOnLoad]
    public class PWEditorInitialization
    {
        static PWEditorInitialization()
        {
            SetupSymbolDefinitions();
        }
        public static void SetupSymbolDefinitions()
        {
            bool updateDefines = false;
            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            // #region G3
            // if (defineSymbols.Contains("G3_USING_UNITY"))
            // {
            //     updateDefines = true;
            //     defineSymbols = defineSymbols.Replace("G3_USING_UNITY", "");
            // }
            // if (!defineSymbols.Contains("G3_USING_UNITY"))
            // {
            //     updateDefines = true;
            //     defineSymbols += ";G3_USING_UNITY";
            // }
            // #endregion
            if (updateDefines)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbols);
            }
        }
    }
}