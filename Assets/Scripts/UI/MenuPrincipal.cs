using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    public string escenaJuego = "Mike_Juego";

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ConstruirMenu();
    }

    void ConstruirMenu()
    {
        var canvas = UiFactory.CreateCanvas("MenuCanvas", 0);
        var root = canvas.transform;

        var fondo = UiFactory.AddImage(root, "Fondo", Vector2.zero, Vector2.one, Color.white);
        var texFondo = Resources.Load<Texture2D>("UI/mike_inicio");
        if (texFondo != null)
        {
            fondo.sprite = UiFactory.SpriteFromTexture(texFondo);
            fondo.preserveAspect = false;
        }
        else
        {
            fondo.color = new Color(0.05f, 0.15f, 0.08f, 1f);
        }

        var titulo = UiFactory.AddImage(root, "Titulo", new Vector2(0.03f, 0.72f), new Vector2(0.55f, 0.97f), Color.white);
        var texTitulo = Resources.Load<Texture2D>("UI/run_nombre_juego");
        if (texTitulo != null)
        {
            titulo.sprite = UiFactory.SpriteFromTexture(texTitulo);
            titulo.preserveAspect = true;
            titulo.color = Color.white;
        }
        else
        {
            titulo.color = Color.clear;
            var fallback = UiFactory.AddText(root, "TituloTexto", "Run Mike Run", 64, TextAnchor.MiddleLeft, Color.yellow);
            fallback.rectTransform.anchorMin = new Vector2(0.05f, 0.8f);
            fallback.rectTransform.anchorMax = new Vector2(0.6f, 0.95f);
            UiFactory.Stretch(fallback.rectTransform);
        }

        var panel = UiFactory.AddImage(root, "Panel", new Vector2(0.06f, 0.28f), new Vector2(0.38f, 0.68f), new Color(0f, 0f, 0f, 0.55f));
        panel.raycastTarget = false;

        var tituloPanel = UiFactory.AddText(panel.transform, "PanelTitulo", "Run Mike Run", 28, TextAnchor.UpperCenter, Color.white);
        tituloPanel.rectTransform.anchorMin = new Vector2(0.05f, 0.78f);
        tituloPanel.rectTransform.anchorMax = new Vector2(0.95f, 0.95f);
        UiFactory.Stretch(tituloPanel.rectTransform);

        var iniciar = UiFactory.AddButton(panel.transform, "Iniciar", "Iniciar", new Vector2(0, 20), new Vector2(360, 70));
        iniciar.onClick.AddListener(() => SceneManager.LoadScene(escenaJuego));

        var salir = UiFactory.AddButton(panel.transform, "Salir", "Salir", new Vector2(0, -70), new Vector2(360, 70));
        salir.onClick.AddListener(Application.Quit);

        var reglas = UiFactory.AddText(root, "Reglas",
            "Consegui un tubo de gritos y llega a la puerta.\nZapatos: mas velocidad. Medias: contaminan. Casco: inmunidad. Ducha: descontamina.",
            22, TextAnchor.LowerLeft, Color.white);
        reglas.rectTransform.anchorMin = new Vector2(0.04f, 0.04f);
        reglas.rectTransform.anchorMax = new Vector2(0.7f, 0.22f);
        UiFactory.Stretch(reglas.rectTransform);
    }
}
