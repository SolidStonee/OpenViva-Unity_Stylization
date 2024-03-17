using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProceduralWorlds.SceneOptimizer
{
    public static class PWEditorUtility
    {
        private static float[] EMPTY_CULLING_LAYERS = new float[32];
        public static void DeleteDirectory(string target_dir)
        {
            //"Assets/Unused" folder should exist before running this Method
            string[] unusedFolder = { target_dir };
            foreach (string asset in AssetDatabase.FindAssets("", unusedFolder))
            {
                string path = AssetDatabase.GUIDToAssetPath(asset);
                AssetDatabase.DeleteAsset(path);
            }
        }
        public static int CountDirectories(string folder, string match)
        {
            int count = 0;
            string fullPath = Path.GetFullPath(folder);
            DirectoryInfo[] directories = new DirectoryInfo(fullPath).GetDirectories();
            foreach (DirectoryInfo dir in directories)
            {
                if (dir.Name.Contains(match))
                    count++;
            }
            return count;
        }
        public static Mesh SaveMeshToDisk(string folderPath, Mesh sharedMesh)
        {
            if (sharedMesh == null)
                return null;
            string extension = ".asset";
            string assetName = sharedMesh.name;
            string filePath = $"{folderPath}/{sharedMesh.name}{extension}";
            string directory = Path.GetDirectoryName(filePath);
            if (directory == null)
                return null;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            Mesh asset = AssetDatabase.LoadAssetAtPath<Mesh>(filePath);
            if (asset != null)
                return asset;
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            sharedMesh.name = fileName;
            AssetDatabase.CreateAsset(sharedMesh, filePath);
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<Mesh>(filePath);
        }
        public static void RenderSceneCullingCamera(float objectDistance, float shadowDistance, Color objectColor, Color shadowColor)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null)
                return;
            Camera camera = sceneView.camera;
            if (sceneView.camera == null)
                return;
            Transform transform = camera.transform;
            Vector3 position = transform.position;
            DrawSpawnRange(position, objectDistance, objectColor);
            DrawSpawnRange(position, shadowDistance, shadowColor);
        }
        public static void GetEdgeHeight(Vector3 location, float defValue, out float edgeHeight)
        {
            edgeHeight = defValue;
            float spawnCheckOffset = 1000f;
            float maxDistance = Mathf.Max(spawnCheckOffset * 2f, 1000f);
            Vector3 origin = location + new Vector3(0f, spawnCheckOffset, 0f);
            Ray ray = new Ray(origin, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance))
                edgeHeight = hitInfo.point.y;
        }
        public static void DrawSpawnRange(Vector3 origin, float range, Color? outerColor = null, Color? innerColor = null)
        {
            Vector3 aboveOrigin = origin;
            // aboveOrigin.y += 5000f;
            Vector3[] outerNodes = null;
            Vector3[] innerNodes = null;
            Vector3 pivot = origin;
            Quaternion angle = Quaternion.Euler(0f, 0f, 0f);
            int res = Mathf.CeilToInt(24 + range * 0.1f);
            outerNodes = new Vector3[res];
            innerNodes = new Vector3[res];
            float step = 360f / (res - 1);
            float radius = range * 0.5f;
            float innerRadius = radius * 0.99f;
            for (int i = 0; i < outerNodes.Length; i++)
            {
                outerNodes[i] = new Vector3(Mathf.Sin(step * i * Mathf.Deg2Rad), 0f, Mathf.Cos(step * i * Mathf.Deg2Rad)) * radius + aboveOrigin;
                innerNodes[i] = new Vector3(Mathf.Sin(step * i * Mathf.Deg2Rad), 0f, Mathf.Cos(step * i * Mathf.Deg2Rad)) * innerRadius + aboveOrigin;
                float height = 0.0f;
                GetEdgeHeight(outerNodes[i], origin.y, out height);
                outerNodes[i].y = innerNodes[i].y = height;
            }
            if (outerNodes != null)
            {
                Handles.color = outerColor ?? Color.blue;
                Handles.DrawAAPolyLine(6f, outerNodes);
            }
            if (innerNodes != null)
            {
                Handles.color = innerColor ?? Color.white;
                Handles.DrawAAPolyLine(1f, innerNodes);
            }
            // // We only got here if the mouse is over the sceneview - also only update if there was more than tiny movement of the mouse
            // if ((m_lastMousePos - mousePos).sqrMagnitude > 4f)
            // {
            //     m_lastMousePos = mousePos;
            // SceneView.lastActiveSceneView.Repaint();
            // }
        }
        public static void RenderSceneCullingCamera(int layer, float objectDistance, float shadowDistance, bool allLayers = false)
        {
            if (layer < 0 || layer >= 32)
                return;
            float[] objectLayerCullingDistances = new float[32];
            if (allLayers)
            {
                for (int i = 0; i < objectLayerCullingDistances.Length; i++)
                    objectLayerCullingDistances[i] = objectDistance;
            }
            else
                objectLayerCullingDistances[layer] = objectDistance;
            float[] shadowLayerCullingDistances = new float[32];
            if (allLayers)
            {
                for (int i = 0; i < shadowLayerCullingDistances.Length; i++)
                    shadowLayerCullingDistances[i] = shadowDistance;
            }
            else
                shadowLayerCullingDistances[layer] = shadowDistance;
            RenderSceneCullingCamera(objectLayerCullingDistances, shadowLayerCullingDistances);
        }
        public static void RenderSceneCullingCamera(float[] objectLayerCullingDistances, float[] shadowLayerCullingDistances)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                Camera camera = sceneView.camera;
                if (camera != null)
                {
                    camera.layerCullDistances = objectLayerCullingDistances;
                }
                if (sceneView.sceneLighting)
                {
                    Light[] lights = Object.FindObjectsOfType<Light>();
                    Light directionalLight = PWUtility.GetDirectionalLight(lights);
                    if (directionalLight != null)
                    {
                        directionalLight.layerShadowCullDistances = shadowLayerCullingDistances;
                    }
                }
            }
        }
        public static void ClearSceneCullingCamera()
        {
            RenderSceneCullingCamera(EMPTY_CULLING_LAYERS, EMPTY_CULLING_LAYERS);
        }
    }
}