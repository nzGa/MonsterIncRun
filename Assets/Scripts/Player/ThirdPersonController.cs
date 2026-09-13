using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip runAnimation;
    public AnimationClip jumpPoseAnimation;
    public AnimationClip winAnimation;
    public AnimationClip loseAnimation;

    public float walkMaxAnimationSpeed = 10f;
    public float trotMaxAnimationSpeed = 1f;
    public float runMaxAnimationSpeed = 3f;
    public float jumpAnimationSpeed = 2f;
    public float landAnimationSpeed = 1f;
    public float walkSpeed = 2f;
    public float trotSpeed = 4f;
    public float runSpeed = 6f;
    public float inAirControlAcceleration = 3f;
    public float jumpHeight = 1.3f;
    public float gravity = 20f;
    public float speedSmoothing = 1000f;
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
        foreach (AnimationState state in _animation)
        {
            if (state.clip != null && state.clip.name == name)
                return state.clip;
        }
        return _animation.GetClip(name);
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
            else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
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
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        if (ObjetosPorJugador.TieneZapato)
        {
            runSpeed = 10f;
            runMaxAnimationSpeed = 6f;
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
                _animation.CrossFade(winAnimation.name);
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
                _animation.CrossFade(loseAnimation.name);
            return;
        }

        if (_characterState == CharacterState.Jumping && jumpPoseAnimation != null)
        {
            _animation[jumpPoseAnimation.name].speed = jumpingReachedApex ? -landAnimationSpeed : jumpAnimationSpeed;
            _animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
            _animation.CrossFade(jumpPoseAnimation.name);
            return;
        }

        if (controller == null || controller.velocity.sqrMagnitude < 0.1f)
        {
            if (idleAnimation != null)
                _animation.CrossFade(idleAnimation.name);
            return;
        }

        if (_characterState == CharacterState.Running && runAnimation != null)
        {
            _animation[runAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0f, runMaxAnimationSpeed);
            _animation.CrossFade(runAnimation.name);
        }
        else if (walkAnimation != null)
        {
            _animation[walkAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0f, walkMaxAnimationSpeed);
            _animation.CrossFade(walkAnimation.name);
        }
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
