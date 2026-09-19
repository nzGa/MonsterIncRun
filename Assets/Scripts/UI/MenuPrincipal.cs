using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    public string escenaJuego = "Mike_Juego";
    const string NombreJugadorPrefsKey = "NombreJugador";

    InputField _nombreInput;

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

        var panel = UiFactory.AddPanel(root, "Panel", new Vector2(0.05f, 0.26f), new Vector2(0.40f, 0.70f), new Color(0.04f, 0.06f, 0.05f, 0.78f));
        panel.raycastTarget = false;

        var nombreLabel = UiFactory.AddText(panel.transform, "NombreLabel", "Nombre del jugador", 22, TextAnchor.MiddleLeft, Color.white, true, FontStyle.Bold);
        nombreLabel.rectTransform.anchorMin = new Vector2(0.12f, 0.76f);
        nombreLabel.rectTransform.anchorMax = new Vector2(0.88f, 0.88f);
        nombreLabel.rectTransform.offsetMin = Vector2.zero;
        nombreLabel.rectTransform.offsetMax = Vector2.zero;

        var inputGo = new GameObject("NombreInput", typeof(RectTransform), typeof(Image), typeof(InputField));
        inputGo.transform.SetParent(panel.transform, false);
        var inputRt = inputGo.GetComponent<RectTransform>();
        inputRt.anchorMin = new Vector2(0.12f, 0.58f);
        inputRt.anchorMax = new Vector2(0.88f, 0.72f);
        inputRt.offsetMin = Vector2.zero;
        inputRt.offsetMax = Vector2.zero;

        var inputImage = inputGo.GetComponent<Image>();
        inputImage.color = new Color(0.12f, 0.18f, 0.15f, 1f);
        inputImage.raycastTarget = true;

        _nombreInput = inputGo.GetComponent<InputField>();
        var textoInput = CrearTextoInput(inputGo.transform, "Text", 26, Color.white, TextAnchor.MiddleLeft);
        var placeholderText = CrearTextoInput(inputGo.transform, "Placeholder", 26, new Color(1f, 1f, 1f, 0.5f), TextAnchor.MiddleLeft);

        _nombreInput.textComponent = textoInput;
        _nombreInput.placeholder = placeholderText;
        _nombreInput.text = PlayerPrefs.GetString(NombreJugadorPrefsKey, "Mike");
        _nombreInput.characterLimit = 20;
        _nombreInput.inputType = InputField.InputType.Standard;
        _nombreInput.lineType = InputField.LineType.SingleLine;
        _nombreInput.selectionColor = new Color(0.3f, 0.7f, 0.45f, 0.4f);
        placeholderText.text = "Mike";

        var iniciar = UiFactory.AddButton(panel.transform, "Iniciar", "Iniciar", new Vector2(0f, -35f), new Vector2(360f, 70f));
        iniciar.onClick.AddListener(() =>
        {
            var nombre = (_nombreInput != null ? _nombreInput.text : "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                nombre = "Mike";
            PlayerPrefs.SetString(NombreJugadorPrefsKey, nombre);
            PlayerPrefs.Save();
            SceneManager.LoadScene(escenaJuego);
        });

        var salir = UiFactory.AddButton(panel.transform, "Salir", "Salir", new Vector2(0f, -125f), new Vector2(360f, 70f));
        salir.onClick.AddListener(UiFactory.Salir);
    }

    Text CrearTextoInput(Transform parent, string name, int fontSize, Color color, TextAnchor alignment)
    {
        var textGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textGo.transform.SetParent(parent, false);
        var text = textGo.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        var rt = textGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(10f, 4f);
        rt.offsetMax = new Vector2(-10f, -4f);

        return text;
    }
}
