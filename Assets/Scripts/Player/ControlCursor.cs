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
        if (scriptMultiJugador == null)
            return;
        scriptMultiJugador.AplicarCursor();
    }
}
