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
        ("walls", new Color(0.78f, 0.72f, 0.58f)),
        ("wallbase", new Color(0.55f, 0.48f, 0.38f)),
        ("roof1", new Color(0.42f, 0.26f, 0.18f)),
        ("roof2", new Color(0.32f, 0.20f, 0.15f)),
        ("ground", new Color(0.42f, 0.40f, 0.36f)),
        ("ground1", new Color(0.38f, 0.36f, 0.32f)),
        ("ground3", new Color(0.34f, 0.32f, 0.28f)),
        ("glass", new Color(0.55f, 0.72f, 0.82f, 0.45f)),
        ("window", new Color(0.25f, 0.32f, 0.38f)),
        ("fence", new Color(0.40f, 0.40f, 0.38f)),
        ("pipe2", new Color(0.55f, 0.35f, 0.18f)),
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
        ("Piel", new Color(0.333f, 0.596f, 0.125f)),
        ("Lengua", new Color(0.75f, 0.22f, 0.28f)),
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
        ReimportMikeAsLegacy();
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
