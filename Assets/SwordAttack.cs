using System.Collections;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public Transform sword;
    public Transform attackPoint;

    public float attackRange = 2.0f;
    public int attackDamage = 1;
    public float attackCoolTime = 0.5f;

    private bool isAttacking = false;
    private Quaternion defaultRotation;

    void Start()
    {
        if (sword == null)
        {
            sword = transform.Find("CameraPoint/Sword");
        }

        if (sword == null)
        {
            Debug.LogError("Swordが見つかりません。CameraPointの子にSwordがあるか確認してください。");
            enabled = false;
            return;
        }

        defaultRotation = sword.localRotation;
        Debug.Log("SwordAttack準備完了");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            TryAttack();
        }
    }

    public void TryAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(SwingSword());
        }
    }

    IEnumerator SwingSword()
    {
        isAttacking = true;

        Attack();

        Quaternion attackRotation = defaultRotation * Quaternion.Euler(120, 0, 0);

        float time = 0.0f;
        float duration = 0.12f;

        while (time < duration)
        {
            sword.localRotation = Quaternion.Slerp(defaultRotation, attackRotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0.0f;

        while (time < duration)
        {
            sword.localRotation = Quaternion.Slerp(attackRotation, defaultRotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        sword.localRotation = defaultRotation;

        yield return new WaitForSeconds(attackCoolTime);

        isAttacking = false;
    }

    void Attack()
    {
        Vector3 center;

        if (attackPoint != null)
        {
            center = attackPoint.position;
        }
        else
        {
            center = transform.position + transform.forward * 1.5f;
        }

        Collider[] hitColliders = Physics.OverlapSphere(center, attackRange);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                PlayerHealth health = hitCollider.GetComponent<PlayerHealth>();

                if (health != null)
                {
                    health.TakeDamage(attackDamage);
                    Debug.Log(hitCollider.name + " に攻撃が当たった");
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center;

        if (attackPoint != null)
        {
            center = attackPoint.position;
        }
        else
        {
            center = transform.position + transform.forward * 1.5f;
        }

        Gizmos.DrawWireSphere(center, attackRange);
    }
}