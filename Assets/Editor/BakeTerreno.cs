using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BakeTerreno
{
    const string MenuPath = "Monster Inc Run/Bake Terrain Into Scene";
    const string GameScenePath = "Assets/Scenes/Mike_Juego.unity";

    [MenuItem(MenuPath, false, 21)]
    public static void BakeDesdeMenu()
    {
        Bake(mostrarDialogos: true);
    }

    [MenuItem(MenuPath, true)]
    static bool PuedeBake()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    public static void BakeFromCommandLine()
    {
        Bake(mostrarDialogos: false);
    }

    static void Bake(bool mostrarDialogos)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (mostrarDialogos)
            {
                EditorUtility.DisplayDialog(
                    "Bake Terrain",
                    "Salí de Play. El bake se hace con Play apagado, en Mike_Juego.",
                    "OK");
            }
            return;
        }

        if (!AsegurarEscenaJuego(mostrarDialogos))
            return;

        if (AmbienteTerreno.HayTerrenoEnEscena())
        {
            if (mostrarDialogos
                && !EditorUtility.DisplayDialog(
                    "Bake Terrain",
                    "Ya hay un Terreno en la escena (Entorno/Terreno). ¿Lo reemplazo con el heightmap y splat originales?",
                    "Reemplazar",
                    "Cancelar"))
                return;
        }

        Undo.SetCurrentGroupName("Bake Terrain");
        int undo = Undo.GetCurrentGroup();

        var entornoPrevio = GameObject.Find(AmbienteTerreno.NombreRaizEntorno);
        var terrain = AmbienteTerreno.BakeTerrenoPersistente();
        if (entornoPrevio == null && terrain != null && terrain.transform.parent != null)
            Undo.RegisterCreatedObjectUndo(terrain.transform.parent.gameObject, "Bake Terrain");

        Undo.CollapseUndoOperations(undo);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        if (terrain != null)
        {
            Selection.activeGameObject = terrain.gameObject;
            EditorGUIUtility.PingObject(terrain.gameObject);
        }

        Debug.Log("Monster Inc Run: terreno horneado en Entorno/Terreno (TerrainData en Resources/Terrain).");

        if (!mostrarDialogos)
        {
            EditorSceneManager.SaveOpenScenes();
            return;
        }

        EditorUtility.DisplayDialog(
            "Bake Terrain",
            "Terreno creado en Entorno/Terreno.\n\n"
            + "1. Play OFF, escena Mike_Juego.\n"
            + "2. El terreno se ve y se puede editar en el Editor.\n"
            + "3. Ctrl+S (Mac: Cmd+S) para guardar la escena.\n\n"
            + "En Play el juego usa este terreno y no duplica. "
            + "Si lo borrás, vuelve a crearlo en runtime como antes.\n"
            + "Los árboles horneados siguen usando AlturaEn sobre este terreno.",
            "OK");
    }

    static bool AsegurarEscenaJuego(bool mostrarDialogos)
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path == GameScenePath)
            return true;

        if (mostrarDialogos
            && !EditorUtility.DisplayDialog(
                "Bake Terrain",
                "Hay que abrir Assets/Scenes/Mike_Juego.unity con Play apagado. ¿La abro ahora?",
                "Abrir Mike_Juego",
                "Cancelar"))
            return false;

        if (scene.isDirty)
        {
            if (mostrarDialogos)
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            else
                EditorSceneManager.SaveOpenScenes();
        }

        var abierta = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        return abierta.IsValid() && abierta.path == GameScenePath;
    }
}
