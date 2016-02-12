//Traduccion a C# del script JS que genera unity al agregar el componente Script -> Third Person Controller
//Modificado para adaparlo a lo que necesito. Se adjunta al jugador.
//Controla el movimiento del jugador y sus animaciones.
//Se agregaron los mensajes RPCs para informar los movimientos al resto de los jugadores en la red
using UnityEngine;
using System.Collections;

public class ThirdPersonController : MonoBehaviour
{

	//animaciones FBX en el inspector
	public AnimationClip idleAnimation;
	public AnimationClip walkAnimation;
	public AnimationClip runAnimation;
	public AnimationClip jumpPoseAnimation;
	public AnimationClip winAnimation;
	public AnimationClip loseAnimation;

	
	//los valores de las variables en el inspector tienen precedencia sobre los indicadas aqui
	public float walkMaxAnimationSpeed = 10.0f;
	public float trotMaxAnimationSpeed = 1.0f;
	public float runMaxAnimationSpeed = 3.0f;
	public float jumpAnimationSpeed = 2.0f;
	public float landAnimationSpeed = 1.0f;
	public float walkSpeed= 2.0f;//velocidad de la caminata
	public float trotSpeed= 4.0f;//luego de los segundos indicados en trotAfterSeconds se comienza a trotar
	public float runSpeed= 6.0f;	
	public float inAirControlAcceleration= 3.0f;
	public float jumpHeight= 1.3f;//cuan alto salta al presionar saltar y soltar
	public float gravity= 20.0f;
	public float speedSmoothing= 1000.0f; //gravedad en modo descenso controlado
	public float rotateSpeed= 500.0f;
	public float trotAfterSeconds= 100000.0f;
	public bool canJump= true;
	
	//referencia al componente Animation del personaje
	private Animation _animation;

	public enum CharacterState {
		Idle 	 = 0,
		Walking  = 1,
		Trotting = 2,
		Running  = 3,
		Jumping  = 4
	}

	private CharacterState _characterState;
	
	private float jumpRepeatTime= 0.05f;
	private float jumpTimeout= 0.15f;
	private float groundedTimeout= 0.25f;
	
	//la camara no comienza a seguir al personaje inmediatamente sino que espera un tiempo para evitar ondulaciones 
	private float lockCameraTimer= 0.0f;
	
	//dirección de movimiento actual en el plano x-z
	private Vector3 moveDirection= Vector3.zero;
	//velocidad vertical actual (0 por que esta sobre el suelo)
	private float verticalSpeed= 0.0f;
	//velocidad de movimiento actual en plano x-z
	private float moveSpeed= 0.0f;
	
	//flags de ultima colision retornadas por controller.Move
	private CollisionFlags collisionFlags; 
	
	//esta saltanto? (iniciado con el boton saltando pero no en tierra todavia)
	private bool jumping= false;
	private bool jumpingReachedApex= false;
	
	//se esta moviendo hacia atras? (bloquea la camara para no rotar 180 grados)
	private bool movingBack= false;
	//el usuario esta presionando alguna tecla?
	private bool isMoving= false;
	//cuando el usuario comenzo a caminar (usado para comenzar a trotar luego de un tiempo)
	private float walkTimeStart= 0.0f;
	//ultima vez que se presiono el boton saltar 
	private float lastJumpButtonTime= -10.0f;
	//ultima vez que se realizo un salto
	private float lastJumpTime= -1.0f;

	//altura desde la que saltamos  (usada para determinar durante cuanto tiempo se aplica una potencia extra luego del salto)
	private float lastJumpStartHeight= 0.0f;
	
	private Vector3 inAirVelocity= Vector3.zero;
	
	private float lastGroundedTime= 0.0f;
	
	private bool isControllable= true;
	
	//Awake es llamada solo una vez cuando la instancia del script esta siendo cargada, aunque el script no esté enabled.
	//Se invoca antes de cualquier funcion Start y luego de que un prefab sea instanciado. 
	void  Awake()
	{
		//La funcion Move de CharacterController que usara este vector de direccion espera un valor en world space 
		//	pero la velocidad esta en local space por lo tanto es necesario convertirla.
		//TransformDirection espera un Vector3 con valores local space y los convierte a world space.
		//Esta operacion no es afectada por la escala o posicion de el transform. El vector resultante tiene la misma 
		//	longitud y direccion.
		//La variable transform (con t minuscula) es el transform del game object que tiene este script. En este caso el jugador
		moveDirection = transform.TransformDirection(Vector3.forward);
		
		_animation = GetComponent<Animation>();
		if(!_animation)
			Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");
		
		if(!idleAnimation) {
			_animation = null;
			Debug.Log("No idle animation found. Turning off animations.");
		}
		if(!walkAnimation) {
			_animation = null;
			Debug.Log("No walk animation found. Turning off animations.");
		}
		if(!runAnimation) {
			_animation = null;
			Debug.Log("No run animation found. Turning off animations.");
		}
		if(!jumpPoseAnimation && canJump) {
			_animation = null;
			Debug.Log("No jump animation found and the character has canJump enabled. Turning off animations.");
		}	
	}


