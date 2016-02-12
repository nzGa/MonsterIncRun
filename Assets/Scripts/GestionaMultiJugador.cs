//este script maneja todo lo relativo al sistema multijugador y los menues correspondientes
//adjuntado al objeto GameManager

using UnityEngine;
using System.Collections;


public class GestionaMultiJugador : MonoBehaviour {

	[SerializeField]
	float _segundosParaPerder = 900.0f;//Inspector 900 s (15 min)
	public static bool juegoIniciado = false;
	public bool juegoFinalizado = false;


	public string nombreJugador;
	public string nombreServidor;
	public string IPservidor;
	private string nombreJuego = "Run Mike Run";
	private int nroPuerto = 25001; //Puerto por default de Unity
	
	//necesarias para el metodo que inicia el servidor
	private bool useNAT = false; //activa NAT punchthrough:   bool useNat = !Network.HavePublicAddress();  //direccion publica
	private int maxConex = 5;//numero de conexiones permitidas (no necesariamente es la cantidad de jugadores)

	
	//flags usados para indicar si se deben mostrar las ventanas "Configurar servidor" y "Conectar a servidor"
	private bool configurarServidor = false;
	private bool conectarConServidor = false;
	
	//private bool esperandoCliente = false;
	
	//usadas para definir la ventana de conexion al servidor
	private Rect posicionVentana;
	private int anchuraBotones = 60;
	private int anchuraBotonesChicos = 30;
	
	
	//usadas para definir el label la ventana de "Conectando..."
	private int anchoLabelConectando = 400;
	private int altoLabelConectando = 60;
	
	
	//rectangulo de la ventana de estado del servidor
	private Rect posicionVentanaServidor;
	
	//usadas para definir la ventana de estado del cliente
	private Rect posicionVentanaEstadoCliente;
	private int anchoVentanaEstadoCliente = 300;
	private int altoVentanaEstadoCliente = 160;

	//la hago publica porque voy a accederla desde el script ControlCursor para activar/desactivar el mouse
	public bool muestraVentanaEstadoCliente = false; //se activa cuando un jugador pulsa ESC en el juego

	public bool cursorBloqueado; //indica cuando el cursor va a estar bloqueado
	
	private bool mostrarLabelConectando = false;//flag para mostrar el label "Conectando..."
	
	//indica que se perdio la conexión con el servidor
	private bool conexionPerdida = false;

	private GameObject[] jugadores;
	private GameObject[] cajas;
	private GameObject[] tubos;
	private GameObject puerta;
	private GameObject ducha;


