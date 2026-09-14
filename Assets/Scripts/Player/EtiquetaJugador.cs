using UnityEngine;
using UnityEngine.UI;

public class EtiquetaJugador : MonoBehaviour
{
    public string nombreJugador;
    Text _label;
    Transform _billboard;

    void Start()
    {
        var holder = new GameObject("NombreCanvas");
        holder.transform.SetParent(transform, false);
        holder.transform.localPosition = new Vector3(0f, 2.35f, 0f);
        holder.transform.localScale = Vector3.one * 0.01f;

        var canvas = holder.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 50;

        var rt = holder.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220f, 42f);

        var fondo = UiFactory.AddPanel(holder.transform, "Fondo", Vector2.zero, Vector2.one, new Color(0.05f, 0.07f, 0.06f, 0.82f));
        UiFactory.Stretch(fondo.rectTransform);

        _label = UiFactory.AddText(holder.transform, "Nombre", nombreJugador, 28, TextAnchor.MiddleCenter, Color.white, true, FontStyle.Bold);
        UiFactory.Stretch(_label.rectTransform);
        _billboard = holder.transform;
    }

    void LateUpdate()
    {
        if (_label != null)
            _label.text = string.IsNullOrEmpty(nombreJugador) ? "Mike" : nombreJugador;

        if (_billboard == null || Camera.main == null)
            return;

        _billboard.rotation = Camera.main.transform.rotation;
    }
}
