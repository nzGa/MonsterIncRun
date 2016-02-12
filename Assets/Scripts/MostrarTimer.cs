using UnityEngine;
using System.Collections;
using System;//TimeSpan

public class MostrarTimer : MonoBehaviour {

	static bool _timer = false;
	public static bool TimerActivado { get{ return _timer; } set{ _timer = value; }}
	
	public static float segundosFaltantes = -1.0f; //lo hago negativo a proposito
	private int roundedRestSeconds; 
	
	GUIStyle estilo = new GUIStyle();


	
	void OnGUI()
	{		
		estilo.fontSize = 30;
		estilo.normal.textColor = Color.white;

		//cuando el timer inicial es -1 no lo muestro
		//cuando llegue a cero va a dejar de decrementarse pero va a seguir siendo visible
		if(segundosFaltantes >= 0)
		{
			roundedRestSeconds = Mathf.CeilToInt(segundosFaltantes);
			
			TimeSpan timeSpan = TimeSpan.FromSeconds(roundedRestSeconds);
			string timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
			GUI.Label (new Rect (400, 25, 100, 30), timeText, estilo);
		}
	
	}



}