	void Start () 
	{	
		//nombre de servidor por defecto en caso que querramos configurar uno
		nombreServidor = "Servidor1";
		
		//nombre de jugador por defecto
		nombreJugador = "Jugador1";	
		
		//IP de servidor por defecto: conexiones locales
		IPservidor = "127.0.0.1";

		//el juego debe iniciarse con el cursor bloqueado, si no la camara no sigue al personaje
		cursorBloqueado = true; 
	}
	
	
	//una vez por frame
	void Update() 
	{
		//si soy el server
		if (Network.peerType == NetworkPeerType.Server)
		{
			//y el timer esta activado
			if(MostrarTimer.TimerActivado)
			{
				//y ademas el timer no ha llegado a cero
				if(_segundosParaPerder > 0)
				{
					//resto un segundo al timer
					_segundosParaPerder -= Time.deltaTime;
				}
				
				//envio los segundos restantes a todos los jugadores
				networkView.RPC("InformaTiempo", RPCMode.AllBuffered, _segundosParaPerder);
			}

		}

		//Si se presiona la tecla ESC aparecera o desaparecera la ventana de desconexion del cliente durante el juego
		//no se usa para el servidor
		if(Input.GetKeyDown(KeyCode.Escape)) 
		{ 
			muestraVentanaEstadoCliente = !muestraVentanaEstadoCliente;

			//activo/desactivo el cursor: de lo contrario si presiono dos veces ESC queda desactivado
			cursorBloqueado =! cursorBloqueado;
		}
		
	}
	
	
	//metodo ejecutado para mostrar y manejar eventos de interfaz de usuario
	//puede ser llamada varias veces por frame (una llamada por evento)
	//un evento puede ser input del usuario (teclado, mouse)
	void OnGUI()
	{		
		//creo un estilo para los mensajes
		GUIStyle estilo = new GUIStyle();
		estilo.fontSize = 30;
		estilo.normal.textColor = Color.white;


		//si el juego se ejecuta en modo servidor se muestra la ventana de estado del servidor
		if (Network.peerType == NetworkPeerType.Server)
		{
			VentanaEstadoServidor();
		}
		
		//si el juego se ejecuta en modo cliente y se aprieta ESC, se muestra la ventana de estado del cliente
		if ((Network.peerType == NetworkPeerType.Client) && muestraVentanaEstadoCliente)
		{
			//voy a mostrar el menu del cliente asi que desbloqueo el cursor para poder elegir una opcion
			cursorBloqueado = false;
			VentanaEstadoCliente();
		}
		
		//si el cliente esta desconectado del servidor muestro la ventana de conexion
		if (Network.peerType == NetworkPeerType.Disconnected)
		{
			if (mostrarLabelConectando)
			{
				GUI.Label(new Rect(Screen.width/2 - anchoLabelConectando/2, Screen.height/2 - altoLabelConectando/2 , 
				                   anchoLabelConectando,  altoLabelConectando), "Conectando...", estilo);

			}
			//si no se está llevando a cabo una conexion muestro el menu principal
			else
			{
				VentanaMenu();
			}
			
		}
		
	}
	
	
	//cuando se inicia el juego se muestra el menu de opciones
	void VentanaMenu()
	{
		//rectangulo que determina la posicion y tamaño de la ventana de menu
		posicionVentana = new Rect(120, Screen.height / 2, 400, 260);
		
		//Se genera la ventana
		//parametros de GUILayout.Window
		//id de la ventana
		//rectangulo en la pantlla para usar la ventana 
		//funcion que crea la GUI adentro de la ventana: toma como parametro el id de la ventana
		//texto para mostrar como titulo de la ventana
		GUILayout.Window(0, posicionVentana, GenerarVentanaMenu, nombreJuego);
	}

	
	void GenerarVentanaMenu(int windowID)
	{
		GUILayout.Space(15); //inserta un espacio vertical de 20 px
		
		//los flags configurarServidor conectarConServidor se inicializan en false para mostrar los botones correspondientes en el menu principal
		if (!configurarServidor && !conectarConServidor )
		{
			if(conexionPerdida)
			{
				GUILayout.Label("Conexion perdida");
				GUILayout.Space(10);
			}

			//se crea un boton y se setea el flag en true si se lo presiona
			if(GUILayout.Button("Configurar Servidor", GUILayout.Height(anchuraBotones))) 
			{
				configurarServidor = true; 
			}
			
			GUILayout.Space(10);
			
			if(GUILayout.Button("Conectarse a un Servidor", GUILayout.Height(anchuraBotones))) 
			{
				conectarConServidor = true; 
			}
			
			GUILayout.Space(10);
			
			if(GUILayout.Button("Salir del Juego", GUILayout.Height(anchuraBotones))) 
			{
				Application.Quit();//se sale del juego
			}
		}
		
		//si se activo el flag para configurar el server se muestran las opciones correspondientes
		if(configurarServidor == true)
		{
			GUILayout.Label("Ingrese el nombre del servidor:");
			//se pide ingresar el nombre del servidor (por defecto se muestra uno)
			nombreServidor = GUILayout.TextField(nombreServidor);
			
			GUILayout.Space(5);
			
			GUILayout.Label("Indicar el puerto:");
			//se pide ingresar el numero de puerto para en el que escuchara el servidor
			nroPuerto = int.Parse(GUILayout.TextField(nroPuerto.ToString())); //parseo el numero a un entero
			
			GUILayout.Space(10);
			
			//se crea el boton para iniciar el servidor
			if(GUILayout.Button("Iniciar Servidor", GUILayout.Height(anchuraBotonesChicos))) 
			{				
				Network.InitializeServer(maxConex, nroPuerto, useNAT);
				
				//se apaga el flag para dejar de mostrar la ventana de configuracion de servidor
				configurarServidor = false;
				//esperandoCliente = true;
			}
			
			GUILayout.Space(5);
			
			//opcion de volver al menu principal, desactivando el flag que muestra el menu de config de servidor
			if(GUILayout.Button("Volver", GUILayout.Height(anchuraBotonesChicos))) 
			{
				configurarServidor = false;	
			}
		}
		
		
		/*if(esperandoCliente)
		{
			GUILayout.Label("Esperando un cliente...");
			
			GUILayout.Space(10);
			
			if(GUILayout.Button("Terminar Servidor"))
			{				
				Network.Disconnect();
				esperandoCliente = false;
			}
		}*/
		
		
		
		if(conectarConServidor == true)
		{
			GUILayout.Label("Ingresa tu nombre:");
			//se pide al jugador que ingrese su nombre para ser identificado por sus oponentes  (por defecto se muestra uno)
			nombreJugador = GUILayout.TextField(nombreJugador);
			
			GUILayout.Space(5);
			
			GUILayout.Label("Ingresa la IP del servidor:");
			//se pide ingresar la IP del servidor al que desea conectarse
			IPservidor = GUILayout.TextField(IPservidor);
			
			GUILayout.Space(5);
			
			// Permite al jugador ingresar el puerto del servidor al que desea conectarse
			GUILayout.Label("Ingresa el puerto:");
			nroPuerto = int.Parse(GUILayout.TextField(nroPuerto.ToString()));
			
			GUILayout.Space(5);
			
			if(GUILayout.Button("Conectar", GUILayout.Height(anchuraBotonesChicos))) 
			{
				//si el usuario no ingreso un nombre ponemos uno 
				if(nombreJugador == "")
				{
					nombreJugador = "Player1";
				}
				
				//conectando al server indicado 
				Network.Connect(IPservidor, nroPuerto);
				conectarConServidor = false; //se apaga el flag para dejar de mostrar esta ventana
				mostrarLabelConectando = true;//se muestra el label "Conectando..."

			}
			
			GUILayout.Space(5);
			
			if(GUILayout.Button("Volver", GUILayout.Height(anchuraBotonesChicos)))
			{
				conectarConServidor = false; //se vuelve al menu principal
			}
		}
	}
	
	
	//crea una ventana de estado del servidor una vez iniciado
	void VentanaEstadoServidor()
	{
		//rectangulo que determina la posicion y tamaño de la ventana
		posicionVentanaServidor = new Rect(120, Screen.height / 2,300, 120);
		
		//Se genera la ventana
		//id de la ventana
		//rectangulo en la pantalla para usar la ventana 
		//funcion que crea la GUI adentro de la ventana: toma como parametro el id de la ventana
		//texto para mostrar como titulo de la ventana
		GUILayout.Window(1, posicionVentanaServidor, GenerarVentanaServidor, "Estado el servidor"); 
	}
	
	
	void GenerarVentanaServidor(int windowID)
	{
		GUILayout.Label("Nombre del servidor: " + nombreServidor);
		GUILayout.Label("Jugadores conectados: " + Network.connections.Length);
		
		if(GUILayout.Button("Apagar el Servidor"))
		{
			//antes de desconectar elimino los elementos solo del servidor 
			//porque los clientes ya estan borrando todo
			jugadores = GameObject.FindGameObjectsWithTag ("Player");
			cajas = GameObject.FindGameObjectsWithTag ("Caja");
			tubos = GameObject.FindGameObjectsWithTag ("Tubo");
			puerta = GameObject.FindGameObjectWithTag ("Puerta");
			ducha = GameObject.FindGameObjectWithTag ("Ducha");
			
			foreach(GameObject jugador in jugadores)
			{
				Destroy (jugador);
			}
			
			foreach(GameObject caja in cajas)
			{
				Destroy (caja);
			}
			
			foreach(GameObject tubo in tubos)
			{
				Destroy (tubo);
			}
			
			Destroy (puerta);
			Destroy (ducha);

			//Close all open connections and shuts down the network interface.
			Network.Disconnect();
			Debug.Log("luego del Network.Disconnect: Jugadores conectados = " + Network.connections.Length);

			//recargo la escena  (tarda mucho y parece que el juego se ha colgado)
			Application.LoadLevel(1);
		}
	}
	
	
	//crea la ventana de estado del cliente que aparece al apretar ESC
	void VentanaEstadoCliente() 
	{
		posicionVentanaEstadoCliente = new Rect(Screen.width / 2 - anchoVentanaEstadoCliente / 2, Screen.height / 2 - altoVentanaEstadoCliente / 2, anchoVentanaEstadoCliente, altoVentanaEstadoCliente);
		//reutilizo el id de ventana del servidor porque esta ventana afecta solo a los clientes
		GUILayout.Window(1, posicionVentanaEstadoCliente, GenerarVentanaEstadoCliente, ""); 
	}
	
	
	void GenerarVentanaEstadoCliente(int windowID)
	{
		GUILayout.Label("Conectado al servidor: " + nombreServidor);	
		GUILayout.Space(7);
		
		if(GUILayout.Button("Desconectarse del Servidor", GUILayout.Height(25)))
		{			
			//desconecto al jugador de la red
			Network.Disconnect();
			
			//apago este flag para que no muestre el label "Conectando..." 
			mostrarLabelConectando = false;
			
			//apago este flag para que si me vuelvo a conectar al servidor no muestre la ventana de estado de cliente
			muestraVentanaEstadoCliente = false;

			cursorBloqueado = false;//desbloqueo cursor

			//recargo la escena  (tarda mucho y parece que el juego se ha colgado)
			Application.LoadLevel(1);
			
		}
		
		GUILayout.Space(5);
		
		if(GUILayout.Button("Volver al Juego", GUILayout.Height(25)))
		{
			muestraVentanaEstadoCliente = false; //se esconde la ventana (se vuelve al juego)
			cursorBloqueado = true;//bloqueo el cursor para seguir jugando
		}

		GUILayout.Space(5);
		
		if(GUILayout.Button("Salir del Juego", GUILayout.Height(25))) 
		{
			Application.Quit();//se sale del juego
		}
	}

	
	

	
	//cuando un cliente se conecta exitosamente al servidor, el servidor ejecuta esta funcion
	void OnPlayerConnected(NetworkPlayer networkPlayer)
	{
		//activo el timer una vez que tengo un jugador conectado
		MostrarTimer.TimerActivado = true;
		//el primer cliente conectado inicia el juego
		juegoIniciado = true;

		//chequeo si el juego ha iniciado
		if (juegoIniciado)
		{
			//luego de que han pasado los segundos indicados en el Inspector, se invoca a la funcion que hace perder a todos
			Invoke("Perder", _segundosParaPerder);
		}

		//activo el timer en todos los jugadores
		networkView.RPC("ActivarTimer", RPCMode.AllBuffered);	

		//invoco a la RPC que informa el nombre del servidor al cliente recien conectado
		networkView.RPC("InformarNombreServidor", networkPlayer, nombreServidor);	

	}
	
