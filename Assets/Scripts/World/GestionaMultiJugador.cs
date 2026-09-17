using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-200)]
public class GestionaMultiJugador : MonoBehaviour
{
    [SerializeField] float _segundosParaPerder = 900f;

    public static GestionaMultiJugador Instancia { get; private set; }

    public static bool juegoIniciado;
    public bool juegoFinalizado;
    public bool cursorBloqueado = true;
    public bool muestraVentanaEstadoCliente;

    public string nombreJugador = "Mike";

    Canvas _pauseCanvas;
    GameObject _pauseRoot;

    public static bool ControlJugadorActivo
    {
        get
        {
            if (Instancia == null)
                return !AltLiberaCursor();
            return Instancia.QuiereCapturarCursor();
        }
    }

    void Awake()
    {
        var nombreGuardado = PlayerPrefs.GetString("NombreJugador", "Mike");
        if (!string.IsNullOrWhiteSpace(nombreGuardado))
            nombreJugador = nombreGuardado;

        Instancia = this;
    }

    void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }

    void Start()
    {
        juegoIniciado = true;
        juegoFinalizado = false;
        ObjetosPorJugador.Reset();
        MostrarTimer.TimerActivado = true;
        MostrarTimer.segundosFaltantes = _segundosParaPerder;
        cursorBloqueado = true;
        AplicarCursor();
        ConstruirPausa();
        Invoke(nameof(PerderPorTiempo), _segundosParaPerder);
    }

    void Update()
    {
        if (MostrarTimer.TimerActivado && MostrarTimer.segundosFaltantes > 0)
            MostrarTimer.segundosFaltantes -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePausa();

        AplicarCursor();
    }

    void TogglePausa()
    {
        muestraVentanaEstadoCliente = !muestraVentanaEstadoCliente;
        cursorBloqueado = !muestraVentanaEstadoCliente;
        if (_pauseRoot != null)
            _pauseRoot.SetActive(muestraVentanaEstadoCliente);
        AplicarCursor();
    }

    bool QuiereCapturarCursor()
    {
        return cursorBloqueado && !AltLiberaCursor();
    }

    static bool AltLiberaCursor()
    {
        return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
    }

    public void AplicarCursor()
    {
        bool capturar = QuiereCapturarCursor();
        Cursor.lockState = capturar ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !capturar;
    }

    void PerderPorTiempo()
    {
        if (!juegoFinalizado)
            ObjetosPorJugador.JugadorHaPerdido = true;
    }

    public void TerminarJuego()
    {
        juegoFinalizado = true;
        MostrarTimer.TimerActivado = false;
        CancelInvoke(nameof(PerderPorTiempo));
    }

    void ConstruirPausa()
    {
        _pauseCanvas = UiFactory.CreateCanvas("PauseCanvas", 50);
        _pauseRoot = _pauseCanvas.gameObject;
        var dim = UiFactory.AddImage(_pauseRoot.transform, "Dim", Vector2.zero, Vector2.one, new Color(0, 0, 0, 0.65f));
        dim.raycastTarget = true;

        var panel = UiFactory.AddPanel(_pauseRoot.transform, "Panel", new Vector2(0.34f, 0.28f), new Vector2(0.66f, 0.72f), new Color(0.06f, 0.08f, 0.06f, 0.94f));

        var titulo = UiFactory.AddText(panel.transform, "Titulo", "Pausa", 40, TextAnchor.MiddleCenter, Color.white, true, FontStyle.Bold);
        var tituloRt = titulo.rectTransform;
        tituloRt.anchorMin = new Vector2(0.08f, 0.84f);
        tituloRt.anchorMax = new Vector2(0.92f, 0.98f);
        tituloRt.offsetMin = Vector2.zero;
        tituloRt.offsetMax = Vector2.zero;
        titulo.verticalOverflow = VerticalWrapMode.Truncate;

        var hint = UiFactory.AddText(panel.transform, "Hint", "Esc para continuar", 18, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.7f), true);
        var hintRt = hint.rectTransform;
        hintRt.anchorMin = new Vector2(0.08f, 0.72f);
        hintRt.anchorMax = new Vector2(0.92f, 0.84f);
        hintRt.offsetMin = Vector2.zero;
        hintRt.offsetMax = Vector2.zero;
        hint.verticalOverflow = VerticalWrapMode.Truncate;

        var volver = UiFactory.AddButton(panel.transform, "Volver", "Volver al juego", new Vector2(0, 20), new Vector2(320, 56));
        volver.onClick.AddListener(() =>
        {
            muestraVentanaEstadoCliente = false;
            cursorBloqueado = true;
            _pauseRoot.SetActive(false);
            AplicarCursor();
        });

        var menu = UiFactory.AddButton(panel.transform, "Menu", "Menú principal", new Vector2(0, -50), new Vector2(320, 56));
        menu.onClick.AddListener(() =>
        {
            cursorBloqueado = false;
            AplicarCursor();
            SceneManager.LoadScene("MainMenu");
        });

        var salir = UiFactory.AddButton(panel.transform, "Salir", "Salir", new Vector2(0, -120), new Vector2(320, 56));
        salir.onClick.AddListener(UiFactory.Salir);

        _pauseRoot.SetActive(false);
    }
}
