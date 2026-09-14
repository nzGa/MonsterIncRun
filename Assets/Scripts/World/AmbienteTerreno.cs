using UnityEngine;

public static class AmbienteTerreno
{
    const int Resolucion = 513;
    const float Ancho = 200f;
    const float AltoOriginal = 50f;
    const float Largo = 200f;
    const float RadioFabricaLibre = 22f;
    const float TerrenoY = -0.02f;
    public const float YCaida = -5f;

    static Material _tronco;
    static Material _hojas;
    static Material _palmera;
    static Material _palmeraHojas;
    static Material _roca;

    public static void Crear()
    {
        var viejoCesped = GameObject.Find("Cesped");
        if (viejoCesped != null)
            Object.Destroy(viejoCesped);

        if (Terrain.activeTerrain == null)
        {
            var data = new TerrainData
            {
                heightmapResolution = Resolucion,
                size = new Vector3(Ancho, AltoOriginal, Largo),
                alphamapResolution = 256,
                baseMapResolution = 256
            };

            if (!AplicarHeightmapOriginal(data))
                GenerarColinasAlrededor(data);

            AplicarCapas(data);
            PintarLaderas(data);

            var go = Terrain.CreateTerrainGameObject(data);
            go.name = "Terreno";
            go.transform.position = OrigenTerreno();

            var terrain = go.GetComponent<Terrain>();
            terrain.heightmapPixelError = 8f;
            terrain.basemapDistance = 180f;
            terrain.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            terrain.drawInstanced = true;
            terrain.Flush();

            try
            {
                ColocarVegetacion(terrain);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("AmbienteTerreno vegetacion: " + e.Message);
            }
        }

        CrearLimites();
    }

    static Vector3 OrigenTerreno()
    {
        return new Vector3(-Ancho * 0.5f, TerrenoY, -Largo * 0.5f);
    }

    static Vector3 TamanoTerreno()
    {
        return new Vector3(Ancho, AltoOriginal, Largo);
    }

    public static Bounds BoundsJugables()
    {
        var terrain = Terrain.activeTerrain;
        if (terrain != null && terrain.terrainData != null)
        {
            var pos = terrain.transform.position;
            var size = terrain.terrainData.size;
            return new Bounds(pos + size * 0.5f, size);
        }

        var origen = OrigenTerreno();
        var tam = TamanoTerreno();
        return new Bounds(origen + tam * 0.5f, tam);
    }

    static void CrearLimites()
    {
        var viejo = GameObject.Find("LimitesMapa");
        if (viejo != null)
            Object.DestroyImmediate(viejo);

        var b = BoundsJugables();
        const float grosor = 4f;
        const float extraAlto = 30f;
        const float margenPiso = 8f;
        float yMin = Mathf.Min(b.min.y, YCaida) - 4f;
        float yMax = b.max.y + extraAlto;
        float alto = yMax - yMin;
        float yCentro = (yMin + yMax) * 0.5f;
        float zMuro = b.size.z + grosor * 2f;

        var padre = new GameObject("LimitesMapa");

        CrearCajaInvisible(padre.transform, "Muro+X",
            new Vector3(b.max.x + grosor * 0.5f, yCentro, b.center.z),
            new Vector3(grosor, alto, zMuro));
        CrearCajaInvisible(padre.transform, "Muro-X",
            new Vector3(b.min.x - grosor * 0.5f, yCentro, b.center.z),
            new Vector3(grosor, alto, zMuro));
        CrearCajaInvisible(padre.transform, "Muro+Z",
            new Vector3(b.center.x, yCentro, b.max.z + grosor * 0.5f),
            new Vector3(b.size.x, alto, grosor));
        CrearCajaInvisible(padre.transform, "Muro-Z",
            new Vector3(b.center.x, yCentro, b.min.z - grosor * 0.5f),
            new Vector3(b.size.x, alto, grosor));

        const float pisoGrosor = 2f;
        float pisoTop = YCaida - 1f;
        CrearCajaInvisible(padre.transform, "PisoCatch",
            new Vector3(b.center.x, pisoTop - pisoGrosor * 0.5f, b.center.z),
            new Vector3(b.size.x + margenPiso * 2f, pisoGrosor, b.size.z + margenPiso * 2f));
    }

