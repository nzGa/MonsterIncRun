using UnityEngine;
using UnityEngine.UI;
using System;

[DefaultExecutionOrder(-100)]
public class GameGUI : MonoBehaviour
{
    static bool _mje_zapato;
    public static bool MensajeZapato { get { return _mje_zapato; } set { _mje_zapato = value; } }
    static bool _mje_tiene_tubo;
    public static bool MjeTieneTubo { get { return _mje_tiene_tubo; } set { _mje_tiene_tubo = value; } }
    static bool _mje_tiene_zoquete;
    public static bool MjeTieneZoquete { get { return _mje_tiene_zoquete; } set { _mje_tiene_zoquete = value; } }
    static bool _mje_tiene_casco;
    public static bool MjeTieneCasco { get { return _mje_tiene_casco; } set { _mje_tiene_casco = value; } }
    static bool _mje_descontaminado;
    public static bool MjeDescontaminado { get { return _mje_descontaminado; } set { _mje_descontaminado = value; } }
    static bool _mje_contaminado;
    public static bool MjeContaminado { get { return _mje_contaminado; } set { _mje_contaminado = value; } }
    static bool _mje_contaminaste;
    public static bool MjeContaminaste { get { return _mje_contaminaste; } set { _mje_contaminaste = value; } }
    static bool _mje_casco_usado;
    public static bool MjeCascoUsado { get { return _mje_casco_usado; } set { _mje_casco_usado = value; } }
    static bool _mje_ya_tenes_tubo;
    public static bool MjeYaTenesTubo { get { return _mje_ya_tenes_tubo; } set { _mje_ya_tenes_tubo = value; } }
    static bool _mje_ya_tenes_item;
    public static bool MjeYaTenesItem { get { return _mje_ya_tenes_item; } set { _mje_ya_tenes_item = value; } }

    static readonly Color SlotVacio = new Color(0.12f, 0.14f, 0.12f, 0.92f);
    static readonly Color SlotActivo = new Color(0.18f, 0.52f, 0.26f, 0.96f);
    static readonly Color SlotAlerta = new Color(0.68f, 0.30f, 0.08f, 0.96f);
    static readonly Color IconoApagado = new Color(1f, 1f, 1f, 0.32f);
    static readonly Color MensajeOk = new Color(0.72f, 1f, 0.78f, 1f);
    static readonly Color MensajeAlerta = new Color(1f, 0.82f, 0.32f, 1f);
    static readonly Color MensajePeligro = new Color(1f, 0.48f, 0.32f, 1f);

    Slot _slotZapato;
    Slot _slotZoquete;
    Slot _slotCasco;
    Slot _slotTubo;
    Image _imgGanaste;
    Image _imgPerdiste;
    Image _veloFinal;
    Image _panelMensaje;
    Text _mensaje;
    Text _timer;
    Canvas _hud;
    string _textoMensaje;
    Color _colorMensaje = Color.white;
    float _ocultarMensajeEn;
    int _ultimoAvisoSegundos = -1;
    bool mje_falta_tubo;

    void Awake()
    {
        DestroyLegacyOverlays();
        ConstruirHud();
    }

    void OnDestroy()
    {
        if (_hud != null)
            Destroy(_hud.gameObject);
    }

    // Replaces Unity 4 IMGUI. Canvas is the only in-game HUD.
    void OnGUI()
    {
    }

    void Update()
    {
        DestroyLegacyOverlays();

        if (ObjetosPorJugador.TocandoPuerta)
        {
            ObjetosPorJugador.TocandoPuerta = false;
            mje_falta_tubo = true;
        }

        if (_hud == null)
            ConstruirHud();

        ActualizarTimer();
        ActualizarInventario();
        ActualizarAvisosTiempo();
        CapturarMensaje();
        ActualizarMensaje();
        ActualizarFinal();
    }

    void DestroyLegacyOverlays()
    {
        var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        for (int i = 0; i < canvases.Length; i++)
        {
            var canvas = canvases[i];
            if (canvas == null || canvas == _hud)
                continue;
            if (canvas.renderMode == RenderMode.WorldSpace)
                continue;
            if (canvas.name == "TimerCanvas" || canvas.name == "HudCanvas")
                Destroy(canvas.gameObject);
            else if (canvas.name == "MonsterHudCanvas" && canvas != _hud)
                Destroy(canvas.gameObject);
        }
    }

    void ActualizarTimer()
    {
        if (_timer == null)
            return;

        if (MostrarTimer.segundosFaltantes < 0f)
        {
            _timer.text = "--:--";
            _timer.color = Color.white;
            return;
        }

        int rounded = Mathf.Max(0, Mathf.CeilToInt(MostrarTimer.segundosFaltantes));
        int minutes = rounded / 60;
        int seconds = rounded % 60;
        _timer.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);

