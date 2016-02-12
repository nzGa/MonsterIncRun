//Traduccion a C# del script JS que genera unity al agregar el componente Camera Control -> Mouse Orbit
//Bajado de internet y modificado para adaparlo a lo que necesito. Se adjunta al jugador.
//Controla el movimiento de la camara: gira alrededor del jugador si esta quieto, o sigue al puntero del mouse.


using UnityEngine;
using System.Collections;

public class MouseOrbit : MonoBehaviour {
	public float alturaOjos = 0.6f;
	public float cameraIniDistance = 2;
	
	public float xSpeed = 250.0f;
	public float ySpeed = 120.0f;
	
	//valores maximo y minimo que puede rotar la camara en vertical
	public float yMaxLimit = 50;
	public float yMinLimit = -20;

	//posicion, rotacion y escala de la main camera
	private Transform _mainCamera;
	
	private float _currentCameraMaxDistance;

	//guardan los movimientos horizontales y verticales del mouse en cada frame
	private float _mouseX = 0.0f;
	private float _mouseY = 0.0f;
	
	private float _relativeDistance;
	

	void Start () 
	{
		//si el network view no esta controlado por este objeto desactivo el script
		if (networkView.isMine) {
			//obtengo la posicion, rotacion, y escala de la camara con tag "MainCamera"
			_mainCamera = Camera.main.transform;
			
			//guardo la rotación de la camara como ángulos de Euler en grados
			Vector3 angles = _mainCamera.eulerAngles;
			
			_mouseX = angles.y;//rotacion sobre el eje global Y
			_mouseY = angles.x;//rotacion sobre el eje global X
			
			_currentCameraMaxDistance = cameraIniDistance;
			_relativeDistance = _currentCameraMaxDistance;		
		} 
		else {
			enabled = false;
		}

	}


	//LateUpdate es llamada una vez por frame luego de que Update y todos sus calculos hayan terminado. 
	//Los movimientos de Mike se estan hacen en Update por lo que es buena idea que los movimientos y rotaciones de la camara se hagan en LateUpdate. 
	//Esto asegura que el jugador se ha movido por completo antes de que la camara trackee su posicion.
	void LateUpdate()
	{
		//solo si el cursor esta lockeado puedo girar la camara
		if(Screen.lockCursor == true)
		{
			//Mouse X captura el movimiento horizontal del mouse en ese frame: a la izquierda valor negativo, a la derecha valor positivo
			_mouseX += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
			
			//niego el valor de retorno porque esta al reves de lo que necesito
			_mouseY -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			
			//restringe el valor del angulo en vertical entre un minimo y un maximo dados
			_mouseY = ClampAngle(_mouseY, yMinLimit, yMaxLimit);
			
			//Quaternion.Euler devuelve una rotacion de 0 grados sobre el eje Z, _mouseY grados sobre el eje X, _mouseX grados sobre el eje Y (en ese orden)
			Quaternion rotation = Quaternion.Euler(_mouseY, _mouseX, 0);
			
			//la variable transform (con t minuscula) es el transform del game object que tiene este script: el jugador
			Vector3 position = rotation * new Vector3(0.0f, alturaOjos, -_relativeDistance) + transform.position;
			
			_mainCamera.rotation = rotation;
			_mainCamera.position = position;
		}

	}
	


	float ClampAngle(float angle, float min, float max)
	{
		//no tiene sentido tener valores mas alla de un giro positivo o negativo
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;

		return Mathf.Clamp (angle, min, max);
	}
}