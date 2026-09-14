using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class LegacyMikeImport : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        if (!EsTakeMike(assetPath))
            return;

        var importer = (ModelImporter)assetImporter;
        importer.animationType = ModelImporterAnimationType.Legacy;
        importer.importAnimation = true;
        importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
        importer.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        importer.materialSearch = ModelImporterMaterialSearch.Local;
        importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
        importer.searchTexturesGlobally = false;

        var clipName = NombreClip(assetPath);
        if (string.IsNullOrEmpty(clipName))
            return;

        var clips = importer.defaultClipAnimations;
        if (clips == null || clips.Length == 0)
            clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
            return;

        bool loop = EsLoop(clipName);
        foreach (var clip in clips)
        {
            clip.name = clipName;
            clip.loopTime = loop;
            clip.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
        }
        importer.clipAnimations = clips;
    }

    Material OnAssignMaterialModel(Material material, Renderer renderer)
    {
        if (!EsTakeMike(assetPath) || material == null)
            return null;

        var path = "Assets/Resources/Models/Mike/Materials/" + material.name + ".mat";
        var existente = AssetDatabase.LoadAssetAtPath<Material>(path);
        return existente != null ? existente : material;
    }

    void OnPostprocessModel(GameObject root)
    {
        if (!EsTakeMike(assetPath))
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
                var path = "Assets/Resources/Models/Mike/Materials/" + actual.name + ".mat";
                var existente = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (existente != null && existente != actual)
                {
                    shared[i] = existente;
                    changed = true;
                }
            }

            if (changed)
                renderer.sharedMaterials = shared;
        }

        ReconectarMateriales.EnMike(root);

        var clipName = NombreClip(assetPath);
        var anim = root.GetComponent<Animation>() ?? root.GetComponentInChildren<Animation>();
        if (anim == null)
            anim = root.AddComponent<Animation>();

        if (string.IsNullOrEmpty(clipName))
            return;

        AnimationClip first = null;
        foreach (AnimationState state in anim)
        {
            if (state.clip == null)
                continue;
            first = state.clip;
            break;
        }

        if (first != null && anim.GetClip(clipName) == null)
            anim.AddClip(first, clipName);
    }

    static bool EsTakeMike(string path)
    {
        return path.IndexOf("Mike@", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static string NombreClip(string path)
    {
        var file = Path.GetFileNameWithoutExtension(path);
        int at = file.LastIndexOf('@');
        if (at < 0 || at + 1 >= file.Length)
            return null;
        return file.Substring(at + 1);
    }

    static bool EsLoop(string clipName)
    {
        return clipName.Equals("Espera", StringComparison.OrdinalIgnoreCase)
            || clipName.Equals("Camina", StringComparison.OrdinalIgnoreCase)
            || clipName.Equals("Corre", StringComparison.OrdinalIgnoreCase);
    }

    [InitializeOnLoad]
    static class ReimportMaterialLocation
    {
        const string MikeFolder = "Assets/Resources/Models/Mike";

        static ReimportMaterialLocation()
        {
            EditorApplication.delayCall += RunOnce;
        }

        static void RunOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (!AssetDatabase.IsValidFolder(MikeFolder))
                return;

            var guids = AssetDatabase.FindAssets("", new[] { MikeFolder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.IndexOf("Mike@", StringComparison.OrdinalIgnoreCase) < 0)
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