	void Start() 
	{
		//si el network view no esta controlado por este objeto desactivo el script
		if(!networkView.isMine)
		{
			enabled = false;
		}
	}

	
	void UpdateSmoothedMovementDirection()
	{
		Transform cameraTransform = Camera.main.transform;
		bool grounded= IsGrounded();
		
		//vector relativo a la camara a lo largo del plano x-z (se pone la componente y del vector en cero)
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
		forward.y = 0;
		//se normaliza el vector: se mantiene la direccion pero su longitud es de 1
		forward = forward.normalized;
		
		//vector relativo a la camera que siempre es ortogonal al vector forward
		Vector3 right= new Vector3(forward.z, 0, -forward.x);
		
		float v= Input.GetAxisRaw("Vertical");
		float h= Input.GetAxisRaw("Horizontal");
		
		//se esta moviendo o mirando hacia atras?
		if (v < -0.2f)
			movingBack = true;
		else
			movingBack = false;
		
		bool wasMoving= isMoving;
		isMoving = Mathf.Abs (h) > 0.1f || Mathf.Abs (v) > 0.1f;
		
		//direccion destino relativa a la camara
		Vector3 targetDirection= h * right + v * forward;
		
		//controles en tierra
		if (grounded)
		{
			//se bloquea la camara por un pequeño periodo cuando se transiciona entre movimiento y quedarse quieto
			lockCameraTimer += Time.deltaTime;
			if (isMoving != wasMoving)
				lockCameraTimer = 0.0f;
			
			//almacenamos velocidad y direccion de forma separada, asi cuando el personaje no se mueva todavia tengamos una dirección valida de movimiento
			//moveDirection siempre es normalizada y solo es actualizada si hay entrada del usuario
			if (targetDirection != Vector3.zero)
			{
				//si la velocidad es muy baja, ajustamos la direccion destino
				if (moveSpeed < walkSpeed * 0.9f && grounded)
				{
					moveDirection = targetDirection.normalized;
				}
				//de lo contrario suavemente giro hacia ella
				else
				{
					moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
					moveDirection = moveDirection.normalized;
				}
			}
			
			//suavizo la velocidad basado en la direccion destino actual 
			float curSmooth= speedSmoothing * Time.deltaTime;
			
			//se elige la velocidad destino
			//queremos soportar input analogo pero asegurando que no se pueda caminar mas rapido diagonalmente que yendo hacia adelante o lateralmente
			float targetSpeed= Mathf.Min(targetDirection.magnitude, 1.0f);
			
			_characterState = CharacterState.Idle;
			
			//se elije el modificador de velocidad
			if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
			{
				targetSpeed *= runSpeed;
				_characterState = CharacterState.Running;
			}
			else if (Time.time - trotAfterSeconds > walkTimeStart)
			{
				targetSpeed *= trotSpeed;
				_characterState = CharacterState.Trotting;
			}
			//agrego los controles de direccion para activar la caminata
			else if(Input.GetKey (KeyCode.W) ||
			        Input.GetKey (KeyCode.S) ||
			        Input.GetKey (KeyCode.A) ||
			        Input.GetKey (KeyCode.D) ) 
			{
				targetSpeed *= walkSpeed;
				_characterState = CharacterState.Walking;
			}
			//el script por default pone el estado en walking en este else. Yo necesito el idle aca
			else
			{
				_characterState = CharacterState.Idle;
			}
			
			moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);
			
			//se resetea el tiempo de inicio de caminata cuando se reduce la velocidad 
			if (moveSpeed < walkSpeed * 0.3f)
				walkTimeStart = Time.time;
		}
		//controles en el aire
		else
		{
			//camara bloqueada mientras se esta en el aire
			if (jumping)
				lockCameraTimer = 0.0f;
			
			if (isMoving)
				inAirVelocity += targetDirection.normalized * Time.deltaTime * inAirControlAcceleration;
		}
	}
	
	void ApplyJumping(){
		//previene saltar demasiado rapido luego de otro salto
		if (lastJumpTime + jumpRepeatTime > Time.time)
			return;
		
		if (IsGrounded()) {
			//saltamos
			//solo cuando se presiona el boton
			//con un timeout para poder presionar el boton ligeramente antes de caer al suelo
			if (canJump && Time.time < lastJumpButtonTime + jumpTimeout) {
				verticalSpeed = CalculateJumpVerticalSpeed (jumpHeight);
				SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	
	void ApplyGravity()
	{
		//si el personaje no es controlable no se mueve en absoluto
		if (isControllable)
		{
			//se le aplica gravedad
			bool jumpButton= Input.GetButton("Jump");

			//cuando se alcanza la cima del salto  se envia un mensaje
			if (jumping && !jumpingReachedApex && verticalSpeed <= 0.0f)
			{
				jumpingReachedApex = true;
				SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
			}
			
			// Si llego a tierra, vuelve velocidad vertical a 0.
			if (IsGrounded ())
				verticalSpeed = 0.0f;
			else
				verticalSpeed -= gravity * Time.deltaTime;
		}
	}
	
	public float CalculateJumpVerticalSpeed( float targetJumpHeight  )
	{
		//a partir de la altura y la gravedad se deduce  la velocidad hacia arriba para el personaje hasta alcanzar la cima
		return Mathf.Sqrt(2 * targetJumpHeight * gravity);
	}
	
	public void DidJump()
	{
		jumping = true;
		jumpingReachedApex = false;
		lastJumpTime = Time.time;
		lastJumpStartHeight = transform.position.y;
		lastJumpButtonTime = -10;
		
		_characterState = CharacterState.Jumping;
	}
	
	void Update()
	{
			//solo si el cursor esta lockeado puedo mover al personaje
			if (Screen.lockCursor == true) 
			{
				//cuando el jugador obtiene el zapato incremento las velocidades de animacion y de desplazamiento
				if (ObjetosPorJugador.TieneZapato) {
						runSpeed = 10.0f;
						runMaxAnimationSpeed = 6.0f;
				}

				if (!isControllable) {
						//mato todos los inputs si no es controlable
						Input.ResetInputAxes ();
				}

				// Si se presiono saltar
				if (Input.GetButtonDown ("Jump")) {
						lastJumpButtonTime = Time.time;
				}

				UpdateSmoothedMovementDirection ();

				//se aplica la gravedad
				//la potencia extra de salto modifica la gravedad
				//el modo controlledDescent modifica la gravedad
				ApplyGravity ();

				//se aplica la logica del salto
				ApplyJumping ();

				//se calcula el movimiento actual
				Vector3 movement = moveDirection * moveSpeed + new Vector3 (0, verticalSpeed, 0) + inAirVelocity;
				movement *= Time.deltaTime;

				
				CharacterController controller = GetComponent<CharacterController> ();
				
				//si el jugador no ha ganado y no ha perdido
				if (!ObjetosPorJugador.JugadorHaGanado && !ObjetosPorJugador.JugadorHaPerdido)
				{
					//entonces muevo al jugador
					collisionFlags = controller.Move (movement);
				}

				//sector de animaciones
				if (_animation) 
				{
						//si el jugador ha ganado
						if (ObjetosPorJugador.JugadorHaGanado)
						{							
							//muestro animacion de ganar
							_animation.CrossFade (winAnimation.name);
							
							//informo al resto de los clientes que el jugador de esta instancia ha ganado
							networkView.RPC ("Wining", RPCMode.Others);

							//hago que el resto pierda
							networkView.RPC("HacerPerder",RPCMode.OthersBuffered);

							//desactivo el timer de todos los jugadores para que se congele el tiempo 
							networkView.RPC("DesactivarTimer",RPCMode.AllBuffered);

							//finalizo el juego para que no se lance el perder por timeout
							networkView.RPC("TerminarJuego",RPCMode.AllBuffered);
						}

						//si el jugador ha perdido
						else if (ObjetosPorJugador.JugadorHaPerdido)
						{							
							//muestro animacion de perder
							_animation.CrossFade (loseAnimation.name);
							
							//informo al resto de los clientes que el jugador de esta instancia ha perdido
							networkView.RPC ("Losing", RPCMode.Others);							
						}

						else if (_characterState == CharacterState.Jumping) 
						{
								if (!jumpingReachedApex) 
								{
										_animation [jumpPoseAnimation.name].speed = jumpAnimationSpeed;
										_animation [jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
										_animation.CrossFade (jumpPoseAnimation.name);

										//informo al resto de los clientes que el jugador de esta instancia esta saltando (para que salte tambien en las otras)
										networkView.RPC ("Jumping", RPCMode.All);
								} 
								else {
										_animation [jumpPoseAnimation.name].speed = -landAnimationSpeed;
										_animation [jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
										_animation.CrossFade (jumpPoseAnimation.name);
								}
						} 
						else 
						{
								if (controller.velocity.sqrMagnitude < 0.1f) 
								{
										_animation.CrossFade (idleAnimation.name);

										//informo al resto de los clientes que el jugador de esta instancia esta en idle
										networkView.RPC ("Waiting", RPCMode.All);
								} 
								else 
								{
										if (_characterState == CharacterState.Running) 
										{
												_animation [runAnimation.name].speed = Mathf.Clamp (controller.velocity.magnitude, 0.0f, runMaxAnimationSpeed);
												_animation.CrossFade (runAnimation.name);

												//informo al resto de los clientes que el jugador de esta instancia esta corriendo
												networkView.RPC ("Running", RPCMode.All);
										} 
										else if (_characterState == CharacterState.Trotting) 
										{
												_animation [walkAnimation.name].speed = Mathf.Clamp (controller.velocity.magnitude, 0.0f, trotMaxAnimationSpeed);
												_animation.CrossFade (walkAnimation.name);
										} 
										else if (_characterState == CharacterState.Walking) 
										{
												_animation [walkAnimation.name].speed = Mathf.Clamp (controller.velocity.magnitude, 0.0f, walkMaxAnimationSpeed);
												_animation.CrossFade (walkAnimation.name);

												//informo al resto de los clientes que el jugador de esta instancia esta caminando
												networkView.RPC ("Walking", RPCMode.All);
										}
										
				
								}
						}
				}

				//sector de animacion

				//se setea la rotacion a la direccion de movimiento
				if (IsGrounded ()) {
						transform.rotation = Quaternion.LookRotation (moveDirection);
				} else {
						Vector3 xzMove = movement;
						xzMove.y = 0;
						if (xzMove.sqrMagnitude > 0.001f) {
								transform.rotation = Quaternion.LookRotation (xzMove);
						}
				}	

				//estamos en el modo de salto, pero recien aterrizado
				if (IsGrounded ()) {
						lastGroundedTime = Time.time;
						inAirVelocity = Vector3.zero;
						if (jumping) {
								jumping = false;
								SendMessage ("DidLand", SendMessageOptions.DontRequireReceiver);
						}
				}

			}

	}
	
	void OnControllerColliderHit( ControllerColliderHit hit   )
	{
		if (hit.moveDirection.y > 0.01f) 
			return;
	}
	
	public float GetSpeed()
	{
		return moveSpeed;
	}
	
	public bool IsJumping()
	{
		return jumping;
	}
	
	public bool IsGrounded()
	{
		return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
	}
	
	public Vector3 GetDirection()
	{
		return moveDirection;
	}
	
	public bool IsMovingBackwards()
	{
		return movingBack;
	}
	
	public float GetLockCameraTimer()
	{
		return lockCameraTimer;
	}
	
	public bool IsMoving()
	{
		return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f;
	}
	
	public bool HasJumpReachedApex()
	{
		return jumpingReachedApex;
	}
	
	public bool IsGroundedWithTimeout()
	{
		return lastGroundedTime + groundedTimeout > Time.time;
	}
	
	public void Reset()
	{
		gameObject.tag = "Player";
	}


	//Las RPCs (Remote Procedure Calls) son funciones declaradas en scripts que son attachados a un GameObject que 
	//contiene un Network View. El Network View debe apuntar al script que contiene la funcion RPC. 
	//La funcion RPC puede ser llamada desde cualquier script dentro de ese GameObject.


	//funciones que seran invocadas por otros clientes para informarme sobre sus animaciones
	[RPC]
	void Waiting()
	{
		animation.Play("Espera", PlayMode.StopAll);
	}
	
	[RPC]
	void Walking()
	{
		animation.Play("Camina", PlayMode.StopAll);
	}
	
	[RPC]
	void Running()
	{
		animation.Play("Corre", PlayMode.StopAll);
	}
	
	[RPC]
	void Jumping()
	{
		animation.Play("Salta", PlayMode.StopAll);
	}

	[RPC]
	void Wining()
	{
		animation.Play("Gana", PlayMode.StopAll);
	}

	[RPC]
	void Losing()
	{
		animation.Play("Pierde", PlayMode.StopAll);
	}


	[RPC]
	void HacerPerder()
	{
		ObjetosPorJugador.JugadorHaPerdido = true;
	}

	[RPC]
	void DesactivarTimer()
	{
		MostrarTimer.TimerActivado = false;
	}

	//finalizo el juego
	[RPC]
	void TerminarJuego()
	{
		GameObject.FindGameObjectWithTag("GameManager").GetComponent<GestionaMultiJugador>().juegoFinalizado = true;
	}
}