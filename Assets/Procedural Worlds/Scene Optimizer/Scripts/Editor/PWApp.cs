// Copyright © 2018 Procedural Worlds Pty Limited.  All Rights Reserved.
using UnityEditor;
using UnityEngine;
using PWCommon5;

namespace ProceduralWorlds.SceneOptimizer
{
    [InitializeOnLoad]
    public static class PWApp
    {
        public const string CONF_NAME = "Scene Optimizer";

        private static AppConfig m_conf; 
        public static AppConfig CONF
        {
            get
            {
                if (m_conf != null)
                {
                    return m_conf;
                }

                m_conf = AssetUtils.GetConfig(CONF_NAME);
                if (m_conf != null)
                {
                    Prod.Checkin(m_conf);
                }

                return m_conf;
            }
        }

        static PWApp()
        {
            // Need to wait for things to import before creating the common menu - Using delegates and only check menu when something gets imported
            AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;

            AssetDatabase.importPackageCancelled -= OnImportPackageCancelled;
            AssetDatabase.importPackageCancelled += OnImportPackageCancelled;

            AssetDatabase.importPackageFailed -= OnImportPackageFailed;
            AssetDatabase.importPackageFailed += OnImportPackageFailed;

            m_conf = AssetUtils.GetConfig(CONF_NAME, true);

            //In case it was a script only import: let's check-in.
            if (m_conf != null)
            {
                Prod.Checkin(m_conf);
            }
        }

        /// <summary>
        /// Called when a package import is Completed.
        /// </summary>
        private static void OnImportPackageCompleted(string packageName)
        {
#if PW_DEBUG
            Debug.LogFormat("[PWApp]: '{0}' Import Completed", packageName);
#endif
            OnPackageImport();
        }

        /// <summary>
        /// Called when a package import is Cancelled.
        /// </summary>
        private static void OnImportPackageCancelled(string packageName)
        {
            Debug.LogWarningFormat("[PWApp]: '{0}' Import was Cancelled.", packageName);
            OnPackageImport();
        }

        /// <summary>
        /// Called when a package import fails.
        /// </summary>
        private static void OnImportPackageFailed(string packageName, string error)
        {
            Debug.LogErrorFormat("[PWApp]: '{0}' Import Failed with error message: '{1}'.", packageName, error);
            OnPackageImport();
        }

        /// <summary>
        /// Used to run things after a package was imported.
        /// </summary>
        private static void OnPackageImport()
        {
            if (m_conf == null)
            {
                m_conf = AssetUtils.GetConfig(CONF_NAME);
            }
            Prod.Checkin(m_conf);

            // No need for these anymore
            AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
            AssetDatabase.importPackageCancelled -= OnImportPackageCancelled;
            AssetDatabase.importPackageFailed -= OnImportPackageFailed;
        }
        
        /// <summary>
        /// Get an editor utils object that can be used for common Editor stuff - DO make sure to Dispose() the instance.
        /// </summary>
        /// <param name="editorObj">The class that uses the utils. Just pass in "this".</param>
        /// <param name="classNameOverride"></param>
        /// <param name="customUpdateMethod">(Optional) The method to be called when the GUI needs to be updated. (Repaint will always be called.)</param>
        /// <returns>Editor Utils</returns>
        public static EditorUtils GetEditorUtils(IPWEditor editorObj, string classNameOverride, System.Action customUpdateMethod = null) => new EditorUtils(CONF, editorObj, classNameOverride, customUpdateMethod);
        
        /// <summary>
        /// Get an editor utils object that can be used for common Editor stuff - DO make sure to Dispose() the instance.
        /// </summary>
        /// <param name="editorObj">The class that uses the utils. Just pass in "this".</param>
        /// <param name="customUpdateMethod">(Optional) The method to be called when the GUI needs to be updated. (Repaint will always be called.)</param>
        /// <param name="customNewsURL">(Optional) A custom URL to fetch the news messages from. Will default to the news URL in the app config if none provided.</param>
        /// <param name="overrideParameters">A custom set of URL Parameters to use when fetching news data. If left empty, the default set of parameters will be used</param>
        /// <returns>Editor Utils</returns>
        public static EditorUtils GetEditorUtils(IPWEditor editorObj, System.Action customUpdateMethod = null, string customNewsURL = null, URLParameters overrideParameters = null)
        {
            return new EditorUtils(CONF, editorObj, null, customUpdateMethod, customNewsURL, overrideParameters);
        }
    }
}
