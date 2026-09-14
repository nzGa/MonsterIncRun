using UnityEngine;
using UnityEngine.UI;

public class EtiquetaJugador : MonoBehaviour
{
    public string nombreJugador;
    Text _label;
    Transform _billboard;

    void Start()
    {
        var holder = new GameObject("NombreChip");
        holder.transform.SetParent(transform, false);

        float y = 2.18f;
        var rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            y = rend.bounds.max.y - transform.position.y + 0.16f;
        holder.transform.localPosition = new Vector3(0f, y, 0f);
        holder.transform.localScale = Vector3.one * 0.0046f;

        var canvas = holder.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 40;
        var rt = holder.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120f, 28f);
        rt.pivot = new Vector2(0.5f, 0f);

        var chip = UiFactory.AddPanelFixed(
            holder.transform,
            "Fondo",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(118f, 26f),
            new Color(0.05f, 0.07f, 0.07f, 0.82f));

        var text = UiFactory.AddText(
            chip.transform,
            "Nombre",
            NombreVisible(),
            16,
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
        text.resizeTextMinSize = 11;
        text.resizeTextMaxSize = 16;
        UiFactory.AddOutline(text, new Color(0f, 0f, 0f, 0.7f), new Vector2(0.8f, -0.8f));

        _label = text;
        _billboard = holder.transform;
    }

    void LateUpdate()
    {
        if (_label != null)
            _label.text = NombreVisible();

        if (_billboard == null || Camera.main == null)
            return;

        _billboard.rotation = Camera.main.transform.rotation;
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
