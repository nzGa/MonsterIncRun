//GUI a nivel del juego en general
//adjuntado al jugador

using UnityEngine;
using System.Collections;


public class GameGUI : MonoBehaviour {
	//[SerializeField] es para hacer visible desde el Inspector sin que sea publica
	[SerializeField]
	Texture2D _ImagenGanador;
	
	[SerializeField]
	Texture2D _ImagenPerdedor;

	[SerializeField]
	Texture2D _ImagenTubo;

	[SerializeField]
	Texture2D _ImagenCasco;

	[SerializeField]
	Texture2D _ImagenZapato;

	[SerializeField]
	Texture2D _ImagenZoquete;

	//uso un skin para setear la fuente de los mensajes emitidos por este script
	[SerializeField]
	GUISkin estiloSkin;

	private Rect posicionTamanioMjes;
	private Rect posicionImagenTubo;
	private Rect posicionImagenCasco;
	private Rect posicionImagenZapato;
	private Rect posicionImagenZoquete;

	private bool mje_falta_tubo = false;

	static bool _mje_zapato = false;
	public static bool MensajeZapato { get{ return _mje_zapato; } set{ _mje_zapato = value; }}

	static bool _mje_tiene_tubo = false;
	public static bool MjeTieneTubo { get{ return _mje_tiene_tubo; } set{ _mje_tiene_tubo = value; }}

	static bool _mje_tiene_zoquete = false;
	public static bool MjeTieneZoquete { get{ return _mje_tiene_zoquete; } set{ _mje_tiene_zoquete = value; }}

	static bool _mje_tiene_casco = false;
	public static bool MjeTieneCasco { get{ return _mje_tiene_casco; } set{ _mje_tiene_casco = value; }}

	static bool _mje_descontaminado = false;
	public static bool MjeDescontaminado { get{ return _mje_descontaminado; } set{ _mje_descontaminado = value; }}

	//usado cuando tiene tubo pero esta contaminado
	static bool _mje_contaminado = false;
	public static bool MjeContaminado { get{ return _mje_contaminado; } set{ _mje_contaminado = value; }}


	//informa al jugador que por medio de un zoquete ha contaminado a sus oponentes
	static bool _mje_contaminaste = false;
	public static bool MjeContaminaste { get{ return _mje_contaminaste; } set{ _mje_contaminaste = value; }}

	//informa al jugador que han intentado contaminarlo pero no pudieron porque tiene el casco 
	static bool _mje_casco_usado = false;
	public static bool MjeCascoUsado { get{ return _mje_casco_usado; } set{ _mje_casco_usado = value; }}

	//informa al jugador que ya tiene un tubo y no puede obtener otro
	static bool _mje_ya_tenes_tubo = false;
	public static bool MjeYaTenesTubo { get{ return _mje_ya_tenes_tubo; } set{ _mje_ya_tenes_tubo = value; }}

	//informa al jugador que ya tiene el item que acaba de obtener
	static bool _mje_ya_tenes_item = false;
	public static bool MjeYaTenesItem { get{ return _mje_ya_tenes_item; } set{ _mje_ya_tenes_item = value; }}


	void Start () 
	{	
		//si el network view esta controlado por este objeto (y no el resto de los jugadores)
		if(networkView.isMine)
		{
			posicionTamanioMjes = new Rect((Screen.width/4), (Screen.height/4)*3 , 800, 60);
			
			posicionImagenTubo = new Rect(10, 10, _ImagenTubo.width, _ImagenTubo.height);
			posicionImagenCasco = new Rect(posicionImagenTubo.x + posicionImagenTubo.width + 10, 10, _ImagenCasco.width, _ImagenCasco.height);
			posicionImagenZapato = new Rect(posicionImagenCasco.x + posicionImagenCasco.width + 10, 10, _ImagenZapato.width, _ImagenZapato.height);
			posicionImagenZoquete = new Rect(posicionImagenZapato.x + posicionImagenZapato.width + 10, 10, _ImagenZoquete.width, _ImagenZoquete.height);	
		}
		else
		{
			enabled = false;	
		}

	}
	


