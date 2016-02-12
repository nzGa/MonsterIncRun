using UnityEngine;
using System.Collections;

public class SpawnObjetos : MonoBehaviour {
	
	//prefabs de los objetos que seran instanciados
	public GameObject puerta;
	public GameObject ducha;
	public GameObject tubo;
	public GameObject caja;
	
	//arrays que guardan los posibles puntos de inicio para los objetos a ser instanciados
	private GameObject[] puntosInicioPuerta;
	private GameObject[] puntosInicioDucha;
	private GameObject[] puntosInicioTubos;
	private GameObject[] puntosInicioCajas;
	
	
	
	
	//al iniciarse el servidor se instanciaran los objetos en puntos aleatorios del terreno
	void OnServerInitialized() {
		SpawnPuerta ();
		SpawnDucha ();
		SpawnTubos ();
		SpawnCajas ();	
	}


	void SpawnDucha()
	{
		//inicializo el array buscando los posibles puntos por su tag y tengo una referencia a ellos	
		puntosInicioDucha = GameObject.FindGameObjectsWithTag("SpawnDucha");
		
		//elijo aleatoriamente un numero para usarlo como indice en el array de puntos posibles
		//Random.Range con enteros: el valor minimo es inclusivo y el maximo es exclusivo, por lo tanto llega a puntosInicioPuerta.Length-1, el indice correcto	
		GameObject inicioAleatorioDucha = puntosInicioDucha[Random.Range(0, puntosInicioDucha.Length)];
		
		//instancio el prefab en el punto de inicio seleccionado
		Network.Instantiate(ducha, inicioAleatorioDucha.transform.position, inicioAleatorioDucha.transform.rotation, 0);
	}


	void SpawnPuerta()
	{
		puntosInicioPuerta = GameObject.FindGameObjectsWithTag("SpawnPuerta");
		GameObject inicioAleatorioPuerta = puntosInicioPuerta[Random.Range(0, puntosInicioPuerta.Length)];
		Network.Instantiate(puerta, inicioAleatorioPuerta.transform.position, inicioAleatorioPuerta.transform.rotation, 0);
	}

	
	//instancio los tubos en la red
	void SpawnTubos()
	{
		//busco los puntos posibles para un tubo
		puntosInicioTubos = GameObject.FindGameObjectsWithTag ("SpawnTubo");
		
		foreach(GameObject spawnPoint in puntosInicioTubos)
		{
			Network.Instantiate (tubo, spawnPoint.transform.position, tubo.transform.rotation, 0);
		}
	}
	
	
	//instancio las cajas en toda la red
	void SpawnCajas()
	{
		puntosInicioCajas = GameObject.FindGameObjectsWithTag ("SpawnCaja");
		
		foreach(GameObject spawnPoint in puntosInicioCajas)
		{
			Network.Instantiate (caja, spawnPoint.transform.position, caja.transform.rotation,0);
		}
	}
}