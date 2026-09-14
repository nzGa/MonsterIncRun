using UnityEngine;

public class EtiquetaJugador : MonoBehaviour
{
    public string nombreJugador;
    TextMesh _label;
    Transform _billboard;

    void Start()
    {
        var holder = new GameObject("NombreBillboard");
        holder.transform.SetParent(transform, false);

        float y = 2.55f;
        var rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            y = rend.bounds.max.y - transform.position.y + 0.5f;
        holder.transform.localPosition = new Vector3(0f, y, 0f);

        var tm = holder.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (tm.font == null)
            tm.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (tm.font == null)
            tm.font = Font.CreateDynamicFontFromOSFont("Arial", 42);
        tm.fontSize = 52;
        tm.characterSize = 0.08f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.color = new Color(1f, 0.95f, 0.12f, 1f);
        tm.text = NombreVisible();

        var mr = holder.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            if (tm.font != null && tm.font.material != null)
                mr.sharedMaterial = tm.font.material;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        _label = tm;
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
