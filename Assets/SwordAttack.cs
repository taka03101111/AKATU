using System.Collections;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public Transform attackPoint;

    public float attackRange = 2.0f;
    public int attackDamage = 1;
    public float attackCoolTime = 0.5f;

    private bool isAttacking = false;

    public void TryAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        Attack();

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