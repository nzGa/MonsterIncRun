using UnityEngine;
using System.Collections;

public class MenuPrincipal : MonoBehaviour {

	[SerializeField]
	Texture2D _imagenFondo;
	
	[SerializeField]
	Texture2D imagenTitulo;
	
	public int nivel;//indicada en el Inspector, coincide con el numero de escena del build

	
	//usadas para definir la ventana de conexion al servidor
	private Rect posicionVentanaPrincipal;
	private int anchuraBotones = 60;

	
	//metodo ejecutado para mostrar y manejar eventos de interfaz de usuario
	//puede ser llamada varias veces por frame (una llamada por evento)
	//un evento puede ser input del usuario (teclado, mouse)
	void OnGUI()
	{		
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _imagenFondo);//imagen background
		GUI.DrawTexture(new Rect(40, 40, imagenTitulo.width/2, imagenTitulo.height/2),imagenTitulo);//imagen con el titulo del juego (señalado por mike)	

		VentanaMenuPrincipal();
	}
	
	
	//cuando se inicia el juego se muestra el menu de opciones
	void VentanaMenuPrincipal()
	{
		//rectangulo que determina la posicion y tamaño de la ventana de menu
		posicionVentanaPrincipal = new Rect(100, (Screen.height/2) -30, 300, 200);
		
		//Se genera la ventana
		//parametros de GUILayout.Window
		//id de la ventana
		//rectangulo en la pantlla para usar la ventana 
		//funcion que crea la GUI adentro de la ventana: toma como parametro el id de la ventana
		//texto para mostrar como titulo de la ventana
		GUILayout.Window(0, posicionVentanaPrincipal, GenerarVentanaMenu, "Run Mike Run");
	}


	void GenerarVentanaMenu(int windowID)
	{
		GUILayout.Space(15); //inserta un espacio vertical de 20 px	

		//se crea un boton y se setea el flag en true si se lo presiona
		if(GUILayout.Button("Iniciar", GUILayout.Height(anchuraBotones))) 
		{

			//cargo la escena del juego propiamente dicho
			Application.LoadLevel(nivel);//variable seteada en el Inspector, corresponde el nro de escena desde el build
		}

		GUILayout.Space(10);
		
		if(GUILayout.Button("Salir", GUILayout.Height(anchuraBotones))) 
		{
			Application.Quit();//se sale del juego
		}

	}

}




	
	