    static void CrearCajaInvisible(Transform padre, string nombre, Vector3 centro, Vector3 tamano)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.position = centro;
        go.transform.rotation = Quaternion.identity;
        var box = go.AddComponent<BoxCollider>();
        box.center = Vector3.zero;
        box.size = tamano;
        box.isTrigger = false;
    }

    public static float AlturaEn(Vector3 mundo)
    {
        var terrain = Terrain.activeTerrain;
        if (terrain == null)
            return 0f;
        return terrain.SampleHeight(mundo) + terrain.transform.position.y;
    }

    static bool AplicarHeightmapOriginal(TerrainData data)
    {
        var raw = Resources.Load<TextAsset>("Terrain/originalHeightmap");
        if (raw == null || raw.bytes == null || raw.bytes.Length < Resolucion * Resolucion * 2)
            return false;

        var bytes = raw.bytes;
        var heights = new float[Resolucion, Resolucion];
        int i = 0;
        for (int z = 0; z < Resolucion; z++)
        {
            for (int x = 0; x < Resolucion; x++)
            {
                ushort h = (ushort)(bytes[i] | (bytes[i + 1] << 8));
                i += 2;
                heights[z, x] = h / 65535f;
            }
        }

        data.SetHeights(0, 0, heights);
        return true;
    }

    static void GenerarColinasAlrededor(TerrainData data)
    {
        var heights = new float[Resolucion, Resolucion];
        float centro = (Resolucion - 1) * 0.5f;
        float radioPlano = Resolucion * 0.22f;
        for (int z = 0; z < Resolucion; z++)
        {
            for (int x = 0; x < Resolucion; x++)
            {
                float dx = x - centro;
                float dz = z - centro;
                float d = Mathf.Sqrt(dx * dx + dz * dz);
                float borde = Mathf.InverseLerp(radioPlano, Resolucion * 0.48f, d);
                if (borde <= 0f)
                    continue;

                float n = Mathf.PerlinNoise(x * 0.035f, z * 0.035f);
                float n2 = Mathf.PerlinNoise(x * 0.09f + 20f, z * 0.09f + 8f);
                float montana = borde * borde * (0.35f + 0.65f * n);
                montana += borde * n2 * 0.25f;
                if (x < Resolucion * 0.12f || z > Resolucion * 0.88f)
                    montana += borde * 0.2f;
                heights[z, x] = Mathf.Clamp01(montana);
            }
        }

        data.SetHeights(0, 0, heights);
    }

    static void AplicarCapas(TerrainData data)
    {
        var cesped = CargarTex("Textures/GrassHill", "Assets/Art/Textures/Grass (Hill).psd")
            ?? CargarTex("Textures/GrassHill", "Assets/Resources/Textures/GrassHill.psd");
        var acantilado = CargarTex("Textures/Cliff", "Assets/Art/Textures/Cliff (Layered Rock).jpg");

        var grass = new TerrainLayer
        {
            diffuseTexture = cesped,
            tileSize = new Vector2(12f, 12f),
            metallic = 0f,
            smoothness = 0.05f
        };
        var cliff = new TerrainLayer
        {
            diffuseTexture = acantilado,
            tileSize = new Vector2(18f, 18f),
            metallic = 0f,
            smoothness = 0.08f
        };
        data.terrainLayers = new[] { grass, cliff };
    }

    static void PintarLaderas(TerrainData data)
    {
        int res = data.alphamapResolution;
        var map = new float[res, res, 2];
        for (int z = 0; z < res; z++)
        {
            for (int x = 0; x < res; x++)
            {
                float u = x / (float)(res - 1);
                float v = z / (float)(res - 1);
                float pendiente = data.GetSteepness(u, v);
                float roca = Mathf.InverseLerp(16f, 38f, pendiente);
                map[z, x, 0] = 1f - roca;
                map[z, x, 1] = roca;
            }
        }

        data.SetAlphamaps(0, 0, map);
    }

    static void ColocarVegetacion(Terrain terrain)
    {
        var padre = new GameObject("Bosque");
        var raw = Resources.Load<TextAsset>("Terrain/originalTrees");
        int colocados = 0;
        if (raw != null && raw.bytes != null && raw.bytes.Length >= 4)
            colocados = ColocarArbolesOriginales(terrain, padre.transform, raw.bytes);

        if (colocados < 40)
            ColocarArbolesRespaldo(terrain, padre.transform);
    }

    static int ColocarArbolesOriginales(Terrain terrain, Transform padre, byte[] bytes)
    {
        int count = System.BitConverter.ToInt32(bytes, 0);
        int offset = 4;
        int ok = 0;
        var palmera = ModelLoader.Load("Environment/Palm", "Assets/Resources/Environment/Palm.fbx");
        var roca = ModelLoader.Load("Environment/RockMesh", "Assets/Resources/Environment/RockMesh.fbx");
        var arbol = ModelLoader.Load("Environment/BigTree", "Assets/Resources/Environment/BigTree.obj");

        for (int i = 0; i < count; i++)
        {
            if (offset + 20 > bytes.Length)
                break;
            float nx = System.BitConverter.ToSingle(bytes, offset);
            float nz = System.BitConverter.ToSingle(bytes, offset + 4);
            float ancho = System.BitConverter.ToSingle(bytes, offset + 8);
            float alto = System.BitConverter.ToSingle(bytes, offset + 12);
            int proto = System.BitConverter.ToInt32(bytes, offset + 16);
            offset += 20;

            float wx = nx * Ancho - Ancho * 0.5f;
            float wz = nz * Largo - Largo * 0.5f;
            if (wx * wx + wz * wz < RadioFabricaLibre * RadioFabricaLibre)
                continue;

            var pos = new Vector3(wx, 0f, wz);
            pos.y = AlturaEn(pos);
            try
            {
                GameObject go;
                if (proto == 2)
                    go = CrearRoca(roca);
                else if (proto == 1)
                    go = CrearPalmera(palmera);
                else
                    go = CrearArbolGrande(arbol);

                go.name = proto == 2 ? "Roca" : proto == 1 ? "Palmera" : "Arbol";
                float yaw = (nx * 360f + nz * 140f + proto * 37f) % 360f;
                float n = Mathf.PerlinNoise(nx * 23.1f, nz * 17.7f);
                float n2 = Mathf.PerlinNoise(nx * 9.4f + 4f, nz * 14.2f);
                float sx = Mathf.Clamp(ancho, 0.8f, 1.3f);
                float sy = Mathf.Clamp(alto, 0.8f, 1.35f);
                if (proto == 0)
                {
                    sx *= 0.72f + n * 0.7f;
                    sy *= 0.78f + n2 * 0.7f;
                }
                ColocarEnTerreno(go, padre, pos, yaw, new Vector3(sx, sy, sx), proto == 2);
                ok++;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("AmbienteTerreno proto " + proto + ": " + e.Message);
            }
        }

        return ok;
    }

    static void ColocarArbolesRespaldo(Terrain terrain, Transform padre)
    {
        var rng = new System.Random(46);
        for (int i = 0; i < 120; i++)
        {
            float ang = (float)(i * 0.53 + rng.NextDouble());
            float radio = 38f + (float)rng.NextDouble() * 52f;
            var pos = new Vector3(Mathf.Cos(ang) * radio, 0f, Mathf.Sin(ang * 1.17f) * radio);
            if (pos.sqrMagnitude < RadioFabricaLibre * RadioFabricaLibre)
                continue;
            pos.y = AlturaEn(pos);
            try
            {
                bool roca = i % 7 == 0;
                var go = roca ? CrearRoca(null) : (i % 5 == 0) ? CrearPalmera(null) : CrearArbolGrande(null);
                float s = 0.75f + (float)rng.NextDouble() * 0.7f;
                ColocarEnTerreno(go, padre, pos, rng.Next(0, 360), Vector3.one * s, roca);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("AmbienteTerreno respaldo " + i + ": " + e.Message);
            }
        }
    }

    static GameObject CrearArbolGrande()
    {
        return CrearArbolGrande(null);
    }

    static GameObject CrearArbolGrande(GameObject prefab)
    {
        if (prefab != null)
        {
            var inst = Object.Instantiate(prefab);
            inst.name = "Arbol";
            AplicarMaterialesArbol(inst);
            foreach (var anim in inst.GetComponentsInChildren<Animator>(true))
                Object.Destroy(anim);
            NormalizarMesh(inst, 11f);
            AsegurarColliderTronco(inst);
            return inst;
        }

        return CrearArbolBillboard();
    }

    static GameObject CrearArbolBillboard()
    {
        var root = new GameObject("Arbol");
        var tronco = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tronco.name = "Tronco";
        tronco.transform.SetParent(root.transform, false);
        tronco.transform.localPosition = new Vector3(0f, 3.2f, 0f);
        tronco.transform.localScale = new Vector3(0.42f, 3.2f, 0.42f);
        AplicarMat(tronco, TroncoMat());
        Object.DestroyImmediate(tronco.GetComponent<Collider>());

        var hojas = HojasMat();
        var uvA = new Rect(0.48f, 0.50f, 0.50f, 0.48f);
        var uvB = new Rect(0.48f, 0.02f, 0.44f, 0.46f);
        for (int i = 0; i < 3; i++)
        {
            var plano = new GameObject("Hojas");
            plano.transform.SetParent(root.transform, false);
            plano.transform.localPosition = new Vector3(0f, 3.4f, 0f);
            plano.transform.localRotation = Quaternion.Euler(0f, i * 60f, 0f);
            plano.transform.localScale = new Vector3(7.4f, 8.2f, 1f);
            var filtro = plano.AddComponent<MeshFilter>();
            filtro.sharedMesh = QuadHojas(i == 2 ? uvB : uvA);
            var rend = plano.AddComponent<MeshRenderer>();
            rend.sharedMaterial = hojas;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        }

        AsegurarColliderTronco(root);
        return root;
    }

    static Mesh _quadHojasA;
    static Mesh _quadHojasB;
    static Rect _uvA;
    static Rect _uvB;

    static Mesh QuadHojas(Rect uv)
    {
        if (_quadHojasA != null && Almost(uv, _uvA))
            return _quadHojasA;
        if (_quadHojasB != null && Almost(uv, _uvB))
            return _quadHojasB;

        var m = new Mesh { name = "HojasBillboard" };
        m.vertices = new[]
        {
            new Vector3(-0.5f, 0f, 0f),
            new Vector3(0.5f, 0f, 0f),
            new Vector3(0.5f, 1f, 0f),
            new Vector3(-0.5f, 1f, 0f)
        };
        m.uv = new[]
        {
            new Vector2(uv.xMin, uv.yMin),
            new Vector2(uv.xMax, uv.yMin),
            new Vector2(uv.xMax, uv.yMax),
            new Vector2(uv.xMin, uv.yMax)
        };
        m.triangles = new[] { 0, 2, 1, 0, 3, 2, 0, 1, 2, 0, 2, 3 };
        m.RecalculateNormals();
        m.RecalculateBounds();

        if (_quadHojasA == null)
        {
            _quadHojasA = m;
            _uvA = uv;
        }
        else
        {
            _quadHojasB = m;
            _uvB = uv;
        }
        return m;
    }

    static bool Almost(Rect a, Rect b)
    {
        return Mathf.Abs(a.x - b.x) < 0.001f && Mathf.Abs(a.y - b.y) < 0.001f
            && Mathf.Abs(a.width - b.width) < 0.001f && Mathf.Abs(a.height - b.height) < 0.001f;
    }

    static void AplicarMaterialesArbol(GameObject inst)
    {
        var tronco = TroncoMat();
        var hojas = HojasMat();
        foreach (var r in inst.GetComponentsInChildren<Renderer>(true))
        {
            r.enabled = true;
            var filtro = r.GetComponent<MeshFilter>();
            var mesh = filtro != null ? filtro.sharedMesh : null;
            int subs = mesh != null ? mesh.subMeshCount : 1;
            var nombre = r.gameObject.name;
            bool hojasPorNombre = Contiene(nombre, "Leaf") || Contiene(nombre, "Hoja")
                || Contiene(nombre, "Leaves") || Contiene(nombre, "Mesh_1")
                || nombre.EndsWith("_1", System.StringComparison.Ordinal);

            if (subs >= 2)
            {
                var mats = new Material[subs];
                mats[0] = tronco;
                for (int i = 1; i < subs; i++)
                    mats[i] = hojas;
                r.sharedMaterials = mats;
            }
            else
            {
                r.sharedMaterial = hojasPorNombre ? hojas : tronco;
            }
        }
    }

    static bool Contiene(string haystack, string needle)
    {
        return !string.IsNullOrEmpty(haystack)
            && haystack.IndexOf(needle, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static GameObject CrearPalmera(GameObject prefab)
    {
        if (prefab != null)
        {
            var inst = Object.Instantiate(prefab);
            ReconectarMateriales.En(inst);
            AplicarMaterialesPalma(inst);
            NormalizarMesh(inst, 9f);
            AsegurarColliderTronco(inst);
            return inst;
        }

        var root = new GameObject("Palmera");
        var tronco = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tronco.name = "Tronco";
        tronco.transform.SetParent(root.transform, false);
        tronco.transform.localPosition = new Vector3(0f, 4.2f, 0f);
        tronco.transform.localScale = new Vector3(0.38f, 4.2f, 0.38f);
        AplicarMat(tronco, PalmeraMat());

        var fronda = PalmeraHojasMat();
        for (int i = 0; i < 6; i++)
        {
            var hoja = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hoja.name = "Fronda";
            hoja.transform.SetParent(root.transform, false);
            float a = i * 60f * Mathf.Deg2Rad;
            hoja.transform.localPosition = new Vector3(Mathf.Cos(a) * 1.4f, 8.4f, Mathf.Sin(a) * 1.4f);
            hoja.transform.localScale = new Vector3(0.12f, 0.35f, 2.8f);
            hoja.transform.localRotation = Quaternion.Euler(-28f, i * 60f, 0f);
            AplicarMat(hoja, fronda);
        }

        AsegurarColliderTronco(root);
        return root;
    }

    static void AplicarMaterialesPalma(GameObject inst)
    {
        var corteza = PalmeraMat();
        var fronda = PalmeraHojasMat();
        foreach (var r in inst.GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0)
            {
                r.sharedMaterial = corteza;
                continue;
            }

            var siguiente = new Material[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                var nombre = mats[i] != null ? mats[i].name : r.gameObject.name;
                bool hoja = Contiene(nombre, "Branch") || Contiene(nombre, "Leaf") || Contiene(nombre, "Hoja");
                siguiente[i] = hoja ? fronda : corteza;
            }
            r.sharedMaterials = siguiente;
        }
    }

    static GameObject CrearRoca(GameObject prefab)
    {
        if (prefab != null)
        {
            var inst = Object.Instantiate(prefab);
            AplicarMaterialesRoca(inst);
            NormalizarMesh(inst, 2.4f);
            AsegurarColliderRoca(inst);
            return inst;
        }

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Roca";
        go.transform.localScale = new Vector3(2.4f, 1.5f, 2.1f);
        go.transform.rotation = Quaternion.Euler(8f, 30f, -6f);
        AplicarMat(go, RocaMat());
        AsegurarColliderRoca(go);
        return go;
    }

    static void ColocarEnTerreno(GameObject go, Transform padre, Vector3 pos, float yaw, Vector3 escala, bool roca)
    {
        go.transform.SetParent(padre, true);
        // Keep FBX -90° on the root; replacing it with yaw-only lays Palm capsules on their side.
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f) * go.transform.rotation;
        go.transform.localScale = Vector3.Scale(go.transform.localScale, escala);
        SentarEnSuelo(go, pos);
        if (roca)
            AsegurarColliderRoca(go);
        else
            AsegurarColliderTronco(go);
    }

    static void SentarEnSuelo(GameObject go, Vector3 destino)
    {
        go.transform.position = destino;
        var b = BoundsDe(go);
        go.transform.position += new Vector3(0f, destino.y - b.min.y, 0f);
    }

    static void NormalizarMesh(GameObject go, float altoObjetivo)
    {
        foreach (var anim in go.GetComponentsInChildren<Animator>(true))
            Object.Destroy(anim);
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            r.enabled = true;

        var b = BoundsDe(go);
        if (b.size.y < 0.01f)
            return;
        go.transform.localScale *= altoObjetivo / b.size.y;
        b = BoundsDe(go);
        go.transform.position += Vector3.up * -b.min.y;
    }

    static Bounds BoundsDe(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0)
            return new Bounds(go.transform.position, Vector3.one);
        var b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++)
            b.Encapsulate(rs[i].bounds);
        return b;
    }

    static void AplicarMat(GameObject go, Material mat)
    {
        if (mat != null && go.TryGetComponent(out Renderer r))
            r.sharedMaterial = mat;
    }

    static Material TroncoMat()
    {
        if (_tronco == null)
            _tronco = MatDifuso("TroncoTerreno", CargarTex("Environment/BigTreeBark", null), new Color(0.38f, 0.24f, 0.12f));
        return _tronco;
    }

    static Material HojasMat()
    {
        if (_hojas == null)
            _hojas = MatCorte("HojasTerreno", CargarTex("Environment/BigTree", null), new Color(0.22f, 0.42f, 0.14f));
        return _hojas;
    }

    static Material PalmeraMat()
    {
        if (_palmera == null)
            _palmera = MatDifuso("PalmaTerreno", CargarTex("Environment/PalmBark", null), new Color(0.45f, 0.32f, 0.16f));
        return _palmera;
    }

    static Material PalmeraHojasMat()
    {
        if (_palmeraHojas == null)
            _palmeraHojas = MatCorte("PalmaHojas", CargarTex("Environment/PalmBranch", null), new Color(0.20f, 0.48f, 0.16f));
        return _palmeraHojas;
    }

    static void AplicarMaterialesRoca(GameObject inst)
    {
        var mat = RocaMat();
        foreach (var r in inst.GetComponentsInChildren<Renderer>(true))
        {
            r.enabled = true;
            r.sharedMaterial = mat;
        }
    }

    static void AsegurarColliderRoca(GameObject go)
    {
        QuitarColliders(go);
        bool any = false;
        foreach (var filter in go.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.sharedMesh == null)
                continue;
            if (!filter.gameObject.TryGetComponent(out MeshCollider col))
                col = filter.gameObject.AddComponent<MeshCollider>();
            col.sharedMesh = filter.sharedMesh;
            col.convex = filter.sharedMesh.vertexCount <= 255;
            col.isTrigger = false;
            any = true;
        }

        if (!any)
            AgregarCajaPorBounds(go);
    }

    static void AsegurarColliderTronco(GameObject go)
    {
        try
        {
            AsegurarColliderTroncoInterno(go);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("AmbienteTerreno collider " + go.name + ": " + e.Message);
        }
    }

    static void AsegurarColliderTroncoInterno(GameObject go)
    {
        QuitarColliders(go);

        bool soloTronco = TryBoundsTronco(go, out var world);
        if (!soloTronco)
            world = BoundsTroncoPorForma(go, out soloTronco);
        AplicarCapsulaTronco(go, world, soloTronco);
    }

    static Bounds BoundsTroncoPorForma(GameObject go, out bool soloTronco)
    {
        soloTronco = false;
        Bounds? mejor = null;
        float mejorRatio = 0f;
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (EsFollaje(r.gameObject.name) || EsTarjeta(r.bounds, r.gameObject.name))
                continue;
            var b = r.bounds;
            float xz = Mathf.Max(Mathf.Min(b.size.x, b.size.z), 0.05f);
            float ratio = b.size.y / xz;
            if (ratio > mejorRatio && b.size.y > 1.2f)
            {
                mejorRatio = ratio;
                mejor = b;
            }
        }

        if (mejor.HasValue && mejorRatio > 2.2f)
        {
            soloTronco = true;
            return mejor.Value;
        }

        return BoundsDe(go);
    }

    static bool EsCorteza(string nombre)
    {
        return Contiene(nombre, "Bark") || Contiene(nombre, "Tronco")
            || Contiene(nombre, "Trunk") || Contiene(nombre, "Cylinder")
            || Contiene(nombre, "PalmaTerreno") || Contiene(nombre, "Corteza");
    }

    static bool TryBoundsTronco(GameObject go, out Bounds world)
    {
        world = new Bounds();
        bool any = false;

        foreach (var filtro in go.GetComponentsInChildren<MeshFilter>(true))
        {
            var mesh = filtro.sharedMesh;
            if (mesh == null || mesh.vertexCount == 0)
                continue;

            filtro.TryGetComponent(out Renderer rend);
            var mats = rend != null ? rend.sharedMaterials : null;
            int subs = Mathf.Max(mesh.subMeshCount, 1);
            string objName = filtro.gameObject.name;
            bool objCorteza = EsCorteza(objName);
            bool objFollaje = EsFollaje(objName);

            for (int s = 0; s < subs; s++)
            {
                string matName = (mats != null && s < mats.Length && mats[s] != null)
                    ? mats[s].name
                    : objName;
                if (EsFollaje(matName)
                    || (objFollaje && !EsCorteza(matName)))
                    continue;

                bool incluir = objCorteza || EsCorteza(matName)
                    || (subs > 1 && !EsFollaje(matName));
                if (!incluir)
                    continue;

                if (!mesh.isReadable)
                {
                    if (rend == null || EsTarjeta(rend.bounds, objName))
                        continue;
                    if (!any)
                        world = rend.bounds;
                    else
                        world.Encapsulate(rend.bounds);
                    any = true;
                    continue;
                }

                int[] tris;
                try
                {
                    tris = mesh.GetTriangles(s);
                }
                catch (System.Exception)
                {
                    continue;
                }

                if (tris == null || tris.Length == 0)
                    continue;

                var verts = mesh.vertices;
                var used = new bool[verts.Length];
                for (int i = 0; i < tris.Length; i++)
                {
                    int idx = tris[i];
                    if ((uint)idx >= (uint)verts.Length || used[idx])
                        continue;
                    used[idx] = true;
                    var p = filtro.transform.TransformPoint(verts[idx]);
                    if (!any)
                    {
                        world = new Bounds(p, Vector3.zero);
                        any = true;
                    }
                    else
                        world.Encapsulate(p);
                }
            }
        }

        if (any)
            return true;

        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            if ((EsFollaje(r.gameObject.name) && !EsCorteza(r.gameObject.name))
                || EsTarjeta(r.bounds, r.gameObject.name))
                continue;
            bool corteza = EsCorteza(r.gameObject.name);
            if (!corteza && r.sharedMaterials != null)
            {
                for (int i = 0; i < r.sharedMaterials.Length; i++)
                {
                    if (r.sharedMaterials[i] != null && EsCorteza(r.sharedMaterials[i].name))
                    {
                        corteza = true;
                        break;
                    }
                }
            }
            if (!corteza)
                continue;
            if (!any)
                world = r.bounds;
            else
                world.Encapsulate(r.bounds);
            any = true;
        }

        return any;
    }

    static void AplicarCapsulaTronco(GameObject go, Bounds world, bool soloTronco)
    {
        var t = go.transform.Find("TroncoCollider");
        GameObject colGo;
        if (t == null)
        {
            colGo = new GameObject("TroncoCollider");
            t = colGo.transform;
        }
        else
            colGo = t.gameObject;

        float worldH;
        float worldR;
        Vector3 center;
        if (soloTronco)
        {
            worldH = Mathf.Clamp(world.size.y * 1.04f, 1.5f, 14f);
            worldR = Mathf.Clamp(Mathf.Min(world.size.x, world.size.z) * 0.48f, 0.14f, 1.15f);
            center = world.center;
        }
        else
        {
            worldH = Mathf.Clamp(world.size.y * 0.52f, 1.6f, 10f);
            worldR = Mathf.Clamp(Mathf.Min(world.size.x, world.size.z) * 0.12f, 0.16f, 0.45f);
            center = new Vector3(world.center.x, world.min.y + worldH * 0.5f, world.center.z);
        }

        // World-up capsule on a child. FBX roots are often rotated -90°,
        // so a capsule on Palm(Clone) itself lies on its side and misses the trunk.
        t.SetParent(null);
        t.position = center;
        t.rotation = Quaternion.identity;
        t.localScale = Vector3.one;
        t.SetParent(go.transform, true);

        if (!colGo.TryGetComponent(out CapsuleCollider cap))
            cap = colGo.AddComponent<CapsuleCollider>();

        var ls = t.lossyScale;
        float sy = Mathf.Max(Mathf.Abs(ls.y), 1e-4f);
        float sxz = Mathf.Max(Mathf.Abs(ls.x), Mathf.Abs(ls.z), 1e-4f);
        cap.direction = 1;
        cap.center = Vector3.zero;
        cap.height = worldH / sy;
        cap.radius = worldR / sxz;
        cap.isTrigger = false;
    }

    static void QuitarColliders(GameObject go)
    {
        var cols = go.GetComponentsInChildren<Collider>(true);
        for (int i = cols.Length - 1; i >= 0; i--)
            Object.DestroyImmediate(cols[i]);
    }

    static void AgregarCajaPorBounds(GameObject go)
    {
        if (!go.TryGetComponent(out BoxCollider box))
            box = go.AddComponent<BoxCollider>();
        var world = BoundsDe(go);
        box.center = go.transform.InverseTransformPoint(world.center);
        box.size = TamanoLocal(go.transform, world.size);
        box.isTrigger = false;
    }

    static Vector3 TamanoLocal(Transform t, Vector3 worldSize)
    {
        var s = t.lossyScale;
        return new Vector3(
            worldSize.x / Mathf.Max(Mathf.Abs(s.x), 1e-4f),
            worldSize.y / Mathf.Max(Mathf.Abs(s.y), 1e-4f),
            worldSize.z / Mathf.Max(Mathf.Abs(s.z), 1e-4f));
    }

    static bool EsFollaje(string nombre)
    {
        return Contiene(nombre, "Leaf") || Contiene(nombre, "Leaves")
            || Contiene(nombre, "Hoja") || Contiene(nombre, "Hojas")
            || Contiene(nombre, "Branch") || Contiene(nombre, "Fronda")
            || Contiene(nombre, "Mesh_1") || Contiene(nombre, "collider")
            || Contiene(nombre, "Card") || Contiene(nombre, "Billboard");
    }

    static bool EsTarjeta(Bounds b, string nombre)
    {
        if (EsCorteza(nombre))
            return false;
        float xz = Mathf.Max(b.size.x, b.size.z, 0.05f);
        float thin = Mathf.Min(b.size.x, Mathf.Min(b.size.y, b.size.z));
        return b.size.y < xz * 0.22f || thin < xz * 0.08f;
    }

    static Material RocaMat()
    {
        if (_roca != null)
            return _roca;

        var tex = CargarTex("Textures/Cliff", "Assets/Art/Textures/Cliff (Layered Rock).jpg")
            ?? CargarTex("Textures/Cliff", "Assets/Resources/Textures/Cliff.jpg");
        var shader = Shader.Find("Standard")
            ?? Shader.Find("Legacy Shaders/Diffuse")
            ?? Shader.Find("Diffuse");
        _roca = new Material(shader) { name = "RocaTerreno" };
        if (tex != null)
        {
            _roca.mainTexture = tex;
            if (_roca.HasProperty("_MainTex"))
                _roca.SetTexture("_MainTex", tex);
            if (_roca.HasProperty("_Color"))
                _roca.color = Color.white;
        }
        else if (_roca.HasProperty("_Color"))
            _roca.color = new Color(0.42f, 0.38f, 0.34f);

        if (_roca.HasProperty("_Glossiness"))
            _roca.SetFloat("_Glossiness", 0.12f);
        if (_roca.HasProperty("_Metallic"))
            _roca.SetFloat("_Metallic", 0.05f);
        if (_roca.HasProperty("_Smoothness"))
            _roca.SetFloat("_Smoothness", 0.12f);
        return _roca;
    }

    static Material MatDifuso(string nombre, Texture tex, Color color)
    {
        var shader = Shader.Find("Legacy Shaders/Diffuse") ?? Shader.Find("Diffuse") ?? Shader.Find("Standard");
        var mat = new Material(shader) { name = nombre };
        if (tex != null)
        {
            mat.mainTexture = tex;
            if (mat.HasProperty("_Color"))
                mat.color = Color.white;
        }
        else if (mat.HasProperty("_Color"))
            mat.color = color;
        return mat;
    }

    static Material MatCorte(string nombre, Texture tex, Color color)
    {
        var shader = Shader.Find("Legacy Shaders/Transparent/Cutout/Diffuse")
            ?? Shader.Find("Transparent/Cutout/Diffuse")
            ?? Shader.Find("Legacy Shaders/Diffuse")
            ?? Shader.Find("Standard");
        var mat = new Material(shader) { name = nombre };
        if (tex != null)
        {
            mat.mainTexture = tex;
            if (mat.HasProperty("_Color"))
                mat.color = Color.white;
        }
        else if (mat.HasProperty("_Color"))
            mat.color = color;
        if (mat.HasProperty("_Cutoff"))
            mat.SetFloat("_Cutoff", 0.35f);
        if (mat.HasProperty("_Cull"))
            mat.SetInt("_Cull", 0);
        return mat;
    }

    static Texture2D CargarTex(string resourcesPath, string assetPath)
    {
        var tex = Resources.Load<Texture2D>(resourcesPath);
        if (tex != null)
            return tex;
#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(assetPath))
            tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
#endif
        return tex;
    }
}
