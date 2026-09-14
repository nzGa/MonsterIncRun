using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FactoryModelImport : AssetPostprocessor
{
    const string ModelsFolder = "Assets/Resources/Models/";
    const string EnvironmentFolder = "Assets/Resources/Environment/";
    const string MaterialsFolder = "Assets/Resources/Models/Materials";

    void OnPreprocessModel()
    {
        if (!EsModeloDeProps(assetPath) && !EsModeloDeEntorno(assetPath))
            return;

        var importer = (ModelImporter)assetImporter;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        importer.materialSearch = ModelImporterMaterialSearch.Local;
        importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
        importer.searchTexturesGlobally = false;
        importer.animationType = ModelImporterAnimationType.None;
        importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
        if (EsModeloDeEntorno(assetPath))
        {
            importer.indexFormat = ModelImporterIndexFormat.UInt32;
            importer.isReadable = true;
        }
    }

    Material OnAssignMaterialModel(Material material, Renderer renderer)
    {
        if (!EsModeloDeProps(assetPath) || material == null)
            return null;

        var existente = BuscarMaterial(material.name);
        if (existente != null)
        {
            AsegurarTexturaTuberiaImport(existente);
            return existente;
        }

        AsegurarTexturaTuberiaImport(material);
        return material;
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
                if (existente != null && existente != actual)
                {
                    shared[i] = existente;
                    actual = existente;
                    changed = true;
                }

                AsegurarTexturaTuberiaImport(actual);
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

    static bool EsModeloDeEntorno(string path)
    {
        path = path.Replace('\\', '/');
        return path.StartsWith(EnvironmentFolder, StringComparison.OrdinalIgnoreCase);
    }

    static Material BuscarMaterial(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        var encontrado = CargarMaterial(name);
        if (encontrado != null)
            return encontrado;

        int colon = name.IndexOf(':');
        if (colon > 0)
        {
            encontrado = CargarMaterial(name.Substring(0, colon));
            if (encontrado != null)
                return encontrado;
        }

        if (name.IndexOf("pipe2", StringComparison.OrdinalIgnoreCase) >= 0)
            return CargarMaterial("pipe2");
        if (name.IndexOf("pipe", StringComparison.OrdinalIgnoreCase) >= 0)
            return CargarMaterial("pipe1") ?? CargarMaterial("metal");
        if (string.Equals(name, "metal", StringComparison.OrdinalIgnoreCase))
            return CargarMaterial("metal");

        return null;
    }

    static Material CargarMaterial(string name)
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

    static void AsegurarTexturaTuberiaImport(Material mat)
    {
        if (mat == null || string.IsNullOrEmpty(mat.name))
            return;

        var nombre = mat.name;
        var metalTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Textures/metal.jpg")
            ?? AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Textures/metal.jpg");
        var galv = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Textures/corrugated_galvanized.png")
            ?? AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Textures/corrugated_galvanized.png");

        bool esPipe = nombre.IndexOf("pipe", StringComparison.OrdinalIgnoreCase) >= 0
            || string.Equals(nombre, "fence", StringComparison.OrdinalIgnoreCase);
        bool esMetal = string.Equals(nombre, "metal", StringComparison.OrdinalIgnoreCase);
        bool esChim = nombre.IndexOf("chim", StringComparison.OrdinalIgnoreCase) >= 0
            || nombre.IndexOf("roof", StringComparison.OrdinalIgnoreCase) >= 0;
        bool esVidrio = nombre.IndexOf("glass", StringComparison.OrdinalIgnoreCase) >= 0
            || nombre.IndexOf("window", StringComparison.OrdinalIgnoreCase) >= 0;
        if (!esPipe && !esMetal && !esChim && !esVidrio)
            return;

        if (esVidrio)
        {
            var glassShader = Shader.Find("Standard")
                ?? Shader.Find("Legacy Shaders/Diffuse")
                ?? Shader.Find("Diffuse");
            if (glassShader != null)
                mat.shader = glassShader;
            if (mat.HasProperty("_MainTex"))
                mat.mainTexture = null;
            if (mat.HasProperty("_Color"))
                mat.color = new Color(0.58f, 0.72f, 0.78f, 1f);
            mat.SetOverrideTag("RenderType", "Opaque");
            if (mat.HasProperty("_Mode"))
                mat.SetFloat("_Mode", 0f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 2000;
            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", 0.88f);
            if (mat.HasProperty("_Glossiness"))
                mat.SetFloat("_Glossiness", 0.95f);
            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", 0.95f);
            if (mat.HasProperty("_GlossyReflections"))
                mat.SetFloat("_GlossyReflections", 1f);
            EditorUtility.SetDirty(mat);
            return;
        }

        var tex = esChim ? galv : metalTex;
        var std = Shader.Find("Standard");
        if ((esPipe || esChim) && std != null)
            mat.shader = std;

        if (tex != null && mat.HasProperty("_MainTex"))
        {
            mat.mainTexture = tex;
            if (esPipe)
                mat.mainTextureScale = new Vector2(5f, 2.5f);
            if (esChim)
                mat.mainTextureScale = new Vector2(1.8f, 3.2f);
        }

        if (esPipe && mat.HasProperty("_Color"))
            mat.color = nombre.IndexOf("pipe2", StringComparison.OrdinalIgnoreCase) >= 0
                ? new Color(0.96f, 0.97f, 1f)
                : Color.white;

        if (esChim && mat.HasProperty("_Color"))
            mat.color = new Color(0.92f, 0.94f, 0.96f);

        if (esPipe && mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", 0.58f);

        if (esPipe && mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", 0.42f);

        if (esPipe && mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", 0.42f);

        if (esChim && mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", 0.72f);

        if (esChim && mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", 0.48f);

        if (esChim && mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", 0.48f);

        EditorUtility.SetDirty(mat);
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
                if (importer == null)
                    continue;
                bool ok = importer.materialLocation == ModelImporterMaterialLocation.InPrefab
                    && importer.animationType == ModelImporterAnimationType.None;
                if (ok)
                    continue;

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}
