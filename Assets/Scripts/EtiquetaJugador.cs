using UnityEngine;
using System.Collections;

public class EtiquetaJugador : MonoBehaviour {

	public string nombreJugador;

	private int labelTop = 18;
	private int labelWidth = 80;
	private int labelHeight = 15;

	private float AlturaEtiqueta = 1.5f;

	//referencias
	private Camera miCamara;
	private Transform miTransform;

	//usado para determinar donde debe mostrarse 
	private Vector3 posicionWorld = new Vector3();
	private Vector3 posicionPantalla = new Vector3();
	private Vector3 posicionRelativaCamara = new Vector3();

	GUIStyle estilo;

	// Use this for initialization
	void Start () 
	{
		miTransform = transform;
		miCamara = Camera.main;

		// Estilo de la fuente
		estilo = new GUIStyle();
		estilo.fontSize = 15;
		estilo.fontStyle = FontStyle.Bold;
		estilo.normal.textColor = Color.yellow;
		estilo.clipping = TextClipping.Overflow;
	}
	
	// Update is called once per frame
	void Update () {
		//captura cuando el jugador esta en frente o detras de la camara
		//posicion relativa del jugador con respecto a la camara
		posicionRelativaCamara = miCamara.transform.InverseTransformPoint (miTransform.position);
	}

	void OnGUI()
	{
		if(posicionRelativaCamara.z > 0)
		{
			//seteo la world position un poco mas arriba del jugador
			posicionWorld = new Vector3(miTransform.position.x, miTransform.position.y + AlturaEtiqueta, miTransform.position.z);

			//convierto la posicion global a un punto en la pantalla
			posicionPantalla = miCamara.WorldToScreenPoint(posicionWorld);

			GUI.Label(new Rect(posicionPantalla.x - labelWidth /2,
			                   Screen.height - posicionPantalla.y - labelTop,
			                   labelWidth, labelHeight), nombreJugador, estilo);
		}
	}
}
