using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class UiFactory
{
    public static readonly Color Verde = new Color(0.14f, 0.58f, 0.30f, 0.96f);
    public static readonly Color VerdeHover = new Color(0.20f, 0.74f, 0.38f, 1f);
    public static readonly Color VerdePressed = new Color(0.08f, 0.40f, 0.20f, 1f);
    public static readonly Color PanelOscuro = new Color(0.05f, 0.07f, 0.06f, 0.88f);
    public static readonly Color Oro = new Color(0.95f, 0.82f, 0.18f, 1f);
    public static readonly Color Texto = Color.white;

    static Sprite _rounded;

    public static void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public static Canvas CreateCanvas(string name, int sortOrder)
    {
        var root = new GameObject(name);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();
        return canvas;
    }

    public static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null)
            return;

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    public static Image AddImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static Image AddPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var image = AddImage(parent, name, anchorMin, anchorMax, color);
        AplicarPanel(image);
        return image;
    }

    public static Image AddPanelFixed(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        AplicarPanel(image);
        return image;
    }

    public static Image AddImageFixed(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    static void AplicarPanel(Image image)
    {
        try
        {
            var sprite = RoundedSprite();
            if (sprite == null)
                return;
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
            image.pixelsPerUnitMultiplier = 1.15f;
        }
        catch (Exception)
        {
            image.sprite = null;
            image.type = Image.Type.Simple;
        }
    }

    public static Image AddTopBar(Transform parent, string name, float height, Color color)
    {
        var image = AddImage(parent, name, new Vector2(0f, 1f), new Vector2(1f, 1f), color);
        var rt = image.rectTransform;
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(0f, -height);
        rt.offsetMax = Vector2.zero;
        return image;
    }

    public static Text AddText(Transform parent, string name, string content, int fontSize, TextAnchor anchor, Color color, bool outline = false, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (text.font == null)
            text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = anchor;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        if (outline)
            AddOutline(text, new Color(0f, 0f, 0f, 0.85f), new Vector2(1.6f, -1.6f));
        return text;
    }

    public static Outline AddOutline(Graphic graphic, Color color, Vector2 distance)
    {
        var outline = graphic.gameObject.GetComponent<Outline>();
        if (outline == null)
            outline = graphic.gameObject.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
        return outline;
    }

    public static Button AddButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;

        var image = go.GetComponent<Image>();
        AplicarPanel(image);
        image.color = Verde;
        image.raycastTarget = true;

        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = Color.white;
        button.colors = colors;
        button.targetGraphic = image;

        var text = AddText(go.transform, "Label", label, 30, TextAnchor.MiddleCenter, Color.white, true, FontStyle.Bold);
        Stretch(text.rectTransform);
        text.raycastTarget = false;
        return button;
    }

    public static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public static Sprite LoadSprite(string resourcePath)
    {
        var sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite != null)
            return sprite;

        var sprites = Resources.LoadAll<Sprite>(resourcePath);
        if (sprites != null)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null)
                    return sprites[i];
            }
        }

        return SpriteFromTextureSafe(Resources.Load<Texture2D>(resourcePath));
    }

    public static Sprite SpriteFromTexture(Texture2D texture)
    {
        return SpriteFromTextureSafe(texture);
    }

    public static Sprite SpriteFromTextureSafe(Texture2D texture)
    {
        if (texture == null)
            return null;

        try
        {
            if (texture.isReadable)
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);

            var rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(texture, rt);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var copy = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
            copy.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            copy.Apply(false, false);
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            copy.wrapMode = TextureWrapMode.Clamp;
            copy.filterMode = FilterMode.Bilinear;
            copy.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(copy, new Rect(0, 0, copy.width, copy.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static Sprite RoundedSprite()
    {
        if (_rounded != null)
            return _rounded;

        const int size = 64;
        const int radius = 16;
        var tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;
                float dx = 0f;
                float dy = 0f;
                if (px < radius)
                    dx = radius - px;
                else if (px > size - radius)
                    dx = px - (size - radius);
                if (py < radius)
                    dy = radius - py;
                else if (py > size - radius)
                    dy = py - (size - radius);

                float alpha = 1f;
                if (dx > 0f && dy > 0f)
                {
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    alpha = Mathf.Clamp01(radius - dist + 0.5f);
                }

                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply(false, false);
        tex.hideFlags = HideFlags.HideAndDontSave;
        _rounded = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        _rounded.hideFlags = HideFlags.HideAndDontSave;
        return _rounded;
    }
}
