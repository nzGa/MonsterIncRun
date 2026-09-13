using UnityEngine;

public class ControlCursor : MonoBehaviour
{
    GestionaMultiJugador scriptMultiJugador;

    void Start()
    {
        var manager = GameObject.Find("GameManager");
        if (manager == null)
            manager = GameObject.FindGameObjectWithTag("GameManager");
        if (manager != null)
            scriptMultiJugador = manager.GetComponent<GestionaMultiJugador>();
    }

    void Update()
    {
        var multi = scriptMultiJugador != null ? scriptMultiJugador : GestionaMultiJugador.Instancia;
        if (multi == null)
            return;
        multi.AplicarCursor();
    }
}
