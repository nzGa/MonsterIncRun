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
        var pos = FueraDeFabrica(punto.transform.position);
        var go = Instanciar("Ducha", "Ducha", ducha, "Models/ducha", "Assets/Resources/Models/ducha.FBX",
            pos, PrimitiveType.Cylinder, new Color(0.45f, 0.78f, 0.95f), 3.3f, true);
        AgregarLluvia(go);
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
        foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("SpawnTubo"))
        {
            var go = Instanciar("Tubo", "Tubo", tubo, "Models/tubo", "Assets/Resources/Models/tubo.FBX",
                spawnPoint.transform.position, PrimitiveType.Capsule, new Color(1f, 0.85f, 0.1f), 1.15f, false);
            if (go.GetComponent<Rotar>() == null)
                go.AddComponent<Rotar>();
        }
    }

    void SpawnCajas()
    {
        foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("SpawnCaja"))
        {
            var go = Instanciar("Caja", "Caja", caja, "Models/caja", "Assets/Resources/Models/caja.FBX",
                spawnPoint.transform.position, PrimitiveType.Cube, new Color(0.8f, 0.2f, 0.2f), 0.95f, false);
            if (go.GetComponent<ContenidoCaja>() == null)
                go.AddComponent<ContenidoCaja>();
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
        AsegurarTrigger(go);
        return go;
    }

    static void QuitarAnimator(GameObject go)
    {
        foreach (var anim in go.GetComponentsInChildren<Animator>(true))
        {
            anim.enabled = false;
            anim.runtimeAnimatorController = null;
            Object.Destroy(anim);
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
        if (tag != "Puerta" && tag != "Ducha")
            return;
        Material mat = null;
        if (tag == "Puerta")
            mat = Resources.Load<Material>("Models/Materials/Puerta")
                ?? Resources.Load<Material>("Models/Materials/madera");
        else if (tag == "Ducha")
        {
            var src = Resources.Load<Material>("Models/Materials/metal");
            mat = src != null ? new Material(src) : null;
            if (mat != null && mat.HasProperty("_Color"))
                mat.color = fallback;
        }

        if (mat != null)
            AmbienteVisual.RepararShader(mat);

        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (r == null)
                continue;
            AmbienteVisual.RepararShader(r.sharedMaterial);
            if (mat != null)
                r.sharedMaterial = mat;
            else if (r.material != null && r.material.HasProperty("_Color"))
                r.material.color = fallback;
        }
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
            Object.Destroy(dummy.GetComponent<Collider>());
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
        var b = BoundsDe(go);
        var box = go.GetComponent<BoxCollider>();
        if (box == null)
            box = go.AddComponent<BoxCollider>();

        var lossy = go.transform.lossyScale;
        box.size = new Vector3(
            SafeDiv(b.size.x, lossy.x),
            SafeDiv(b.size.y, lossy.y),
            SafeDiv(b.size.z, lossy.z));
        box.center = go.transform.InverseTransformPoint(b.center);
        box.isTrigger = true;

        foreach (var childCol in go.GetComponentsInChildren<Collider>(true))
            childCol.isTrigger = true;
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
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 1f;
        main.prewarm = true;
        main.simulationSpeed = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Local;
        main.startDelay = 0f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.8f, 4.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.11f);
        main.startColor = new Color(0.72f, 0.9f, 1f, 0.75f);
        main.gravityModifier = 0.55f;
        main.maxParticles = 280;
        main.cullingMode = ParticleSystemCullingMode.PauseAndCatchup;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 56f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 13f;
        shape.radius = 0.16f;
        shape.length = 0.2f;
        shape.radiusThickness = 1f;
        shape.arc = 360f;
        shape.randomDirectionAmount = 0.08f;

        var colorLife = ps.colorOverLifetime;
        colorLife.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.7f, 0.88f, 1f), 0f),
                new GradientColorKey(new Color(0.85f, 0.95f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.2f, 0f),
                new GradientAlphaKey(0.8f, 0.15f),
                new GradientAlphaKey(0.35f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            });
        colorLife.color = grad;

        var sizeLife = ps.sizeOverLifetime;
        sizeLife.enabled = true;
        sizeLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.35f));

        var psr = lluvia.GetComponent<ParticleSystemRenderer>();
        psr.renderMode = ParticleSystemRenderMode.Billboard;
        psr.sharedMaterial = MaterialLluvia();
        psr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        psr.receiveShadows = false;
        psr.maxParticleSize = 0.12f;

        lluvia.SetActive(true);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
            _matLluvia.color = new Color(0.7f, 0.9f, 1f, 1f);
        if (_matLluvia.HasProperty("_TintColor"))
            _matLluvia.SetColor("_TintColor", new Color(0.55f, 0.8f, 1f, 0.45f));
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
