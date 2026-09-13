//script attachado al prefab Caja
//accede al script ObjetoCaja y obtiene el valor que determina el item que contiene la caja

using UnityEngine;
using System.Collections;


public class ContenidoCaja : MonoBehaviour {


	private int numeroItem= -1;
	public  int NumeroItem { get{ return numeroItem; } set{ numeroItem = value; }}

	void Start(){
		//0: zoquete
		//1: casco
		//2: zapato
		numeroItem = GameObject.FindGameObjectWithTag("ObjetoCaja").GetComponent<ObjetoCaja>().Item;

		//incremento el valor para la proxima caja, sin salirme del rango 0-2
		GameObject.FindGameObjectWithTag("ObjetoCaja").GetComponent<ObjetoCaja>().Item = (numeroItem+1) % 3;
	}
	
	void Update(){
	}
}
