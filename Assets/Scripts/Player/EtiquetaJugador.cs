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
        holder.transform.localScale = Vector3.one * 0.008f;
        holder.transform.localRotation = Quaternion.identity;

        var canvas = holder.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 80;
        canvas.planeDistance = 0.6f;
        var raycaster = holder.AddComponent<GraphicRaycaster>();
        raycaster.enabled = false;
        AsignarCamara(canvas);

        var rt = holder.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(90f, 20f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        var chip = UiFactory.AddPanelFixed(
            holder.transform,
            "Fondo",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(88f, 18f),
            new Color(0.05f, 0.07f, 0.07f, 0.82f));

        var text = UiFactory.AddText(
            chip.transform,
            "Nombre",
            NombreVisible(),
            12,
            TextAnchor.MiddleCenter,
            new Color(0.96f, 0.98f, 0.94f, 1f),
            true,
            FontStyle.Bold);
        UiFactory.Stretch(text.rectTransform);
        text.rectTransform.offsetMin = new Vector2(4f, 1f);
        text.rectTransform.offsetMax = new Vector2(-4f, -1f);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 8;
        text.resizeTextMaxSize = 12;
        UiFactory.AddOutline(text, new Color(0f, 0f, 0f, 0.7f), new Vector2(0.5f, -0.5f));

        _label = text;
        _canvas = canvas;
        _billboard = holder.transform;
        ChipMundoListo = GetCamera() != null;
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

        var cam = GetCamera();
        if (_canvas != null)
            AsignarCamara(_canvas, cam);

        if (_billboard == null)
            return;

        _billboard.localPosition = new Vector3(0f, Mathf.Max(0.5f, AlturaSobreCabeza() - 0.5f), 0f);

        if (cam == null)
            return;

        var dirToCamera = _billboard.position - cam.transform.position;
        if (dirToCamera.sqrMagnitude > 0.0001f)
            _billboard.rotation = Quaternion.LookRotation(dirToCamera, Vector3.up);

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

    static Camera GetCamera()
    {
        if (Camera.main != null)
            return Camera.main;

        var cam = UnityEngine.Object.FindAnyObjectByType<Camera>();
        return cam != null ? cam : null;
    }

    static void AsignarCamara(Canvas canvas, Camera cam = null)
    {
        if (canvas == null)
            return;

        if (cam == null)
            cam = GetCamera();
        if (cam == null)
            return;

        canvas.worldCamera = cam;
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
