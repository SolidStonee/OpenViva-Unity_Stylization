using UnityEngine;
using UnityEditor;

public class DDSImportProcessor : AssetPostprocessor {
    void OnPreprocessTexture() {
        if (assetPath.ToLower().EndsWith(".dds")) {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.textureType = TextureImporterType.NormalMap;
        }
    }
}
