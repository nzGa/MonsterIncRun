using UnityEngine;

public class ObjetosPorJugador : MonoBehaviour
{
    static bool _jugadorHaGanado;
    public static bool JugadorHaGanado { get { return _jugadorHaGanado; } set { _jugadorHaGanado = value; } }

    static bool _jugadorHaPerdido;
    public static bool JugadorHaPerdido { get { return _jugadorHaPerdido; } set { _jugadorHaPerdido = value; } }

    static bool _tieneTubo;
    public static bool TieneTubo { get { return _tieneTubo; } set { _tieneTubo = value; } }

    static bool _tocandoPuerta;
    public static bool TocandoPuerta { get { return _tocandoPuerta; } set { _tocandoPuerta = value; } }

    static bool _tieneZapato;
    public static bool TieneZapato { get { return _tieneZapato; } set { _tieneZapato = value; } }

    static bool _tieneZoquete;
    public static bool TieneZoquete { get { return _tieneZoquete; } set { _tieneZoquete = value; } }

    static bool _tieneCasco;
    public static bool TieneCasco { get { return _tieneCasco; } set { _tieneCasco = value; } }

    public static void Reset()
    {
        _jugadorHaGanado = false;
        _jugadorHaPerdido = false;
        _tieneTubo = false;
        _tocandoPuerta = false;
        _tieneZapato = false;
        _tieneZoquete = false;
        _tieneCasco = false;
        GameGUI.MensajeZapato = false;
        GameGUI.MjeTieneTubo = false;
        GameGUI.MjeTieneZoquete = false;
        GameGUI.MjeTieneCasco = false;
        GameGUI.MjeDescontaminado = false;
        GameGUI.MjeContaminado = false;
        GameGUI.MjeContaminaste = false;
        GameGUI.MjeCascoUsado = false;
        GameGUI.MjeYaTenesTubo = false;
        GameGUI.MjeYaTenesItem = false;
        MostrarTimer.segundosFaltantes = -1f;
        MostrarTimer.TimerActivado = false;
    }
}
