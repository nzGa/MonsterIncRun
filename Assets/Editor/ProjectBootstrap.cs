using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class ProjectBootstrap
{
    const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
    const string GameScenePath = "Assets/Scenes/Mike_Juego.unity";
    const string MikeFolder = "Assets/Resources/Models/Mike";
    const string MaterialsFolder = "Assets/Resources/Models/Materials";
    const string MikeMaterialsFolder = "Assets/Resources/Models/Mike/Materials";

    static readonly string[] RequiredScenes =
    {
        MainMenuPath,
        GameScenePath
    };

    static readonly (string name, Color color)[] PropMaterials =
    {
        ("No Name", Color.white),
        ("walls", new Color(0.86f, 0.84f, 0.80f)),
        ("wallbase", new Color(0.84f, 0.82f, 0.78f)),
        ("walltop", new Color(0.86f, 0.84f, 0.80f)),
        ("roof1", new Color(0.92f, 0.94f, 0.96f)),
        ("roof2", new Color(0.90f, 0.92f, 0.94f)),
        ("ground", new Color(0.42f, 0.40f, 0.36f)),
        ("ground1", new Color(0.38f, 0.36f, 0.32f)),
        ("ground3", new Color(0.34f, 0.32f, 0.28f)),
        ("glass", new Color(0.58f, 0.72f, 0.78f, 1f)),
        ("window", new Color(0.58f, 0.72f, 0.78f, 1f)),
        ("fence", new Color(0.85f, 0.85f, 0.86f)),
        ("pipe1", Color.white),
        ("pipe2", new Color(0.96f, 0.97f, 1f)),
        ("chim", new Color(0.92f, 0.94f, 0.96f)),
        ("15_verti", new Color(0.62f, 0.58f, 0.48f)),
        ("7cdred", new Color(0.72f, 0.16f, 0.14f)),
        ("7cdcolor", new Color(0.20f, 0.55f, 0.28f)),
        ("01 - Default", new Color(0.75f, 0.22f, 0.18f)),
        ("02 - Default", new Color(0.62f, 0.42f, 0.22f)),
        ("06 - Default", new Color(0.70f, 0.55f, 0.28f)),
        ("07 - Default", new Color(0.55f, 0.18f, 0.16f)),
        ("Material #37", new Color(0.95f, 0.82f, 0.12f)),
        ("Material #38", new Color(0.90f, 0.75f, 0.10f)),
        ("Puerta", new Color(0.48f, 0.30f, 0.16f)),
        ("metal", new Color(0.55f, 0.55f, 0.55f)),
        ("madera", new Color(0.52f, 0.34f, 0.18f)),
        ("logo", new Color(0.85f, 0.80f, 0.15f)),
        ("Rojo", new Color(0.78f, 0.12f, 0.12f)),
        ("Piso", new Color(0.38f, 0.36f, 0.32f))
    };

    static readonly (string name, Color color)[] MikeMaterials =
    {
        ("Piel", new Color(0.50f, 0.78f, 0.15f)),
        ("Lengua", new Color(0.86f, 0.12f, 0.20f)),
        ("Ojo", new Color(0.80f, 0.80f, 0.80f)),
        ("Paladar", new Color(0.65f, 0.20f, 0.22f)),
        ("Unias", new Color(0.85f, 0.82f, 0.70f)),
        ("Dientes", Color.white)
    };

    static ProjectBootstrap()
    {
        EditorApplication.delayCall += Bootstrap;
    }

    [MenuItem("Monster Inc Run/Bootstrap Project")]
    static void MenuBootstrap()
    {
        Bootstrap();
        Debug.Log("Monster Inc Run: bootstrap aplicado (build scenes, Play Mode, Mike Legacy, materiales).");
    }

    static void Bootstrap()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        EnsureBuildScenes();
        EnsurePlayModeStartScene();
        EnsureMaterialsBesideFbx();
        EnsurePipeMetalTextures();
        EnsureFactoryAlbedos();
        EnsureGlassMaterials();
        ReimportMikeAsLegacy();
        EnsureUiSprites();
        EnsureEnvironmentAssets();
    }

    static void EnsureEnvironmentAssets()
    {
        const string folder = "Assets/Resources/Environment";
        if (!AssetDatabase.IsValidFolder(folder))
            return;

        var texGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        foreach (var guid in texGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;

            bool hojas = path.IndexOf("BigTree.png", StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf("PalmBranch", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!hojas)
                continue;
            if (importer.alphaIsTransparency
                && importer.wrapMode == TextureWrapMode.Clamp
                && importer.mipmapEnabled)
                continue;

            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.mipMapsPreserveCoverage = true;
            importer.alphaTestReferenceValue = 0.5f;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }

        var modelGuids = AssetDatabase.FindAssets("t:Model", new[] { folder });
        foreach (var guid in modelGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
                continue;
            bool ok = importer.animationType == ModelImporterAnimationType.None
                && importer.indexFormat == ModelImporterIndexFormat.UInt32
                && importer.isReadable;
            if (ok)
                continue;
            importer.animationType = ModelImporterAnimationType.None;
            importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
            importer.indexFormat = ModelImporterIndexFormat.UInt32;
            importer.isReadable = true;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            importer.SaveAndReimport();
        }
    }

    static void EnsureUiSprites()
    {
        const string folder = "Assets/Resources/UI";
        if (!AssetDatabase.IsValidFolder(folder))
            return;

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;
            if (importer.textureType == TextureImporterType.Sprite
                && importer.spriteImportMode == SpriteImportMode.Single
                && !importer.mipmapEnabled
                && importer.npotScale == TextureImporterNPOTScale.None
                && importer.wrapMode == TextureWrapMode.Clamp)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
    }

    static void EnsureBuildScenes()
    {
        var current = EditorBuildSettings.scenes;
        if (ScenesAlreadyCorrect(current))
            return;

        var scenes = new EditorBuildSettingsScene[RequiredScenes.Length];
        for (int i = 0; i < RequiredScenes.Length; i++)
            scenes[i] = new EditorBuildSettingsScene(RequiredScenes[i], true);
        EditorBuildSettings.scenes = scenes;
    }

    static bool ScenesAlreadyCorrect(EditorBuildSettingsScene[] current)
    {
        if (current == null || current.Length != RequiredScenes.Length)
            return false;
        for (int i = 0; i < RequiredScenes.Length; i++)
        {
            if (!current[i].enabled || current[i].path != RequiredScenes[i])
                return false;
        }
        return true;
    }

    static void EnsurePlayModeStartScene()
    {
        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath);
        if (scene == null)
            return;
        if (EditorSceneManager.playModeStartScene != scene)
            EditorSceneManager.playModeStartScene = scene;
    }

    static void ReimportMikeAsLegacy()
    {
        if (!AssetDatabase.IsValidFolder(MikeFolder))
            return;

        var guids = AssetDatabase.FindAssets("", new[] { MikeFolder });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.IndexOf("Mike@", System.StringComparison.OrdinalIgnoreCase) < 0)
                continue;
            if (!path.EndsWith(".FBX", System.StringComparison.OrdinalIgnoreCase)
                && !path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                continue;

            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
                continue;
            if (importer.animationType == ModelImporterAnimationType.Legacy
                && importer.materialLocation == ModelImporterMaterialLocation.InPrefab)
                continue;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }

    static void EnsureMaterialsBesideFbx()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Models");
        EnsureFolder(MaterialsFolder);
        EnsureFolder(MikeFolder);
        EnsureFolder(MikeMaterialsFolder);

        foreach (var entry in PropMaterials)
            CreateDiffuseIfMissing(Path.Combine(MaterialsFolder, entry.name + ".mat").Replace('\\', '/'), entry.name, entry.color);
        foreach (var entry in MikeMaterials)
            CreateDiffuseIfMissing(Path.Combine(MikeMaterialsFolder, entry.name + ".mat").Replace('\\', '/'), entry.name, entry.color);
    }

    static void EnsurePipeMetalTextures()
    {
        var metalTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Textures/metal.jpg")
            ?? AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Textures/metal.jpg");

        foreach (var name in new[] { "pipe1", "pipe2", "fence", "metal" })
        {
            var path = Path.Combine(MaterialsFolder, name + ".mat").Replace('\\', '/');
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
                continue;

            AmbienteVisual.RepararShader(mat);
            bool dirty = false;
            bool esPipe = name.StartsWith("pipe", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "fence", StringComparison.OrdinalIgnoreCase);
            var std = Shader.Find("Standard");
            if (esPipe && std != null && mat.shader != std)
            {
                mat.shader = std;
                dirty = true;
            }

            if (metalTex != null && mat.HasProperty("_MainTex") && mat.mainTexture != metalTex)
            {
                mat.mainTexture = metalTex;
                dirty = true;
            }

            var tilePipe = new Vector2(5f, 2.5f);
            if (esPipe && mat.HasProperty("_MainTex") && mat.mainTextureScale != tilePipe)
            {
                mat.mainTextureScale = tilePipe;
                dirty = true;
            }

            var colorPipe = name == "pipe2"
                ? new Color(0.96f, 0.97f, 1f)
                : Color.white;
            if (esPipe && mat.HasProperty("_Color") && mat.color != colorPipe)
            {
                mat.color = colorPipe;
                dirty = true;
            }

            if (esPipe && mat.HasProperty("_Metallic") && !Mathf.Approximately(mat.GetFloat("_Metallic"), 0.58f))
            {
                mat.SetFloat("_Metallic", 0.58f);
                dirty = true;
            }

            if (esPipe && mat.HasProperty("_Glossiness") && !Mathf.Approximately(mat.GetFloat("_Glossiness"), 0.42f))
            {
                mat.SetFloat("_Glossiness", 0.42f);
                dirty = true;
            }

            if (esPipe && mat.HasProperty("_Smoothness") && !Mathf.Approximately(mat.GetFloat("_Smoothness"), 0.42f))
            {
                mat.SetFloat("_Smoothness", 0.42f);
                dirty = true;
            }

            if (esPipe && mat.HasProperty("_EmissionColor"))
            {
                mat.DisableKeyword("_EMISSION");
                if (mat.GetColor("_EmissionColor") != Color.black)
                {
                    mat.SetColor("_EmissionColor", Color.black);
                    dirty = true;
                }
            }

            if (dirty)
                EditorUtility.SetDirty(mat);
        }
    }

    static void EnsureFactoryAlbedos()
    {
        var concreto = "Assets/Art/Textures/floor_concrete.jpg";
        var galvanizado = "Assets/Art/Textures/corrugated_galvanized.png";
        AsignarAlbedo("walls", concreto, new Vector2(16f, 16f), new Color(0.86f, 0.84f, 0.80f));
        AsignarAlbedo("walltop", concreto, new Vector2(12f, 12f), new Color(0.86f, 0.84f, 0.80f));
        AsignarAlbedo("wallbase", concreto, new Vector2(10f, 10f), new Color(0.84f, 0.82f, 0.78f));
        AsignarAlbedo("ground", "Assets/Art/Textures/floor_adoquin.jpg", new Vector2(12f, 12f), Color.white);
        AsignarAlbedo("ground1", "Assets/Art/Textures/floor_hexagon.jpg", new Vector2(10f, 10f), Color.white);
        AsignarAlbedo("ground3", concreto, new Vector2(10f, 10f), Color.white);
        AsignarAlbedo("Piso", "Assets/Art/Textures/floor_adoquin.jpg", new Vector2(12f, 12f), Color.white);
        AsignarAlbedo("fence", "Assets/Art/Textures/metal.jpg", new Vector2(5f, 2.5f), Color.white);
        AsignarAlbedo("madera", "Assets/Art/Textures/madera.GIF", new Vector2(2.5f, 2.5f), Color.white);
        AsignarAlbedo("15_verti", concreto, new Vector2(14f, 14f), new Color(0.86f, 0.84f, 0.80f));
        AsignarAlbedo("chim", galvanizado, new Vector2(1.8f, 3.2f), new Color(0.92f, 0.94f, 0.96f));
        AsignarAlbedo("roof1", galvanizado, new Vector2(1.8f, 3.2f), new Color(0.92f, 0.94f, 0.96f));
        AsignarAlbedo("roof2", galvanizado, new Vector2(1.8f, 3.2f), new Color(0.90f, 0.92f, 0.94f));
    }

    static void EnsureGlassMaterials()
    {
        var tint = new Color(0.58f, 0.72f, 0.78f, 1f);
        const float metal = 0.62f;
        const float brillo = 0.78f;
        const int colaGeometria = 2000;
        var shader = Shader.Find("Standard")
            ?? Shader.Find("Legacy Shaders/Diffuse")
            ?? Shader.Find("Diffuse");

        foreach (var name in new[] { "glass", "window" })
        {
            var path = Path.Combine(MaterialsFolder, name + ".mat").Replace('\\', '/');
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
                continue;

            bool dirty = false;
            if (shader != null && mat.shader != shader)
            {
                mat.shader = shader;
                dirty = true;
            }
            if (mat.HasProperty("_MainTex") && mat.mainTexture != null)
            {
                mat.mainTexture = null;
                dirty = true;
            }
            if (mat.HasProperty("_Color") && mat.color != tint)
            {
                mat.color = tint;
                dirty = true;
            }

            mat.SetOverrideTag("RenderType", "Opaque");
            if (mat.HasProperty("_Mode") && !Mathf.Approximately(mat.GetFloat("_Mode"), 0f))
            {
                mat.SetFloat("_Mode", 0f);
                dirty = true;
            }
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            if (mat.renderQueue != colaGeometria)
            {
                mat.renderQueue = colaGeometria;
                dirty = true;
            }
            if (mat.HasProperty("_Metallic") && !Mathf.Approximately(mat.GetFloat("_Metallic"), metal))
            {
                mat.SetFloat("_Metallic", metal);
                dirty = true;
            }
            if (mat.HasProperty("_Glossiness") && !Mathf.Approximately(mat.GetFloat("_Glossiness"), brillo))
            {
                mat.SetFloat("_Glossiness", brillo);
                dirty = true;
            }
            if (mat.HasProperty("_Smoothness") && !Mathf.Approximately(mat.GetFloat("_Smoothness"), brillo))
            {
                mat.SetFloat("_Smoothness", brillo);
                dirty = true;
            }
            if (mat.HasProperty("_GlossyReflections"))
                mat.SetFloat("_GlossyReflections", 1f);
            if (dirty)
                EditorUtility.SetDirty(mat);
        }
    }

    static void AsignarAlbedo(string materialName, string texturePath, Vector2 tile, Color tint)
    {
        var path = Path.Combine(MaterialsFolder, materialName + ".mat").Replace('\\', '/');
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (mat == null || tex == null || !mat.HasProperty("_MainTex"))
            return;

        AmbienteVisual.RepararShader(mat);
        bool dirty = false;
        if (mat.mainTexture != tex)
        {
            mat.mainTexture = tex;
            dirty = true;
        }
        if (mat.mainTextureScale != tile)
        {
            mat.mainTextureScale = tile;
            dirty = true;
        }
        if (mat.HasProperty("_Color") && mat.color != tint)
        {
            mat.color = tint;
            dirty = true;
        }
        if (dirty)
            EditorUtility.SetDirty(mat);
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;
        var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        var name = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name))
            AssetDatabase.CreateFolder(parent, name);
    }

    static void CreateDiffuseIfMissing(string path, string name, Color color)
    {
        if (File.Exists(path) || AssetDatabase.LoadAssetAtPath<Material>(path) != null)
            return;

        var shader = Shader.Find("Legacy Shaders/Diffuse")
            ?? Shader.Find("Diffuse")
            ?? Shader.Find("Standard");
        if (shader == null)
            return;

        var mat = new Material(shader)
        {
            name = name,
            color = color
        };
        AssetDatabase.CreateAsset(mat, path);
    }
}
