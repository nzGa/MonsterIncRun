using UnityEngine;
using UnityEngine.UI;

public class EtiquetaJugador : MonoBehaviour
{
    public string nombreJugador;
    public static bool ChipMundoListo { get; private set; }

    Text _label;
    Canvas _canvas;
    Transform _billboard;

    void Start()
    {
        var holder = new GameObject("NombreChip");
        holder.transform.SetParent(transform, false);
        holder.transform.localPosition = new Vector3(0f, AlturaSobreCabeza(), 0f);

        var ls = transform.lossyScale;
        const float world = 0.0048f;
        holder.transform.localScale = new Vector3(
            world / Mathf.Max(Mathf.Abs(ls.x), 1e-4f),
            world / Mathf.Max(Mathf.Abs(ls.y), 1e-4f),
            world / Mathf.Max(Mathf.Abs(ls.z), 1e-4f));

        var canvas = holder.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 80;
        AsignarCamara(canvas);
        var rt = holder.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(110f, 26f);
        rt.pivot = new Vector2(0.5f, 0f);

        var chip = UiFactory.AddPanelFixed(
            holder.transform,
            "Fondo",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(108f, 24f),
            new Color(0.05f, 0.07f, 0.07f, 0.82f));

        var text = UiFactory.AddText(
            chip.transform,
            "Nombre",
            NombreVisible(),
            15,
            TextAnchor.MiddleCenter,
            new Color(0.96f, 0.98f, 0.94f, 1f),
            true,
            FontStyle.Bold);
        UiFactory.Stretch(text.rectTransform);
        text.rectTransform.offsetMin = new Vector2(6f, 1f);
        text.rectTransform.offsetMax = new Vector2(-6f, -1f);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = 15;
        UiFactory.AddOutline(text, new Color(0f, 0f, 0f, 0.7f), new Vector2(0.8f, -0.8f));

        _label = text;
        _canvas = canvas;
        _billboard = holder.transform;
        ChipMundoListo = Camera.main != null;
    }

    void OnDestroy()
    {
        if (_billboard != null)
            ChipMundoListo = false;
    }

    void LateUpdate()
    {
        if (_label != null)
            _label.text = NombreVisible();

        if (_canvas != null)
            AsignarCamara(_canvas);

        if (_billboard == null)
            return;

        _billboard.localPosition = new Vector3(0f, AlturaSobreCabeza(), 0f);

        if (Camera.main == null)
            return;

        _billboard.rotation = Camera.main.transform.rotation;
        ChipMundoListo = true;
    }

    float AlturaSobreCabeza()
    {
        // Last change used bounds.max.y - 0.28 vs the previous +0.16 (a 0.44m drop)
        // into the mesh, so the chip sat inside the skull / behind the orbit camera.
        float y = 2.12f;

        var cabeza = BuscarHijo(transform, "Bip002 Head001")
            ?? BuscarHijo(transform, "Bip002 Head")
            ?? BuscarHijo(transform, "Bip001 Head");
        if (cabeza != null)
            y = Mathf.Max(y, cabeza.position.y - transform.position.y + 0.28f);

        var b = BoundsMallas();
        if (b.HasValue)
            y = Mathf.Max(y, b.Value.max.y - transform.position.y + 0.14f);

        return y;
    }

    Bounds? BoundsMallas()
    {
        Bounds? acc = null;
        var rs = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rs.Length; i++)
        {
            var r = rs[i];
            if (r == null || !r.enabled || r is ParticleSystemRenderer)
                continue;
            if (!acc.HasValue)
                acc = r.bounds;
            else
            {
                var t = acc.Value;
                t.Encapsulate(r.bounds);
                acc = t;
            }
        }

        return acc;
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

    static void AsignarCamara(Canvas canvas)
    {
        if (canvas == null || Camera.main == null)
            return;
        canvas.worldCamera = Camera.main;
    }

    string NombreVisible()
    {
        if (!string.IsNullOrEmpty(nombreJugador))
            return nombreJugador;
        if (GestionaMultiJugador.Instancia != null && !string.IsNullOrEmpty(GestionaMultiJugador.Instancia.nombreJugador))
            return GestionaMultiJugador.Instancia.nombreJugador;
        return "Mike";
    }
}
