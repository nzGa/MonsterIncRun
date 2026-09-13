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
        AsegurarSuelo();
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
        if (GameObject.Find("Suelo") != null)
            return;

        var suelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        suelo.name = "Suelo";
        suelo.transform.position = Vector3.zero;
        suelo.transform.localScale = new Vector3(20f, 1f, 20f);
        var renderer = suelo.GetComponent<Renderer>();
        renderer.material.color = new Color(0.35f, 0.35f, 0.32f);
    }

    void AsegurarFabrica()
    {
        if (GameObject.Find("Fabrica") != null)
            return;

        var model = ModelLoader.Load("Models/fabrica", "Assets/Resources/Models/fabrica.FBX");
        if (model == null)
            return;

        var instance = Instantiate(model);
        instance.name = "Fabrica";
        instance.transform.position = Vector3.zero;
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
        var punto = puntos[Random.Range(0, puntos.Length)];
        Instanciar("Ducha", "Ducha", ducha, "Models/ducha", "Assets/Resources/Models/ducha.FBX", punto.transform.position, PrimitiveType.Cylinder, new Color(0.4f, 0.7f, 1f));
    }

    void SpawnPuerta()
    {
        var puntos = GameObject.FindGameObjectsWithTag("SpawnPuerta");
        var punto = puntos[Random.Range(0, puntos.Length)];
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
