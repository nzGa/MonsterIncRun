using UnityEngine;
using UnityEngine.Rendering;

public static class AmbienteVisual
{
    const string TexAdoquin = "Textures/floor_adoquin";
    const string TexCesped = "Textures/GrassHill";
    const string TexOjo = "Textures/Ojo";

    public static void AplicarCielo()
    {
        var sky = Resources.Load<Material>("Skyboxes/Sunny1")
            ?? Resources.Load<Material>("Skyboxes/MoonShine")
            ?? ConstruirSkybox6("Sunny1", "Assets/Art/Sky/Textures/Sunny1/Sunny1")
            ?? ConstruirSkybox6("MoonShine", "Assets/Art/Sky/Textures/MoonShine/MoonShine");

        if (sky == null)
            return;

        RepararShader(sky, Shader.Find("Skybox/6 Sided") ?? Shader.Find("Skybox/Cubemap"));
        RenderSettings.skybox = sky;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.62f, 0.66f, 0.72f);
        RenderSettings.ambientEquatorColor = new Color(0.42f, 0.43f, 0.45f);
        RenderSettings.ambientGroundColor = new Color(0.22f, 0.20f, 0.18f);
        RenderSettings.ambientIntensity = 1.15f;
        RenderSettings.reflectionIntensity = 1f;
        DynamicGI.UpdateEnvironment();

