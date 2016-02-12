//incorpora los nombres de los jugadores en el juego
//accede a EtiquetaJugador para proveerle el nombre del jugador

using UnityEngine;
using System.Collections;

public class NombreJugador : MonoBehaviour {

	// Tiene el Nombre del Jugador
	public string nombreJugador;

	// Controla cuando el nombreJugador fue actualizado en este script
	public bool fNombreSeteado = false;

	// Controla si el nombreJugador fue notificado ya a los otros clientes
	public bool fNombreNotificado = false;

	// Update is called once per frame
	void Update () 
	{
		// Solo la instancia de juego dueña del player notificara el nombreJugador en los demas clientes
		if(networkView.isMine && !fNombreNotificado && fNombreSeteado )
		{
			// Notifica el nombre a los demas clientes
			networkView.RPC("NotificarNombre", RPCMode.OthersBuffered, nombreJugador);

			// Actualiza el nombreJugador en el script que lo muestra
			GetComponent<EtiquetaJugador>().nombreJugador = nombreJugador;

			// Impide que se vuelva a notificar el nombre en cada frame
			fNombreNotificado = true;
		}
	}

	[RPC]
	void NotificarNombre(string nombre)
	{
		nombreJugador = nombre;

		// Actualiza el nombreJugador en el script que lo muestra
		GetComponent<EtiquetaJugador>().nombreJugador = nombre;
	}
}