        if (rounded <= 30)
            _timer.color = MensajePeligro;
        else if (rounded <= 120)
            _timer.color = MensajeAlerta;
        else
            _timer.color = Color.white;
    }

    void ActualizarInventario()
    {
        if (_slotZapato != null)
            _slotZapato.SetHeld(ObjetosPorJugador.TieneZapato, false);
        if (_slotZoquete != null)
            _slotZoquete.SetHeld(ObjetosPorJugador.TieneZoquete, ObjetosPorJugador.TieneZoquete);
        if (_slotCasco != null)
            _slotCasco.SetHeld(ObjetosPorJugador.TieneCasco, false);
        if (_slotTubo != null)
            _slotTubo.SetHeld(ObjetosPorJugador.TieneTubo, false);
    }

    void ActualizarAvisosTiempo()
    {
        if (ObjetosPorJugador.JugadorHaGanado || ObjetosPorJugador.JugadorHaPerdido)
            return;
        if (MostrarTimer.segundosFaltantes < 0f)
            return;

        int s = Mathf.CeilToInt(MostrarTimer.segundosFaltantes);
        if (s == _ultimoAvisoSegundos)
            return;

        string aviso = null;
        Color color = MensajeAlerta;
        if (s == 300)
            aviso = "Quedan 5 minutos!";
        else if (s == 120)
            aviso = "Quedan 2 minutos!";
        else if (s == 60)
            aviso = "Queda 1 minuto!";
        else if (s == 30)
        {
            aviso = "Quedan 30 segundos!";
            color = MensajePeligro;
        }

        if (aviso == null)
            return;

        _ultimoAvisoSegundos = s;
        MostrarToast(aviso, color);
    }

    void CapturarMensaje()
    {
        if (ObjetosPorJugador.JugadorHaGanado || ObjetosPorJugador.JugadorHaPerdido)
            return;

        string texto = null;
        Color color = Color.white;

        if (mje_falta_tubo)
        {
            texto = "Necesitas un tubo para poder pasar";
            color = MensajeAlerta;
        }
        else if (MensajeZapato)
        {
            texto = "Ahora corres mas rapido!";
            color = MensajeOk;
        }
        else if (MjeTieneTubo)
        {
            texto = "Busca la puerta y gana!";
            color = MensajeOk;
        }
        else if (MjeTieneZoquete)
        {
            texto = "Te han contaminado! Busca la ducha!";
            color = MensajePeligro;
        }
        else if (MjeTieneCasco)
        {
            texto = "Sos inmune a las contaminaciones!";
            color = MensajeOk;
        }
        else if (MjeDescontaminado)
        {
            texto = "Estas descontaminado!";
            color = MensajeOk;
        }
        else if (MjeContaminado)
        {
            texto = "Debes descontaminarte en la ducha!";
            color = MensajePeligro;
        }
        else if (MjeContaminaste)
        {
            texto = "Contaminaste a todos tus oponentes";
            color = MensajeAlerta;
        }
        else if (MjeCascoUsado)
        {
            texto = "No han podido contaminarte";
            color = MensajeOk;
        }
        else if (MjeYaTenesTubo)
        {
            texto = "Ya tenes un tubo";
            color = MensajeAlerta;
        }
        else if (MjeYaTenesItem)
        {
            texto = "Ya tenes este objeto";
            color = MensajeAlerta;
        }

        if (texto == null)
            return;

        LimpiarFlags();
        MostrarToast(texto, color);
    }

    void MostrarToast(string texto, Color color)
    {
        _textoMensaje = texto;
        _colorMensaje = color;
        _ocultarMensajeEn = Time.unscaledTime + 3.2f;
    }

    void ActualizarMensaje()
    {
        if (_mensaje == null || _panelMensaje == null)
            return;

        if (ObjetosPorJugador.JugadorHaGanado || ObjetosPorJugador.JugadorHaPerdido)
        {
            _panelMensaje.gameObject.SetActive(false);
            return;
        }

        bool visible = !string.IsNullOrEmpty(_textoMensaje) && Time.unscaledTime < _ocultarMensajeEn;
        _panelMensaje.gameObject.SetActive(visible);
        if (!visible)
            return;

        _mensaje.text = _textoMensaje;
        _mensaje.color = _colorMensaje;
    }

    void ActualizarFinal()
    {
        bool gano = ObjetosPorJugador.JugadorHaGanado;
        bool perdio = ObjetosPorJugador.JugadorHaPerdido && !gano;
        if (_imgGanaste != null)
            _imgGanaste.enabled = gano;
        if (_imgPerdiste != null)
            _imgPerdiste.enabled = perdio;
        if (_veloFinal != null)
            _veloFinal.enabled = false;
    }

    void LimpiarFlags()
    {
        mje_falta_tubo = false;
        MensajeZapato = false;
        MjeTieneTubo = false;
        MjeTieneZoquete = false;
        MjeDescontaminado = false;
        MjeContaminado = false;
        MjeTieneCasco = false;
        MjeContaminaste = false;
        MjeCascoUsado = false;
        MjeYaTenesTubo = false;
        MjeYaTenesItem = false;
    }

    void ConstruirHud()
    {
        var         leftover = GameObject.Find("HudCanvas");
        if (leftover != null)
            DestroyImmediate(leftover);
        leftover = GameObject.Find("TimerCanvas");
        if (leftover != null)
            DestroyImmediate(leftover);
        leftover = GameObject.Find("MonsterHudCanvas");
        if (leftover != null)
            DestroyImmediate(leftover);

        _hud = UiFactory.CreateCanvas("MonsterHudCanvas", 80);
        var canvas = _hud.transform;

        var barra = UiFactory.AddTopBar(canvas, "BarraSuperior", 96f, new Color(0.02f, 0.03f, 0.03f, 0.82f));

        var timerPanel = UiFactory.AddPanelFixed(barra.transform, "Timer", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280f, 76f), new Color(0.06f, 0.09f, 0.06f, 0.95f));

        _timer = UiFactory.AddText(timerPanel.transform, "Valor", "15:00", 44, TextAnchor.MiddleCenter, Color.white, true, FontStyle.Bold);
        _timer.rectTransform.anchorMin = Vector2.zero;
        _timer.rectTransform.anchorMax = Vector2.one;
        _timer.rectTransform.offsetMin = new Vector2(8f, 4f);
        _timer.rectTransform.offsetMax = new Vector2(-8f, -4f);
        _timer.horizontalOverflow = HorizontalWrapMode.Overflow;

        const float slot = 76f;
        const float gap = 8f;
        _slotZapato = Slot.Crear(barra.transform, "Zapato", "UI/zapato", "Zapato", -24f - 3f * (slot + gap), slot);
        _slotZoquete = Slot.Crear(barra.transform, "Zoquete", "UI/zoquete", "Media", -24f - 2f * (slot + gap), slot);
        _slotCasco = Slot.Crear(barra.transform, "Casco", "UI/casco", "Casco", -24f - 1f * (slot + gap), slot);
        _slotTubo = Slot.Crear(barra.transform, "Tubo", "UI/tubo", "Tubo", -24f, slot);

        _panelMensaje = UiFactory.AddPanelFixed(canvas, "MensajePanel", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(1100f, 96f), new Color(0.04f, 0.05f, 0.04f, 0.92f));
        _mensaje = UiFactory.AddText(_panelMensaje.transform, "Mensaje", "", 34, TextAnchor.MiddleCenter, Color.white, true, FontStyle.Bold);
        UiFactory.Stretch(_mensaje.rectTransform);
        _mensaje.rectTransform.offsetMin = new Vector2(24f, 8f);
        _mensaje.rectTransform.offsetMax = new Vector2(-24f, -8f);
        _panelMensaje.gameObject.SetActive(false);

        _veloFinal = UiFactory.AddImage(canvas, "VeloFinal", Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.42f));
        _veloFinal.enabled = false;

        _imgGanaste = BannerEsquina(canvas, "Ganaste", "UI/ganaste");
        _imgPerdiste = BannerEsquina(canvas, "Perdiste", "UI/perdiste");
    }

    static Image BannerEsquina(Transform parent, string name, string resource)
    {
        var img = UiFactory.AddImageFixed(
            parent,
            name,
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-24f, 24f),
            new Vector2(300f, 340f),
            Color.white);
        img.preserveAspect = true;
        img.sprite = UiFactory.LoadSprite(resource);
        img.enabled = false;
        return img;
    }

    class Slot
    {
        Image _marco;
        Image _icono;

        public static Slot Crear(Transform parent, string name, string resource, string etiqueta, float x, float size)
        {
            var slot = new Slot();
            slot._marco = UiFactory.AddPanelFixed(parent, "Slot" + name, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(x, 0f), new Vector2(size, size + 8f), SlotVacio);

            slot._icono = UiFactory.AddImageFixed(slot._marco.transform, "Icono", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(size - 22f, size - 28f), IconoApagado);
            slot._icono.preserveAspect = true;
            var sprite = UiFactory.LoadSprite(resource);
            if (sprite != null)
                slot._icono.sprite = sprite;

            var label = UiFactory.AddText(slot._marco.transform, "Label", etiqueta, 14, TextAnchor.LowerCenter, new Color(1f, 1f, 1f, 0.9f), true, FontStyle.Bold);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(1f, 0f);
            label.rectTransform.pivot = new Vector2(0.5f, 0f);
            label.rectTransform.anchoredPosition = new Vector2(0f, 4f);
            label.rectTransform.sizeDelta = new Vector2(0f, 20f);
            slot.SetHeld(false, false);
            return slot;
        }

        public void SetHeld(bool held, bool alerta)
        {
            if (_marco == null || _icono == null)
                return;
            _icono.color = held ? Color.white : IconoApagado;
            if (held && alerta)
                _marco.color = SlotAlerta;
            else if (held)
                _marco.color = SlotActivo;
            else
                _marco.color = SlotVacio;
        }
    }
}
