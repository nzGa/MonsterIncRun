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

    public float walkMaxAnimationSpeed = 1.2f;
    public float trotMaxAnimationSpeed = 1f;
    public float runMaxAnimationSpeed = 1.35f;
    public float jumpAnimationSpeed = 1.15f;
    public float landAnimationSpeed = 1f;
    public float walkSpeed = 2f;
    public float trotSpeed = 4f;
    public float runSpeed = 6f;
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

    void Awake()
    {
        moveDirection = transform.TransformDirection(Vector3.forward);
        _animation = GetComponent<Animation>();
        if (_animation == null)
            _animation = GetComponentInChildren<Animation>();
        AsignarClipsSiFaltan();
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
    }

    void Start()
    {
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
        if (Camera.main == null)
            return;

        Transform cameraTransform = Camera.main.transform;
        bool grounded = IsGrounded();

        Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;
        Vector3 right = new Vector3(forward.z, 0, -forward.x);

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");
        movingBack = v < -0.2f;
        bool wasMoving = isMoving;
        isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
        Vector3 targetDirection = h * right + v * forward;

        if (grounded)
        {
            lockCameraTimer += Time.deltaTime;
            if (isMoving != wasMoving)
                lockCameraTimer = 0f;

            if (targetDirection != Vector3.zero)
            {
                if (moveSpeed < walkSpeed * 0.9f && grounded)
                    moveDirection = targetDirection.normalized;
                else
                {
                    moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
                    moveDirection = moveDirection.normalized;
                }
            }

            float curSmooth = speedSmoothing * Time.deltaTime;
            float targetSpeed = Mathf.Min(targetDirection.magnitude, 1f);
            _characterState = CharacterState.Idle;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                targetSpeed *= runSpeed;
                _characterState = CharacterState.Running;
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
            runSpeed = 10f;
            runMaxAnimationSpeed = 1.8f;
        }

        if (!isControllable)
            Input.ResetInputAxes();

        if (Input.GetButtonDown("Jump"))
            lastJumpButtonTime = Time.time;

        UpdateSmoothedMovementDirection();
        ApplyGravity();
        ApplyJumping();

        Vector3 movement = moveDirection * moveSpeed + new Vector3(0, verticalSpeed, 0) + inAirVelocity;
        movement *= Time.deltaTime;

        var controller = GetComponent<CharacterController>();
        if (controller != null && !ObjetosPorJugador.JugadorHaGanado && !ObjetosPorJugador.JugadorHaPerdido)
            collisionFlags = controller.Move(movement);

        Animar(controller);

        if (IsGrounded())
            transform.rotation = Quaternion.LookRotation(moveDirection);
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
    }

    void Animar(CharacterController controller)
    {
        if (_animation == null)
            return;

        if (ObjetosPorJugador.JugadorHaGanado)
        {
            if (winAnimation != null)
                _animation.CrossFade(Estado(winAnimation, "Gana"));
            if (!_anuncioFinal)
            {
                _anuncioFinal = true;
                var manager = GameObject.FindGameObjectWithTag("GameManager");
                if (manager != null)
                    manager.GetComponent<GestionaMultiJugador>().TerminarJuego();
            }
            return;
        }

        if (ObjetosPorJugador.JugadorHaPerdido)
        {
            if (loseAnimation != null)
                _animation.CrossFade(Estado(loseAnimation, "Pierde"));
            return;
        }

        if (_characterState == CharacterState.Jumping && jumpPoseAnimation != null)
        {
            var salta = Estado(jumpPoseAnimation, "Salta");
            _animation[salta].speed = jumpingReachedApex ? -landAnimationSpeed : jumpAnimationSpeed;
            _animation[salta].wrapMode = WrapMode.ClampForever;
            _animation.CrossFade(salta);
            return;
        }

        bool caminando = _characterState == CharacterState.Walking
            || _characterState == CharacterState.Trotting
            || isMoving;
        float vel = controller != null ? controller.velocity.magnitude : moveSpeed;

        if (_characterState == CharacterState.Running && runAnimation != null)
        {
            var corre = Estado(runAnimation, "Corre");
            float rel = runSpeed > 0.05f ? vel / runSpeed : 1f;
            _animation[corre].speed = Mathf.Clamp(rel * 1.2f, 0.9f, runMaxAnimationSpeed);
            _animation.CrossFade(corre);
            return;
        }

        if (caminando && walkAnimation != null)
        {
            var camina = Estado(walkAnimation, "Camina");
            float rel = walkSpeed > 0.05f ? vel / walkSpeed : 1f;
            _animation[camina].speed = Mathf.Clamp(rel * 1.15f, 0.9f, walkMaxAnimationSpeed);
            _animation.CrossFade(camina);
            return;
        }

        if (idleAnimation != null)
            _animation.CrossFade(Estado(idleAnimation, "Espera"));
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
