//script attachado al jugador
//controla las interacciones con los objetos que el jugador puede encontrarse en el terreno

using UnityEngine;
using System.Collections;

public class ObtieneObjeto : MonoBehaviour {

	//OnTriggerEnter es llamada cuando el Collider c entra en el trigger.
	//Este mensaje es enviado al trigger collider y al collider que toco el trigger. 
	void OnTriggerEnter(Collider c) 
	{
		//Con esto evitamos que cuando el otro personaje colisiona con algo en mi juego, no me gestione la colision 
		if (networkView.isMine) 
		{
			switch (c.gameObject.tag)
			{
				case "Tubo":
					if(!ObjetosPorJugador.TieneTubo)
					{
						GameGUI.MjeTieneTubo = true;
						ObjetosPorJugador.TieneTubo = true;
						Network.Destroy(c.gameObject);
					}
					else
					{
						GameGUI.MjeYaTenesTubo = true;
					}
					break;

				case "Caja":

					switch (c.gameObject.GetComponent<ContenidoCaja>().NumeroItem) 
					{
						//zoquete: jugador contaminado 
						case 0:
							GameGUI.MjeContaminaste = true;
							networkView.RPC("AplicaZoquete",RPCMode.Others);
							Network.Destroy(c.gameObject);		
							break;	
							
						//casco: inmune a los zoquetes
						case 1:
							if(ObjetosPorJugador.TieneCasco)
							{
								GameGUI.MjeYaTenesItem = true;
							}
							else 
							{
								GameGUI.MjeTieneCasco = true;
								ObjetosPorJugador.TieneCasco = true;
							}
							
							Network.Destroy(c.gameObject);
							break;
							
						//zapatos veloces
						case 2:
							if(ObjetosPorJugador.TieneZapato)
							{
								GameGUI.MjeYaTenesItem = true;
							}
							else 
							{
								ObjetosPorJugador.TieneZapato = true;
								GameGUI.MensajeZapato = true;
							}
							
							Network.Destroy(c.gameObject);
							break;
						
						default: 
							break;
					}

					
					
					break;
					
				case "Ducha":
					//descontamina y muestra el mensaje
					GameGUI.MjeDescontaminado = true;
					ObjetosPorJugador.TieneZoquete = false;					
					break;

				case "Puerta":
					if(ObjetosPorJugador.TieneTubo)
					{
						if(!ObjetosPorJugador.TieneZoquete)
						{
							ObjetosPorJugador.JugadorHaGanado = true;
						}
						else {
							//si tiene el tubo pero agarro un zoquete no puede cruzar la puerta
							GameGUI.MjeContaminado = true;
						}
					}
					else 
					{
						//si no tiene el tubo muestro mensaje desde el GUI
						ObjetosPorJugador.TocandoPuerta = true;
					}
					break;

				default: break;
			}
		}
	}

	void Start(){
	}
	
	void Update(){
	}

	[RPC]
	void AplicaZoquete()
	{
		if (!ObjetosPorJugador.TieneCasco)
		{
			GameGUI.MjeTieneZoquete = true;
			ObjetosPorJugador.TieneZoquete = true;
		}
		else
		{
			GameGUI.MjeCascoUsado = true;
		}
	}


}