        var luces = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude);
        for (int i = 0; i < luces.Length; i++)
        {
            if (luces[i] != null && luces[i].type == LightType.Directional && luces[i].intensity < 1.25f)
                luces[i].intensity = 1.25f;
        }
    }

    public static void AplicarSuelo(GameObject suelo)
    {
        if (suelo == null)
            return;

        AplicarTexturaTiled(
            suelo,
            Resources.Load<Material>("Models/Materials/Piso"),
            CargarTextura(TexAdoquin, "Assets/Art/Textures/floor_adoquin.jpg"),
            16f,
            new Color(0.55f, 0.50f, 0.42f));
    }

    public static void AsegurarCesped()
    {
        if (Terrain.activeTerrain != null)
            return;

        var cesped = GameObject.Find("Cesped");
        if (cesped == null)
            return;

        AplicarTexturaTiled(
            cesped,
            Resources.Load<Material>("Models/Materials/Cesped"),
            CargarTextura(TexCesped, "Assets/Art/Textures/Grass (Hill).psd"),
            40f,
            new Color(0.32f, 0.48f, 0.20f));
    }

    public static void AsignarPupila(GameObject mike)
    {
        if (mike == null)
            return;

        var ojo = Resources.Load<Material>("Models/Mike/Materials/Ojo");
        RepararShader(ojo);
        var tex = ojo != null ? ojo.mainTexture : null;
        if (tex == null)
            tex = CargarTextura(TexOjo, "Assets/Art/Textures/Ojo.jpg");
        if (ojo != null)
        {
            if (tex != null)
                ojo.mainTexture = tex;
            if (ojo.HasProperty("_Color"))
            {
                var c = ojo.color;
                bool rosa = c.r > 0.35f && c.g < 0.45f && c.b < 0.45f && c.r > c.g + 0.1f;
                if (rosa || c.maxColorComponent < 0.3f)
                    ojo.color = Color.white;
            }
        }

        foreach (var renderer in mike.GetComponentsInChildren<Renderer>(true))
        {
            var shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0)
                continue;

            bool changed = false;
            var siguiente = (Material[])shared.Clone();
            int disco = ReconectarMateriales.IndiceDiscoFacial(renderer);
            for (int i = 0; i < shared.Length; i++)
            {
                if (!EsSlotOjo(shared[i], renderer, shared.Length, i, disco))
                    continue;

                if (ojo != null)
                {
                    siguiente[i] = ojo;
                    changed = true;
                }
                else if (tex != null && siguiente[i] != null)
                {
                    RepararShader(siguiente[i]);
                    siguiente[i].mainTexture = tex;
                    if (siguiente[i].HasProperty("_Color"))
                        siguiente[i].color = Color.white;
                    changed = true;
                }
            }

            if (ReconectarMateriales.ForzarOjoEnDisco(siguiente, renderer, ojo))
                changed = true;

            if (changed)
                renderer.sharedMaterials = siguiente;
        }
    }

    static bool EsSlotOjo(Material material, Renderer renderer, int slots, int slot, int disco)
    {
        var nombreGuard = material != null ? material.name : null;
        if (nombreGuard != null && nombreGuard.EndsWith(" (Instance)"))
            nombreGuard = nombreGuard.Substring(0, nombreGuard.Length - " (Instance)".Length);
        if (!string.IsNullOrEmpty(nombreGuard)
            && nombreGuard.IndexOf("Piel", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return false;

        if (disco >= 0 && slot == disco)
            return true;

        if (ReconectarMateriales.EsNombreOjo(renderer != null ? renderer.gameObject.name : null)
            && slots <= 1)
            return true;

        var nombre = material != null ? material.name : null;
        if (nombre != null && nombre.EndsWith(" (Instance)"))
            nombre = nombre.Substring(0, nombre.Length - " (Instance)".Length);
        if (string.IsNullOrEmpty(nombre))
            return false;
        if (nombre.IndexOf("Lengua", System.StringComparison.OrdinalIgnoreCase) >= 0
            || nombre.IndexOf("Paladar", System.StringComparison.OrdinalIgnoreCase) >= 0
            || nombre.IndexOf("Diente", System.StringComparison.OrdinalIgnoreCase) >= 0
            || nombre.IndexOf("Unia", System.StringComparison.OrdinalIgnoreCase) >= 0
            || nombre.IndexOf("Piel", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return false;
        return ReconectarMateriales.EsNombreOjo(nombre);
    }

    static void AplicarTexturaTiled(GameObject go, Material fuente, Texture textura, float tile, Color fallback)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null)
            return;

        Material mat;
        if (fuente != null)
        {
            mat = Object.Instantiate(fuente);
            mat.name = fuente.name;
        }
        else
        {
            mat = renderer.material;
        }

        RepararShader(mat);
        if (textura != null)
        {
            mat.mainTexture = textura;
            mat.mainTextureScale = new Vector2(tile, tile);
            if (mat.HasProperty("_Color"))
                mat.color = Color.white;
        }
        else if (mat.HasProperty("_Color") && (mat.mainTexture == null))
        {
            mat.color = fallback;
        }

        renderer.material = mat;
    }

    static Texture2D CargarTextura(string resourcesPath, string assetPath)
    {
        var tex = Resources.Load<Texture2D>(resourcesPath);
        if (tex != null)
            return tex;

#if UNITY_EDITOR
        tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
#endif
        return tex;
    }

    static Material ConstruirSkybox6(string nombre, string assetPrefix)
    {
        var shader = Shader.Find("Skybox/6 Sided");
        if (shader == null)
            return null;

        var front = CargarCara(nombre + "_front", assetPrefix + "_front.tif");
        var back = CargarCara(nombre + "_back", assetPrefix + "_back.tif");
        var left = CargarCara(nombre + "_left", assetPrefix + "_left.tif");
        var right = CargarCara(nombre + "_right", assetPrefix + "_right.tif");
        var up = CargarCara(nombre + "_up", assetPrefix + "_up.tif");
        var down = CargarCara(nombre + "_down", assetPrefix + "_down.tif");
        if (front == null || back == null)
            return null;

        var mat = new Material(shader) { name = nombre };
        mat.SetTexture("_FrontTex", front);
        mat.SetTexture("_BackTex", back);
        mat.SetTexture("_LeftTex", left);
        mat.SetTexture("_RightTex", right);
        mat.SetTexture("_UpTex", up);
        mat.SetTexture("_DownTex", down);
        return mat;
    }

    static Texture CargarCara(string shortName, string assetPath)
    {
        var tex = Resources.Load<Texture>(shortName);
        if (tex != null)
            return tex;
#if UNITY_EDITOR
        tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
#endif
        return tex;
    }

    public static void RepararShader(Material material, Shader preferido = null)
    {
        if (material == null)
            return;

        if (material.shader != null
            && material.shader.name != "Hidden/InternalErrorShader"
            && !material.shader.name.Contains("InternalError"))
            return;

        var shader = preferido
            ?? Shader.Find("Legacy Shaders/Diffuse")
            ?? Shader.Find("Diffuse")
            ?? Shader.Find("Standard");
        if (shader == null)
            return;

        var tex = material.HasProperty("_MainTex") ? material.mainTexture : null;
        var color = material.HasProperty("_Color") ? material.color : Color.white;
        material.shader = shader;
        if (tex != null && material.HasProperty("_MainTex"))
            material.mainTexture = tex;
        if (material.HasProperty("_Color"))
            material.color = color;
    }
}
