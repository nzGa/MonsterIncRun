using System;
using System.Collections.Generic;
using UnityEngine;

public static class ReconectarMateriales
{
    static readonly Color VerdePiel = new Color(0.50f, 0.78f, 0.15f);
    static readonly Color ColorPipe1 = new Color(0.78f, 0.78f, 0.76f);
    static readonly Color ColorPipe2 = new Color(0.76f, 0.77f, 0.80f);
    static readonly Color EmisionPipe = new Color(0.14f, 0.14f, 0.145f);

    // FBX ByPolygon / connection order on the skinned "Mike" mesh.
    static readonly string[] SlotsMikePorIndice =
    {
        "Piel", "Unias", "Lengua", "Paladar", "Ojo", "Dientes"
    };

    static Dictionary<string, Material> _porNombre;
    static Material[] _mike;
    static Material _pielFallback;
    static Texture _texMetal;

    public static void En(GameObject root)
    {
        if (root == null)
            return;

        var catalogo = Catalogo();

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer is ParticleSystemRenderer || renderer.GetComponent<TextMesh>() != null)
                continue;

            var shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0)
                continue;

            bool changed = false;
            for (int i = 0; i < shared.Length; i++)
            {
                var actual = shared[i];
                var reemplazo = BuscarReemplazo(catalogo, actual, renderer);
                if (EsTuberia(actual, renderer) || PareceRielOscuro(actual, renderer))
                {
                    var forzado = MaterialTuberia(EsPipe2(actual, renderer));
                    if (forzado != null)
                        reemplazo = forzado;
                }

                if (reemplazo != null && reemplazo != actual)
                {
                    shared[i] = reemplazo;
                    changed = true;
                }

                if (AsegurarTexturaTuberia(shared[i], renderer))
                    changed = true;
                else if (Reparar(shared[i] != null ? shared[i] : actual))
                    changed = true;
            }

            if (changed)
                renderer.sharedMaterials = shared;
        }
    }

    public static void EnMike(GameObject root)
    {
        if (root == null)
            return;

        var mats = MaterialesMike();
        var paladar = BuscarMike(mats, "Paladar");
        var lengua = BuscarMike(mats, "Lengua");
        var dientes = BuscarMike(mats, "Dientes");
        var unias = BuscarMike(mats, "Unias");
        var ojo = AsegurarOjo(BuscarMike(mats, "Ojo")
            ?? Resources.Load<Material>("Models/Mike/Materials/Ojo"));
        var piel = AsegurarPiel(
            BuscarMike(mats, "Piel") ?? Resources.Load<Material>("Models/Mike/Materials/Piel"),
            paladar, lengua, ojo);
        AsegurarMaterialBoca(lengua, new Color(0.75f, 0.22f, 0.28f), piel, ojo);
        AsegurarMaterialBoca(paladar, new Color(0.65f, 0.20f, 0.22f), piel, ojo);
        AsegurarMaterialBoca(dientes, Color.white, piel, ojo);
        AsegurarMaterialBoca(unias, Color.white, piel, ojo);

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.GetComponent<TextMesh>() != null)
                continue;

            if (EsSplineAyuda(renderer))
            {
                renderer.enabled = false;
                continue;
            }

            var shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0)
            {
                var unico = ElegirMatMike(mats, null, renderer, piel, ojo, 0, 0, -1);
                if (unico != null)
                    renderer.sharedMaterial = unico;
                continue;
            }

            int mayorSub = IndiceSubmeshMayor(renderer);
            int discoOjo = IndiceDiscoFacial(renderer);
            bool changed = false;
            var siguiente = (Material[])shared.Clone();
            for (int i = 0; i < shared.Length; i++)
            {
                var elegido = ElegirMatMike(mats, shared[i], renderer, piel, ojo, i, mayorSub, discoOjo);
                if (elegido != null && elegido != siguiente[i])
                {
                    siguiente[i] = elegido;
                    changed = true;
                }
                else if (Reparar(siguiente[i]))
                    changed = true;
            }

            if (ForzarPielEnCuerpo(siguiente, renderer, piel, discoOjo))
                changed = true;
            if (ForzarOjoEnDisco(siguiente, renderer, ojo))
                changed = true;

            if (changed)
                renderer.sharedMaterials = siguiente;
        }
    }

    public static bool EsNombreOjo(string nombre)
    {
        if (string.IsNullOrEmpty(nombre))
            return false;
        return Contiene(nombre, "Ojo")
            || Contiene(nombre, "Eye")
            || Contiene(nombre, "Pupil")
            || Contiene(nombre, "Iris");
    }

    static Material[] MaterialesMike()
    {
        if (_mike != null)
            return _mike;

        var cargados = Resources.LoadAll<Material>("Models/Mike/Materials");
        if (cargados == null)
            cargados = Array.Empty<Material>();

        var lista = new List<Material>(cargados.Length);
        foreach (var mat in cargados)
        {
            if (mat == null)
                continue;
            AmbienteVisual.RepararShader(mat);
            lista.Add(mat);
        }

        lista.Sort((a, b) => b.name.Length.CompareTo(a.name.Length));
        _mike = lista.ToArray();
        return _mike;
    }

    public static int IndiceDiscoFacial(Renderer renderer)
    {
        var mesh = MeshDe(renderer);
        if (mesh == null || mesh.subMeshCount < 2)
            return -1;

        int mayor = IndiceSubmeshMayor(renderer);
        float cuerpo = ExtensionSubmesh(mesh, mayor);
        int mejor = -1;
        float mejorVol = -1f;
        int limite = Mathf.Min(mesh.subMeshCount, renderer.sharedMaterials != null
            ? renderer.sharedMaterials.Length
            : mesh.subMeshCount);

        for (int i = 0; i < limite; i++)
        {
            if (i == mayor)
                continue;
            float ext = ExtensionSubmesh(mesh, i);
            if (cuerpo > 0.001f && ext > cuerpo * 0.55f)
                continue;
            float vol = VolumenSubmesh(mesh, i);
            if (vol > mejorVol)
            {
                mejorVol = vol;
                mejor = i;
            }
        }

        return mejor;
    }

    public static bool ForzarOjoEnDisco(Material[] slots, Renderer renderer, Material ojo)
    {
        if (slots == null || ojo == null)
            return false;

        int disco = IndiceDiscoFacial(renderer);
        if (disco < 0 || disco >= slots.Length)
            disco = IndicePaladarGrande(slots, renderer);
        if (disco < 0 || disco >= slots.Length)
            return false;
        int mayor = IndiceSubmeshMayor(renderer);
        if (disco == mayor)
            return false;
        if (slots[disco] == ojo)
            return false;

        slots[disco] = ojo;
        return true;
    }

    static int IndicePaladarGrande(Material[] slots, Renderer renderer)
    {
        var mesh = MeshDe(renderer);
        if (mesh == null || slots == null)
            return -1;

        int mayor = IndiceSubmeshMayor(renderer);
        int mejor = -1;
        float mejorVol = -1f;
        int limite = Mathf.Min(slots.Length, mesh.subMeshCount);
        for (int i = 0; i < limite; i++)
        {
            if (i == mayor)
                continue;
            var nombre = NombreMaterial(slots[i]);
            if (!EsPaladar(nombre) && !EsLengua(nombre) && !EsNombreOjo(nombre))
                continue;
            float vol = VolumenSubmesh(mesh, i);
            if (vol > mejorVol)
            {
                mejorVol = vol;
                mejor = i;
            }
        }

        return mejor;
    }

    static bool ForzarPielEnCuerpo(Material[] slots, Renderer renderer, Material piel, int discoOjo)
    {
        if (slots == null || piel == null)
            return false;

        int mayor = IndiceSubmeshMayor(renderer);
        if (mayor < 0 || mayor >= slots.Length)
            return false;
        if (discoOjo >= 0 && mayor == discoOjo)
            return false;
        if (slots[mayor] == piel)
            return false;

        slots[mayor] = piel;
        return true;
    }

    static Material ElegirMatMike(
        Material[] mats,
        Material actual,
        Renderer renderer,
        Material piel,
        Material ojo,
        int slot,
        int mayorSub,
        int discoOjo)
    {
        var nombre = NombreMaterial(actual);
        var rendererName = renderer != null ? renderer.gameObject.name : null;
        int slots = renderer != null && renderer.sharedMaterials != null
            ? renderer.sharedMaterials.Length
            : 1;

        if (discoOjo >= 0 && slot == discoOjo)
            return ojo ?? piel;

        if (EsNombreOjo(rendererName) && slots <= 1)
            return ojo ?? piel;

        if (slots > 1 && slot == mayorSub)
            return piel;

        if (EsCuerpoEntero(renderer, slots))
            return piel;

        var porIndice = SlotPorIndice(mats, slot, slots);
        if (porIndice != null && !EsNombreOjo(porIndice.name) && !EsPiel(porIndice.name))
            return porIndice;

        if (slot != discoOjo)
        {
            var parte = MatchParteEspecifica(mats, nombre, rendererName);
            if (parte != null)
                return parte;
        }

        if (EsNombreOjo(nombre))
            return ojo ?? piel;

        return MatchMikeNoOjo(mats, nombre, rendererName) ?? piel;
    }

    static Material SlotPorIndice(Material[] mats, int slot, int slots)
    {
        if (slots < 6 || slot < 0 || slot >= SlotsMikePorIndice.Length)
            return null;
        return BuscarMike(mats, SlotsMikePorIndice[slot]);
    }

    static Material MatchParteEspecifica(Material[] mats, string nombreSlot, string rendererName)
    {
        foreach (var mat in mats)
        {
            if (mat == null || EsNombreOjo(mat.name) || EsPiel(mat.name))
                continue;
            if (NombreCoincide(nombreSlot, mat.name))
                return mat;
            if (AliasParteCoincide(nombreSlot, mat.name) || AliasParteCoincide(rendererName, mat.name))
                return mat;
        }
        return null;
    }

    static bool AliasParteCoincide(string haystack, string matName)
    {
        if (string.IsNullOrEmpty(haystack) || string.IsNullOrEmpty(matName))
            return false;
        if (EsPiel(matName) || EsNombreOjo(matName))
            return false;
        if (EsLengua(matName))
            return Contiene(haystack, "Lengua") || Contiene(haystack, "Tongue");
        if (EsPaladar(matName))
            return Contiene(haystack, "Paladar") || Contiene(haystack, "Palate");
        if (Contiene(matName, "Diente"))
            return Contiene(haystack, "Diente") || Contiene(haystack, "Teeth") || Contiene(haystack, "Tooth");
        if (Contiene(matName, "Unia"))
            return Contiene(haystack, "Unia") || Contiene(haystack, "Nail") || Contiene(haystack, "Claw");
        return false;
    }

    static bool EsPiel(string nombre) => Contiene(nombre, "Piel") || Contiene(nombre, "Skin");
    static bool EsLengua(string nombre) => Contiene(nombre, "Lengua") || Contiene(nombre, "Tongue");
    static bool EsPaladar(string nombre) => Contiene(nombre, "Paladar") || Contiene(nombre, "Palate");

    static Material MatchMikeNoOjo(Material[] mats, string nombreSlot, string rendererName)
    {
        foreach (var mat in mats)
        {
            if (mat == null || EsNombreOjo(mat.name))
                continue;
            if (EsPiel(nombreSlot) && (EsPaladar(mat.name) || EsLengua(mat.name)))
                continue;
            if (NombreCoincide(nombreSlot, mat.name) || NombreCoincide(rendererName, mat.name))
                return mat;
        }
        return null;
    }

    static Material BuscarMike(Material[] mats, string nombre)
    {
        if (mats == null)
            return null;
        foreach (var mat in mats)
        {
            if (mat != null && string.Equals(mat.name, nombre, StringComparison.OrdinalIgnoreCase))
                return mat;
        }
        return null;
    }

    static Material AsegurarPiel(Material piel, Material paladar, Material lengua, Material ojo)
    {
        if (piel != null && (ReferenceEquals(piel, paladar)
            || ReferenceEquals(piel, lengua)
            || ReferenceEquals(piel, ojo)
            || EsPaladar(piel.name)
            || EsLengua(piel.name)
            || EsNombreOjo(piel.name)))
        {
            piel = UnityEngine.Object.Instantiate(piel);
            piel.name = "Piel";
        }

        if (piel != null)
        {
            AplicarPielLisa(piel);
            return piel;
        }

        if (_pielFallback == null)
        {
            var shader = ShaderPiel();
            if (shader != null)
            {
                _pielFallback = new Material(shader) { name = "Piel" };
                AplicarPielLisa(_pielFallback);
            }
        }
        return _pielFallback;
    }

    static Shader ShaderPiel()
    {
        return Shader.Find("MonsterInc/MikePiel")
            ?? Shader.Find("Legacy Shaders/Diffuse")
            ?? Shader.Find("Diffuse")
            ?? Shader.Find("Standard");
    }

    static void AplicarPielLisa(Material piel)
    {
        if (piel == null)
            return;

        // Cylindrical UVs on the sphere stretch any 2D grain into wood/watermelon veins.
        if (piel.HasProperty("_MainTex"))
            piel.mainTexture = null;

        var shader = ShaderPiel();
        if (shader != null)
            piel.shader = shader;

        if (piel.HasProperty("_MainTex"))
            piel.mainTexture = null;

        if (piel.HasProperty("_Color"))
            piel.color = VerdePiel;
        if (piel.HasProperty("_PoreScale"))
            piel.SetFloat("_PoreScale", 48f);
        if (piel.HasProperty("_PoreAmount"))
            piel.SetFloat("_PoreAmount", 0.04f);
        if (piel.HasProperty("_Metallic"))
            piel.SetFloat("_Metallic", 0f);
        if (piel.HasProperty("_Glossiness"))
            piel.SetFloat("_Glossiness", 0.36f);
        if (piel.HasProperty("_Smoothness"))
            piel.SetFloat("_Smoothness", 0.36f);
    }

    static bool ColorCasiBlanco(Color c)
    {
        return c.r > 0.85f && c.g > 0.85f && c.b > 0.85f
            && (c.maxColorComponent - Mathf.Min(c.r, Mathf.Min(c.g, c.b))) < 0.08f;
    }

    static bool EsSplineAyuda(Renderer renderer)
    {
        if (renderer == null || renderer is SkinnedMeshRenderer)
            return false;
        var nombre = renderer.gameObject.name;
        return Contiene(nombre, "Circle") || Contiene(nombre, "NGon");
    }

    static Material AsegurarOjo(Material ojo)
    {
        if (ojo == null)
            return null;

        AmbienteVisual.RepararShader(ojo);
        var tex = Resources.Load<Texture2D>("Textures/Ojo")
            ?? Resources.Load<Texture2D>("Models/Mike/Ojo");
        if (tex != null)
            ojo.mainTexture = tex;
        if (ojo.HasProperty("_Color"))
            ojo.color = Color.white;

        return ojo;
    }

    static void AsegurarMaterialBoca(Material mat, Color visible, Material piel, Material ojo)
    {
        if (mat == null)
            return;
        if (ReferenceEquals(mat, piel) || ReferenceEquals(mat, ojo))
            return;
        if (EsPiel(mat.name) || EsNombreOjo(mat.name))
            return;

        AmbienteVisual.RepararShader(mat);
        if (mat.HasProperty("_Color"))
        {
            if (EsLengua(mat.name) || EsPaladar(mat.name))
                mat.color = visible;
            else if (mat.color.maxColorComponent < 0.35f || ColorCasiBlanco(mat.color))
                mat.color = visible;
        }
        if (mat.HasProperty("_Cull"))
            mat.SetInt("_Cull", 0);
    }

    static bool EsCuerpoEntero(Renderer renderer, int slots)
    {
        if (renderer == null || slots != 1)
            return false;
        var nombre = renderer.gameObject.name;
        if (EsNombreOjo(nombre))
            return false;
        if (Contiene(nombre, "Circle") || Contiene(nombre, "NGon"))
            return true;
        return Contiene(nombre, "Sphere") || Contiene(nombre, "Body");
    }

    static Mesh MeshDe(Renderer renderer)
    {
        if (renderer is SkinnedMeshRenderer skin)
            return skin.sharedMesh;
        var filter = renderer != null ? renderer.GetComponent<MeshFilter>() : null;
        return filter != null ? filter.sharedMesh : null;
    }

    static float VolumenSubmesh(Mesh mesh, int sub)
    {
        var size = TamanoSubmesh(mesh, sub);
        return Mathf.Abs(size.x * size.y * size.z);
    }

    static float ExtensionSubmesh(Mesh mesh, int sub)
    {
        return TamanoSubmesh(mesh, sub).magnitude;
    }

    static Vector3 TamanoSubmesh(Mesh mesh, int sub)
    {
        if (mesh == null || sub < 0 || sub >= mesh.subMeshCount)
            return Vector3.zero;

        var desc = mesh.GetSubMesh(sub);
        var size = desc.bounds.size;
        if (size.sqrMagnitude > 0.0001f)
            return size;

        if (!mesh.isReadable)
            return Vector3.zero;

        var tris = mesh.GetTriangles(sub);
        var verts = mesh.vertices;
        if (tris == null || tris.Length == 0 || verts == null || verts.Length == 0)
            return Vector3.zero;

        var min = verts[tris[0]];
        var max = min;
        for (int i = 0; i < tris.Length; i++)
        {
            int vi = tris[i];
            if (vi < 0 || vi >= verts.Length)
                continue;
            var v = verts[vi];
            min = Vector3.Min(min, v);
            max = Vector3.Max(max, v);
        }

        return max - min;
    }

    static int IndiceSubmeshMayor(Renderer renderer)
    {
        var mesh = MeshDe(renderer);
        if (mesh == null || mesh.subMeshCount <= 1)
            return 0;

        int mejor = 0;
        int mejorCount = -1;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int count = mesh.GetSubMesh(i).indexCount;
            if (count > mejorCount)
            {
                mejorCount = count;
                mejor = i;
            }
        }
        return mejor;
    }

    static bool Contiene(string haystack, string needle)
    {
        return !string.IsNullOrEmpty(haystack)
            && haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static bool NombreCoincide(string haystack, string needle)
    {
        if (string.IsNullOrEmpty(haystack) || string.IsNullOrEmpty(needle))
            return false;
        if (string.Equals(haystack, needle, StringComparison.OrdinalIgnoreCase))
            return true;
        return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static Material _pipe1Live;
    static Material _pipe2Live;

    static bool EsNombreTuberia(string nombre)
    {
        if (string.IsNullOrEmpty(nombre))
            return false;
        if (Contiene(nombre, "pipe"))
            return true;
        return string.Equals(nombre, "metal", StringComparison.OrdinalIgnoreCase);
    }

    static bool NombreEnJerarquia(Transform t, string needle)
    {
        while (t != null)
        {
            if (Contiene(t.name, needle))
                return true;
            t = t.parent;
        }
        return false;
    }

    static bool EnFabrica(Renderer renderer)
    {
        return renderer != null && NombreEnJerarquia(renderer.transform, "Fabrica");
    }

    static bool EsTuberia(Material material, Renderer renderer)
    {
        if (Contiene(NombreMaterial(material), "pipe"))
            return true;
        return renderer != null && NombreEnJerarquia(renderer.transform, "pipe");
    }

    static bool EsPipe2(Material material, Renderer renderer)
    {
        if (Contiene(NombreMaterial(material), "pipe2"))
            return true;
        return renderer != null && NombreEnJerarquia(renderer.transform, "pipe2");
    }

    static bool EsExcluidoDeRiel(string nombre)
    {
        return Contiene(nombre, "glass")
            || Contiene(nombre, "window")
            || Contiene(nombre, "roof")
            || Contiene(nombre, "ground")
            || Contiene(nombre, "wall")
            || Contiene(nombre, "logo")
            || Contiene(nombre, "madera")
            || Contiene(nombre, "Piel")
            || Contiene(nombre, "Cesped")
            || Contiene(nombre, "7cd")
            || Contiene(nombre, "red");
    }

    static bool PareceRielOscuro(Material material, Renderer renderer)
    {
        if (material == null || !EnFabrica(renderer) || !material.HasProperty("_Color"))
            return false;
        if (EsExcluidoDeRiel(NombreMaterial(material)))
            return false;
        return material.color.maxColorComponent < 0.22f;
    }

    static Shader ShaderTuberia()
    {
        return Shader.Find("Standard")
            ?? Shader.Find("Legacy Shaders/Diffuse")
            ?? Shader.Find("Diffuse");
    }

    static Material MaterialTuberia(bool pipe2)
    {
        if (pipe2)
        {
            if (_pipe2Live == null)
                _pipe2Live = CrearTuberia("pipe2", ColorPipe2);
            return _pipe2Live;
        }

        if (_pipe1Live == null)
            _pipe1Live = CrearTuberia("pipe1", ColorPipe1);
        return _pipe1Live;
    }

    static Material CrearTuberia(string nombre, Color color)
    {
        var shader = ShaderTuberia();
        if (shader == null)
            return null;

        var mat = new Material(shader) { name = nombre };
        PintarTuberia(mat, color);
        return mat;
    }

    static Texture TexturaMetal()
    {
        if (_texMetal != null)
            return _texMetal;

        var metal = Resources.Load<Material>("Models/Materials/metal");
        if (metal != null)
        {
            AmbienteVisual.RepararShader(metal);
            if (metal.mainTexture != null)
            {
                _texMetal = metal.mainTexture;
                return _texMetal;
            }
        }

        _texMetal = Resources.Load<Texture2D>("Textures/metal");
#if UNITY_EDITOR
        if (_texMetal == null)
            _texMetal = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Textures/metal.jpg");
#endif
        return _texMetal;
    }

    static void PintarTuberia(Material material, Color color)
    {
        if (material == null)
            return;

        var shader = ShaderTuberia();
        if (shader != null && (material.shader == null
            || material.shader.name.Contains("InternalError")
            || material.shader.name.IndexOf("Standard", StringComparison.OrdinalIgnoreCase) < 0))
            material.shader = shader;

        var tex = TexturaMetal();
        if (tex != null && material.HasProperty("_MainTex"))
        {
            material.mainTexture = tex;
            material.mainTextureScale = new Vector2(2f, 2f);
        }

        if (material.HasProperty("_Color"))
            material.color = color;
        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", 0.28f);
        if (material.HasProperty("_Glossiness"))
            material.SetFloat("_Glossiness", 0.4f);
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.4f);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            material.SetColor("_EmissionColor", EmisionPipe);
        }
    }

    static bool AsegurarTexturaTuberia(Material material, Renderer renderer)
    {
        if (material == null || (!EsTuberia(material, renderer) && !PareceRielOscuro(material, renderer)))
            return false;

        var color = EsPipe2(material, renderer) ? ColorPipe2 : ColorPipe1;
        var anterior = material.color;
        var texAntes = material.HasProperty("_MainTex") ? material.mainTexture : null;
        PintarTuberia(material, color);
        return material.color != anterior
            || (material.HasProperty("_MainTex") && material.mainTexture != texAntes);
    }

    static Material BuscarReemplazo(Dictionary<string, Material> catalogo, Material actual, Renderer renderer)
    {
        if (catalogo.Count == 0)
            return null;

        var nombre = NombreMaterial(actual);
        if (!string.IsNullOrEmpty(nombre) && catalogo.TryGetValue(nombre, out var porNombre))
            return porNombre;

        if (!string.IsNullOrEmpty(nombre))
        {
            int colon = nombre.IndexOf(':');
            if (colon > 0 && catalogo.TryGetValue(nombre.Substring(0, colon), out var porBase))
                return porBase;
        }

        if (EsNombreTuberia(nombre) || (renderer != null && NombreEnJerarquia(renderer.transform, "pipe")))
        {
            if (Contiene(nombre, "pipe2") || (renderer != null && NombreEnJerarquia(renderer.transform, "pipe2")))
            {
                if (catalogo.TryGetValue("pipe2", out var pipe2))
                    return pipe2;
            }
            if (Contiene(nombre, "pipe") && catalogo.TryGetValue("pipe1", out var pipe1))
                return pipe1;
            if (renderer != null && NombreEnJerarquia(renderer.transform, "pipe")
                && catalogo.TryGetValue("pipe1", out var pipePorNodo))
                return pipePorNodo;
            if (catalogo.TryGetValue("metal", out var metal))
                return metal;
        }

        var rendererName = renderer != null ? renderer.gameObject.name : null;
        if (!string.IsNullOrEmpty(rendererName) && catalogo.TryGetValue(rendererName, out var porRenderer))
            return porRenderer;

        Material mejor = null;
        int mejorLen = 0;
        foreach (var kv in catalogo)
        {
            var key = kv.Key;
            if (string.IsNullOrEmpty(key))
                continue;

            bool match = NombreCoincide(nombre, key) || NombreCoincide(rendererName, key);
            if (!match && !string.IsNullOrEmpty(nombre) && nombre.Length >= 3
                && key.IndexOf(nombre, StringComparison.OrdinalIgnoreCase) >= 0)
                match = true;

            if (match && key.Length > mejorLen)
            {
                mejor = kv.Value;
                mejorLen = key.Length;
            }
        }

        return mejor;
    }

    static bool Reparar(Material material)
    {
        if (material == null || material.shader == null)
            return false;
        if (material.shader.name != "Hidden/InternalErrorShader"
            && !material.shader.name.Contains("InternalError"))
            return false;

        var anterior = material.shader;
        AmbienteVisual.RepararShader(material);
        return material.shader != anterior;
    }

    static Dictionary<string, Material> Catalogo()
    {
        if (_porNombre != null)
            return _porNombre;

        _porNombre = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
        Registrar(Resources.LoadAll<Material>("Models/Materials"));
        Registrar(Resources.LoadAll<Material>("Models/Mike/Materials"));
        return _porNombre;
    }

    static void Registrar(Material[] materiales)
    {
        if (materiales == null)
            return;
        foreach (var material in materiales)
        {
            if (material == null || _porNombre.ContainsKey(material.name))
                continue;
            AmbienteVisual.RepararShader(material);
            _porNombre[material.name] = material;
        }
    }

    static string NombreMaterial(Material material)
    {
        if (material == null)
            return null;
        var nombre = material.name;
        const string sufijo = " (Instance)";
        if (nombre.EndsWith(sufijo))
            nombre = nombre.Substring(0, nombre.Length - sufijo.Length);
        return nombre;
    }
}
