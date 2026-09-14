using UnityEngine;
using UnityEngine.SceneManagement;

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
        var spriteFondo = UiFactory.LoadSprite("UI/mike_inicio");
        if (spriteFondo != null)
        {
            fondo.sprite = spriteFondo;
            fondo.preserveAspect = false;
        }
        else
        {
            fondo.color = new Color(0.05f, 0.15f, 0.08f, 1f);
        }

        var titulo = UiFactory.AddImage(root, "Titulo", new Vector2(0.03f, 0.72f), new Vector2(0.55f, 0.97f), Color.white);
        var spriteTitulo = UiFactory.LoadSprite("UI/run_nombre_juego");
        if (spriteTitulo != null)
        {
            titulo.sprite = spriteTitulo;
            titulo.preserveAspect = true;
            titulo.color = Color.white;
        }
        else
        {
            titulo.color = Color.clear;
            var fallback = UiFactory.AddText(root, "TituloTexto", "Run Mike Run", 64, TextAnchor.MiddleLeft, UiFactory.Oro, true, FontStyle.Bold);
            fallback.rectTransform.anchorMin = new Vector2(0.05f, 0.8f);
            fallback.rectTransform.anchorMax = new Vector2(0.6f, 0.95f);
            UiFactory.Stretch(fallback.rectTransform);
        }

        var panel = UiFactory.AddPanel(root, "Panel", new Vector2(0.05f, 0.26f), new Vector2(0.40f, 0.70f), new Color(0.04f, 0.06f, 0.05f, 0.78f));
        panel.raycastTarget = false;

        var tituloPanel = UiFactory.AddText(panel.transform, "PanelTitulo", "Run Mike Run", 30, TextAnchor.UpperCenter, Color.white, true, FontStyle.Bold);
        tituloPanel.rectTransform.anchorMin = new Vector2(0.06f, 0.78f);
        tituloPanel.rectTransform.anchorMax = new Vector2(0.94f, 0.96f);
        UiFactory.Stretch(tituloPanel.rectTransform);

        var iniciar = UiFactory.AddButton(panel.transform, "Iniciar", "Iniciar", new Vector2(0f, 18f), new Vector2(360f, 70f));
        iniciar.onClick.AddListener(() => SceneManager.LoadScene(escenaJuego));

        var salir = UiFactory.AddButton(panel.transform, "Salir", "Salir", new Vector2(0f, -72f), new Vector2(360f, 70f));
        salir.onClick.AddListener(UiFactory.Salir);
    }
}
