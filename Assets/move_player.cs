using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move_player : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 0.15f;
    public float rotateSpeed = 3.0f;

    [Header("Animator")]
    public Animator animator;

    [Header("ダメージ設定")]
    public int maxDamageCount = 5;
    private int damageCount = 0;

    // Animatorのステート名
    private const string Idle = "武器を以て立っている状態";

    private const string RunForward = "前に走る";
    private const string RunBack = "後ろに走る";
    private const string RunLeft = "左に走る";
    private const string RunRight = "右に走る";

    private const string Attack1 = "sasi_attack";
    private const string Attack2 = "hidarihur_attack";
    private const string Attack3 = "HumanM@AttackPolearm01";
    private const string Attack4 = "huru_attack";

    private const string Damage = "Take Damage (1)";
    private const string Death = "Death 01";

    private bool isAttacking = false;
    private bool isDamaging = false;
    private bool isDead = false;

    private string currentActionState = "";

    private Vector3 beforePosition;

    void Start()
    {
        Application.targetFrameRate = 60;

        if (animator == null)
        {
            animator = GetComponent<Animator>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        PlayAnimation(Idle);
    }

    void Update()
    {
        if (isDead) return;

        // 攻撃中・ダメージ中は終わるまで待つ
        if (isAttacking || isDamaging)
        {
            CheckActionEnd();
            return;
        }

        // 攻撃
        if (Input.GetKeyDown(KeyCode.J))
        {
            StartAttack(Attack1);
            return;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            StartAttack(Attack2);
            return;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            StartAttack(Attack3);
            return;
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            StartAttack(Attack4);
            return;
        }

        // テスト用：Hキーでダメージ
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage();
            return;
        }

        Move();
    }

    void Move()
    {
        beforePosition = transform.position;

        bool isMoving = false;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * moveSpeed;
            PlayAnimation(RunForward);
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * moveSpeed;
            PlayAnimation(RunBack);
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0f, rotateSpeed, 0f);
            PlayAnimation(RunRight);
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0f, -rotateSpeed, 0f);
            PlayAnimation(RunLeft);
            isMoving = true;
        }

        if (!isMoving)
        {
            PlayAnimation(Idle);
        }
    }

    void StartAttack(string attackStateName)
    {
        if (isDead) return;
        if (isAttacking) return;
        if (isDamaging) return;

        isAttacking = true;
        currentActionState = attackStateName;

        PlayAnimation(attackStateName);
    }

    public void TakeDamage()
    {
        if (isDead) return;

        // 攻撃中はダメージアクションに行かない
        if (isAttacking) return;

        damageCount++;

        if (damageCount >= maxDamageCount)
        {
            Die();
            return;
        }

        isDamaging = true;
        currentActionState = Damage;

        PlayAnimation(Damage);
    }

    void Die()
    {
        isDead = true;
        isAttacking = false;
        isDamaging = false;

        currentActionState = Death;
        PlayAnimation(Death);
    }

    void CheckActionEnd()
    {
        if (animator == null) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(currentActionState) &&
            stateInfo.normalizedTime >= 0.95f &&
            !animator.IsInTransition(0))
        {
            isAttacking = false;
            isDamaging = false;
            currentActionState = "";

            // 攻撃・ダメージが終わったら行動状態に戻す
            PlayAnimation(Idle);
        }
    }

    void PlayAnimation(string stateName)
    {
        if (animator == null) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName(stateName))
        {
            animator.CrossFade(stateName, 0.15f);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (isDead) return;

        Debug.Log("object = " + other.gameObject.name);

        // 床との接触ではダメージにしない
        if (other.gameObject.name.Contains("床"))
        {
            return;
        }

        // 壁などにめり込まないように戻す
        transform.position = beforePosition;

        // 攻撃中以外ならダメージアクション
        if (!isAttacking)
        {
            TakeDamage();
        }
    }
}