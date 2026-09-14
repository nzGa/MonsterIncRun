using UnityEngine;

public class MostrarTimer : MonoBehaviour
{
    static bool _timer;
    public static bool TimerActivado { get { return _timer; } set { _timer = value; } }
    public static float segundosFaltantes = -1f;

    void Awake()
    {
        DestroyLegacyTimer();
    }

    void Update()
    {
        DestroyLegacyTimer();
    }

    void OnGUI()
    {
    }

    static void DestroyLegacyTimer()
    {
        var go = GameObject.Find("TimerCanvas");
        if (go != null)
            Destroy(go);
    }
}
