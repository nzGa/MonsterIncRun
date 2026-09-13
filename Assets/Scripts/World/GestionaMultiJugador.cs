using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-200)]
public class GestionaMultiJugador : MonoBehaviour
{
    [SerializeField] float _segundosParaPerder = 900f;

    public static bool juegoIniciado;
    public bool juegoFinalizado;
    public bool cursorBloqueado = true;
    public bool muestraVentanaEstadoCliente;

    public string nombreJugador = "Mike";

    Canvas _pauseCanvas;
    GameObject _pauseRoot;

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
    }

    void TogglePausa()
    {
        muestraVentanaEstadoCliente = !muestraVentanaEstadoCliente;
        cursorBloqueado = !muestraVentanaEstadoCliente;
        if (_pauseRoot != null)
            _pauseRoot.SetActive(muestraVentanaEstadoCliente);
        AplicarCursor();
    }

    public void AplicarCursor()
    {
        Cursor.lockState = cursorBloqueado ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !cursorBloqueado;
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
        _pauseCanvas = UiFactory.CreateCanvas("PauseCanvas", 20);
        _pauseRoot = _pauseCanvas.gameObject;
        var dim = UiFactory.AddImage(_pauseRoot.transform, "Dim", Vector2.zero, Vector2.one, new Color(0, 0, 0, 0.65f));
        dim.raycastTarget = true;

        var panel = UiFactory.AddImage(_pauseRoot.transform, "Panel", new Vector2(0.35f, 0.3f), new Vector2(0.65f, 0.7f), new Color(0.08f, 0.08f, 0.08f, 0.92f));

        var titulo = UiFactory.AddText(panel.transform, "Titulo", "Pausa", 36, TextAnchor.UpperCenter, Color.white);
        titulo.rectTransform.anchorMin = new Vector2(0.1f, 0.75f);
        titulo.rectTransform.anchorMax = new Vector2(0.9f, 0.95f);
        UiFactory.Stretch(titulo.rectTransform);

        var volver = UiFactory.AddButton(panel.transform, "Volver", "Volver al juego", new Vector2(0, 20), new Vector2(320, 56));
        volver.onClick.AddListener(() =>
        {
            muestraVentanaEstadoCliente = false;
            cursorBloqueado = true;
            _pauseRoot.SetActive(false);
            AplicarCursor();
        });

        var menu = UiFactory.AddButton(panel.transform, "Menu", "Menu principal", new Vector2(0, -50), new Vector2(320, 56));
        menu.onClick.AddListener(() =>
        {
            cursorBloqueado = false;
            AplicarCursor();
            SceneManager.LoadScene("MainMenu");
        });

        var salir = UiFactory.AddButton(panel.transform, "Salir", "Salir", new Vector2(0, -120), new Vector2(320, 56));
        salir.onClick.AddListener(Application.Quit);

        _pauseRoot.SetActive(false);
    }
}
