using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_move : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 3.0f;
    public float rotateSpeed = 10.0f;

    [Header("Animator")]
    public Animator animator;

    [Header("体力設定")]
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

    void Start()
    {
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

        // テスト用：Hキーでダメージ
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage();
        }

        // 攻撃中・ダメージ中は、終わるまで移動しない
        if (isAttacking || isDamaging)
        {
            CheckActionEnd();
            return;
        }

        // 攻撃
        if (Input.GetKeyDown(KeyCode.J))
        {
            Attack(Attack1);
            return;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Attack(Attack2);
            return;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Attack(Attack3);
            return;
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Attack(Attack4);
            return;
        }

        // 移動
        Move();
    }

    void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A D
        float vertical = Input.GetAxisRaw("Vertical");     // W S

        Vector3 move = new Vector3(horizontal, 0, vertical).normalized;

        if (move.magnitude > 0.1f)
        {
            transform.position += move * moveSpeed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );

            if (Mathf.Abs(vertical) >= Mathf.Abs(horizontal))
            {
                if (vertical > 0)
                {
                    PlayAnimation(RunForward);
                }
                else
                {
                    PlayAnimation(RunBack);
                }
            }
            else
            {
                if (horizontal > 0)
                {
                    PlayAnimation(RunRight);
                }
                else
                {
                    PlayAnimation(RunLeft);
                }
            }
        }
        else
        {
            PlayAnimation(Idle);
        }
    }

    void Attack(string attackStateName)
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

        // 今再生しているアクションが終わったか確認
        if (stateInfo.IsName(currentActionState) &&
            stateInfo.normalizedTime >= 0.95f &&
            !animator.IsInTransition(0))
        {
            isAttacking = false;
            isDamaging = false;
            currentActionState = "";

            // 攻撃・ダメージが終わったら、行動アクションに戻る
            PlayAnimation(Idle);
        }
    }

    void PlayAnimation(string stateName)
    {
        if (animator == null) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // 同じアニメーションを毎フレーム再生し直さない
        if (!stateInfo.IsName(stateName))
        {
            animator.CrossFade(stateName, 0.15f);
        }
    }
}