using UnityEditor;

public class LegacyMikeImport : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        var importer = (ModelImporter)assetImporter;
        if (assetPath.Contains("Mike@") || assetPath.Contains("/Mike/"))
            importer.animationType = ModelImporterAnimationType.Legacy;
    }
}
