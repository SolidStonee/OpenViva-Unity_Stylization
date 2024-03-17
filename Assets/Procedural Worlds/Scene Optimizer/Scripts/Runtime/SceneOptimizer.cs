using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
#if GAIA_PRO_PRESENT
using Gaia;
#endif
namespace ProceduralWorlds.SceneOptimizer
{
    [CreateAssetMenu(fileName = "Scene Optimizer", menuName = "Procedural Worlds/Scene Optimizer")]
    public class SceneOptimizer : SceneOptimizerInternal
    {
#if GAIA_PRO_PRESENT
        public override bool IsGaiaTerrainLoadedScene() => GaiaUtils.HasDynamicLoadedTerrains();
#endif
        public override void ProcessSceneOptimization(OptimizeCall optimizeCall, bool useGaia, bool recordUndo)
        {
            if (useGaia)
            {
#if GAIA_PRO_PRESENT
                optimizeCall.isGaiaLoadedTerrain = true;
                System.Action<Terrain> act = (t) =>
                {
                    GameObject gameObject = t.gameObject;
                    GameObjectEntry entry = new GameObjectEntry
                    {
                        Enabled = true,
                        DisableChildren = true,
                        GameObject = gameObject
                    };
                    optimizeCall.optimizedRoots.Clear();
                    optimizeCall.rootObjects.Add(entry);
                    RevertOptimization();
                    ProcessSceneOptimization(optimizeCall);
                    PostProcessSceneOptimization(optimizeCall);
                    optimizeCall.rootObjects.Remove(entry);
                };
                GaiaUtils.CallFunctionOnDynamicLoadedTerrains(act, true);
#endif
            }
            else
            {
                RevertOptimization();
                ProcessSceneOptimization(optimizeCall);
                PostProcessSceneOptimization(optimizeCall);
            }
        }
        public List<Scene> GetUniqueScenes(List<GameObject> gameObjects)
        {
            List<Scene> scenes = new List<Scene>();
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject == null)
                    continue;
                Scene scene = gameObject.scene;
                if (scenes.Contains(scene))
                    continue;
                scenes.Add(scene);
            }
            return scenes;
        }
        public void PostProcessSceneOptimization(OptimizeCall optimizeCall, bool recordUndo = true)
        {
            List<GameObject> optimizedRoots = optimizeCall.optimizedRoots;
#if UNITY_EDITOR
            if (optimizedRoots.Count > 0)
            {
                if (recordUndo)
                {
                    foreach (GameObject optimizedRoot in optimizedRoots)
                    {
                        Undo.RegisterCreatedObjectUndo(optimizedRoot, "Optimized");
                    }
                }
                List<Scene> uniqueScenes = GetUniqueScenes(optimizedRoots);
                foreach (Scene scene in uniqueScenes)
                {
                    // Mark this Scene as Dirty
                    EditorSceneManager.MarkSceneDirty(scene);
                }
                if (Tools.SaveToDisk)
                {
                    try
                    {
                        AssetDatabase.StartAssetEditing();
                        foreach (Scene scene in uniqueScenes)
                        {
                            string fullPath = $"{Constants.TEMP_DIRECTORY}/{scene.name}";
                            if (Directory.Exists(fullPath))
                                FileUtil.DeleteFileOrDirectory($"{fullPath}");
                        }
                        foreach (GameObject rootObject in optimizedRoots)
                        {
                            if (rootObject == null)
                                continue;
                            Scene scene = rootObject.scene;
                            MeshFilter[] meshFilters = rootObject.GetComponentsInChildren<MeshFilter>();
                            MeshCollider[] meshColliders = rootObject.GetComponentsInChildren<MeshCollider>();
                            foreach (MeshFilter meshFilter in meshFilters)
                                meshFilter.sharedMesh = PWEvents.SaveMeshToDisk(scene, meshFilter.sharedMesh);
                            foreach (MeshCollider meshCollider in meshColliders)
                                meshCollider.sharedMesh = PWEvents.SaveMeshToDisk(scene, meshCollider.sharedMesh);
                        }
                    }
                    finally
                    {
                        AssetDatabase.StopAssetEditing();
                        AssetDatabase.SaveAssets();
                    }
                }
                else
                {
                    foreach (GameObject rootObject in optimizedRoots)
                    {
                        if (rootObject == null)
                            continue;
                        MeshFilter[] meshFilters = rootObject.GetComponentsInChildren<MeshFilter>();
                        MeshCollider[] meshColliders = rootObject.GetComponentsInChildren<MeshCollider>();
                        foreach (MeshFilter meshFilter in meshFilters)
                        {
                            Mesh sharedMesh = meshFilter.sharedMesh;
                            if (sharedMesh == null)
                                continue;
                            string filePath = sharedMesh.name;
                            sharedMesh.name = Path.GetFileNameWithoutExtension(filePath);
                        }
                        foreach (MeshCollider meshCollider in meshColliders)
                        {
                            Mesh sharedMesh = meshCollider.sharedMesh;
                            if (sharedMesh == null)
                                continue;
                            string filePath = sharedMesh.name;
                            sharedMesh.name = Path.GetFileNameWithoutExtension(filePath);
                        }
                    }
                }
                if (Tools.DebugPerformance)
                {
                    GameObject fpsTesterPrefab = Tools.FpsTesterPrefab;
                    if (fpsTesterPrefab != null)
                    {
                        // Check if an instance already exists
                        FPSTester fpsTesterInstance = FindObjectOfType<FPSTester>();
                        if (fpsTesterInstance == null)
                        {
                            // Create Instance of FPS Tester
                            PrefabUtility.InstantiatePrefab(fpsTesterPrefab);
                        }
                    }
                    else
                    {
                        PWDebug.LogWarning("FPS Tester Prefab is Missing!");
                    }
                }
                
                // Parent Remaining to Optimized
                // Parent all the remaining parents last
                foreach (var pair in optimizeCall.remainingParents)
                {
                    var sceneData = pair.Key;
                    if (sceneData == null)
                        continue;
                    var parent = pair.Value;
                    if (parent == null)
                        continue;
                    parent.transform.SetParent(sceneData.transform);
                }
                
                // Selection.objects = optimizedRoots.ToArray();
                AssetDatabase.SaveAssets();
                // m_canRevert = true;
            }
#endif
        }
        public override CullingSystemInternal GetCullingSystem()
        {
            CullingSystem cullingSystem = FindObjectOfType<CullingSystem>();
            if (cullingSystem == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    GameObject cameraObject = mainCamera.gameObject;
                    cullingSystem = cameraObject.GetComponent<CullingSystem>();
                    if (cullingSystem == null)
                    {
                        cullingSystem = cameraObject.AddComponent<CullingSystem>();
                    }
                }
            }
            return cullingSystem;
        }
        public static void RevertOptimization()
        {
            
        }
        /// <summary>
        /// Reverts the last Optimization
        /// Note: This operation destroys Optimized GameObjects and cannot be undone!
        /// </summary>
        public static void RevertOptimization(Scene scene)
        {
           
        }
        public static void Cleanup()
        {
            
        }
    }
}