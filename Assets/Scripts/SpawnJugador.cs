//hago spawn del jugador en la escena en los puntos disponibles
using UnityEngine;
using System.Collections;

public class SpawnJugador : MonoBehaviour {
	//usada para determinar si es necesario que el jugador haga un spawn en el juego
	private bool recienConectadoAlServidor = false; 
	
	//prefabs del jugador que sera instanciado
	public GameObject mikePrefab;
	
	
	//arrays que guardan los posibles puntos de inicio para los objetos a ser instanciados
	private GameObject[] puntosInicioMike;
	
	
	private int _mikeGroup = 0;//permite controlar el filtrado de mensajes. Seteado a cero porque no es necesario
	
	
	//funcion ejecutada en el cliente cuando se conecta al server
	void OnConnectedToServer ()
	{
		//seteo el flag que genera el spawn
		recienConectadoAlServidor = true;	
	}
	
	void OnGUI()
	{	
		//Debug.Log ("recienConectadoAlServidor = " + recienConectadoAlServidor);

		if(recienConectadoAlServidor && (Network.peerType == NetworkPeerType.Client))
		{
			recienConectadoAlServidor = false;	
			SpawnPlayer();
		}
	}
	
	
	void SpawnPlayer()
	{
		//busco los puntos de inicio posibles y obtengo una referencia en el array
		puntosInicioMike = GameObject.FindGameObjectsWithTag("SpawnMike");
		
		//elijo aleatoriamente uno de los puntos de inicio
		GameObject randomSpawnPoint = puntosInicioMike[Random.Range(0, puntosInicioMike.Length)];
		
		//instancio un prefab en el punto de inicio seleccionado
		//el cliente que hace la instancia es el que controla el objeto pero los cambios se ven en toda la red
		//tecnicamente funciona como un RPC que es buffereada y de esta manera los objetos instanciados apareceran en los nuevos clientes que se conecten
		//el ultimo parametro sirve para seleccionar los clientes que recibiran mensajes particulares. Si dos clientes no necesitan comunicarse se pondran en grupos separados
		
		GameObject miPlayer = (GameObject) Network.Instantiate(mikePrefab, randomSpawnPoint.transform.position,randomSpawnPoint.transform.rotation, _mikeGroup);

		//seteo el nombre del jugador en el prefab ya instanciado
		miPlayer.GetComponent<NombreJugador>().nombreJugador = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GestionaMultiJugador>().nombreJugador;
		miPlayer.GetComponent<NombreJugador>().fNombreSeteado = true;
	}
	
	
	
	
}
