using UnityEngine;

[DefaultExecutionOrder(-50)]
public class SpawnJugador : MonoBehaviour
{
    public GameObject mikePrefab;

    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        var puntos = GameObject.FindGameObjectsWithTag("SpawnMike");
        Transform spawn = puntos.Length > 0 ? puntos[Random.Range(0, puntos.Length)].transform : null;
        Vector3 pos = spawn != null ? spawn.position : new Vector3(0, 1, -14);
        Quaternion rot = spawn != null ? spawn.rotation : Quaternion.identity;

        GameObject source = mikePrefab != null
            ? mikePrefab
            : ModelLoader.Load("Models/Mike/Mike@Espera", "Assets/Resources/Models/Mike/Mike@Espera.FBX");

        GameObject player;
        if (source != null)
            player = Instantiate(source, pos, rot);
        else
        {
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.transform.position = pos;
            player.GetComponent<Renderer>().material.color = new Color(0.45f, 0.85f, 0.25f);
        }

        player.name = "Mike";
        player.tag = "Player";

        if (player.GetComponent<CharacterController>() == null)
        {
            var cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.center = new Vector3(0, 1f, 0);
            cc.radius = 0.4f;
        }

        if (player.GetComponent<ThirdPersonController>() == null)
            player.AddComponent<ThirdPersonController>();
        if (player.GetComponent<MouseOrbit>() == null)
            player.AddComponent<MouseOrbit>();
        if (player.GetComponent<ObtieneObjeto>() == null)
            player.AddComponent<ObtieneObjeto>();
        if (player.GetComponent<ControlCursor>() == null)
            player.AddComponent<ControlCursor>();
        if (player.GetComponent<ObjetosPorJugador>() == null)
            player.AddComponent<ObjetosPorJugador>();
        if (player.GetComponent<NombreJugador>() == null)
            player.AddComponent<NombreJugador>();
        if (player.GetComponent<EtiquetaJugador>() == null)
            player.AddComponent<EtiquetaJugador>();

        var manager = GameObject.FindGameObjectWithTag("GameManager");
        if (manager != null)
        {
            var multi = manager.GetComponent<GestionaMultiJugador>();
            var nombre = player.GetComponent<NombreJugador>();
            nombre.nombreJugador = multi != null ? multi.nombreJugador : "Mike";
            nombre.fNombreSeteado = true;
        }
    }
}
