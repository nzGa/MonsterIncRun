using UnityEngine;

[DefaultExecutionOrder(-100)]
public class SpawnObjetos : MonoBehaviour
{
    public GameObject puerta;
    public GameObject ducha;
    public GameObject tubo;
    public GameObject caja;

    void Start()
    {
        AmbienteVisual.AplicarCielo();
        AsegurarSuelo();
        AmbienteVisual.AsegurarCesped();
        AsegurarFabrica();
        AsegurarPuntosSiFaltan();

        if (GameObject.FindGameObjectWithTag("ObjetoCaja") == null)
        {
            var holder = new GameObject("ObjetoCaja");
            holder.tag = "ObjetoCaja";
            holder.AddComponent<ObjetoCaja>();
        }

        SpawnPuerta();
        SpawnDucha();
        SpawnTubos();
        SpawnCajas();
    }

    void AsegurarSuelo()
    {
        var suelo = GameObject.Find("Suelo");
        if (suelo == null)
        {
            suelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            suelo.name = "Suelo";
            suelo.transform.position = Vector3.zero;
            suelo.transform.localScale = new Vector3(12f, 1f, 12f);
        }

        AmbienteVisual.AplicarSuelo(suelo);
    }

    void AsegurarFabrica()
    {
        var instance = GameObject.Find("Fabrica");
        if (instance == null)
        {
            var model = ModelLoader.Load("Models/fabrica", "Assets/Resources/Models/fabrica.FBX");
            if (model == null)
                return;

            instance = Instantiate(model);
            instance.name = "Fabrica";
            instance.transform.position = Vector3.zero;
        }

        ReconectarMateriales.En(instance);
        AsegurarColisionFabrica(instance);
    }

    static void AsegurarColisionFabrica(GameObject fabrica)
    {
        if (fabrica == null)
            return;

        foreach (var filter in fabrica.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.sharedMesh == null)
                continue;

            var col = filter.GetComponent<MeshCollider>();
            if (col == null)
                col = filter.gameObject.AddComponent<MeshCollider>();

            col.sharedMesh = filter.sharedMesh;
            col.convex = false;
            col.isTrigger = false;
        }

        foreach (var renderer in fabrica.GetComponentsInChildren<MeshRenderer>(true))
        {
            var filter = renderer.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
                continue;
            if (renderer.GetComponent<MeshCollider>() != null)
                continue;

            var col = renderer.gameObject.AddComponent<MeshCollider>();
            col.sharedMesh = filter.sharedMesh;
            col.convex = false;
            col.isTrigger = false;
        }

        foreach (var col in fabrica.GetComponentsInChildren<Collider>(true))
            col.isTrigger = false;
    }

    void AsegurarPuntosSiFaltan()
    {
        if (GameObject.FindGameObjectsWithTag("SpawnPuerta").Length == 0)
            CrearPunto("SpawnPuerta", "SpawnPuerta", new Vector3(8, 0, 12));
        if (GameObject.FindGameObjectsWithTag("SpawnDucha").Length == 0)
            CrearPunto("SpawnDucha", "SpawnDucha", new Vector3(-10, 0, 8));
        if (GameObject.FindGameObjectsWithTag("SpawnTubo").Length == 0)
        {
            CrearPunto("SpawnTubo1", "SpawnTubo", new Vector3(4, 1, -6));
            CrearPunto("SpawnTubo2", "SpawnTubo", new Vector3(-6, 1, 4));
            CrearPunto("SpawnTubo3", "SpawnTubo", new Vector3(10, 1, 2));
        }
        if (GameObject.FindGameObjectsWithTag("SpawnCaja").Length == 0)
        {
            CrearPunto("SpawnCaja1", "SpawnCaja", new Vector3(-3, 1, -8));
            CrearPunto("SpawnCaja2", "SpawnCaja", new Vector3(6, 1, -2));
            CrearPunto("SpawnCaja3", "SpawnCaja", new Vector3(-8, 1, -2));
        }
        if (GameObject.FindGameObjectsWithTag("SpawnMike").Length == 0)
            CrearPunto("SpawnMike", "SpawnMike", new Vector3(0, 1, -14));
    }

    static void CrearPunto(string name, string tag, Vector3 pos)
    {
        var go = new GameObject(name);
        go.tag = tag;
        go.transform.position = pos;
    }

    void SpawnDucha()
    {
        var puntos = GameObject.FindGameObjectsWithTag("SpawnDucha");
        var punto = puntos[UnityEngine.Random.Range(0, puntos.Length)];
        Instanciar("Ducha", "Ducha", ducha, "Models/ducha", "Assets/Resources/Models/ducha.FBX", punto.transform.position, PrimitiveType.Cylinder, new Color(0.4f, 0.7f, 1f));
    }

    void SpawnPuerta()
    {
        var puntos = GameObject.FindGameObjectsWithTag("SpawnPuerta");
        var punto = puntos[UnityEngine.Random.Range(0, puntos.Length)];
        Instanciar("Puerta", "Puerta", puerta, "Models/puerta", "Assets/Resources/Models/puerta.FBX", punto.transform.position, PrimitiveType.Cube, new Color(0.55f, 0.35f, 0.15f));
    }

    void SpawnTubos()
    {
        foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("SpawnTubo"))
        {
            var go = Instanciar("Tubo", "Tubo", tubo, "Models/tubo", "Assets/Resources/Models/tubo.FBX", spawnPoint.transform.position, PrimitiveType.Capsule, new Color(1f, 0.85f, 0.1f));
            if (go.GetComponent<Rotar>() == null)
                go.AddComponent<Rotar>();
        }
    }

    void SpawnCajas()
    {
        foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("SpawnCaja"))
        {
            var go = Instanciar("Caja", "Caja", caja, "Models/caja", "Assets/Resources/Models/caja.FBX", spawnPoint.transform.position, PrimitiveType.Cube, new Color(0.8f, 0.2f, 0.2f));
            if (go.GetComponent<ContenidoCaja>() == null)
                go.AddComponent<ContenidoCaja>();
        }
    }

    static GameObject Instanciar(string name, string tag, GameObject prefab, string resource, string path, Vector3 position, PrimitiveType fallback, Color color)
    {
        GameObject go;
        var source = prefab != null ? prefab : ModelLoader.Load(resource, path);
        if (source != null)
        {
            go = Instantiate(source, position, source.transform.rotation);
        }
        else
        {
            go = GameObject.CreatePrimitive(fallback);
            go.transform.position = position;
            go.GetComponent<Renderer>().material.color = color;
        }

        go.name = name;
        go.tag = tag;
        ReconectarMateriales.En(go);
        AsegurarTrigger(go);
        return go;
    }

    static void AsegurarTrigger(GameObject go)
    {
        var col = go.GetComponent<Collider>();
        if (col == null)
            col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;

        foreach (var childCol in go.GetComponentsInChildren<Collider>())
            childCol.isTrigger = true;
    }
}
