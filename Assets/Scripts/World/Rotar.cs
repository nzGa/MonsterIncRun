using UnityEngine;
using System.Collections;

public class Rotar : MonoBehaviour {
	
	public float _rotateSpeed = 20;
	

	// Update is called once per frame
	void Update () {
		//roto los grados indicados por segundo, no por frame para evitar que rote mas lento en maquinas mas lentas
		//deltaTime es un float que contiene los segundos que se tardaron en renderizar el ultimo frame
		transform.Rotate(0,0,_rotateSpeed * Time.deltaTime);
	}
}