	void OnGUI()
	{

		GUI.skin = estiloSkin;

		if (ObjetosPorJugador.JugadorHaGanado)
		{
			//float x = (Screen.width - _ImagenGanador.width) / 2;
			//float y = (Screen.height - _ImagenGanador.height) / 2;
			GUI.DrawTexture(new Rect(10.0f, 120.0f, _ImagenGanador.width * 0.6f, _ImagenGanador.height * 0.6f), _ImagenGanador);
			GUI.Label(posicionTamanioMjes, "GANASTE !!!");
		}
		if (ObjetosPorJugador.JugadorHaPerdido)
		{
			//float x = (Screen.width - _ImagenPerdedor.width) / 2;
			//float y = (Screen.height - _ImagenPerdedor.height) / 2;
			GUI.DrawTexture(new Rect(10.0f, 120.0f, _ImagenPerdedor.width * 0.6f, _ImagenPerdedor.height * 0.6f),_ImagenPerdedor);
			GUI.Label(posicionTamanioMjes, "PERDISTE :(");
		}
		//si toca la puerta pero no tiene el tubo se informa al usuario
		if (ObjetosPorJugador.TocandoPuerta) 
		{
			ObjetosPorJugador.TocandoPuerta = false;
			mje_falta_tubo = true;
			StartCoroutine(Esperar_OcultarMje());
		}


		//si algun flag para mostrar mensaje es activado invoco a la funcion que espera y desactiva ese flag
		if(MensajeZapato || MjeTieneTubo || MjeTieneZoquete || MjeDescontaminado || MjeContaminado || 
		   MjeTieneCasco || MjeContaminaste || MjeCascoUsado || MjeYaTenesTubo || MjeYaTenesItem)
		{
			StartCoroutine(Esperar_OcultarMje());
		}



		// mensajes que se mostratran por unos segundos dependiendo del item obtenido en una caja
		if (mje_falta_tubo)
		{
			GUI.Label(posicionTamanioMjes, "Necesitas un tubo para poder pasar");
		}
		else if (MensajeZapato)
		{
			//GUI.DrawTexture(new Rect(10, 10, _ImagenZapato.width, _ImagenZapato.height),_ImagenZapato);
			GUI.Label(posicionTamanioMjes, "Ahora corres mas rapido!");
		}
		else if (MjeTieneTubo)
		{
			//GUI.DrawTexture(new Rect(10, 10, _ImagenTubo.width, _ImagenTubo.height),_ImagenTubo);
			GUI.Label(posicionTamanioMjes, "Busca la puerta y gana!");
		}
		else if (MjeTieneZoquete)
		{
			//GUI.DrawTexture(new Rect(10, 10, _ImagenZoquete.width, _ImagenZoquete.height),_ImagenZoquete);
			GUI.Label(posicionTamanioMjes, "Te han contaminado! Busca la ducha!");
		}
		else if (MjeTieneCasco)
		{
			//GUI.DrawTexture(new Rect(10, 10, _ImagenCasco.width, _ImagenCasco.height),_ImagenCasco);
			GUI.Label(posicionTamanioMjes, "Sos inmune a las contaminaciones!");
		}
		else if (MjeDescontaminado)
		{
			GUI.Label(posicionTamanioMjes, "Estas descontaminado!");
		}
		else if (MjeContaminado)
		{
			GUI.Label(posicionTamanioMjes, "Debes descontaminarte en la ducha!");
		}
		else if (MjeContaminaste)
		{
			GUI.Label(posicionTamanioMjes, "Contaminaste a todos tus oponentes");
		}
		else if (MjeCascoUsado)
		{
			GUI.Label(posicionTamanioMjes, "No han podido contaminarte");
		}
		else if (MjeYaTenesTubo)
		{
			GUI.Label(posicionTamanioMjes, "Ya tenes un tubo");
		}
		else if (MjeYaTenesItem)
		{
			GUI.Label(posicionTamanioMjes, "Ya tenes este objeto");
		}





		//muestro en pantalla una imagen de cada item que tiene el jugador 
		if (ObjetosPorJugador.TieneTubo) 
		{
			GUI.DrawTexture (posicionImagenTubo, _ImagenTubo);
		}
		if (ObjetosPorJugador.TieneCasco) 
		{
			GUI.DrawTexture (posicionImagenCasco, _ImagenCasco);
		}
		if (ObjetosPorJugador.TieneZapato) 
		{
			GUI.DrawTexture (posicionImagenZapato, _ImagenZapato);
		}
		if (ObjetosPorJugador.TieneZoquete) 
		{
			GUI.DrawTexture (posicionImagenZoquete, _ImagenZoquete);
		}

	}


	//espera mientras se muestra un mensaje y luego dependiendo del item obtenido desactiva el flag para que no se muestre mas el mensaje
	IEnumerator Esperar_OcultarMje()
	{
		yield return new WaitForSeconds (3.0f);

		if (mje_falta_tubo) {
			mje_falta_tubo = false;
		}
		else if (MensajeZapato){
			MensajeZapato=false;
		}
		else if (MjeTieneTubo){
			MjeTieneTubo=false;
		}
		else if (MjeTieneZoquete){
			MjeTieneZoquete=false;
		}
		else if (MjeDescontaminado){
			MjeDescontaminado=false;
		}
		else if (MjeContaminado){
			MjeContaminado=false;
		}
		else if (MjeTieneCasco){
			MjeTieneCasco=false;
		}
		else if (MjeContaminaste){
			MjeContaminaste=false;
		}
		else if (MjeCascoUsado){
			MjeCascoUsado=false;
		}
		else if (MjeYaTenesTubo){
			MjeYaTenesTubo=false;
		}
		else if (MjeYaTenesItem){
			MjeYaTenesItem=false;
		}



	}



}
