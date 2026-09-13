using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FactoryModelImport : AssetPostprocessor
{
    const string ModelsFolder = "Assets/Resources/Models/";
    const string MaterialsFolder = "Assets/Resources/Models/Materials";

    void OnPreprocessModel()
    {
        if (!EsModeloDeProps(assetPath))
            return;

        var importer = (ModelImporter)assetImporter;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        importer.materialSearch = ModelImporterMaterialSearch.Local;
        importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
        importer.searchTexturesGlobally = false;
    }

    Material OnAssignMaterialModel(Material material, Renderer renderer)
    {
        if (!EsModeloDeProps(assetPath) || material == null)
            return null;

        var existente = BuscarMaterial(material.name);
        return existente != null ? existente : material;
    }

    void OnPostprocessModel(GameObject root)
    {
        if (!EsModeloDeProps(assetPath) || root == null)
            return;

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            var shared = renderer.sharedMaterials;
            if (shared == null)
                continue;

            bool changed = false;
            for (int i = 0; i < shared.Length; i++)
            {
                var actual = shared[i];
                if (actual == null)
                    continue;
                var existente = BuscarMaterial(actual.name);
                if (existente == null || existente == actual)
                    continue;
                shared[i] = existente;
                changed = true;
            }

            if (changed)
                renderer.sharedMaterials = shared;
        }
    }

    static bool EsModeloDeProps(string path)
    {
        path = path.Replace('\\', '/');
        if (!path.StartsWith(ModelsFolder, StringComparison.OrdinalIgnoreCase))
            return false;
        return path.IndexOf("Mike@", StringComparison.OrdinalIgnoreCase) < 0;
    }

    static Material BuscarMaterial(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        var directo = AssetDatabase.LoadAssetAtPath<Material>(Path.Combine(MaterialsFolder, name + ".mat").Replace('\\', '/'));
        if (directo != null)
            return directo;

        var guids = AssetDatabase.FindAssets(name + " t:Material", new[] { MaterialsFolder });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && string.Equals(mat.name, name, StringComparison.OrdinalIgnoreCase))
                return mat;
        }

        return null;
    }

    [InitializeOnLoad]
    static class ReimportMaterialLocation
    {
        static ReimportMaterialLocation()
        {
            EditorApplication.delayCall += RunOnce;
        }

        static void RunOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (!AssetDatabase.IsValidFolder(ModelsFolder.TrimEnd('/')))
                return;

            var guids = AssetDatabase.FindAssets("", new[] { ModelsFolder.TrimEnd('/') });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!EsModeloDeProps(path))
                    continue;
                if (!path.EndsWith(".FBX", StringComparison.OrdinalIgnoreCase)
                    && !path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                    continue;

                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null || importer.materialLocation == ModelImporterMaterialLocation.InPrefab)
                    continue;

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}
