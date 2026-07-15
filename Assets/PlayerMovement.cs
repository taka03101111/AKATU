using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float rotateSpeed = 120.0f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Camera")]
    [SerializeField] private Transform viewCamera;
    [SerializeField] private float cameraLookSpeed = 80.0f;
    [SerializeField] private float minCameraAngle = -30.0f;
    [SerializeField] private float maxCameraAngle = 45.0f;

    [Header("Action Time")]
    [SerializeField] private float attackTime = 1.0f;
    [SerializeField] private float damageTime = 0.8f;

    private CharacterController controller;
    private Animator animator;

    private float verticalVelocity;
    private float cameraPitch;

    private int currentAnimId = -1;

    private bool damageRequested;
    private bool stabRequested;
    private bool fullAttackRequested;

    [Networked] private int NetworkAnimId { get; set; }
    [Networked] private int DamageCount { get; set; }
    [Networked] private NetworkBool IsDead { get; set; }
    [Networked] private TickTimer ActionTimer { get; set; }

    private const int ANIM_IDLE = 0;
    private const int ANIM_RUN_FORWARD = 1;
    private const int ANIM_RUN_BACK = 2;
    private const int ANIM_RUN_LEFT = 3;
    private const int ANIM_RUN_RIGHT = 4;
    private const int ANIM_ATTACK_STAB = 5;
    private const int ANIM_ATTACK_FULL = 6;
    private const int ANIM_DAMAGE = 7;
    private const int ANIM_DEATH = 8;

    private const string IDLE = "Combat (1)";
    private const string RUN_FORWARD = "run force";
    private const string RUN_BACK = "run back";
    private const string RUN_LEFT = "run left";
    private const string RUN_RIGHT = "run right";
    private const string ATTACK_STAB = "sword sasu";
    private const string ATTACK_FULL = "sword attack";
    private const string DAMAGE = "Take Damage";
    private const string DEATH = "Death 01";

    private bool HasLocalControl
    {
        get
        {
            return Object != null && Object.HasStateAuthority;
        }
    }

    public override void Spawned()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        SetupCamera();

        if (HasLocalControl)
        {
            DamageCount = 0;
            IsDead = false;
            ActionTimer = TickTimer.None;
            ChangeAnim(ANIM_IDLE);
        }
    }

    private void Update()
    {
        if (!HasLocalControl)
        {
            return;
        }

        if (IsDead)
        {
            return;
        }

        if (!ActionTimer.IsRunning)
        {
            UpdateCameraPitch();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            damageRequested = true;
            return;
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            stabRequested = true;
            return;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            fullAttackRequested = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasLocalControl)
        {
            return;
        }

        if (controller == null)
        {
            return;
        }

        float deltaTime = Runner.DeltaTime;

        if (IsDead)
        {
            ClearRequests();
            MoveWithGravity(Vector3.zero, deltaTime);
            ChangeAnim(ANIM_DEATH);
            return;
        }

        if (ActionTimer.IsRunning)
        {
            if (ActionTimer.Expired(Runner))
            {
                ActionTimer = TickTimer.None;
                ChangeAnim(ANIM_IDLE);
            }
            else
            {
                ClearRequests();
                MoveWithGravity(Vector3.zero, deltaTime);
                return;
            }
        }

        if (damageRequested)
        {
            ClearRequests();
            TakeDamage();
            MoveWithGravity(Vector3.zero, deltaTime);
            return;
        }

        if (stabRequested)
        {
            ClearRequests();
            StartAction(ANIM_ATTACK_STAB, attackTime);
            MoveWithGravity(Vector3.zero, deltaTime);
            return;
        }

        if (fullAttackRequested)
        {
            ClearRequests();
            StartAction(ANIM_ATTACK_FULL, attackTime);
            MoveWithGravity(Vector3.zero, deltaTime);
            return;
        }

        RotateCharacter(deltaTime);
        MoveCharacter(deltaTime);
    }

    public override void Render()
    {
        PlayAnimById(NetworkAnimId);
    }

    private void MoveCharacter(float deltaTime)
    {
        float horizontal = 0.0f;
        float vertical = 0.0f;

        if (Input.GetKey(KeyCode.A))
        {
            horizontal -= 1.0f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            horizontal += 1.0f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            vertical += 1.0f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            vertical -= 1.0f;
        }

        Vector3 moveDirection =
            transform.right * horizontal +
            transform.forward * vertical;

        if (moveDirection.sqrMagnitude > 1.0f)
        {
            moveDirection.Normalize();
        }

        int nextAnim = GetMoveAnimation(horizontal, vertical);

        MoveWithGravity(moveDirection, deltaTime);
        ChangeAnim(nextAnim);
    }

    private void MoveWithGravity(
        Vector3 moveDirection,
        float deltaTime
    )
    {
        if (controller.isGrounded && verticalVelocity < 0.0f)
        {
            verticalVelocity = -1.0f;
        }
        else
        {
            verticalVelocity += gravity * deltaTime;
        }

        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * deltaTime);
    }

    private int GetMoveAnimation(
        float horizontal,
        float vertical
    )
    {
        if (Mathf.Abs(vertical) >= Mathf.Abs(horizontal))
        {
            if (vertical > 0.0f)
            {
                return ANIM_RUN_FORWARD;
            }

            if (vertical < 0.0f)
            {
                return ANIM_RUN_BACK;
            }
        }
        else
        {
            if (horizontal < 0.0f)
            {
                return ANIM_RUN_LEFT;
            }

            if (horizontal > 0.0f)
            {
                return ANIM_RUN_RIGHT;
            }
        }

        return ANIM_IDLE;
    }

    private void RotateCharacter(float deltaTime)
    {
        float rotateInput = 0.0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rotateInput -= 1.0f;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            rotateInput += 1.0f;
        }

        if (Mathf.Abs(rotateInput) > 0.01f)
        {
            transform.Rotate(
                0.0f,
                rotateInput * rotateSpeed * deltaTime,
                0.0f
            );
        }
    }

    private void UpdateCameraPitch()
    {
        if (viewCamera == null)
        {
            return;
        }

        float lookInput = 0.0f;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            lookInput -= 1.0f;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            lookInput += 1.0f;
        }

        cameraPitch +=
            lookInput *
            cameraLookSpeed *
            Time.deltaTime;

        cameraPitch = Mathf.Clamp(
            cameraPitch,
            minCameraAngle,
            maxCameraAngle
        );

        viewCamera.localRotation =
            Quaternion.Euler(cameraPitch, 0.0f, 0.0f);
    }

    private void StartAction(
        int animId,
        float duration
    )
    {
        ActionTimer = TickTimer.CreateFromSeconds(
            Runner,
            duration
        );

        ChangeAnim(animId);
    }

    private void TakeDamage()
    {
        DamageCount++;

        if (DamageCount >= 3)
        {
            IsDead = true;
            ActionTimer = TickTimer.None;
            ChangeAnim(ANIM_DEATH);
            return;
        }

        StartAction(ANIM_DAMAGE, damageTime);
    }

    private void ChangeAnim(int animId)
    {
        if (!HasLocalControl)
        {
            return;
        }

        if (NetworkAnimId != animId)
        {
            NetworkAnimId = animId;
        }

        PlayAnimById(animId);
    }

    private void PlayAnimById(int animId)
    {
        if (animator == null)
        {
            return;
        }

        if (currentAnimId == animId)
        {
            return;
        }

        currentAnimId = animId;

        string stateName = GetAnimName(animId);

        animator.CrossFade(
            stateName,
            0.1f,
            0
        );
    }

    private string GetAnimName(int animId)
    {
        switch (animId)
        {
            case ANIM_RUN_FORWARD:
                return RUN_FORWARD;

            case ANIM_RUN_BACK:
                return RUN_BACK;

            case ANIM_RUN_LEFT:
                return RUN_LEFT;

            case ANIM_RUN_RIGHT:
                return RUN_RIGHT;

            case ANIM_ATTACK_STAB:
                return ATTACK_STAB;

            case ANIM_ATTACK_FULL:
                return ATTACK_FULL;

            case ANIM_DAMAGE:
                return DAMAGE;

            case ANIM_DEATH:
                return DEATH;

            case ANIM_IDLE:
            default:
                return IDLE;
        }
    }

    private void SetupCamera()
    {
        Camera[] cameras =
            GetComponentsInChildren<Camera>(true);

        AudioListener[] listeners =
            GetComponentsInChildren<AudioListener>(true);

        if (!HasLocalControl)
        {
            foreach (Camera cameraComponent in cameras)
            {
                cameraComponent.enabled = false;
            }

            foreach (AudioListener listener in listeners)
            {
                listener.enabled = false;
            }

            return;
        }

        if (viewCamera == null && cameras.Length > 0)
        {
            viewCamera = cameras[0].transform;
        }
    }

    private void ClearRequests()
    {
        damageRequested = false;
        stabRequested = false;
        fullAttackRequested = false;
    }
}