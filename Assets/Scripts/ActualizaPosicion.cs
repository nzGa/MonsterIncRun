//este script se adjunta al jugador y asegura que la posicion, rotacion, escala de cada jugador son mantenidos actualizados en toda la red

using UnityEngine;
using System.Collections;


public class ActualizaPosicion : MonoBehaviour {

	// Variable que va guardando la posición con cada frame
	private Vector3 ultimaPosicion;
	// Variable que va guardando la rotación con cada frame
	private Quaternion ultimaRotacion;
	// Variable de tipo Transform que informará la posición y rotación actual
	private Transform miTransform;
	

	void Start () 
	{
		//si el network view esta controlado por este objeto (y no el resto de los jugadores)
		if(networkView.isMine)
		{
			miTransform = transform;
			ultimaPosicion = miTransform.position;
			ultimaRotacion = miTransform.rotation;
			// Informa a los otros jugadores mi posicion al momento de iniciar el juego

			//asegura que todos vean al jugador en la posicion correcta en ese momento
			//se envia a todos excepto a este jugador
			//se bufferea para que los viejos buffers se muestren a nuevos jugadores
			//los nuevos jugadores veran ls ultimas posiciones de todos
			networkView.RPC("InformarMiPosicion", RPCMode.OthersBuffered,miTransform.position, miTransform.rotation);
		}
		else
		{
			enabled = false;	
		}
	}

	void Update () 
	{
		// Si me muevo, informo por RPC mi nueva posicion para que se actualice en la vista
		// de los otros jugadores
		// Si el personaje se movio en linea recta...

		//si el jugador se movio entonces se invoca el RPC a los jugadores para informar a los jugadores la ultima posicion y giro
		if(Vector3.Distance(miTransform.position, ultimaPosicion) >= 0.1)
		{
			// Captura la posición del jugador y la guarda en ultimaPosicion para
			// que la proxima vez que entre al IF, sea ésta la ultima posicion conocida
			// y evalue si el jugador se movio o no
			ultimaPosicion = miTransform.position;
			networkView.RPC("InformarMiPosicion", RPCMode.OthersBuffered,
			                miTransform.position, miTransform.rotation);
		}

		// Si el personaje giro en el mismo lugar...
		if(Quaternion.Angle(miTransform.rotation, ultimaRotacion) >= 1)
		{
			// Captura la rotacion del jugador y la guarda en ultimaRotacion para
			// que la proxima vez que entre al IF, sea ésta la ultima rotacion conocida
			// y evalue si el jugador giro sobre sí mismo o no
			ultimaRotacion = miTransform.rotation;
			networkView.RPC("InformarMiPosicion", RPCMode.OthersBuffered,
			                miTransform.position, miTransform.rotation);
		}
	}
	
	//cuando se llame a esta funcion, el gameobject que tiene attachado este script sera posicionado y girado a los valores indicados por parametro
	[RPC]
	void InformarMiPosicion (Vector3 newPosition, Quaternion newRotation)
	{
		transform.position = newPosition;
		transform.rotation = newRotation;
		
	}
}