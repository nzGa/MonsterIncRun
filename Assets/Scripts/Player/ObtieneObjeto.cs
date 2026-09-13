using UnityEngine;

public class ObtieneObjeto : MonoBehaviour
{
    void OnTriggerEnter(Collider c)
    {
        switch (c.gameObject.tag)
        {
            case "Tubo":
                if (!ObjetosPorJugador.TieneTubo)
                {
                    GameGUI.MjeTieneTubo = true;
                    ObjetosPorJugador.TieneTubo = true;
                    Destroy(c.gameObject);
                }
                else
                    GameGUI.MjeYaTenesTubo = true;
                break;

            case "Caja":
                var contenido = c.gameObject.GetComponent<ContenidoCaja>();
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
                        Destroy(c.gameObject);
                        break;
                    case 1:
                        if (ObjetosPorJugador.TieneCasco)
                            GameGUI.MjeYaTenesItem = true;
                        else
                        {
                            GameGUI.MjeTieneCasco = true;
                            ObjetosPorJugador.TieneCasco = true;
                        }
                        Destroy(c.gameObject);
                        break;
                    case 2:
                        if (ObjetosPorJugador.TieneZapato)
                            GameGUI.MjeYaTenesItem = true;
                        else
                        {
                            ObjetosPorJugador.TieneZapato = true;
                            GameGUI.MensajeZapato = true;
                        }
                        Destroy(c.gameObject);
                        break;
                }
                break;

            case "Ducha":
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
}
