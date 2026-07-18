using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(PlayerHealth))]
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

    [Header("Attack Hitbox")]
    [SerializeField] private AttackHitbox attackHitbox;

    [SerializeField] private int stabDamage = 25;
    [SerializeField] private int fullAttackDamage = 35;

    [SerializeField] private float stabHitDelay = 0.2f;
    [SerializeField] private float stabHitDuration = 0.3f;

    [SerializeField] private float fullAttackHitDelay = 0.35f;
    [SerializeField] private float fullAttackHitDuration = 0.4f;

    [Header("Debug")]
    [SerializeField] private int testDamage = 25;

    private CharacterController controller;
    private Animator animator;
    private PlayerHealth playerHealth;

    private float verticalVelocity;
    private float cameraPitch;

    private int currentAnimId = -1;

    private bool stabRequested;
    private bool fullAttackRequested;

    private Coroutine attackHitboxRoutine;

    [Networked]
    private int NetworkAnimId { get; set; }

    [Networked]
    private TickTimer ActionTimer { get; set; }

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
            return Object != null &&
                   Object.HasStateAuthority;
        }
    }

    public override void Spawned()
    {
        controller =
            GetComponent<CharacterController>();

        animator =
            GetComponentInChildren<Animator>();

        playerHealth =
            GetComponent<PlayerHealth>();

        if (attackHitbox == null)
        {
            attackHitbox =
                GetComponentInChildren<AttackHitbox>(true);
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        if (attackHitbox != null)
        {
            attackHitbox.EndAttack();
        }

        SetupCamera();

        if (HasLocalControl)
        {
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

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            return;
        }

        if (!ActionTimer.IsRunning)
        {
            UpdateCameraPitch();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (playerHealth != null)
            {
                playerHealth.RpcTakeDamage(testDamage);
            }

            return;
        }

        if (ActionTimer.IsRunning)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            stabRequested = true;
            return;
        }

        if (Input.GetKeyDown(KeyCode.J))
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

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            ClearRequests();
            StopAttackHitbox();

            MoveWithGravity(
                Vector3.zero,
                deltaTime
            );

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

                MoveWithGravity(
                    Vector3.zero,
                    deltaTime
                );

                return;
            }
        }

        if (stabRequested)
        {
            ClearRequests();

            StartAttack(
                ANIM_ATTACK_STAB,
                attackTime,
                stabDamage,
                stabHitDelay,
                stabHitDuration
            );

            MoveWithGravity(
                Vector3.zero,
                deltaTime
            );

            return;
        }

        if (fullAttackRequested)
        {
            ClearRequests();

            StartAttack(
                ANIM_ATTACK_FULL,
                attackTime,
                fullAttackDamage,
                fullAttackHitDelay,
                fullAttackHitDuration
            );

            MoveWithGravity(
                Vector3.zero,
                deltaTime
            );

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

        int nextAnim =
            GetMoveAnimation(
                horizontal,
                vertical
            );

        MoveWithGravity(
            moveDirection,
            deltaTime
        );

        ChangeAnim(nextAnim);
    }

    private void MoveWithGravity(
        Vector3 moveDirection,
        float deltaTime
    )
    {
        if (controller.isGrounded &&
            verticalVelocity < 0.0f)
        {
            verticalVelocity = -1.0f;
        }
        else
        {
            verticalVelocity +=
                gravity * deltaTime;
        }

        Vector3 velocity =
            moveDirection * moveSpeed;

        velocity.y = verticalVelocity;

        controller.Move(
            velocity * deltaTime
        );
    }

    private int GetMoveAnimation(
        float horizontal,
        float vertical
    )
    {
        if (Mathf.Abs(vertical) >=
            Mathf.Abs(horizontal))
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

        transform.Rotate(
            0.0f,
            rotateInput * rotateSpeed * deltaTime,
            0.0f
        );
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
            Quaternion.Euler(
                cameraPitch,
                0.0f,
                0.0f
            );
    }

    private void StartAttack(
        int animId,
        float actionDuration,
        int damage,
        float hitDelay,
        float hitDuration
    )
    {
        StartAction(
            animId,
            actionDuration
        );

        StopAttackHitbox();

        if (attackHitbox != null)
        {
            attackHitboxRoutine =
                StartCoroutine(
                    AttackWindowRoutine(
                        damage,
                        hitDelay,
                        hitDuration
                    )
                );
        }
    }

    private IEnumerator AttackWindowRoutine(
        int damage,
        float delay,
        float duration
    )
    {
        if (delay > 0.0f)
        {
            yield return
                new WaitForSeconds(delay);
        }

        if (!HasLocalControl)
        {
            attackHitboxRoutine = null;
            yield break;
        }

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            attackHitboxRoutine = null;
            yield break;
        }

        attackHitbox.BeginAttack(damage);

        if (duration > 0.0f)
        {
            yield return
                new WaitForSeconds(duration);
        }

        attackHitbox.EndAttack();
        attackHitboxRoutine = null;
    }

    private void StartAction(
        int animId,
        float duration
    )
    {
        ActionTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                duration
            );

        ChangeAnim(animId);
    }

    public void ApplyDamageReaction(bool dead)
    {
        if (!HasLocalControl)
        {
            return;
        }

        StopAttackHitbox();
        ClearRequests();

        if (dead)
        {
            ActionTimer = TickTimer.None;
            ChangeAnim(ANIM_DEATH);
            return;
        }

        ActionTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                damageTime
            );

        ChangeAnim(ANIM_DAMAGE);
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

        animator.CrossFade(
            GetAnimName(animId),
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
            foreach (
                Camera cameraComponent
                in cameras
            )
            {
                cameraComponent.enabled = false;
            }

            foreach (
                AudioListener listener
                in listeners
            )
            {
                listener.enabled = false;
            }

            return;
        }

        if (viewCamera == null &&
            cameras.Length > 0)
        {
            viewCamera =
                cameras[0].transform;
        }
    }
    public void RequestStabAttack()
    {
        if (!HasLocalControl)
        {
            return;
        }

        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        if (ActionTimer.IsRunning)
        {
            return;
        }

        stabRequested = true;
    }

    public void RequestFullAttack()
    {
        if (!HasLocalControl)
        {
            return;
        }

        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        if (ActionTimer.IsRunning)
        {
            return;
        }

        fullAttackRequested = true;
    }
    private void ClearRequests()
    {
        stabRequested = false;
        fullAttackRequested = false;
    }

    private void StopAttackHitbox()
    {
        if (attackHitboxRoutine != null)
        {
            StopCoroutine(
                attackHitboxRoutine
            );

            attackHitboxRoutine = null;
        }

        if (attackHitbox != null)
        {
            attackHitbox.EndAttack();
        }
    }

    private void OnDisable()
    {
        StopAttackHitbox();
    }
}