using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace ProceduralWorlds.SceneOptimizer
{
    [InitializeOnLoad]
    public static class EditorEvents
    {
        #region Variables
        public static Action<string> onImportPackageCompleted;
        public static Action<string> onImportPackageCancelled;
        public static Action<string, string> onImportPackageFailed;
        public static Action onHeierarchyChanged;
        public static Action onEditorUpdate;
        public static Action onBeforeAssemblyReloads;
        public static Action onAfterAssemblyReloads;
        #endregion
        #region Constructors
        static EditorEvents()
        {
            // On Import Package Completed
            AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
            // On Import Package Cancelled
            AssetDatabase.importPackageCancelled -= OnImportPackageCancelled;
            AssetDatabase.importPackageCancelled += OnImportPackageCancelled;
            // On Import Package Failed
            AssetDatabase.importPackageFailed -= OnImportPackageFailed;
            AssetDatabase.importPackageFailed += OnImportPackageFailed;
            // On Before Assembly Reloads
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReloads;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReloads;
            // On After Assembly Reloads
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReloads;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReloads;
            // On Editor Update
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            // On Hierarchy Changed
            EditorApplication.hierarchyChanged -= OnHeierarchyChanged;
            EditorApplication.hierarchyChanged += OnHeierarchyChanged;
            // Initialize();
            SubscribeEvents();
        }
        #endregion
        #region Methods
        private static void SubscribeEvents()
        {
            PWEvents.Destroy = EditorDestroy;
            PWEvents.DisplayProgress = EditorDisplayProgress;
            PWEvents.DisplayCancelableProgress = EditorDisplayCancelableProgress;
            PWEvents.ClearProgressBar = EditorClearProgressBar;
            PWEvents.SaveMeshToDisk = EditorSaveMeshToDisk;
            PWEvents.Instantiate = EditorInstantiate;
        }
        private static void OnImportPackageCompleted(string packageName) => onImportPackageCompleted?.Invoke(packageName);
        /// <summary>
        /// Called when a package import is Cancelled.
        /// </summary>
        private static void OnImportPackageCancelled(string packageName) => onImportPackageCancelled?.Invoke(packageName);
        /// <summary>
        /// Called when a package import fails.
        /// </summary>
        private static void OnImportPackageFailed(string packageName, string error) => onImportPackageFailed?.Invoke(packageName, error);
        /// <summary>
        /// Called Before Assembly Reloads
        /// </summary>
        private static void OnBeforeAssemblyReloads()
        {
            onBeforeAssemblyReloads?.Invoke();
            // GeNaFactory.Dispose();
        }
        /// <summary>
        /// Called After Assembly Reloads
        /// </summary> 
        private static void OnAfterAssemblyReloads() => onAfterAssemblyReloads?.Invoke();
        /// <summary>
        /// Called when Editor Updates
        /// </summary>
        private static void OnEditorUpdate() => onEditorUpdate?.Invoke();
        /// <summary>
        /// Event that is raised when an object or group of objects in the hierarchy changes.
        /// </summary>
        private static void OnHeierarchyChanged() => onHeierarchyChanged?.Invoke();
        private static void EditorDestroy(Object @object) => Object.DestroyImmediate(@object);
        private static void EditorDisplayProgress(string title, string info, float progress) => EditorUtility.DisplayProgressBar(title, info, progress);
        private static bool EditorDisplayCancelableProgress(string title, string info, float progress) => EditorUtility.DisplayCancelableProgressBar(title, info, progress);
        private static void EditorClearProgressBar() => EditorUtility.ClearProgressBar();
        private static Mesh EditorSaveMeshToDisk(Scene scene, Mesh sharedMesh)
        {
            string meshPath = $"{Constants.TEMP_DIRECTORY}/{scene.name}";
            string filePath = $"{meshPath}/{sharedMesh.name}.asset";
            string directory = Path.GetDirectoryName(filePath);
            if (directory != null)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
            sharedMesh.name = Path.GetFileNameWithoutExtension(filePath);
            AssetDatabase.CreateAsset(sharedMesh, filePath);
            return AssetDatabase.LoadAssetAtPath<Mesh>(filePath);
        }
        private static GameObject EditorInstantiate(GameObject gameObject, Transform parent, bool worldPositionStays)
        {
            GameObject instance = Object.Instantiate(gameObject, parent, worldPositionStays);
            instance.name = gameObject.name;
            return instance;
        }
        #endregion
    }
}