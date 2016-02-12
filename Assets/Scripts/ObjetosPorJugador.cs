using UnityEngine;
using System.Collections;

public class ObjetosPorJugador : MonoBehaviour {
		
	//Its set to static so any part of our code can easily access it.
	static bool _jugadorHaGanado = false;
	//so that other parts of the code can access the boolean, we'll make a public property
	public static bool JugadorHaGanado { get{ return _jugadorHaGanado; } set{ _jugadorHaGanado = value; }}
	
	static bool _jugadorHaPerdido = false;
	public static bool JugadorHaPerdido { get{ return _jugadorHaPerdido; } set{ _jugadorHaPerdido = value; }}
	
	static bool _tieneTubo = false;
	public static bool TieneTubo { get{ return _tieneTubo; } set{ _tieneTubo = value; }}
	
	static bool _tocandoPuerta = false;
	public static bool TocandoPuerta { get{ return _tocandoPuerta; } set{ _tocandoPuerta = value; }}
	
	static bool _tieneZapato = false;
	public static bool TieneZapato { get{ return _tieneZapato; } set{ _tieneZapato = value; }}
	
	static bool _tieneZoquete = false;
	public static bool TieneZoquete { get{ return _tieneZoquete; } set{ _tieneZoquete = value; }}
	
	static bool _tieneCasco = false;
	public static bool TieneCasco { get{ return _tieneCasco; } set{ _tieneCasco = value; }}
	
	
}
