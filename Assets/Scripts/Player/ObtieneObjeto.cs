using UnityEngine;

public class ObtieneObjeto : MonoBehaviour
{
    void OnTriggerEnter(Collider c)
    {
        Procesar(c, true);
    }

    void OnTriggerStay(Collider c)
    {
        Procesar(c, false);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit != null && hit.collider != null)
            Procesar(hit.collider, true);
    }

    void Procesar(Collider c, bool entrar)
    {
        if (c == null)
            return;

        var pickup = RaizPickup(c.transform);
        if (pickup == null)
            return;

        if (pickup.CompareTag("Puerta"))
        {
            IntentarPuerta();
            return;
        }

        if (pickup.CompareTag("Ducha"))
        {
            DescontaminarEnDucha();
            return;
        }

        if (!entrar)
            return;

        if (pickup.CompareTag("Tubo"))
        {
            if (!ObjetosPorJugador.TieneTubo)
            {
                GameGUI.MjeTieneTubo = true;
                ObjetosPorJugador.TieneTubo = true;
                Destroy(pickup.gameObject);
            }
            else
                GameGUI.MjeYaTenesTubo = true;
            return;
        }

        if (pickup.CompareTag("Caja"))
        {
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
        }
    }

    public void DescontaminarEnDucha()
    {
        if (!ObjetosPorJugador.TieneZoquete)
            return;

        ObjetosPorJugador.TieneZoquete = false;
        GameGUI.MjeDescontaminado = true;
    }

    static void IntentarPuerta()
    {
        if (ObjetosPorJugador.JugadorHaGanado || ObjetosPorJugador.JugadorHaPerdido)
            return;

        if (!ObjetosPorJugador.TieneTubo)
        {
            ObjetosPorJugador.TocandoPuerta = true;
            return;
        }

        if (ObjetosPorJugador.TieneZoquete)
        {
            GameGUI.MjeContaminado = true;
            return;
        }

        ObjetosPorJugador.JugadorHaGanado = true;
        if (GestionaMultiJugador.Instancia != null)
            GestionaMultiJugador.Instancia.TerminarJuego();
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

public class DuchaTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Avisar(other);
    }

    void OnTriggerStay(Collider other)
    {
        Avisar(other);
    }

    static void Avisar(Collider other)
    {
        if (other == null)
            return;

        var obtiene = other.GetComponent<ObtieneObjeto>()
            ?? other.GetComponentInParent<ObtieneObjeto>();
        if (obtiene != null)
            obtiene.DescontaminarEnDucha();
    }
}
