using System;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip runAnimation;
    public AnimationClip jumpPoseAnimation;
    public AnimationClip winAnimation;
    public AnimationClip loseAnimation;

    public float walkMaxAnimationSpeed = 1.05f;
    public float trotMaxAnimationSpeed = 1f;
    public float runMaxAnimationSpeed = 1.15f;
    public float jumpAnimationSpeed = 1.15f;
    public float landAnimationSpeed = 1f;
    public float walkSpeed = 2f;
    public float trotSpeed = 4f;
    public float runSpeed = 9f;
    const float WalkSpeedUnity4 = 2f;
    const float RunSpeedBase = 9f;
    const float RunSpeedZapato = 12f;
    const float RunAnimSpeed = 1.15f;
    const float RunAnimSpeedZapato = 1.3f;
    const float FadeAnimacion = 0.12f;
    public float inAirControlAcceleration = 3f;
    public float jumpHeight = 1.3f;
    public float gravity = 20f;
    public float speedSmoothing = 10f;
    public float rotateSpeed = 500f;
    public float trotAfterSeconds = 100000f;
    public bool canJump = true;

    Animation _animation;

    public enum CharacterState
    {
        Idle = 0,
        Walking = 1,
        Trotting = 2,
        Running = 3,
        Jumping = 4
    }

    CharacterState _characterState;
    float jumpRepeatTime = 0.05f;
    float jumpTimeout = 0.15f;
    float groundedTimeout = 0.25f;
    float lockCameraTimer;
    Vector3 moveDirection = Vector3.zero;
    float verticalSpeed;
    float moveSpeed;
    CollisionFlags collisionFlags;
    bool jumping;
    bool jumpingReachedApex;
    bool movingBack;
    bool isMoving;
    float walkTimeStart;
    float lastJumpButtonTime = -10f;
    float lastJumpTime = -1f;
    Vector3 inAirVelocity = Vector3.zero;
    float lastGroundedTime;
    bool isControllable = true;
    bool _anuncioFinal;
    string _clipActual;
    Transform _huesoLocomocion;
    Vector3 _huesoLocomocionBind;
    float _huesoLocomocionYaw;
    Vector3 _posTrasMover;
    Quaternion _rotTrasMover;
    bool _poseFijada;

    void Awake()
    {
        moveDirection = transform.TransformDirection(Vector3.forward);
        _animation = GetComponent<Animation>();
        if (_animation == null)
            _animation = GetComponentInChildren<Animation>();
        AsignarClipsSiFaltan();
        AnclarHuesoLocomocion();
        walkSpeed = WalkSpeedUnity4;
        runSpeed = RunSpeedBase;
        runMaxAnimationSpeed = RunAnimSpeed;
    }

    public void RecargarClips()
    {
        if (_animation == null)
            _animation = GetComponent<Animation>() ?? GetComponentInChildren<Animation>();
        idleAnimation = null;
        walkAnimation = null;
        runAnimation = null;
        jumpPoseAnimation = null;
        winAnimation = null;
        loseAnimation = null;
        AsignarClipsSiFaltan();
        AnclarHuesoLocomocion();
    }

    void Start()
    {
        AnclarHuesoLocomocion();
        var controller = GetComponent<CharacterController>();
        if (controller != null)
            collisionFlags = controller.Move(Vector3.down * 0.05f);
    }

    void AsignarClipsSiFaltan()
    {
        if (_animation == null)
            return;
        if (idleAnimation == null) idleAnimation = Clip("Espera");
        if (walkAnimation == null) walkAnimation = Clip("Camina");
        if (runAnimation == null) runAnimation = Clip("Corre");
        if (jumpPoseAnimation == null) jumpPoseAnimation = Clip("Salta");
        if (winAnimation == null) winAnimation = Clip("Gana");
        if (loseAnimation == null) loseAnimation = Clip("Pierde");
    }

    AnimationClip Clip(string name)
    {
        if (_animation == null)
            return null;

        var exact = _animation.GetClip(name);
        if (exact != null)
            return exact;

        AnimationClip first = null;
        foreach (AnimationState state in _animation)
        {
            if (state.clip == null)
                continue;
            if (first == null)
                first = state.clip;
            if (string.Equals(state.clip.name, name, StringComparison.OrdinalIgnoreCase))
                return state.clip;
            if (state.clip.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                return state.clip;
        }

        if (name.Equals("Espera", StringComparison.OrdinalIgnoreCase))
            return first;
        return null;
    }

    void UpdateSmoothedMovementDirection()
    {
        Transform cameraTransform = Camera.main != null ? Camera.main.transform : transform;
        bool grounded = IsGrounded();

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.ProjectOnPlane(cameraTransform.up, Vector3.up);
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.forward;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        if (right.sqrMagnitude < 0.0001f)
            right = new Vector3(forward.z, 0f, -forward.x);
        right.Normalize();

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");
        movingBack = v < -0.2f;
        bool wasMoving = isMoving;
        isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
        Vector3 targetDirection = h * right + v * forward;
        if (targetDirection.sqrMagnitude > 1f)
            targetDirection.Normalize();

        if (grounded)
        {
            lockCameraTimer += Time.deltaTime;
            if (isMoving != wasMoving)
                lockCameraTimer = 0f;

            if (targetDirection.sqrMagnitude > 0.0001f)
            {
                Vector3 wish = targetDirection.normalized;
                float angle = Vector3.Angle(moveDirection, wish);
                // Snap only for modest turns from rest. A 180 from S or camera
                // orbit must RotateTowards so the body does not flip every frame.
                if (moveSpeed < walkSpeed * 0.9f && angle < 90f)
                    moveDirection = wish;
                else
                {
                    moveDirection = Vector3.RotateTowards(
                        moveDirection,
                        wish,
                        rotateSpeed * Mathf.Deg2Rad * Time.deltaTime,
                        1000f);
                    moveDirection = moveDirection.normalized;
                }
            }

            float curSmooth = speedSmoothing * Time.deltaTime;
            float targetSpeed = Mathf.Min(targetDirection.magnitude, 1f);
            _characterState = CharacterState.Idle;
            bool corriendo = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (corriendo && isMoving)
            {
                targetSpeed *= runSpeed;
                _characterState = CharacterState.Running;
                curSmooth = 1f;
            }
            else if (Time.time - trotAfterSeconds > walkTimeStart)
            {
                targetSpeed *= trotSpeed;
                _characterState = CharacterState.Trotting;
            }
            else if (isMoving)
            {
                targetSpeed *= walkSpeed;
                _characterState = CharacterState.Walking;
            }
            else
                _characterState = CharacterState.Idle;

            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);
            if (moveSpeed < walkSpeed * 0.3f)
                walkTimeStart = Time.time;
        }
        else
        {
            if (jumping)
                lockCameraTimer = 0f;
            if (isMoving)
                inAirVelocity += targetDirection.normalized * Time.deltaTime * inAirControlAcceleration;
        }
    }

    void ApplyJumping()
    {
        if (lastJumpTime + jumpRepeatTime > Time.time)
            return;
        if (IsGrounded() && canJump && Time.time < lastJumpButtonTime + jumpTimeout)
        {
            verticalSpeed = CalculateJumpVerticalSpeed(jumpHeight);
            SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
        }
    }

    void ApplyGravity()
    {
        if (!isControllable)
            return;
        if (jumping && !jumpingReachedApex && verticalSpeed <= 0f)
        {
            jumpingReachedApex = true;
            SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
        }
        if (IsGrounded())
            verticalSpeed = 0f;
        else
            verticalSpeed -= gravity * Time.deltaTime;
    }

    public float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        return Mathf.Sqrt(2 * targetJumpHeight * gravity);
    }

    public void DidJump()
    {
        jumping = true;
        jumpingReachedApex = false;
        lastJumpTime = Time.time;
        lastJumpButtonTime = -10;
        _characterState = CharacterState.Jumping;
    }

    void Update()
    {
        if (!GestionaMultiJugador.ControlJugadorActivo)
            return;

        if (ObjetosPorJugador.TieneZapato)
        {
            runSpeed = RunSpeedZapato;
            runMaxAnimationSpeed = RunAnimSpeedZapato;
        }
        else
        {
            runSpeed = RunSpeedBase;
            runMaxAnimationSpeed = RunAnimSpeed;
        }

        if (!isControllable)
            Input.ResetInputAxes();

        if (Input.GetButtonDown("Jump"))
            lastJumpButtonTime = Time.time;

        UpdateSmoothedMovementDirection();
        ApplyGravity();
        ApplyJumping();

        Vector3 planar = moveDirection.sqrMagnitude > 0.0001f
            ? moveDirection.normalized * moveSpeed
            : Vector3.zero;
        Vector3 movement = planar + new Vector3(0, verticalSpeed, 0) + inAirVelocity;
        movement *= Time.deltaTime;

        var controller = GetComponent<CharacterController>();
        if (controller != null && !ObjetosPorJugador.JugadorHaGanado && !ObjetosPorJugador.JugadorHaPerdido)
            collisionFlags = controller.Move(movement);

        if (transform.position.y < AmbienteTerreno.YCaida)
        {
            SpawnJugador.Recolocar(transform);
            verticalSpeed = 0f;
            inAirVelocity = Vector3.zero;
            jumping = false;
            jumpingReachedApex = false;
            if (controller != null)
                collisionFlags = controller.Move(Vector3.down * 0.05f);
        }

        Animar(controller);

        // Face the travel direction only. Mouse orbit never copies onto transform.forward.
        if (IsGrounded())
        {
            if (isMoving && moveDirection.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(moveDirection);
        }
        else
        {
            Vector3 xzMove = movement;
            xzMove.y = 0;
            if (xzMove.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(xzMove);
        }

        if (IsGrounded())
        {
            lastGroundedTime = Time.time;
            inAirVelocity = Vector3.zero;
            if (jumping)
            {
                jumping = false;
                SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
            }
        }

        _posTrasMover = transform.position;
        _rotTrasMover = transform.rotation;
        _poseFijada = true;
    }

    void LateUpdate()
    {
        if (_poseFijada)
        {
            transform.SetPositionAndRotation(_posTrasMover, _rotTrasMover);
            _poseFijada = false;
        }

        if (_huesoLocomocion == null)
            return;
        var pos = _huesoLocomocion.localPosition;
        pos.x = _huesoLocomocionBind.x;
        pos.z = _huesoLocomocionBind.z;
        _huesoLocomocion.localPosition = pos;
        var euler = _huesoLocomocion.localEulerAngles;
        euler.y = _huesoLocomocionYaw;
        _huesoLocomocion.localEulerAngles = euler;
    }

    void Animar(CharacterController controller)
    {
        if (_animation == null)
            return;

        if (ObjetosPorJugador.JugadorHaGanado)
        {
            if (winAnimation != null)
                Reproducir(Estado(winAnimation, "Gana"), 1f, WrapMode.Once);
            if (!_anuncioFinal)
            {
                _anuncioFinal = true;
                if (GestionaMultiJugador.Instancia != null)
                    GestionaMultiJugador.Instancia.TerminarJuego();
            }
            return;
        }

        if (ObjetosPorJugador.JugadorHaPerdido)
        {
            if (loseAnimation != null)
                Reproducir(Estado(loseAnimation, "Pierde"), 1f, WrapMode.Once);
            return;
        }

        if (_characterState == CharacterState.Jumping && jumpPoseAnimation != null)
        {
            var salta = Estado(jumpPoseAnimation, "Salta");
            float velSalta = jumpingReachedApex ? -landAnimationSpeed : jumpAnimationSpeed;
            Reproducir(salta, velSalta, WrapMode.ClampForever);
            return;
        }

        float vel = controller != null ? controller.velocity.magnitude : moveSpeed;
        bool enMarcha = vel > 0.12f || isMoving;

        if (_characterState == CharacterState.Running && runAnimation != null && enMarcha)
        {
            Reproducir(Estado(runAnimation, "Corre"), runMaxAnimationSpeed, WrapMode.Loop);
            return;
        }

        if (enMarcha && walkAnimation != null)
        {
            float rel = walkSpeed > 0.05f ? vel / walkSpeed : 1f;
            float speed = Mathf.Clamp(rel, 0.9f, walkMaxAnimationSpeed);
            Reproducir(Estado(walkAnimation, "Camina"), speed, WrapMode.Loop);
            return;
        }

        if (idleAnimation != null)
            Reproducir(Estado(idleAnimation, "Espera"), 1f, WrapMode.Loop);
    }

    void Reproducir(string clip, float speed, WrapMode wrap)
    {
        if (string.IsNullOrEmpty(clip) || _animation[clip] == null)
            return;
        _animation[clip].speed = speed;
        _animation[clip].wrapMode = wrap;
        if (_clipActual == clip && _animation.IsPlaying(clip))
            return;
        _animation.CrossFade(clip, FadeAnimacion);
        _clipActual = clip;
    }

    void AnclarHuesoLocomocion()
    {
        _huesoLocomocion = BuscarHijo(transform, "Bip003")
            ?? BuscarHijo(transform, "Bip002")
            ?? BuscarHijo(transform, "Bip001");
        if (_huesoLocomocion == null)
            return;
        _huesoLocomocionBind = _huesoLocomocion.localPosition;
        _huesoLocomocionYaw = _huesoLocomocion.localEulerAngles.y;
    }

    static Transform BuscarHijo(Transform root, string name)
    {
        if (root.name == name)
            return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var found = BuscarHijo(root.GetChild(i), name);
            if (found != null)
                return found;
        }
        return null;
    }

    string Estado(AnimationClip clip, string alias)
    {
        if (_animation == null)
            return alias;
        if (!string.IsNullOrEmpty(alias) && _animation.GetClip(alias) != null)
            return alias;
        if (clip != null && _animation.GetClip(clip.name) != null)
            return clip.name;
        if (clip != null)
        {
            foreach (AnimationState state in _animation)
            {
                if (state.clip == clip)
                    return state.name;
            }
        }
        return clip != null ? clip.name : alias;
    }

    public float GetSpeed() { return moveSpeed; }
    public bool IsJumping() { return jumping; }
    public bool IsGrounded() { return (collisionFlags & CollisionFlags.CollidedBelow) != 0; }
    public Vector3 GetDirection() { return moveDirection; }
    public bool IsMovingBackwards() { return movingBack; }
    public float GetLockCameraTimer() { return lockCameraTimer; }
    public bool IsMoving()
    {
        return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f;
    }
    public bool HasJumpReachedApex() { return jumpingReachedApex; }
    public bool IsGroundedWithTimeout() { return lastGroundedTime + groundedTimeout > Time.time; }
    public void Reset() { gameObject.tag = "Player"; }
}
