using UnityEngine;

public class EtiquetaJugador : MonoBehaviour
{
    public string nombreJugador;
    TextMesh _label;

    void Start()
    {
        var holder = new GameObject("Nombre");
        holder.transform.SetParent(transform, false);
        holder.transform.localPosition = new Vector3(0, 2.2f, 0);
        _label = holder.AddComponent<TextMesh>();
        _label.alignment = TextAlignment.Center;
        _label.anchor = TextAnchor.LowerCenter;
        _label.characterSize = 0.08f;
        _label.fontSize = 48;
        _label.color = Color.yellow;
        _label.text = nombreJugador;
    }

    void LateUpdate()
    {
        if (_label == null)
            return;
        _label.text = nombreJugador;
        if (Camera.main != null)
            _label.transform.rotation = Quaternion.LookRotation(_label.transform.position - Camera.main.transform.position);
    }
}
