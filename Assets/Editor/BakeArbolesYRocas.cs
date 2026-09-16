using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BakeArbolesYRocas
{
    const string MenuPath = "Monster Inc Run/Bake Trees And Rocks Into Scene";
    const string GameScenePath = "Assets/Scenes/Mike_Juego.unity";

    [MenuItem(MenuPath, false, 20)]
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
                    "Bake Trees And Rocks",
                    "Salí de Play. El bake se hace con Play apagado, en Mike_Juego.",
                    "OK");
            }
            return;
        }

        if (!AsegurarEscenaJuego(mostrarDialogos))
            return;

        Undo.SetCurrentGroupName("Bake Trees And Rocks");
        int undo = Undo.GetCurrentGroup();

        var entornoPrevio = GameObject.Find(AmbienteTerreno.NombreRaizEntorno);
        var padrePrevio = AmbienteTerreno.BuscarPadreVegetacion();
        var padre = AmbienteTerreno.AsegurarPadreVegetacion();
        if (entornoPrevio == null)
            Undo.RegisterCreatedObjectUndo(padre.root.gameObject, "Bake Trees And Rocks");
        else if (padrePrevio == null)
            Undo.RegisterCreatedObjectUndo(padre.gameObject, "Bake Trees And Rocks");

        if (padre.childCount > 0)
        {
            if (mostrarDialogos
                && !EditorUtility.DisplayDialog(
                    "Bake Trees And Rocks",
                    "Entorno/ArbolesYRocas ya tiene " + padre.childCount
                    + " objetos. ¿Los reemplazo por las posiciones originales?",
                    "Reemplazar",
                    "Cancelar"))
                return;

            for (int i = padre.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(padre.GetChild(i).gameObject);
        }

        AmbienteTerreno.AsegurarMaterialesPersistentes();
        int n = AmbienteTerreno.PoblarVegetacionConAltura(padre);
        Undo.CollapseUndoOperations(undo);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeTransform = padre;
        EditorGUIUtility.PingObject(padre.gameObject);

        Debug.Log("Monster Inc Run: " + n + " árboles/rocas en Entorno/ArbolesYRocas.");

        if (!mostrarDialogos)
            return;

        EditorUtility.DisplayDialog(
            "Bake Trees And Rocks",
            "Se crearon " + n + " objetos en Entorno/ArbolesYRocas.\n\n"
            + "1. Play OFF, escena Mike_Juego (ya estás ahí).\n"
            + "2. Seleccioná un árbol o roca y movelo con W.\n"
            + "3. Ctrl+S (Mac: Cmd+S) para guardar la escena. Queda para siempre.\n\n"
            + "En Play el juego usa estos objetos y no duplica. "
            + "Si vaciás el padre, vuelve a spawnear desde originalTrees.bytes.",
            "OK");
    }

    static bool AsegurarEscenaJuego(bool mostrarDialogos)
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path == GameScenePath)
            return true;

        if (mostrarDialogos
            && !EditorUtility.DisplayDialog(
                "Bake Trees And Rocks",
                "Hay que abrir Assets/Scenes/Mike_Juego.unity con Play apagado. ¿La abro ahora?",
                "Abrir Mike_Juego",
                "Cancelar"))
            return false;

        if (scene.isDirty)
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        var abierta = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        return abierta.IsValid() && abierta.path == GameScenePath;
    }
}
