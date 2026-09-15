using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class ReconectarMateriales
{
    static readonly Color VerdePiel = new Color(0.50f, 0.78f, 0.15f);
    public static readonly Color ColorLengua = new Color(0.86f, 0.12f, 0.20f);
    public static readonly Color ColorPaladar = new Color(0.65f, 0.20f, 0.22f);
    static readonly Color ColorPipe1 = Color.white;
    static readonly Color ColorPipe2 = new Color(0.96f, 0.97f, 1f);
    static readonly Color ColorVidrio = new Color(0.58f, 0.72f, 0.78f, 1f);
    static readonly Color ColorChimenea = new Color(0.92f, 0.94f, 0.96f, 1f);
    const float MetalicoPipe = 0.58f;
    const float BrilloPipe = 0.42f;
    const float MetalicoChim = 0.72f;
    const float BrilloChim = 0.48f;
    const float MetalicoVidrio = 0.88f;
    const float BrilloVidrio = 0.95f;
    const int ColaGeometria = 2000;

    // Unity sharedMaterials first-occurrence on the skinned Mike mesh.
    // FBX connection order is Piel, Unias, Lengua, Paladar, Ojo, Dientes — do not use that.
    public const int SlotMikePiel = 0;
    public const int SlotMikeUnias = 1;
    public const int SlotMikeOjo = 2;
    public const int SlotMikePaladar = 3;
    public const int SlotMikeLengua = 4;
    public const int SlotMikeDientes = 5;

    static readonly string[] SlotsMikePorIndice =
    {
        "Piel", "Unias", "Ojo", "Paladar", "Lengua", "Dientes"
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
            if (renderer.GetComponentInParent<Canvas>() != null)
                continue;

            var shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0)
                continue;

            bool changed = false;
            for (int i = 0; i < shared.Length; i++)
            {
                var actual = shared[i];
                var reemplazo = BuscarReemplazo(catalogo, actual, renderer);
                if (EsVidrioNombrado(actual, renderer))
                {
                    var vidrio = MaterialVidrio(actual, renderer, catalogo);
                    if (vidrio != null)
                        reemplazo = vidrio;
                }
                else if (EsTorre(actual, renderer))
                {
                    var chim = MaterialChimenea();
                    if (chim != null)
                        reemplazo = chim;
                }
                else if (EsPipeNombrado(renderer) || Contiene(NombreMaterial(actual), "pipe")
                    || EsCerca(actual, renderer) || PareceTuboPorMalla(renderer))
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

                var aplicado = shared[i] != null ? shared[i] : actual;
                if (EsVidrioNombrado(aplicado, renderer))
                {
                    if (PintarVidrio(aplicado))
                        changed = true;
                }
                else if (EsTorre(aplicado, renderer))
                {
                    if (PintarChimenea(aplicado))
                        changed = true;
                }
                else if (AsegurarTexturaTuberia(aplicado, renderer))
                    changed = true;
                else if (AsegurarAlbedo(aplicado, renderer))
                    changed = true;
                else if (Reparar(aplicado))
                    changed = true;
            }

            if (changed)
                renderer.sharedMaterials = shared;
        }

        ForzarPipesEnFabrica(root);
        ForzarChimeneasEnFabrica(root);
        ForzarVidriosEnFabrica(root);
        RepararUvPisoFabrica(root);
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
        AsegurarMaterialBoca(lengua, ColorLengua, piel, ojo);
        AsegurarMaterialBoca(paladar, ColorPaladar, piel, ojo);
        AsegurarMaterialBoca(dientes, Color.white, piel, ojo);
        AsegurarMaterialBoca(unias, Color.white, piel, ojo);

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.GetComponent<TextMesh>() != null)
                continue;
            if (renderer.GetComponentInParent<Canvas>() != null)
                continue;

            if (EsSplineAyuda(renderer))
            {
                renderer.enabled = false;
                continue;
            }

            var shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0)
            {
                var unico = ElegirMatMike(mats, null, renderer, piel, ojo, 0, -1);
                if (unico != null)
                    renderer.sharedMaterial = unico;
                continue;
            }

            int discoOjo = IndiceDiscoFacial(renderer);
            bool changed = false;
            var siguiente = (Material[])shared.Clone();
            for (int i = 0; i < shared.Length; i++)
            {
                var elegido = ElegirMatMike(mats, shared[i], renderer, piel, ojo, i, discoOjo);
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

    public static void AsegurarBoca(GameObject root)
    {
        if (root == null)
            return;

        var mats = MaterialesMike();
        var paladar = BuscarMike(mats, "Paladar");
        var lengua = BuscarMike(mats, "Lengua");
        AsegurarMaterialBoca(lengua, ColorLengua, null, null);
        AsegurarMaterialBoca(paladar, ColorPaladar, null, null);

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.GetComponent<TextMesh>() != null)
                continue;
            if (renderer.GetComponentInParent<Canvas>() != null)
                continue;

            var shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0)
                continue;

            var siguiente = (Material[])shared.Clone();
            if (!ForzarBocaEnSlots(siguiente, lengua, paladar, renderer))
                continue;
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

    static bool EsIndiceOjo(int slot, int slots)
    {
        return slots >= 6 && slot == SlotMikeOjo;
    }

    static bool EsIndiceBoca(int slot, int slots)
    {
        return slots >= 6 && (slot == SlotMikePaladar || slot == SlotMikeLengua);
    }

    public static bool EsSlotOjo(int slot, int slots, Material actual, string rendererName, int disco)
    {
        if (EsIndiceOjo(slot, slots))
            return true;
        if (disco >= 0 && slot == disco && !EsIndiceBoca(slot, slots))
            return true;
        if (EsNombreOjo(rendererName) && slots <= 1)
            return true;
        if (EsNombreOjo(NombreMaterial(actual)) && !EsIndiceBoca(slot, slots))
            return true;
        return false;
    }

    public static int IndiceDiscoFacial(Renderer renderer)
    {
        int slots = renderer != null && renderer.sharedMaterials != null
            ? renderer.sharedMaterials.Length
            : 0;
        if (slots >= 6)
            return SlotMikeOjo;

        var shared = renderer != null ? renderer.sharedMaterials : null;
        if (shared != null)
        {
            for (int i = 0; i < shared.Length; i++)
            {
                if (EsNombreOjo(NombreMaterial(shared[i])) && !EsIndiceBoca(i, shared.Length))
                    return i;
            }

            if (EsNombreOjo(renderer.gameObject.name) && shared.Length <= 1)
                return 0;
        }

        var mesh = MeshDe(renderer);
        if (mesh == null || mesh.subMeshCount < 2)
            return -1;

        int mayor = IndiceSubmeshMayor(renderer);
        float cuerpo = ExtensionSubmesh(mesh, mayor);
        int mejor = -1;
        float mejorVol = -1f;
        int limite = Mathf.Min(mesh.subMeshCount, slots > 0 ? slots : mesh.subMeshCount);

        for (int i = 0; i < limite; i++)
        {
            if (i == mayor)
                continue;
            if (EsIndiceBoca(i, limite))
                continue;
            var nombre = shared != null && i < shared.Length ? NombreMaterial(shared[i]) : null;
            if (EsLengua(nombre) || EsPaladar(nombre) || EsPiel(nombre))
                continue;
            if (Contiene(nombre, "Unia") || Contiene(nombre, "Diente"))
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

        int disco = slots.Length >= 6 ? SlotMikeOjo : IndiceDiscoFacial(renderer);
        if (disco < 0 || disco >= slots.Length)
            return false;
        if (EsIndiceBoca(disco, slots.Length))
            return false;
        if (slots[disco] == ojo)
            return false;

        slots[disco] = ojo;
        return true;
    }

    static bool ForzarBocaEnSlots(Material[] slots, Material lengua, Material paladar, Renderer renderer)
    {
        if (slots == null)
            return false;

        bool changed = false;
        int disco = IndiceDiscoFacial(renderer);
        var rendererName = renderer != null ? renderer.gameObject.name : null;

        if (slots.Length >= 6)
        {
            if (paladar != null
                && !EsSlotOjo(SlotMikePaladar, slots.Length, slots[SlotMikePaladar], rendererName, disco)
                && slots[SlotMikePaladar] != paladar)
            {
                slots[SlotMikePaladar] = paladar;
                changed = true;
            }

            if (lengua != null
                && !EsSlotOjo(SlotMikeLengua, slots.Length, slots[SlotMikeLengua], rendererName, disco)
                && slots[SlotMikeLengua] != lengua)
            {
                slots[SlotMikeLengua] = lengua;
                changed = true;
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (EsIndiceOjo(i, slots.Length) || EsSlotOjo(i, slots.Length, slots[i], rendererName, disco))
                continue;

            var nombre = NombreMaterial(slots[i]);
            if (lengua != null && EsLengua(nombre) && slots[i] != lengua)
            {
                slots[i] = lengua;
                changed = true;
            }
            else if (paladar != null && EsPaladar(nombre) && slots[i] != paladar)
            {
                slots[i] = paladar;
                changed = true;
            }
        }

        return changed;
    }

    public static bool EsSlotBoca(int slot, int slots, Material actual, string rendererName)
    {
        if (EsIndiceOjo(slot, slots))
            return false;
        if (EsNombreOjo(rendererName) && slots <= 1)
            return false;
        if (EsNombreOjo(NombreMaterial(actual)) && !EsIndiceBoca(slot, slots))
            return false;
        if (EsIndiceBoca(slot, slots))
            return true;
        return EsLengua(NombreMaterial(actual))
            || EsPaladar(NombreMaterial(actual))
            || EsLengua(rendererName)
            || EsPaladar(rendererName);
    }

    static bool ForzarPielEnCuerpo(Material[] slots, Renderer renderer, Material piel, int discoOjo)
    {
        if (slots == null || piel == null)
            return false;

        var rendererName = renderer != null ? renderer.gameObject.name : null;
        int destino = slots.Length >= 6 ? SlotMikePiel : -1;
        if (destino < 0)
        {
            int mayor = IndiceSubmeshMayor(renderer);
            if (mayor >= 0
                && mayor < slots.Length
                && !EsSlotOjo(mayor, slots.Length, slots[mayor], rendererName, discoOjo)
                && !EsSlotBoca(mayor, slots.Length, slots[mayor], rendererName))
                destino = mayor;
        }

        if (destino < 0 || destino >= slots.Length)
            return false;
        if (EsSlotOjo(destino, slots.Length, slots[destino], rendererName, discoOjo))
            return false;
        if (EsSlotBoca(destino, slots.Length, slots[destino], rendererName))
            return false;
        if (slots[destino] == piel)
            return false;

        slots[destino] = piel;
        return true;
    }

    static Material ElegirMatMike(
        Material[] mats,
        Material actual,
        Renderer renderer,
        Material piel,
        Material ojo,
        int slot,
        int discoOjo)
    {
        var nombre = NombreMaterial(actual);
        var rendererName = renderer != null ? renderer.gameObject.name : null;
        int slots = renderer != null && renderer.sharedMaterials != null
            ? renderer.sharedMaterials.Length
            : 1;

        var porIndice = SlotPorIndice(mats, slot, slots);
        if (porIndice != null)
            return porIndice;

        if (EsSlotOjo(slot, slots, actual, rendererName, discoOjo))
            return ojo ?? piel;

        if (EsCuerpoEntero(renderer, slots))
            return piel;

        if (slot != discoOjo)
        {
            var parte = MatchParteEspecifica(mats, nombre, rendererName);
            if (parte != null)
                return parte;
        }

        if (EsNombreOjo(nombre) && !EsIndiceBoca(slot, slots))
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
            piel.SetFloat("_Glossiness", 0.12f);
        if (piel.HasProperty("_Smoothness"))
            piel.SetFloat("_Smoothness", 0.12f);
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
        if (!EsLengua(mat.name) && !EsPaladar(mat.name)
            && !Contiene(mat.name, "Diente") && !Contiene(mat.name, "Unia"))
            return;

        var shaderBoca = Shader.Find("Legacy Shaders/Diffuse")
            ?? Shader.Find("Diffuse")
            ?? Shader.Find("Standard");
        if (shaderBoca != null && (EsLengua(mat.name) || EsPaladar(mat.name)))
            mat.shader = shaderBoca;

        AmbienteVisual.RepararShader(mat);
        if ((EsLengua(mat.name) || EsPaladar(mat.name)) && mat.HasProperty("_MainTex"))
            mat.mainTexture = null;
        if (mat.HasProperty("_Color"))
        {
            if (EsLengua(mat.name) || EsPaladar(mat.name))
                mat.color = visible;
            else if (mat.color.maxColorComponent < 0.35f || ColorCasiBlanco(mat.color))
                mat.color = visible;
        }
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", 0f);
        if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", 0.08f);
        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", 0.08f);
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
    static Material _chimLive;
    static Material _glassLive;
    static Material _windowLive;
    static Texture _texGalv;

    static bool EsNombreTuberia(string nombre)
    {
        return Contiene(nombre, "pipe");
    }

    static bool EsPipeNombrado(Renderer renderer)
    {
        return renderer != null && NombreEnJerarquia(renderer.transform, "pipe");
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
        if (EsNombreTuberia(NombreMaterial(material)))
            return true;
        if (EsPipeNombrado(renderer))
            return true;
        return PareceTuboPorMalla(renderer);
    }

    static void ForzarPipesEnFabrica(GameObject root)
    {
        if (root == null)
            return;

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null || renderer is ParticleSystemRenderer || renderer.GetComponent<TextMesh>() != null)
                continue;
            if (EsVidrioNombrado(renderer.sharedMaterial, renderer) || EsTorre(renderer.sharedMaterial, renderer))
                continue;
            if (!EsTuberia(renderer.sharedMaterial, renderer) && !EsCerca(renderer.sharedMaterial, renderer))
                continue;

            var mat = MaterialTuberia(EsPipe2(renderer.sharedMaterial, renderer));
            if (mat == null)
                continue;

            PintarTuberia(mat, EsPipe2(renderer.sharedMaterial, renderer) ? ColorPipe2 : ColorPipe1);

            var shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0)
            {
                renderer.sharedMaterial = mat;
                continue;
            }

            var siguiente = new Material[shared.Length];
            for (int i = 0; i < shared.Length; i++)
                siguiente[i] = mat;
            renderer.sharedMaterials = siguiente;
        }
    }

    static void ForzarChimeneasEnFabrica(GameObject root)
    {
        if (root == null)
            return;

        var chim = MaterialChimenea();
        if (chim == null)
            return;

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null || renderer is ParticleSystemRenderer || renderer.GetComponent<TextMesh>() != null)
                continue;
            if (!EsTorre(renderer.sharedMaterial, renderer))
                continue;

            PintarChimenea(chim);

            var shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0)
            {
                renderer.sharedMaterial = chim;
                continue;
            }

            var siguiente = new Material[shared.Length];
            for (int i = 0; i < shared.Length; i++)
                siguiente[i] = chim;
            renderer.sharedMaterials = siguiente;
        }
    }

    static void ForzarVidriosEnFabrica(GameObject root)
    {
        if (root == null)
            return;

        var catalogo = Catalogo();
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null || renderer is ParticleSystemRenderer || renderer.GetComponent<TextMesh>() != null)
                continue;
            if (!EsVidrioNombrado(renderer.sharedMaterial, renderer))
                continue;

            var shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0)
            {
                var unico = MaterialVidrio(null, renderer, catalogo);
                if (unico != null)
                {
                    PintarVidrio(unico);
                    renderer.sharedMaterial = unico;
                    renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
                }
                continue;
            }

            var siguiente = (Material[])shared.Clone();
            bool changed = false;
            for (int i = 0; i < siguiente.Length; i++)
            {
                if (!EsVidrioNombrado(siguiente[i], renderer))
                    continue;
                var vidrio = MaterialVidrio(siguiente[i], renderer, catalogo);
                if (vidrio != null && vidrio != siguiente[i])
                {
                    siguiente[i] = vidrio;
                    changed = true;
                }
                if (PintarVidrio(siguiente[i]))
                    changed = true;
            }
            renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
            if (changed)
                renderer.sharedMaterials = siguiente;
        }
    }

    // fabrica.FBX ground/ground1/ground3 share one UV (a single texel), so albedo looks untextured.
    // ground1 is the raised apron between the building and the 80x80 tiled Suelo.
    static void RepararUvPisoFabrica(GameObject root)
    {
        if (root == null)
            return;

        foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
        {
            var renderer = filter != null ? filter.GetComponent<MeshRenderer>() : null;
            var mesh = filter != null ? filter.sharedMesh : null;
            if (renderer == null || mesh == null || !mesh.isReadable)
                continue;
            if (!string.IsNullOrEmpty(mesh.name) && mesh.name.EndsWith("_PisoUV", StringComparison.Ordinal))
                continue;

            var mats = renderer.sharedMaterials;
            if (mats == null || mats.Length == 0)
                continue;

            int subCount = Mathf.Min(mesh.subMeshCount, mats.Length);
            bool hayPiso = false;
            for (int i = 0; i < subCount; i++)
            {
                if (EsPisoFbx(NombreMaterial(mats[i])))
                {
                    hayPiso = true;
                    break;
                }
            }
            if (!hayPiso)
                continue;

            var copy = Object.Instantiate(mesh);
            copy.name = mesh.name + "_PisoUV";
            var verts = copy.vertices;
            var uv = copy.uv;
            if (verts == null || verts.Length == 0)
            {
                Object.Destroy(copy);
                continue;
            }
            if (uv == null || uv.Length != verts.Length)
                uv = new Vector2[verts.Length];

            var xf = filter.transform;
            const float tilesPorMetro = 0.2f;
            bool dirty = false;
            for (int s = 0; s < subCount; s++)
            {
                if (!EsPisoFbx(NombreMaterial(mats[s])))
                    continue;
                var tris = copy.GetTriangles(s);
                if (tris == null)
                    continue;
                for (int t = 0; t < tris.Length; t++)
                {
                    int i = tris[t];
                    if (i < 0 || i >= verts.Length)
                        continue;
                    var w = xf.TransformPoint(verts[i]);
                    uv[i] = new Vector2(w.x * tilesPorMetro, w.z * tilesPorMetro);
                    dirty = true;
                }
            }

            if (!dirty)
            {
                Object.Destroy(copy);
                continue;
            }

            copy.uv = uv;
            filter.sharedMesh = copy;
        }
    }

    static bool EsPisoFbx(string nombre)
    {
        return Contiene(nombre, "ground");
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
            || Contiene(nombre, "chim")
            || Contiene(nombre, "tower")
            || Contiene(nombre, "silo")
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

    static Material MaterialChimenea()
    {
        if (_chimLive != null)
            return _chimLive;

        var catalogo = Catalogo();
        if (catalogo.TryGetValue("chim", out var chim) && chim != null)
            _chimLive = chim;
        else
        {
            var shader = ShaderTuberia();
            if (shader == null)
                return null;
            _chimLive = new Material(shader) { name = "chim" };
        }

        PintarChimenea(_chimLive);
        return _chimLive;
    }

    static Material MaterialVidrio(Material actual, Renderer renderer, Dictionary<string, Material> catalogo)
    {
        bool window = Contiene(NombreMaterial(actual), "window")
            || (renderer != null && NombreEnJerarquia(renderer.transform, "window"));
        if (window)
        {
            if (_windowLive != null)
                return _windowLive;
            if (catalogo != null && catalogo.TryGetValue("window", out var windowMat) && windowMat != null)
                _windowLive = windowMat;
            else if (catalogo != null && catalogo.TryGetValue("glass", out var glassAsWindow) && glassAsWindow != null)
                _windowLive = glassAsWindow;
            else
            {
                var shader = ShaderVidrio();
                if (shader != null)
                    _windowLive = new Material(shader) { name = "window" };
            }
            if (_windowLive != null)
                PintarVidrio(_windowLive);
            return _windowLive;
        }

        if (_glassLive != null)
            return _glassLive;
        if (catalogo != null && catalogo.TryGetValue("glass", out var glass) && glass != null)
            _glassLive = glass;
        else if (actual != null)
            _glassLive = actual;
        else
        {
            var shader = ShaderVidrio();
            if (shader != null)
                _glassLive = new Material(shader) { name = "glass" };
        }
        if (_glassLive != null)
            PintarVidrio(_glassLive);
        return _glassLive;
    }

    static Shader ShaderVidrio()
    {
        return Shader.Find("Standard")
            ?? Shader.Find("Legacy Shaders/Diffuse")
            ?? Shader.Find("Diffuse");
    }

    static bool PintarVidrio(Material material)
    {
        if (material == null)
            return false;

        var anterior = material.shader;
        var colorAntes = material.HasProperty("_Color") ? material.color : Color.clear;
        float metalAntes = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : -1f;
        float brilloAntes = material.HasProperty("_Glossiness") ? material.GetFloat("_Glossiness") : -1f;
        int colaAntes = material.renderQueue;
        var shader = ShaderVidrio();
        if (shader != null)
            material.shader = shader;

        if (material.HasProperty("_MainTex"))
            material.mainTexture = null;
        if (material.HasProperty("_Color"))
            material.color = ColorVidrio;

        // Opaque Standard: hide the factory interior; metal/smooth samples the realtime probe.
        material.SetOverrideTag("RenderType", "Opaque");
        if (material.HasProperty("_Mode"))
            material.SetFloat("_Mode", 0f);
        material.SetInt("_SrcBlend", (int)BlendMode.One);
        material.SetInt("_DstBlend", (int)BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = ColaGeometria;

        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", MetalicoVidrio);
        if (material.HasProperty("_Glossiness"))
            material.SetFloat("_Glossiness", BrilloVidrio);
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", BrilloVidrio);
        if (material.HasProperty("_GlossyReflections"))
            material.SetFloat("_GlossyReflections", 1f);
        if (material.HasProperty("_SpecularHighlights"))
            material.SetFloat("_SpecularHighlights", 1f);

        return material.shader != anterior
            || colaAntes != ColaGeometria
            || (material.HasProperty("_Color") && material.color != colorAntes)
            || (material.HasProperty("_Metallic") && !Mathf.Approximately(metalAntes, MetalicoVidrio))
            || (material.HasProperty("_Glossiness") && !Mathf.Approximately(brilloAntes, BrilloVidrio));
    }

    static bool PintarChimenea(Material material)
    {
        if (material == null)
            return false;

        var shader = ShaderTuberia();
        if (shader != null && (material.shader == null
            || material.shader.name.Contains("InternalError")
            || material.shader.name.IndexOf("Standard", StringComparison.OrdinalIgnoreCase) < 0))
            material.shader = shader;

        var texAntes = material.HasProperty("_MainTex") ? material.mainTexture : null;
        var colorAntes = material.HasProperty("_Color") ? material.color : Color.white;
        var tex = TexturaGalvanizado();
        if (tex != null && material.HasProperty("_MainTex"))
        {
            tex.wrapMode = TextureWrapMode.Repeat;
            material.mainTexture = tex;
            material.mainTextureScale = new Vector2(1.8f, 3.2f);
        }

        if (material.HasProperty("_Color"))
            material.color = ColorChimenea;
        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", MetalicoChim);
        if (material.HasProperty("_Glossiness"))
            material.SetFloat("_Glossiness", BrilloChim);
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", BrilloChim);
        if (material.HasProperty("_EmissionColor"))
        {
            material.DisableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black);
        }

        return (material.HasProperty("_MainTex") && material.mainTexture != texAntes)
            || (material.HasProperty("_Color") && material.color != colorAntes);
    }

    static bool PareceChimenea(Renderer renderer)
    {
        if (renderer == null || !EnFabrica(renderer))
            return false;

        var nombre = renderer.gameObject.name;
        if (EsVidrio(nombre) || Contiene(nombre, "wall") || Contiene(nombre, "ground")
            || Contiene(nombre, "pipe") || Contiene(nombre, "fence") || Contiene(nombre, "logo"))
            return false;
        if (EsNombreTorre(nombre) || Contiene(nombre, "Cylinder") || Contiene(nombre, "silo"))
            return true;

        var b = renderer.bounds.size;
        float y = b.y;
        float xz = Mathf.Max(b.x, b.z);
        float minxz = Mathf.Min(b.x, b.z);
        if (y < 4.5f || xz < 0.4f)
            return false;
        return y > xz * 1.55f && minxz > xz * 0.45f;
    }

    static Texture TexturaGalvanizado()
    {
        if (_texGalv != null)
            return _texGalv;

        _texGalv = CargarTex("Textures/corrugated_galvanized",
            "Assets/Art/Textures/corrugated_galvanized.png",
            "Assets/Resources/Textures/corrugated_galvanized.png");
        return _texGalv;
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

        _texMetal = CargarTex("Textures/metal",
            "Assets/Art/Textures/metal.jpg",
            "Assets/Resources/Textures/metal.jpg");
        return _texMetal;
    }

    static Texture _texOxido;

    static Texture TexturaOxido()
    {
        if (_texOxido != null)
            return _texOxido;

        _texOxido = CargarTex("Textures/pipe_rust",
            "Assets/Art/Textures/pipe_rust.png",
            "Assets/Resources/Textures/pipe_rust.png");
        if (_texOxido != null)
            return _texOxido;

        _texOxido = TexturaMetal();
        return _texOxido;
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
            tex.wrapMode = TextureWrapMode.Repeat;
            material.mainTexture = tex;
            material.mainTextureScale = new Vector2(5f, 2.5f);
        }

        if (material.HasProperty("_Color"))
            material.color = color.maxColorComponent < 0.5f ? Color.white : color;
        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", MetalicoPipe);
        if (material.HasProperty("_Glossiness"))
            material.SetFloat("_Glossiness", BrilloPipe);
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", BrilloPipe);
        if (material.HasProperty("_EmissionColor"))
        {
            material.DisableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black);
        }
    }

    static bool AsegurarTexturaTuberia(Material material, Renderer renderer)
    {
        if (material == null || EsVidrioNombrado(material, renderer) || EsTorre(material, renderer))
            return false;
        if (!EsTuberia(material, renderer) && !EsCerca(material, renderer) && !PareceRielOscuro(material, renderer))
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

    static Texture _texAdoquin;
    static Texture _texConcrete;
    static Texture _texHexagon;
    static Texture _texCliff;
    static Texture _texMadera;

    static bool AsegurarAlbedo(Material material, Renderer renderer)
    {
        if (material == null || !material.HasProperty("_MainTex"))
            return false;
        if (EsPiel(NombreMaterial(material))
            || EsNombreOjo(NombreMaterial(material))
            || EsLengua(NombreMaterial(material))
            || EsPaladar(NombreMaterial(material)))
            return false;
        if (EsVidrio(NombreMaterial(material)))
            return false;

        var rol = RolAlbedo(material, renderer);
        if (rol == RolTextura.Ninguno)
            return false;

        var tex = TexturaDeRol(rol);
        if (tex == null)
            return false;

        tex.wrapMode = TextureWrapMode.Repeat;
        var anterior = material.mainTexture;
        var scaleAntes = material.mainTextureScale;
        var colorAntes = material.HasProperty("_Color") ? material.color : Color.white;

        material.mainTexture = tex;
        material.mainTextureScale = EsPisoFbx(NombreMaterial(material))
            ? Vector2.one
            : EscalaDeRol(rol, renderer);
        if (material.HasProperty("_Color"))
            material.color = TintDeRol(rol, material.color);

        if (rol == RolTextura.Metal)
        {
            PintarTuberia(material, EsPipe2(material, renderer) ? ColorPipe2 : ColorPipe1);
            return true;
        }

        if (rol == RolTextura.Torre)
        {
            PintarChimenea(material);
            return true;
        }

        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", 0f);
        if (material.HasProperty("_Glossiness"))
            material.SetFloat("_Glossiness", 0.18f);
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.18f);

        return anterior != tex
            || scaleAntes != material.mainTextureScale
            || (material.HasProperty("_Color") && material.color != colorAntes);
    }

    enum RolTextura
    {
        Ninguno,
        Piso,
        Pared,
        Madera,
        Metal,
        Torre
    }

    static bool EsVidrio(string nombre)
    {
        return Contiene(nombre, "glass") || Contiene(nombre, "window");
    }

    static bool EsVidrioNombrado(Material material, Renderer renderer)
    {
        if (EsVidrio(NombreMaterial(material)))
            return true;
        return renderer != null && (NombreEnJerarquia(renderer.transform, "glass")
            || NombreEnJerarquia(renderer.transform, "window"));
    }

    static bool EsCerca(Material material, Renderer renderer)
    {
        if (Contiene(NombreMaterial(material), "fence"))
            return true;
        return renderer != null && NombreEnJerarquia(renderer.transform, "fence");
    }

    static bool EsNombreTorre(string nombre)
    {
        return Contiene(nombre, "chim")
            || Contiene(nombre, "tower")
            || Contiene(nombre, "silo")
            || Contiene(nombre, "chimney");
    }

    static bool EsTorre(Material material, Renderer renderer)
    {
        if (EsNombreTorre(NombreMaterial(material)))
            return true;
        if (Contiene(NombreMaterial(material), "roof"))
            return true;
        if (renderer != null && (NombreEnJerarquia(renderer.transform, "chim")
            || NombreEnJerarquia(renderer.transform, "tower")
            || NombreEnJerarquia(renderer.transform, "silo")
            || NombreEnJerarquia(renderer.transform, "roof")))
            return true;
        return PareceChimenea(renderer);
    }

    static RolTextura RolAlbedo(Material material, Renderer renderer)
    {
        var nombre = NombreMaterial(material);
        var nodo = renderer != null ? renderer.gameObject.name : null;

        if (EsVidrio(nombre) || EsVidrio(nodo))
            return RolTextura.Ninguno;
        if (EsNombreTuberia(nombre) || Contiene(nodo, "pipe") || EsCerca(material, renderer) || PareceTuboPorMalla(renderer))
            return RolTextura.Metal;
        if (EsTorre(material, renderer) || Contiene(nombre, "roof"))
            return RolTextura.Torre;

        if (Contiene(nombre, "madera") || Contiene(nombre, "wood") || Contiene(nombre, "Puerta"))
            return RolTextura.Madera;
        if (Contiene(nombre, "wall") || Contiene(nombre, "15_verti") || Contiene(nombre, "7cd"))
            return RolTextura.Pared;
        if (Contiene(nombre, "ground") || Contiene(nombre, "piso") || Contiene(nombre, "floor"))
            return RolTextura.Piso;

        if (!EnFabrica(renderer))
            return RolTextura.Ninguno;

        if (material.mainTexture != null)
            return RolTextura.Ninguno;

        var b = renderer.bounds.size;
        bool piso = b.y < 1.6f && b.x > 6f && b.z > 6f;
        if (piso)
            return RolTextura.Piso;
        bool pared = b.y > 3.5f && (b.x > 6f || b.z > 6f);
        if (pared)
            return RolTextura.Pared;
        return RolTextura.Ninguno;
    }

    static Texture TexturaDeRol(RolTextura rol)
    {
        switch (rol)
        {
            case RolTextura.Piso:
                return TexAdoquin() ?? TexHexagon() ?? TexConcrete();
            case RolTextura.Pared:
                return TexConcrete() ?? TexHexagon();
            case RolTextura.Madera:
                return TexMadera() ?? TexConcrete();
            case RolTextura.Metal:
                return TexturaMetal();
            case RolTextura.Torre:
                return TexturaGalvanizado();
            default:
                return null;
        }
    }

    static Vector2 EscalaDeRol(RolTextura rol, Renderer renderer)
    {
        var b = renderer != null ? renderer.bounds.size : Vector3.one * 8f;
        float span = Mathf.Max(b.x, b.z, 4f);
        switch (rol)
        {
            case RolTextura.Piso:
                return Vector2.one * Mathf.Clamp(span * 0.35f, 8f, 18f);
            case RolTextura.Pared:
                return Vector2.one * Mathf.Clamp(span * 0.55f, 14f, 24f);
            case RolTextura.Madera:
                return new Vector2(2.5f, 2.5f);
            case RolTextura.Metal:
                return new Vector2(5f, 2.5f);
            case RolTextura.Torre:
                return new Vector2(1.8f, 3.2f);
            default:
                return Vector2.one;
        }
    }

    static Color TintDeRol(RolTextura rol, Color actual)
    {
        switch (rol)
        {
            case RolTextura.Piso:
                return Color.white;
            case RolTextura.Pared:
                return new Color(0.86f, 0.84f, 0.80f);
            case RolTextura.Madera:
                return Color.white;
            case RolTextura.Torre:
                return ColorChimenea;
            default:
                return actual.maxColorComponent < 0.35f ? Color.white : actual;
        }
    }

    static bool PareceTuboPorMalla(Renderer renderer)
    {
        if (renderer == null || !EnFabrica(renderer))
            return false;
        if (EsVidrio(renderer.gameObject.name) || Contiene(renderer.gameObject.name, "wall")
            || Contiene(renderer.gameObject.name, "ground") || Contiene(renderer.gameObject.name, "logo")
            || EsNombreTorre(renderer.gameObject.name) || Contiene(renderer.gameObject.name, "roof")
            || Contiene(renderer.gameObject.name, "fence"))
            return false;
        if (PareceChimenea(renderer))
            return false;

        var b = renderer.bounds.size;
        float max = Mathf.Max(b.x, Mathf.Max(b.y, b.z));
        float min = Mathf.Min(b.x, Mathf.Min(b.y, b.z));
        float mid = b.x + b.y + b.z - max - min;
        if (max < 3.5f || min > 1.35f)
            return false;
        return max > min * 6.5f && mid < 2.4f;
    }

    static Texture CargarTex(string resource, params string[] assetPaths)
    {
        var tex = Resources.Load<Texture>(resource);
        if (tex != null)
            return tex;
#if UNITY_EDITOR
        if (assetPaths != null)
        {
            for (int i = 0; i < assetPaths.Length; i++)
            {
                tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(assetPaths[i]);
                if (tex != null)
                    return tex;
            }
        }
#endif
        return null;
    }

    static Texture TexAdoquin()
    {
        if (_texAdoquin == null)
            _texAdoquin = CargarTex("Textures/floor_adoquin", "Assets/Art/Textures/floor_adoquin.jpg");
        return _texAdoquin;
    }

    static Texture TexConcrete()
    {
        if (_texConcrete == null)
            _texConcrete = CargarTex("Textures/floor_concrete", "Assets/Art/Textures/floor_concrete.jpg");
        return _texConcrete;
    }

    static Texture TexHexagon()
    {
        if (_texHexagon == null)
            _texHexagon = CargarTex("Textures/floor_hexagon", "Assets/Art/Textures/floor_hexagon.jpg");
        return _texHexagon;
    }

    static Texture TexCliff()
    {
        if (_texCliff == null)
            _texCliff = CargarTex("Textures/Cliff",
                "Assets/Art/Textures/Cliff (Layered Rock).jpg",
                "Assets/Resources/Textures/Cliff.jpg");
        return _texCliff;
    }

    static Texture TexMadera()
    {
        if (_texMadera == null)
            _texMadera = CargarTex("Textures/madera", "Assets/Art/Textures/madera.GIF");
        return _texMadera;
    }
}
