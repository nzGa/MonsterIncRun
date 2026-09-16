using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class SpawnObjetos : MonoBehaviour
{
    public GameObject puerta;
    public GameObject ducha;
    public GameObject tubo;
    public GameObject caja;

    static readonly Vector3[] PuntosPuerta =
    {
        new Vector3(18f, 0.05f, 16f),
        new Vector3(-16f, 0.05f, 20f),
        new Vector3(22f, 0.05f, -10f),
        new Vector3(-20f, 0.05f, -14f),
        new Vector3(8f, 0.05f, 24f),
        new Vector3(-24f, 0.05f, 6f),
        new Vector3(16f, 0.05f, -22f),
        new Vector3(4f, 0.05f, 20f)
    };

    static readonly Vector3[] PuntosDucha =
    {
        new Vector3(-14f, 0.05f, 12f),
        new Vector3(12f, 0.05f, 14f),
        new Vector3(-18f, 0.05f, -6f)
    };

    void Start()
    {
        AmbienteVisual.AplicarCielo();
        AsegurarSuelo();
        try
        {
            AmbienteTerreno.Crear();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("SpawnObjetos terreno: " + e.Message);
        }
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
            suelo.transform.position = new Vector3(0f, 0.01f, 0f);
            suelo.transform.localScale = new Vector3(8f, 1f, 8f);
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
        AmbienteVisual.AsegurarSondaReflexion(instance);
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
        {
            for (int i = 0; i < PuntosPuerta.Length; i++)
                CrearPunto("SpawnPuerta" + (i + 1), "SpawnPuerta", PuntosPuerta[i]);
        }

        if (GameObject.FindGameObjectsWithTag("SpawnDucha").Length == 0)
        {
            for (int i = 0; i < PuntosDucha.Length; i++)
                CrearPunto("SpawnDucha" + (i + 1), "SpawnDucha", PuntosDucha[i]);
        }

        if (GameObject.FindGameObjectsWithTag("SpawnTubo").Length == 0)
        {
            var tubos = new[]
            {
                new Vector3(4f, 1f, -6f),
                new Vector3(-6f, 1f, 4f),
                new Vector3(10f, 1f, 2f),
                new Vector3(-12f, 1f, -8f),
                new Vector3(14f, 1f, 12f),
                new Vector3(-18f, 1f, 10f),
                new Vector3(8f, 1f, -16f),
                new Vector3(-4f, 1f, 16f),
                new Vector3(18f, 1f, -4f)
            };
            for (int i = 0; i < tubos.Length; i++)
                CrearPunto("SpawnTubo" + (i + 1), "SpawnTubo", tubos[i]);
        }
        if (GameObject.FindGameObjectsWithTag("SpawnCaja").Length == 0)
        {
            var cajas = new[]
            {
                new Vector3(-3f, 1f, -8f),
                new Vector3(6f, 1f, -2f),
                new Vector3(-8f, 1f, -2f),
                new Vector3(12f, 1f, 8f),
                new Vector3(-14f, 1f, 6f),
                new Vector3(2f, 1f, 14f),
                new Vector3(-10f, 1f, -14f),
                new Vector3(16f, 1f, -10f)
            };
            for (int i = 0; i < cajas.Length; i++)
                CrearPunto("SpawnCaja" + (i + 1), "SpawnCaja", cajas[i]);
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
        var pos = FueraDeFabrica(punto.transform.position);
        var go = Instanciar("Ducha", "Ducha", ducha, "Models/ducha", "Assets/Resources/Models/ducha.FBX",
            pos, PrimitiveType.Cylinder, new Color(0.45f, 0.78f, 0.95f), 3.3f, true);
        AgregarLluvia(go);
        AsegurarTriggerDucha(go);
    }

    void SpawnPuerta()
    {
        var puntos = GameObject.FindGameObjectsWithTag("SpawnPuerta");
        var punto = puntos[UnityEngine.Random.Range(0, puntos.Length)];
        var pos = FueraDeFabrica(punto.transform.position);
        Instanciar("Puerta", "Puerta", puerta, "Models/puerta", "Assets/Resources/Models/puerta.FBX",
            pos, PrimitiveType.Cube, new Color(0.55f, 0.35f, 0.15f), 3.2f, true);
    }

    void SpawnTubos()
    {
        var posiciones = ElegirPosiciones("SpawnTubo", 3, 8f, 22f, 6.5f);
        for (int i = 0; i < posiciones.Count; i++)
        {
            var go = Instanciar("Tubo", "Tubo", tubo, "Models/tubo", "Assets/Resources/Models/tubo.FBX",
                posiciones[i], PrimitiveType.Capsule, new Color(1f, 0.85f, 0.1f), 1.15f, false);
            if (go.GetComponent<Rotar>() == null)
                go.AddComponent<Rotar>();
        }
    }

    void SpawnCajas()
    {
        var posiciones = ElegirPosiciones("SpawnCaja", 3, 7f, 20f, 5.5f);
        for (int i = 0; i < posiciones.Count; i++)
        {
            var go = Instanciar("Caja", "Caja", caja, "Models/caja", "Assets/Resources/Models/caja.FBX",
                posiciones[i], PrimitiveType.Cube, new Color(0.8f, 0.2f, 0.2f), 0.95f, false);
            if (go.GetComponent<ContenidoCaja>() == null)
                go.AddComponent<ContenidoCaja>();
        }
    }

    static System.Collections.Generic.List<Vector3> ElegirPosiciones(
        string tag, int cantidad, float minR, float maxR, float minDist)
    {
        var candidatos = new System.Collections.Generic.List<Vector3>();
        var puntos = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < puntos.Length; i++)
        {
            if (puntos[i] != null)
                candidatos.Add(OffsetAleatorio(puntos[i].transform.position, 1.5f, 6f));
        }

        while (candidatos.Count < cantidad + 6)
            candidatos.Add(PosicionAnillo(minR, maxR));

        Barajar(candidatos);

        var elegidos = new System.Collections.Generic.List<Vector3>();
        for (int i = 0; i < candidatos.Count && elegidos.Count < cantidad; i++)
        {
            var p = candidatos[i];
            bool lejos = true;
            for (int j = 0; j < elegidos.Count; j++)
            {
                var d = elegidos[j] - p;
                d.y = 0f;
                if (d.sqrMagnitude < minDist * minDist)
                {
                    lejos = false;
                    break;
                }
            }
            if (lejos)
                elegidos.Add(p);
        }

        while (elegidos.Count < cantidad)
            elegidos.Add(PosicionAnillo(minR, maxR));

        return elegidos;
    }

    static Vector3 PosicionAnillo(float minR, float maxR)
    {
        float ang = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float r = UnityEngine.Random.Range(minR, maxR);
        return new Vector3(Mathf.Cos(ang) * r, 1f, Mathf.Sin(ang) * r);
    }

    static Vector3 OffsetAleatorio(Vector3 origen, float min, float max)
    {
        float ang = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float r = UnityEngine.Random.Range(min, max);
        return origen + new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);
    }

    static void Barajar(System.Collections.Generic.List<Vector3> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var tmp = lista[i];
            lista[i] = lista[j];
            lista[j] = tmp;
        }
    }

    static GameObject Instanciar(
        string name,
        string tag,
        GameObject prefab,
        string resource,
        string path,
        Vector3 position,
        PrimitiveType fallback,
        Color color,
        float altoObjetivo,
        bool forzarAlto)
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
            var rend = go.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = color;
        }

        go.name = name;
        go.tag = tag;
        QuitarAnimator(go);
        RepararEscalaCero(go);
        ActivarRenderers(go);
        ReconectarMateriales.En(go);
        PintarProp(go, tag, color);
        AjustarTamano(go, altoObjetivo, forzarAlto, fallback, color);
        ColocarSobreSuelo(go, position);
        if (tag == "Ducha")
            go = EnvolverSinRotacion(go);
        if (tag != "Ducha")
            AsegurarTrigger(go);
        return go;
    }

    static GameObject EnvolverSinRotacion(GameObject modelo)
    {
        var wrap = new GameObject(modelo.name);
        wrap.tag = "Ducha";
        wrap.layer = 0;
        wrap.transform.position = modelo.transform.position;
        wrap.transform.rotation = Quaternion.identity;
        wrap.transform.localScale = Vector3.one;
        modelo.transform.SetParent(wrap.transform, true);
        modelo.name = "Modelo";
        modelo.tag = "Ducha";
        return wrap;
    }

    static void QuitarAnimator(GameObject go)
    {
        foreach (var anim in go.GetComponentsInChildren<Animator>(true))
        {
            anim.enabled = false;
            anim.runtimeAnimatorController = null;
            UnityEngine.Object.Destroy(anim);
        }
    }

    static void RepararEscalaCero(GameObject go)
    {
        foreach (var t in go.GetComponentsInChildren<Transform>(true))
        {
            var s = t.localScale;
            if (Mathf.Abs(s.x) < 1e-4f || Mathf.Abs(s.y) < 1e-4f || Mathf.Abs(s.z) < 1e-4f)
            {
                t.localScale = new Vector3(
                    Mathf.Abs(s.x) < 1e-4f ? 1f : s.x,
                    Mathf.Abs(s.y) < 1e-4f ? 1f : s.y,
                    Mathf.Abs(s.z) < 1e-4f ? 1f : s.z);
            }
        }
    }

    static void ActivarRenderers(GameObject go)
    {
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            r.enabled = true;
            r.forceRenderingOff = false;
            if (r.sharedMaterial == null)
            {
                var shader = Shader.Find("Legacy Shaders/Diffuse") ?? Shader.Find("Standard");
                if (shader != null)
                    r.sharedMaterial = new Material(shader);
            }
        }
    }

    static void PintarProp(GameObject go, string tag, Color fallback)
    {
        if (tag == "Ducha")
        {
            PintarDucha(go);
            return;
        }

        if (tag != "Puerta")
            return;

        var mat = Resources.Load<Material>("Models/Materials/Puerta")
            ?? Resources.Load<Material>("Models/Materials/madera");
        if (mat != null)
            AmbienteVisual.RepararShader(mat);

        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (r == null || r is ParticleSystemRenderer)
                continue;
            AmbienteVisual.RepararShader(r.sharedMaterial);
            if (mat != null)
                r.sharedMaterial = mat;
            else if (r.material != null && r.material.HasProperty("_Color"))
                r.material.color = fallback;
        }
    }

    static void PintarDucha(GameObject go)
    {
        var madera = Resources.Load<Material>("Models/Materials/madera");
        var metal = Resources.Load<Material>("Models/Materials/metal");
        AmbienteVisual.RepararShader(madera);
        AmbienteVisual.RepararShader(metal);

        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (r == null || r is ParticleSystemRenderer)
                continue;
            AmbienteVisual.RepararShader(r.sharedMaterial);
            var elegido = EsMaderaDucha(r.gameObject.name) ? madera : metal;
            if (elegido != null)
                r.sharedMaterial = elegido;
        }
    }

    static bool EsMaderaDucha(string nombre)
    {
        if (string.IsNullOrEmpty(nombre))
            return false;
        return nombre.IndexOf("Box", StringComparison.OrdinalIgnoreCase) >= 0
            || nombre.IndexOf("Wall", StringComparison.OrdinalIgnoreCase) >= 0
            || nombre.IndexOf("Base", StringComparison.OrdinalIgnoreCase) >= 0
            || nombre.IndexOf("madera", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static void AjustarTamano(GameObject go, float objetivo, bool forzar, PrimitiveType fallback, Color color)
    {
        var b = BoundsDe(go);
        if (go.GetComponentInChildren<Renderer>(true) == null || b.size.sqrMagnitude < 1e-6f)
        {
            var dummy = GameObject.CreatePrimitive(fallback);
            dummy.transform.SetParent(go.transform, false);
            dummy.transform.localPosition = Vector3.up * (objetivo * 0.5f);
            dummy.transform.localScale = Vector3.one * objetivo;
            var rend = dummy.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = color;
            UnityEngine.Object.Destroy(dummy.GetComponent<Collider>());
            b = BoundsDe(go);
        }

        float h = b.size.y;
        if (h < 0.02f)
        {
            go.transform.localScale *= 100f;
            b = BoundsDe(go);
            h = b.size.y;
        }

        if (h > 0.001f && (forzar || h < objetivo * 0.3f || h > objetivo * 8f))
            go.transform.localScale *= objetivo / h;
    }

    static void ColocarSobreSuelo(GameObject go, Vector3 destino)
    {
        destino.y = Mathf.Max(0.02f, AmbienteTerreno.AlturaEn(destino));
        var b = BoundsDe(go);
        var delta = new Vector3(
            destino.x - b.center.x,
            destino.y - b.min.y,
            destino.z - b.center.z);
        go.transform.position += delta;
    }

    static Vector3 FueraDeFabrica(Vector3 p)
    {
        var fabrica = GameObject.Find("Fabrica");
        if (fabrica == null)
            return p;

        var b = BoundsEdificio(fabrica);
        var xz = new Vector3(p.x, b.center.y, p.z);
        if (!b.Contains(xz))
            return p;

        var d = new Vector3(p.x - b.center.x, 0f, p.z - b.center.z);
        if (d.sqrMagnitude < 0.01f)
            d = Vector3.forward;
        d.Normalize();
        float extra = 3.5f;
        var afuera = new Vector3(b.center.x, p.y, b.center.z) + d * (new Vector3(b.extents.x, 0f, b.extents.z).magnitude + extra);
        if (afuera.sqrMagnitude > 38f * 38f)
            afuera = d * 26f;
        afuera.y = p.y;
        return afuera;
    }

    static Bounds BoundsEdificio(GameObject fabrica)
    {
        Bounds? acc = null;
        foreach (var r in fabrica.GetComponentsInChildren<Renderer>(true))
        {
            var b = r.bounds;
            bool piso = b.size.y < 1.2f && b.size.x > 25f && b.size.z > 25f;
            if (piso)
                continue;
            if (!acc.HasValue)
                acc = b;
            else
            {
                var t = acc.Value;
                t.Encapsulate(b);
                acc = t;
            }
        }

        return acc ?? new Bounds(Vector3.zero, new Vector3(20f, 10f, 20f));
    }

    static Bounds BoundsDe(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>(true);
        if (rs == null || rs.Length == 0)
            return new Bounds(go.transform.position, Vector3.one * 0.5f);

        Bounds? acc = null;
        for (int i = 0; i < rs.Length; i++)
        {
            if (rs[i] == null || !rs[i].enabled || rs[i] is ParticleSystemRenderer)
                continue;
            if (!acc.HasValue)
                acc = rs[i].bounds;
            else
            {
                var t = acc.Value;
                t.Encapsulate(rs[i].bounds);
                acc = t;
            }
        }

        return acc ?? new Bounds(go.transform.position, Vector3.one * 0.5f);
    }

    static void AsegurarTrigger(GameObject go)
    {
        bool puerta = go.CompareTag("Puerta");
        QuitarColliders(go);
        if (puerta)
            AsegurarColliderSolidoPuerta(go);

        var b = BoundsDe(go);
        float padXz = puerta ? 0.8f : 0.5f;
        float minXz = puerta ? 2.4f : 0.85f;
        float minY = puerta ? 3.2f : 0.7f;
        float sx = Mathf.Max(b.size.x + padXz * 2f, minXz);
        float sy = Mathf.Max(b.size.y + 0.5f, minY);
        float sz = Mathf.Max(b.size.z + padXz * 2f, minXz);
        if (puerta)
        {
            // Keep a thick catch volume in front/around the slab so the
            // CharacterController can win without clipping through wood.
            const float profundidad = 2.4f;
            if (b.size.x <= b.size.z)
                sx = Mathf.Max(b.size.x + profundidad, 2.4f);
            else
                sz = Mathf.Max(b.size.z + profundidad, 2.4f);
        }

        var t = go.transform.Find("Trigger");
        GameObject host;
        if (t == null)
        {
            host = new GameObject("Trigger");
            t = host.transform;
        }
        else
            host = t.gameObject;

        host.layer = go.layer;
        host.tag = go.tag;

        // World-aligned trigger. FBX roots are often rotated -90°, so a box
        // sized from world AABB / lossyScale on the root is paper-thin and
        // CharacterController never fires OnTriggerEnter.
        t.SetParent(null);
        t.position = new Vector3(b.center.x, b.min.y + sy * 0.5f, b.center.z);
        t.rotation = Quaternion.identity;
        t.localScale = Vector3.one;
        t.SetParent(go.transform, true);

        if (!host.TryGetComponent(out BoxCollider box))
            box = host.AddComponent<BoxCollider>();

        var ls = t.lossyScale;
        box.center = Vector3.zero;
        box.size = new Vector3(
            sx / Mathf.Max(Mathf.Abs(ls.x), 1e-4f),
            sy / Mathf.Max(Mathf.Abs(ls.y), 1e-4f),
            sz / Mathf.Max(Mathf.Abs(ls.z), 1e-4f));
        box.isTrigger = true;
    }

    static void AsegurarTriggerDucha(GameObject wrap)
    {
        if (wrap == null)
            return;

        QuitarColliders(wrap);
        AsegurarColliderSolidoDucha(wrap);

        var lluvia = BuscarHijo(wrap.transform, "Lluvia");
        var cabeza = BuscarHijo(wrap.transform, "Cylinder001");
        Vector3 xz;
        if (lluvia != null)
            xz = lluvia.position;
        else if (cabeza != null)
        {
            var rend = cabeza.GetComponent<Renderer>();
            xz = rend != null ? rend.bounds.center : cabeza.position;
        }
        else
            xz = BoundsDe(wrap).center;

        const float alto = 2.9f;
        const float ancho = 2.4f;
        float suelo = AmbienteTerreno.AlturaEn(xz);
        var centro = new Vector3(xz.x, suelo + alto * 0.5f, xz.z);

        var viejo = wrap.transform.Find("Ducha Trigger");
        if (viejo != null)
            UnityEngine.Object.DestroyImmediate(viejo.gameObject);

        var host = new GameObject("Ducha Trigger");
        host.tag = "Ducha";
        host.layer = 0;

        // Identity wrap, never parented under the -90° FBX model.
        var t = host.transform;
        t.SetParent(null);
        t.position = centro;
        t.rotation = Quaternion.identity;
        t.localScale = Vector3.one;
        t.SetParent(wrap.transform, true);

        var box = host.AddComponent<BoxCollider>();
        var ls = t.lossyScale;
        box.center = Vector3.zero;
        box.size = new Vector3(
            ancho / Mathf.Max(Mathf.Abs(ls.x), 1e-4f),
            alto / Mathf.Max(Mathf.Abs(ls.y), 1e-4f),
            ancho / Mathf.Max(Mathf.Abs(ls.z), 1e-4f));
        box.isTrigger = true;

        var rb = host.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        host.AddComponent<DuchaTrigger>();
    }

    static void AsegurarColliderSolidoDucha(GameObject wrap)
    {
        bool hayMadera = false;
        foreach (var filter in wrap.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter == null || filter.sharedMesh == null)
                continue;
            if (!EsMaderaDucha(filter.gameObject.name))
                continue;

            if (!filter.gameObject.TryGetComponent(out MeshCollider col))
                col = filter.gameObject.AddComponent<MeshCollider>();
            col.sharedMesh = filter.sharedMesh;
            col.convex = false;
            col.isTrigger = false;
            hayMadera = true;
            AgregarCajasMaderaDucha(filter);
        }

        if (hayMadera)
            return;

        var b = BoundsDe(wrap);
        if (!wrap.TryGetComponent(out BoxCollider caja))
            caja = wrap.AddComponent<BoxCollider>();
        var ls = wrap.transform.lossyScale;
        caja.center = wrap.transform.InverseTransformPoint(b.center);
        caja.size = new Vector3(
            b.size.x / Mathf.Max(Mathf.Abs(ls.x), 1e-4f),
            b.size.y / Mathf.Max(Mathf.Abs(ls.y), 1e-4f),
            b.size.z / Mathf.Max(Mathf.Abs(ls.z), 1e-4f));
        caja.isTrigger = false;
    }

    static void AgregarCajasMaderaDucha(MeshFilter filter)
    {
        var mesh = filter.sharedMesh;
        if (mesh == null || !mesh.isReadable)
            return;

        var verts = mesh.vertices;
        var tris = mesh.triangles;
        if (verts == null || tris == null || tris.Length < 3)
            return;

        var tr = filter.transform;
        var paredes = new System.Collections.Generic.List<Bounds>();
        var suelos = new System.Collections.Generic.List<Bounds>();

        for (int i = 0; i + 2 < tris.Length; i += 3)
        {
            var a = verts[tris[i]];
            var b = verts[tris[i + 1]];
            var c = verts[tris[i + 2]];
            var min = Vector3.Min(a, Vector3.Min(b, c));
            var max = Vector3.Max(a, Vector3.Max(b, c));
            var local = new Bounds(
                (min + max) * 0.5f,
                Vector3.Max(max - min, new Vector3(0.02f, 0.02f, 0.02f)));

            var wa = tr.TransformPoint(a);
            var wb = tr.TransformPoint(b);
            var wc = tr.TransformPoint(c);
            var wmin = Vector3.Min(wa, Vector3.Min(wb, wc));
            var wmax = Vector3.Max(wa, Vector3.Max(wb, wc));
            var wsize = wmax - wmin;
            if (wsize.y > 0.8f && Mathf.Min(wsize.x, wsize.z) < 0.45f)
                paredes.Add(local);
            else if (wsize.y < 0.35f && Mathf.Max(wsize.x, wsize.z) > 0.6f)
                suelos.Add(local);
        }

        FusionarBoundsCercanos(paredes, 0.08f);
        FusionarBoundsCercanos(suelos, 0.08f);

        for (int i = 0; i < paredes.Count; i++)
            AgregarCajaSolidaDucha(filter.gameObject, paredes[i], 0.28f);
        for (int i = 0; i < suelos.Count; i++)
            AgregarCajaSolidaDucha(filter.gameObject, suelos[i], 0.12f);
    }

    static void FusionarBoundsCercanos(System.Collections.Generic.List<Bounds> lista, float pad)
    {
        bool cambio = true;
        while (cambio)
        {
            cambio = false;
            for (int i = 0; i < lista.Count; i++)
            {
                for (int j = i + 1; j < lista.Count; j++)
                {
                    var a = lista[i];
                    a.Expand(pad);
                    if (!a.Intersects(lista[j]))
                        continue;
                    var m = lista[i];
                    m.Encapsulate(lista[j]);
                    lista[i] = m;
                    lista.RemoveAt(j);
                    cambio = true;
                    break;
                }
                if (cambio)
                    break;
            }
        }
    }

    static void AgregarCajaSolidaDucha(GameObject go, Bounds local, float grosorMundo)
    {
        var box = go.AddComponent<BoxCollider>();
        box.isTrigger = false;
        var size = local.size;
        var ls = go.transform.lossyScale;
        float wx = size.x * Mathf.Max(Mathf.Abs(ls.x), 1e-4f);
        float wy = size.y * Mathf.Max(Mathf.Abs(ls.y), 1e-4f);
        float wz = size.z * Mathf.Max(Mathf.Abs(ls.z), 1e-4f);
        if (wx <= wy && wx <= wz && wx < grosorMundo)
            size.x = grosorMundo / Mathf.Max(Mathf.Abs(ls.x), 1e-4f);
        else if (wy <= wz && wy < grosorMundo)
            size.y = grosorMundo / Mathf.Max(Mathf.Abs(ls.y), 1e-4f);
        else if (wz < grosorMundo)
            size.z = grosorMundo / Mathf.Max(Mathf.Abs(ls.z), 1e-4f);

        box.center = local.center;
        box.size = size;
    }

    static void AsegurarColliderSolidoPuerta(GameObject go)
    {
        bool hayMalla = false;
        foreach (var filter in go.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter == null || filter.sharedMesh == null)
                continue;
            if (filter.gameObject.name == "Trigger")
                continue;

            var col = filter.gameObject.AddComponent<MeshCollider>();
            col.sharedMesh = filter.sharedMesh;
            col.convex = false;
            col.isTrigger = false;
            hayMalla = true;
        }

        if (hayMalla)
            return;

        var b = BoundsDe(go);
        if (!go.TryGetComponent(out BoxCollider caja))
            caja = go.AddComponent<BoxCollider>();
        var ls = go.transform.lossyScale;
        caja.center = go.transform.InverseTransformPoint(b.center);
        caja.size = new Vector3(
            b.size.x / Mathf.Max(Mathf.Abs(ls.x), 1e-4f),
            b.size.y / Mathf.Max(Mathf.Abs(ls.y), 1e-4f),
            b.size.z / Mathf.Max(Mathf.Abs(ls.z), 1e-4f));
        caja.isTrigger = false;
    }

    static void QuitarColliders(GameObject go)
    {
        var cols = go.GetComponentsInChildren<Collider>(true);
        for (int i = cols.Length - 1; i >= 0; i--)
        {
            if (cols[i] != null)
                UnityEngine.Object.DestroyImmediate(cols[i]);
        }
    }

    static float SafeDiv(float a, float b)
    {
        float d = Mathf.Abs(b) < 1e-4f ? 1f : b;
        return Mathf.Abs(a / d);
    }

    static Material _matLluvia;

    static void AgregarLluvia(GameObject ducha)
    {
        if (ducha == null)
            return;
        if (BuscarHijo(ducha.transform, "Lluvia") != null)
            return;

        var b = BoundsDeMalla(ducha);
        var cabeza = BuscarHijo(ducha.transform, "Cylinder001");
        Vector3 origen;
        if (cabeza != null)
        {
            var rb = cabeza.GetComponent<Renderer>();
            var c = rb != null ? rb.bounds.center : cabeza.position;
            origen = new Vector3(c.x, b.max.y - Mathf.Max(0.1f, b.size.y * 0.07f), c.z);
        }
        else
            origen = new Vector3(b.center.x, b.max.y - Mathf.Max(0.1f, b.size.y * 0.07f), b.center.z);

        var lluvia = new GameObject("Lluvia");
        lluvia.SetActive(false);
        lluvia.transform.SetParent(ducha.transform, false);
        lluvia.transform.position = origen;
        lluvia.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
        var lossy = ducha.transform.lossyScale;
        lluvia.transform.localScale = new Vector3(
            SafeDiv(1f, lossy.x),
            SafeDiv(1f, lossy.y),
            SafeDiv(1f, lossy.z));

        var ps = lluvia.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = true;
        main.loop = true;
        main.duration = 2.5f;
        main.prewarm = true;
        main.simulationSpeed = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.startDelay = 0f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.85f, 1.35f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.6f, 3.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
        main.startColor = new Color(0.88f, 0.97f, 1f, 1f);
        main.gravityModifier = 0.85f;
        main.maxParticles = 420;
        main.cullingMode = ParticleSystemCullingMode.Automatic;
        main.stopAction = ParticleSystemStopAction.None;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 110f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 16f;
        shape.radius = 0.22f;
        shape.length = 0.15f;
        shape.radiusThickness = 1f;
        shape.arc = 360f;
        shape.randomDirectionAmount = 0.05f;

        var colorLife = ps.colorOverLifetime;
        colorLife.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.85f, 0.96f, 1f), 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.85f, 0f),
                new GradientAlphaKey(1f, 0.12f),
                new GradientAlphaKey(0.7f, 0.7f),
                new GradientAlphaKey(0.15f, 1f)
            });
        colorLife.color = grad;

        var sizeLife = ps.sizeOverLifetime;
        sizeLife.enabled = true;
        sizeLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.55f));

        var psr = lluvia.GetComponent<ParticleSystemRenderer>();
        psr.enabled = true;
        psr.renderMode = ParticleSystemRenderMode.Stretch;
        psr.velocityScale = 0.12f;
        psr.lengthScale = 1.55f;
        psr.cameraVelocityScale = 0f;
        psr.sharedMaterial = MaterialLluvia();
        psr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        psr.receiveShadows = false;
        psr.minParticleSize = 0.004f;
        psr.maxParticleSize = 0.35f;
        psr.sortingFudge = -40f;
        psr.sortingOrder = 8;
        psr.allowOcclusionWhenDynamic = false;

        lluvia.SetActive(true);
        ps.Play(true);
    }

    static Material MaterialLluvia()
    {
        if (_matLluvia != null)
            return _matLluvia;

        var shader = Shader.Find("Legacy Shaders/Particles/Additive")
            ?? Shader.Find("Particles/Standard Unlit")
            ?? Shader.Find("Mobile/Particles/Additive")
            ?? Shader.Find("Sprites/Default")
            ?? Shader.Find("Unlit/Color")
            ?? Shader.Find("Standard");
        if (shader == null)
            return null;
        _matLluvia = new Material(shader);
        _matLluvia.name = "LluviaDucha";
        _matLluvia.mainTexture = TexturaGota();
        if (_matLluvia.HasProperty("_Color"))
            _matLluvia.color = new Color(0.9f, 0.97f, 1f, 1f);
        if (_matLluvia.HasProperty("_TintColor"))
            _matLluvia.SetColor("_TintColor", new Color(0.8f, 0.93f, 1f, 0.7f));
        if (_matLluvia.HasProperty("_ColorMode"))
        {
            _matLluvia.SetFloat("_ColorMode", 1f);
            _matLluvia.EnableKeyword("_COLORADDSUBDIFF_ON");
        }
        return _matLluvia;
    }

    static Texture2D TexturaGota()
    {
        const int n = 32;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        float cx = (n - 1) * 0.5f;
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = (x - cx) / cx;
                float dy = (y - cx) / cx;
                float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                a *= a;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply(false, true);
        tex.name = "GotaLluvia";
        return tex;
    }

    static Bounds BoundsDeMalla(GameObject go)
    {
        var rs = go.GetComponentsInChildren<MeshRenderer>(true);
        if (rs == null || rs.Length == 0)
            return BoundsDe(go);

        var b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++)
        {
            if (rs[i] != null && rs[i].enabled)
                b.Encapsulate(rs[i].bounds);
        }

        return b;
    }

    static Transform BuscarHijo(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == name)
                return t;
        }

        return null;
    }
}
