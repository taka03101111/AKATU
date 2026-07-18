using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class player_move_com : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 8.0f;
    public float backSpeed = 5.0f;
    public float rotateSpeed = 180.0f;
    public float gravity = -9.8f;

    [Header("カメラ設定")]
    public Transform viewCamera;

    [Header("アニメーション時間")]
    public float attackTime = 1.0f;
    public float damageTime = 0.8f;

    [Header("アニメーション名")]
    public string idleAnim = "Combat (1)";
    public string runForwardAnim = "run force";
    public string runBackAnim = "run back";
    public string attackStabAnim = "sword sasu";
    public string attackFullAnim = "sword attack";
    public string damageAnim = "Take Damage";
    public string deathAnim = "Death 01";

    private Animator animator;
    private CharacterController controller;
    private SwordAttack swordAttack;

    private int damageCount = 0;
    private bool isAction = false;
    private bool isDead = false;

    private float verticalVelocity = 0.0f;

    private string currentAnim = "";

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        swordAttack = GetComponent<SwordAttack>();

        if (viewCamera == null)
        {
            Transform cameraPoint = transform.Find("CameraPoint");

            if (cameraPoint != null)
            {
                viewCamera = cameraPoint;
            }
        }

        PlayAnim(idleAnim);
    }

    void Update()
    {
        if (isDead)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage();
            return;
        }

        if (isAction)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (swordAttack != null)
            {
                swordAttack.TryAttack();
            }

            StartCoroutine(ActionAnimation(attackFullAnim, attackTime));
            return;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (swordAttack != null)
            {
                swordAttack.TryAttack();
            }

            StartCoroutine(ActionAnimation(attackStabAnim, attackTime));
            return;
        }

        MoveControl();
    }

    void MoveControl()
    {
        float moveInput = 0.0f;
        float rotateInput = 0.0f;

        if (Input.GetKey(KeyCode.W))
        {
            moveInput = 1.0f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveInput = -1.0f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            rotateInput = -1.0f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rotateInput = 1.0f;
        }

        transform.Rotate(0, rotateInput * rotateSpeed * Time.deltaTime, 0);

        string nextAnim = idleAnim;

        if (moveInput > 0)
        {
            nextAnim = runForwardAnim;
        }
        else if (moveInput < 0)
        {
            nextAnim = runBackAnim;
        }

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -1.0f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        float speed = moveInput >= 0 ? moveSpeed : backSpeed;

        Vector3 velocity = transform.forward * moveInput * speed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        PlayAnim(nextAnim);
    }

    void TakeDamage()
    {
        damageCount++;

        if (damageCount >= 3)
        {
            isDead = true;
            isAction = true;
            PlayAnim(deathAnim);
        }
        else
        {
            StartCoroutine(ActionAnimation(damageAnim, damageTime));
        }
    }

    IEnumerator ActionAnimation(string animName, float waitTime)
    {
        isAction = true;

        PlayAnim(animName);

        yield return new WaitForSeconds(waitTime);

        if (isDead)
        {
            yield break;
        }

        isAction = false;
        PlayAnim(idleAnim);
    }

    void PlayAnim(string animName)
    {
        if (animator == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(animName))
        {
            return;
        }

        if (currentAnim == animName)
        {
            return;
        }

        currentAnim = animName;
        animator.CrossFade(animName, 0.1f);
    }
}