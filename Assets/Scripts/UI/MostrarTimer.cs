using UnityEngine;
using System;

public class MostrarTimer : MonoBehaviour
{
    static bool _timer;
    public static bool TimerActivado { get { return _timer; } set { _timer = value; } }
    public static float segundosFaltantes = -1f;

    UnityEngine.UI.Text _label;

    void Start()
    {
        var canvas = UiFactory.CreateCanvas("TimerCanvas", 11).transform;
        _label = UiFactory.AddText(canvas, "Timer", "", 40, TextAnchor.UpperCenter, Color.white);
        _label.rectTransform.anchorMin = new Vector2(0.4f, 0.9f);
        _label.rectTransform.anchorMax = new Vector2(0.6f, 0.98f);
        UiFactory.Stretch(_label.rectTransform);
    }

    void Update()
    {
        if (_label == null || segundosFaltantes < 0)
            return;

        int rounded = Mathf.CeilToInt(segundosFaltantes);
        var span = TimeSpan.FromSeconds(Mathf.Max(0, rounded));
        _label.text = string.Format("{0:D2}:{1:D2}:{2:D2}", span.Hours, span.Minutes, span.Seconds);
    }
}
