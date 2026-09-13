//este script  va attachado al objeto ObjetoCaja
//mantiene los valores que determinan los elementos que puede tener una caja al inicializarse

using UnityEngine;
using System.Collections;

public class ObjetoCaja : MonoBehaviour {

	public int Item;

	//antes de instanciar ningun objeto ya estoy seteando este valor
	void Awake () {
		//0: zoquete
		//1: casco
		//2: zapato
		Item = Random.Range (0, 3);
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
