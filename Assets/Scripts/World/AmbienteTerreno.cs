using System.Collections.Generic;
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
    public const string NombreRaizEntorno = "Entorno";
    public const string NombrePadreVegetacion = "ArbolesYRocas";

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
        }

        try
        {
            if (!HayVegetacionEnEscena())
                ColocarVegetacion();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("AmbienteTerreno vegetacion: " + e.Message);
        }

        CrearLimites();
    }

    public static bool HayVegetacionEnEscena()
    {
        var padre = BuscarPadreVegetacion();
        return padre != null && padre.childCount > 0;
    }

    public static Transform BuscarPadreVegetacion()
    {
        var entorno = GameObject.Find(NombreRaizEntorno);
        if (entorno != null)
        {
            var hijo = entorno.transform.Find(NombrePadreVegetacion);
            if (hijo != null)
                return hijo;
        }

        var plano = GameObject.Find(NombrePadreVegetacion);
        return plano != null ? plano.transform : null;
    }

    public static Transform AsegurarPadreVegetacion()
    {
        var entorno = GameObject.Find(NombreRaizEntorno);
        if (entorno == null)
            entorno = new GameObject(NombreRaizEntorno);

        var padre = entorno.transform.Find(NombrePadreVegetacion);
        if (padre == null)
        {
            var go = new GameObject(NombrePadreVegetacion);
            go.transform.SetParent(entorno.transform, false);
            padre = go.transform;
        }

        return padre;
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
        if (terrain != null)
            return terrain.SampleHeight(mundo) + terrain.transform.position.y;
        return AlturaDesdeHeightmap(mundo);
    }

    static float AlturaDesdeHeightmap(Vector3 mundo)
    {
        var heights = HeightmapCache();
        if (heights == null)
            return 0f;

        var origen = OrigenTerreno();
        float u = Mathf.Clamp01((mundo.x - origen.x) / Ancho);
        float v = Mathf.Clamp01((mundo.z - origen.z) / Largo);
        float fx = u * (Resolucion - 1);
        float fz = v * (Resolucion - 1);
        int x0 = Mathf.FloorToInt(fx);
        int z0 = Mathf.FloorToInt(fz);
        int x1 = Mathf.Min(x0 + 1, Resolucion - 1);
        int z1 = Mathf.Min(z0 + 1, Resolucion - 1);
        float tx = fx - x0;
        float tz = fz - z0;
        float h = Mathf.Lerp(
            Mathf.Lerp(heights[z0, x0], heights[z0, x1], tx),
            Mathf.Lerp(heights[z1, x0], heights[z1, x1], tx),
            tz);
        return TerrenoY + h * AltoOriginal;
    }

    static float[,] _heightmapCache;

    static float[,] HeightmapCache()
    {
        if (_heightmapCache != null)
            return _heightmapCache;

        var raw = Resources.Load<TextAsset>("Terrain/originalHeightmap");
        if (raw == null || raw.bytes == null || raw.bytes.Length < Resolucion * Resolucion * 2)
            return null;

        var bytes = raw.bytes;
        _heightmapCache = new float[Resolucion, Resolucion];
        int i = 0;
        for (int z = 0; z < Resolucion; z++)
        {
            for (int x = 0; x < Resolucion; x++)
            {
                ushort h = (ushort)(bytes[i] | (bytes[i + 1] << 8));
                i += 2;
                _heightmapCache[z, x] = h / 65535f;
            }
        }

        return _heightmapCache;
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
            smoothness = 0f,
            specular = Color.black,
            diffuseRemapMin = new Vector4(0.06f, 0.08f, 0.04f, 0f),
            diffuseRemapMax = new Vector4(0.86f, 0.90f, 0.72f, 1f)
        };
        var cliff = new TerrainLayer
        {
            diffuseTexture = acantilado,
            tileSize = new Vector2(18f, 18f),
            metallic = 0f,
            smoothness = 0.04f,
            specular = new Color(0.08f, 0.08f, 0.08f, 1f)
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

    static void ColocarVegetacion()
    {
        PoblarVegetacion(AsegurarPadreVegetacion());
    }

    public static int PoblarVegetacionConAltura(Transform padre)
    {
        GameObject temporal = null;
        try
        {
            if (Terrain.activeTerrain == null)
                temporal = CrearTerrenoSoloAltura();
            return PoblarVegetacion(padre);
        }
        finally
        {
            if (temporal != null)
            {
                var terrain = temporal.GetComponent<Terrain>();
                var data = terrain != null ? terrain.terrainData : null;
                Object.DestroyImmediate(temporal);
                if (data != null)
                    Object.DestroyImmediate(data);
            }
        }
    }

    static GameObject CrearTerrenoSoloAltura()
    {
        var data = new TerrainData
        {
            heightmapResolution = Resolucion,
            size = new Vector3(Ancho, AltoOriginal, Largo)
        };
        data.hideFlags = HideFlags.HideAndDontSave;
        if (!AplicarHeightmapOriginal(data))
            GenerarColinasAlrededor(data);

        var go = Terrain.CreateTerrainGameObject(data);
        go.name = "TerrenoBakeTemporal";
        go.hideFlags = HideFlags.HideAndDontSave;
        go.transform.position = OrigenTerreno();
        return go;
    }

    public static int PoblarVegetacion(Transform padre)
    {
        var raw = Resources.Load<TextAsset>("Terrain/originalTrees");
        int colocados = 0;
        if (raw != null && raw.bytes != null && raw.bytes.Length >= 4)
            colocados = ColocarArbolesOriginales(padre, raw.bytes);

        if (colocados < 40)
            ColocarArbolesRespaldo(padre);
        return padre.childCount;
    }

    static int ColocarArbolesOriginales(Transform padre, byte[] bytes)
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

    static void ColocarArbolesRespaldo(Transform padre)
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
                Destruir(anim);
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
        var importRot = go.transform.localRotation;
        var importScale = go.transform.localScale;

        // Yaw-only wrapper so the capsule stays world-up. FBX -90° stays on the visual
        // child; scaling the wrapper (not the FBX) keeps height on world Y.
        var wrapper = new GameObject(go.name);
        wrapper.transform.SetParent(padre, false);
        wrapper.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        wrapper.transform.localScale = escala;

        go.name = "Modelo";
        go.transform.SetParent(wrapper.transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = importRot;
        go.transform.localScale = importScale;

        SentarEnSuelo(wrapper, pos);
        if (roca)
            AsegurarColliderRoca(go);
        else
            AsegurarColliderTronco(wrapper);
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.Undo.RegisterCreatedObjectUndo(wrapper, "Bake Trees And Rocks");
#endif
    }

    static void Destruir(Object obj)
    {
        if (obj == null)
            return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Object.DestroyImmediate(obj);
            return;
        }
#endif
        Object.Destroy(obj);
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
            Destruir(anim);
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
            _tronco = Resources.Load<Material>("Environment/Materials/TroncoTerreno");
        if (_tronco == null)
            _tronco = MatDifuso("TroncoTerreno", CargarTex("Environment/BigTreeBark", null), new Color(0.38f, 0.24f, 0.12f));
        return _tronco;
    }

    static Material HojasMat()
    {
        if (_hojas == null)
            _hojas = Resources.Load<Material>("Environment/Materials/HojasTerreno");
        if (_hojas == null)
            _hojas = MatCorte("HojasTerreno", CargarTex("Environment/BigTree", null), new Color(0.22f, 0.42f, 0.14f));
        return _hojas;
    }

    static Material PalmeraMat()
    {
        if (_palmera == null)
            _palmera = Resources.Load<Material>("Environment/Materials/PalmaTerreno");
        if (_palmera == null)
            _palmera = MatDifuso("PalmaTerreno", CargarTex("Environment/PalmBark", null), new Color(0.45f, 0.32f, 0.16f));
        return _palmera;
    }

    static Material PalmeraHojasMat()
    {
        if (_palmeraHojas == null)
            _palmeraHojas = Resources.Load<Material>("Environment/Materials/PalmaHojas");
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
        DesactivarMeshCollidersHojas(go);

        var world = BoundsTroncoVertical(go, out bool arbusto, out float radioRaiz);
        AplicarCapsulaTronco(go, world, arbusto, radioRaiz);
    }

    static bool EsCorteza(string nombre)
    {
        return Contiene(nombre, "Bark") || Contiene(nombre, "Tronco")
            || Contiene(nombre, "Trunk") || Contiene(nombre, "Cylinder")
            || Contiene(nombre, "PalmaTerreno") || Contiene(nombre, "Corteza");
    }

    static Bounds BoundsTroncoVertical(GameObject go, out bool arbusto, out float radioRaiz)
    {
        arbusto = false;
        radioRaiz = 0f;
        var puntos = new List<Vector3>(512);
        RecolectarPuntosTronco(go, puntos);

        if (puntos.Count < 6)
        {
            arbusto = true;
            var full = BoundsDe(go);
            return new Bounds(
                new Vector3(full.center.x, full.min.y + 0.52f, full.center.z),
                new Vector3(0.68f, 1.05f, 0.68f));
        }

        float yMin = puntos[0].y;
        float yMax = puntos[0].y;
        for (int i = 1; i < puntos.Count; i++)
        {
            float y = puntos[i].y;
            if (y < yMin) yMin = y;
            if (y > yMax) yMax = y;
        }

        float alto = yMax - yMin;
        if (alto < 0.35f)
        {
            arbusto = true;
            float cx0 = 0f, cz0 = 0f;
            for (int i = 0; i < puntos.Count; i++)
            {
                cx0 += puntos[i].x;
                cz0 += puntos[i].z;
            }
            cx0 /= puntos.Count;
            cz0 /= puntos.Count;
            return new Bounds(new Vector3(cx0, yMin + 0.52f, cz0), new Vector3(0.68f, 1.05f, 0.68f));
        }

        float yLo = yMin + alto * 0.03f;
        float yHi = yMin + Mathf.Max(0.4f, alto * 0.22f);
        float cx = 0f, cz = 0f;
        int nLow = 0;
        var radios = new List<float>(128);
        for (int i = 0; i < puntos.Count; i++)
        {
            var p = puntos[i];
            if (p.y < yLo || p.y > yHi)
                continue;
            cx += p.x;
            cz += p.z;
            nLow++;
        }

        if (nLow < 4)
        {
            cx = 0f;
            cz = 0f;
            nLow = puntos.Count;
            for (int i = 0; i < puntos.Count; i++)
            {
                cx += puntos[i].x;
                cz += puntos[i].z;
            }
        }

        cx /= nLow;
        cz /= nLow;

        for (int i = 0; i < puntos.Count; i++)
        {
            var p = puntos[i];
            if (p.y < yLo || p.y > yHi)
                continue;
            float dx = p.x - cx;
            float dz = p.z - cz;
            radios.Add(Mathf.Sqrt(dx * dx + dz * dz));
        }

        if (radios.Count == 0)
        {
            for (int i = 0; i < puntos.Count; i++)
            {
                float dx = puntos[i].x - cx;
                float dz = puntos[i].z - cz;
                radios.Add(Mathf.Sqrt(dx * dx + dz * dz));
            }
        }

        float rBase = Mathf.Max(Percentil(radios, 0.8f), 0.12f);
        float rLim = rBase * 2.55f;
        float rRaiz = RadioRaizEnBase(puntos, cx, cz, yMin, alto, rBase);
        float yTronco = yMin;
        const int bands = 18;
        for (int b = 0; b < bands; b++)
        {
            float a = yMin + alto * (b / (float)bands);
            float c = yMin + alto * ((b + 1) / (float)bands);
            int count = 0;
            float sumR = 0f;
            float maxR = 0f;
            for (int i = 0; i < puntos.Count; i++)
            {
                var p = puntos[i];
                if (p.y < a || p.y > c)
                    continue;
                float dx = p.x - cx;
                float dz = p.z - cz;
                float r = Mathf.Sqrt(dx * dx + dz * dz);
                sumR += r;
                if (r > maxR) maxR = r;
                count++;
            }

            if (count < 3)
                continue;

            float medio = sumR / count;
            bool copa = b > 2 && (medio > rLim || maxR > rBase * 4.2f);
            if (copa)
                break;
            yTronco = c;
        }

        float h = yTronco - yMin;
        if (h < 1.15f || (h < 2.15f && rBase > h * 0.48f))
        {
            arbusto = true;
            return new Bounds(new Vector3(cx, yMin + 0.52f, cz), new Vector3(0.68f, 1.05f, 0.68f));
        }

        float radius = Mathf.Clamp(rBase * 1.06f, 0.16f, 1.4f);
        float height = Mathf.Clamp(h * 1.02f, 1.3f, 16f);
        radioRaiz = Mathf.Clamp(Mathf.Max(rRaiz, radius * 1.55f), 0.28f, 2.45f);
        return new Bounds(
            new Vector3(cx, yMin + height * 0.5f, cz),
            new Vector3(radius * 2f, height, radius * 2f));
    }

    static float RadioRaizEnBase(List<Vector3> puntos, float cx, float cz, float yMin, float alto, float rTronco)
    {
        float yHi = yMin + Mathf.Min(1.2f, Mathf.Max(0.7f, alto * 0.10f));
        var radios = new List<float>(64);
        for (int i = 0; i < puntos.Count; i++)
        {
            var p = puntos[i];
            if (p.y < yMin - 0.02f || p.y > yHi)
                continue;
            float dx = p.x - cx;
            float dz = p.z - cz;
            radios.Add(Mathf.Sqrt(dx * dx + dz * dz));
        }

        if (radios.Count == 0)
            return rTronco * 1.7f;

        float p80 = Percentil(radios, 0.80f);
        float p95 = Percentil(radios, 0.95f);
        return Mathf.Max(p95 * 1.06f, p80 * 1.18f, rTronco * 1.55f);
    }

    static void RecolectarPuntosTronco(GameObject go, List<Vector3> puntos)
    {
        foreach (var filtro in go.GetComponentsInChildren<MeshFilter>(true))
            RecolectarDeMesh(filtro.gameObject, filtro.sharedMesh, filtro.transform.localToWorldMatrix, puntos);
        foreach (var skin in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            RecolectarDeMesh(skin.gameObject, skin.sharedMesh, skin.localToWorldMatrix, puntos);
    }

    static void RecolectarDeMesh(GameObject host, Mesh mesh, Matrix4x4 l2w, List<Vector3> puntos)
    {
        if (mesh == null || mesh.vertexCount == 0)
            return;
        if (host.name == "TroncoCollider" || host.name == "RaizCollider")
            return;
        if (EsFollaje(host.name) && !EsCorteza(host.name))
            return;
        if (host.TryGetComponent(out Renderer rend) && EsTarjeta(rend.bounds, host.name))
            return;

        host.TryGetComponent(out Renderer r);
        var mats = r != null ? r.sharedMaterials : null;
        int subs = Mathf.Max(mesh.subMeshCount, 1);

        if (!mesh.isReadable)
        {
            if (r != null)
                AgregarEsquinas(r.bounds, puntos);
            return;
        }

        Vector3[] verts;
        try
        {
            verts = mesh.vertices;
        }
        catch (System.Exception)
        {
            return;
        }

        if (verts == null || verts.Length == 0)
            return;

        for (int s = 0; s < subs; s++)
        {
            string matName = (mats != null && s < mats.Length && mats[s] != null)
                ? mats[s].name
                : host.name;
            if (EsFollaje(matName) && !EsCorteza(matName))
                continue;

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

            int stride = tris.Length > 18000 ? 12 : 3;
            for (int i = 0; i < tris.Length; i += stride)
            {
                int idx = tris[i];
                if ((uint)idx >= (uint)verts.Length)
                    continue;
                puntos.Add(l2w.MultiplyPoint3x4(verts[idx]));
            }
        }
    }

    static void AgregarEsquinas(Bounds b, List<Vector3> puntos)
    {
        var e = b.extents;
        var c = b.center;
        puntos.Add(c + new Vector3(-e.x, -e.y, -e.z));
        puntos.Add(c + new Vector3(e.x, -e.y, -e.z));
        puntos.Add(c + new Vector3(-e.x, -e.y, e.z));
        puntos.Add(c + new Vector3(e.x, -e.y, e.z));
        puntos.Add(c + new Vector3(-e.x, e.y, -e.z));
        puntos.Add(c + new Vector3(e.x, e.y, -e.z));
        puntos.Add(c + new Vector3(-e.x, e.y, e.z));
        puntos.Add(c + new Vector3(e.x, e.y, e.z));
    }

    static float Percentil(List<float> values, float p)
    {
        if (values.Count == 0)
            return 0f;
        values.Sort();
        int i = Mathf.Clamp(Mathf.RoundToInt((values.Count - 1) * p), 0, values.Count - 1);
        return values[i];
    }

    static void AplicarCapsulaTronco(GameObject go, Bounds world, bool arbusto, float radioRaiz)
    {
        var hijos = go.GetComponentsInChildren<Transform>(true);
        for (int i = hijos.Length - 1; i >= 0; i--)
        {
            if (hijos[i] == null || hijos[i] == go.transform)
                continue;
            if (hijos[i].name == "TroncoCollider" || hijos[i].name == "RaizCollider")
                Object.DestroyImmediate(hijos[i].gameObject);
        }

        if (!go.TryGetComponent(out CapsuleCollider cap))
            cap = go.AddComponent<CapsuleCollider>();

        float worldH;
        float worldR;
        Vector3 center;
        if (arbusto)
        {
            worldH = 1.05f;
            worldR = 0.34f;
            center = new Vector3(world.center.x, world.min.y + worldH * 0.5f, world.center.z);
        }
        else
        {
            worldH = Mathf.Clamp(world.size.y, 1.3f, 16f);
            worldR = Mathf.Clamp(Mathf.Min(world.size.x, world.size.z) * 0.5f, 0.16f, 1.4f);
            center = new Vector3(world.center.x, world.min.y + worldH * 0.5f, world.center.z);
        }

        cap.direction = 1;
        cap.isTrigger = false;
        cap.center = go.transform.InverseTransformPoint(center);

        var ls = go.transform.lossyScale;
        float sy = Mathf.Max(Mathf.Abs(ls.y), 1e-4f);
        float sxz = Mathf.Max(Mathf.Abs(ls.x), Mathf.Abs(ls.z), 1e-4f);
        cap.height = worldH / sy;
        cap.radius = worldR / sxz;

        if (!arbusto)
            AplicarColliderRaiz(go, world.center.x, world.center.z, world.min.y, radioRaiz);
    }

    static void AplicarColliderRaiz(GameObject go, float cx, float cz, float yMin, float radioMundo)
    {
        float r = Mathf.Clamp(radioMundo, 0.28f, 2.45f);
        var host = new GameObject("RaizCollider");
        host.transform.SetParent(go.transform, false);

        var esfera = host.AddComponent<SphereCollider>();
        esfera.isTrigger = false;
        var centro = new Vector3(cx, yMin + r * 0.42f, cz);
        esfera.center = host.transform.InverseTransformPoint(centro);

        var ls = host.transform.lossyScale;
        float s = Mathf.Max(Mathf.Abs(ls.x), Mathf.Abs(ls.y), Mathf.Abs(ls.z), 1e-4f);
        esfera.radius = r / s;
    }

    static void DesactivarMeshCollidersHojas(GameObject go)
    {
        var cols = go.GetComponentsInChildren<MeshCollider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            var col = cols[i];
            if (col == null)
                continue;
            if (EsFollaje(col.gameObject.name) && !EsCorteza(col.gameObject.name))
                col.enabled = false;
        }
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

        _roca = Resources.Load<Material>("Environment/Materials/RocaTerreno");
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

    public static void InvalidarCacheMateriales()
    {
        _tronco = null;
        _hojas = null;
        _palmera = null;
        _palmeraHojas = null;
        _roca = null;
    }

#if UNITY_EDITOR
    public static void AsegurarMaterialesPersistentes()
    {
        InvalidarCacheMateriales();
        var _ = TroncoMat();
        _ = HojasMat();
        _ = PalmeraMat();
        _ = PalmeraHojasMat();
        _ = RocaMat();

        const string env = "Assets/Resources/Environment";
        const string folder = env + "/Materials";
        if (!UnityEditor.AssetDatabase.IsValidFolder(env))
            UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", "Environment");
        if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
            UnityEditor.AssetDatabase.CreateFolder(env, "Materials");

        _tronco = PersistirMaterial(_tronco, folder + "/TroncoTerreno.mat");
        _hojas = PersistirMaterial(_hojas, folder + "/HojasTerreno.mat");
        _palmera = PersistirMaterial(_palmera, folder + "/PalmaTerreno.mat");
        _palmeraHojas = PersistirMaterial(_palmeraHojas, folder + "/PalmaHojas.mat");
        _roca = PersistirMaterial(_roca, folder + "/RocaTerreno.mat");
        UnityEditor.AssetDatabase.SaveAssets();
    }

    static Material PersistirMaterial(Material mat, string path)
    {
        if (mat == null)
            return null;
        if (!string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(mat)))
            return mat;

        var existing = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
            return existing;

        UnityEditor.AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
#endif
}