	//invocada solo desde el servidor: 
	//cuando un jugador se desconecta el server recibe el id del jugador y elimina los RPCs y objetos del jugador
	//de lo contrario aunque el jugador se haya desconectado pueden quedar elementos fantasma
	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("SERVER: Cliente desconectado. Borro RPCs y objetos del cliente");
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
		//Network.Destroy(gameObject);
	}
	

	//ejecutada en el cliente cuando la conexion se pierde o se desconecta del servidor
	//si se pierde la conexion con el servidor o se desconecta se activa el flag
	void OnDisconnectedFromServer()
	{
		Debug.Log("CLIENTE: Conexion con el server perdida");
		conexionPerdida = true;

		mostrarLabelConectando = false;//desactivo el flag para que no se muestre el label "Conectando..."
		
		//se eliminan de la vista todos los objetos para que no queden fantasmas al reconectar
		jugadores = GameObject.FindGameObjectsWithTag ("Player");
		cajas = GameObject.FindGameObjectsWithTag ("Caja");
		tubos = GameObject.FindGameObjectsWithTag ("Tubo");
		puerta = GameObject.FindGameObjectWithTag ("Puerta");
		ducha = GameObject.FindGameObjectWithTag ("Ducha");
		
		foreach(GameObject jugador in jugadores)
		{
			Destroy (jugador);
		}
		
		foreach(GameObject caja in cajas)
		{
			Destroy (caja);
		}
		
		foreach(GameObject tubo in tubos)
		{
			Destroy (tubo);
		}
		
		Destroy (puerta);
		Destroy (ducha);
	}

	//se ejecuta en el cliente cuando este se establece la conexion con el servidor
	void OnConnectedToServer()
	{
		Debug.Log("Server Joined");
	}

	
	//llamada en el servidor cuando  Network.InitializeServer es invocado y completado
	void OnServerInitialized() {

	}
	
	
	void Perder()
	{
		//si no se ha finalizado el juego
		if(!juegoFinalizado)
		{
			//invoco la RPC que hace perder al jugador en todos los clientes conectados y los que se conecten luego
			networkView.RPC("HacerPerder",RPCMode.AllBuffered);
		}
	}


	//Las RPCs (Remote Procedure Calls) son funciones declaradas en scripts que son attachados a un GameObject que 
	//contiene un Network View. 
	//La funcion RPC puede ser llamada desde cualquier script dentro de ese GameObject.


	//El servidor invoca esta RPC 
	//la instancia de este script del cliente la ejecuta
	[RPC]
	void InformarNombreServidor (string servername)
	{
		nombreServidor = servername;	
	}


	//hace perder al jugador
	[RPC]
	void HacerPerder()
	{
		ObjetosPorJugador.JugadorHaPerdido = true;
	}


	[RPC]
	void ActivarTimer()
	{
		MostrarTimer.TimerActivado = true;
	}

	//el servidor ejecuta este RPC para pasarle el tiempo restante a los clientes
	[RPC]
	void InformaTiempo(float segundosFaltantes)
	{
		MostrarTimer.segundosFaltantes = segundosFaltantes;
	}

}
