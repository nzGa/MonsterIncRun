using UnityEngine;

public class NombreJugador : MonoBehaviour
{
    public string nombreJugador;
    public bool fNombreSeteado;
    public bool fNombreNotificado;

    void Update()
    {
        if (!fNombreNotificado && fNombreSeteado)
        {
            var etiqueta = GetComponent<EtiquetaJugador>();
            if (etiqueta != null)
                etiqueta.nombreJugador = nombreJugador;
            fNombreNotificado = true;
        }
    }
}
