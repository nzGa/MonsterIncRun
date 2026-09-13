using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    bool mje_falta_tubo;
    Text _mensaje;
    Image _imgTubo;
    Image _imgCasco;
    Image _imgZapato;
    Image _imgZoquete;
    Image _imgGanaste;
    Image _imgPerdiste;
    bool _ocultando;

    void Start()
    {
        ConstruirHud();
    }

    void Update()
    {
        if (ObjetosPorJugador.TocandoPuerta)
        {
            ObjetosPorJugador.TocandoPuerta = false;
            mje_falta_tubo = true;
            PedirOcultar();
        }

        if (MensajeZapato || MjeTieneTubo || MjeTieneZoquete || MjeDescontaminado || MjeContaminado ||
            MjeTieneCasco || MjeContaminaste || MjeCascoUsado || MjeYaTenesTubo || MjeYaTenesItem)
            PedirOcultar();

        if (_imgGanaste != null)
            _imgGanaste.enabled = ObjetosPorJugador.JugadorHaGanado;
        if (_imgPerdiste != null)
            _imgPerdiste.enabled = ObjetosPorJugador.JugadorHaPerdido && !ObjetosPorJugador.JugadorHaGanado;

        if (_imgTubo != null) _imgTubo.enabled = ObjetosPorJugador.TieneTubo;
        if (_imgCasco != null) _imgCasco.enabled = ObjetosPorJugador.TieneCasco;
        if (_imgZapato != null) _imgZapato.enabled = ObjetosPorJugador.TieneZapato;
        if (_imgZoquete != null) _imgZoquete.enabled = ObjetosPorJugador.TieneZoquete;

        if (_mensaje == null)
            return;

        if (ObjetosPorJugador.JugadorHaGanado)
            _mensaje.text = "GANASTE !!!";
        else if (ObjetosPorJugador.JugadorHaPerdido)
            _mensaje.text = "PERDISTE :(";
        else if (mje_falta_tubo)
            _mensaje.text = "Necesitas un tubo para poder pasar";
        else if (MensajeZapato)
            _mensaje.text = "Ahora corres mas rapido!";
        else if (MjeTieneTubo)
            _mensaje.text = "Busca la puerta y gana!";
        else if (MjeTieneZoquete)
            _mensaje.text = "Te han contaminado! Busca la ducha!";
        else if (MjeTieneCasco)
            _mensaje.text = "Sos inmune a las contaminaciones!";
        else if (MjeDescontaminado)
            _mensaje.text = "Estas descontaminado!";
        else if (MjeContaminado)
            _mensaje.text = "Debes descontaminarte en la ducha!";
        else if (MjeContaminaste)
            _mensaje.text = "Contaminaste a todos tus oponentes";
        else if (MjeCascoUsado)
            _mensaje.text = "No han podido contaminarte";
        else if (MjeYaTenesTubo)
            _mensaje.text = "Ya tenes un tubo";
        else if (MjeYaTenesItem)
            _mensaje.text = "Ya tenes este objeto";
        else
            _mensaje.text = "";
    }

    void PedirOcultar()
    {
        if (!_ocultando)
            StartCoroutine(Esperar_OcultarMje());
    }

    IEnumerator Esperar_OcultarMje()
    {
        _ocultando = true;
        yield return new WaitForSeconds(3f);
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
        _ocultando = false;
    }

    void ConstruirHud()
    {
        var canvas = UiFactory.CreateCanvas("HudCanvas", 10).transform;

        _imgTubo = Icono(canvas, "IconoTubo", "UI/tubo", new Vector2(24, -24));
        _imgCasco = Icono(canvas, "IconoCasco", "UI/casco", new Vector2(104, -24));
        _imgZapato = Icono(canvas, "IconoZapato", "UI/zapato", new Vector2(184, -24));
        _imgZoquete = Icono(canvas, "IconoZoquete", "UI/zoquete", new Vector2(264, -24));

        _imgGanaste = Banner(canvas, "Ganaste", "UI/ganaste");
        _imgPerdiste = Banner(canvas, "Perdiste", "UI/perdiste");

        _mensaje = UiFactory.AddText(canvas, "Mensaje", "", 36, TextAnchor.LowerCenter, Color.white);
        _mensaje.rectTransform.anchorMin = new Vector2(0.15f, 0.08f);
        _mensaje.rectTransform.anchorMax = new Vector2(0.85f, 0.22f);
        UiFactory.Stretch(_mensaje.rectTransform);
    }

    static Image Icono(Transform parent, string name, string resource, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(64, 64);
        var img = go.GetComponent<Image>();
        var tex = Resources.Load<Texture2D>(resource);
        if (tex != null)
            img.sprite = UiFactory.SpriteFromTexture(tex);
        img.enabled = false;
        img.raycastTarget = false;
        return img;
    }

    static Image Banner(Transform parent, string name, string resource)
    {
        var img = UiFactory.AddImage(parent, name, new Vector2(0.02f, 0.35f), new Vector2(0.32f, 0.75f), Color.white);
        img.preserveAspect = true;
        var tex = Resources.Load<Texture2D>(resource);
        if (tex != null)
            img.sprite = UiFactory.SpriteFromTexture(tex);
        img.enabled = false;
        return img;
    }
}
