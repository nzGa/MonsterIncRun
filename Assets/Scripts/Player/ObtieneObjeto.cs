using UnityEngine;

public class ObtieneObjeto : MonoBehaviour
{
    void OnTriggerEnter(Collider c)
    {
        var pickup = RaizPickup(c.transform);
        if (pickup == null)
            return;

        switch (pickup.tag)
        {
            case "Tubo":
                if (!ObjetosPorJugador.TieneTubo)
                {
                    GameGUI.MjeTieneTubo = true;
                    ObjetosPorJugador.TieneTubo = true;
                    Destroy(pickup.gameObject);
                }
                else
                    GameGUI.MjeYaTenesTubo = true;
                break;

            case "Caja":
                var contenido = pickup.GetComponent<ContenidoCaja>()
                    ?? pickup.GetComponentInChildren<ContenidoCaja>();
                int item = contenido != null ? contenido.NumeroItem : 0;
                switch (item)
                {
                    case 0:
                        // En un jugador la media te contamina a vos (en multi contaminaba a los demas).
                        if (ObjetosPorJugador.TieneCasco)
                            GameGUI.MjeCascoUsado = true;
                        else
                        {
                            GameGUI.MjeTieneZoquete = true;
                            ObjetosPorJugador.TieneZoquete = true;
                        }
                        Destroy(pickup.gameObject);
                        break;
                    case 1:
                        if (ObjetosPorJugador.TieneCasco)
                            GameGUI.MjeYaTenesItem = true;
                        else
                        {
                            GameGUI.MjeTieneCasco = true;
                            ObjetosPorJugador.TieneCasco = true;
                        }
                        Destroy(pickup.gameObject);
                        break;
                    case 2:
                        if (ObjetosPorJugador.TieneZapato)
                            GameGUI.MjeYaTenesItem = true;
                        else
                        {
                            ObjetosPorJugador.TieneZapato = true;
                            GameGUI.MensajeZapato = true;
                        }
                        Destroy(pickup.gameObject);
                        break;
                }
                break;

            case "Ducha":
                if (ObjetosPorJugador.TieneZoquete)
                    GameGUI.MjeDescontaminado = true;
                ObjetosPorJugador.TieneZoquete = false;
                break;

            case "Puerta":
                if (ObjetosPorJugador.TieneTubo)
                {
                    if (!ObjetosPorJugador.TieneZoquete)
                    {
                        ObjetosPorJugador.JugadorHaGanado = true;
                        var manager = GameObject.FindGameObjectWithTag("GameManager");
                        if (manager != null)
                            manager.GetComponent<GestionaMultiJugador>().TerminarJuego();
                    }
                    else
                        GameGUI.MjeContaminado = true;
                }
                else
                    ObjetosPorJugador.TocandoPuerta = true;
                break;
        }
    }

    static Transform RaizPickup(Transform t)
    {
        if (t == null)
            return null;

        var root = t;
        while (root.parent != null && !EsPickup(root))
            root = root.parent;

        if (!EsPickup(root))
            return null;

        while (root.parent != null && EsPickup(root.parent))
            root = root.parent;

        return root;
    }

    static bool EsPickup(Transform t)
    {
        return t.CompareTag("Tubo")
            || t.CompareTag("Caja")
            || t.CompareTag("Puerta")
            || t.CompareTag("Ducha");
    }
}
