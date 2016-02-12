//script attachado al jugador. Controla cuando el cursor debe estar activado o desactivado
//Accede al script GestionaMultiJugador

using UnityEngine;
using System.Collections;

public class ControlCursor : MonoBehaviour {
	//game object que tiene el script GestionaMultiJugador
	private GameObject GameManager;

	//script GestionaMultiJugador para detectar cuando desactivar el puntero del mouse
	private GestionaMultiJugador scriptMultiJugador;



	void Start () {
		if (networkView.isMine) {
			//seteo la referencia al game object que contiene el script GestionaMultiJugador
			GameManager = GameObject.Find ("GameManager");

			//accedo al script del game object
			scriptMultiJugador = GameManager.GetComponent<GestionaMultiJugador> ();
		} 
		else 
		{
			enabled = false;
		}
			
	}
	

	void Update () {
		//si estoy en el juego debo ocultar el cursor
		if(scriptMultiJugador.cursorBloqueado == true)
		{
			Screen.lockCursor = true;
		}

		//si se esta mostrando el menu de estado del cliente (presionando ESC) entonces debo hacer visible el cursor
		if(scriptMultiJugador.cursorBloqueado == false)
		{
			Screen.lockCursor = false;
		}
	}
}
