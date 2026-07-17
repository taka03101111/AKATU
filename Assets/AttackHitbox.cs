using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class AttackHitbox : MonoBehaviour
{
    [SerializeField]
    private PlayerHealth ownerHealth;

    private BoxCollider hitboxCollider;
    private Rigidbody hitboxRigidbody;

    private readonly HashSet<PlayerHealth> hitTargets =
        new HashSet<PlayerHealth>();

    private bool attackActive;
    private int attackDamage;

    private void Awake()
    {
        hitboxCollider = GetComponent<BoxCollider>();
        hitboxRigidbody = GetComponent<Rigidbody>();

        hitboxCollider.isTrigger = true;
        hitboxCollider.enabled = false;

        hitboxRigidbody.useGravity = false;
        hitboxRigidbody.isKinematic = true;

        if (ownerHealth == null)
        {
            ownerHealth = GetComponentInParent<PlayerHealth>();
        }
    }

    public void BeginAttack(int damage)
    {
        if (ownerHealth == null)
        {
            return;
        }

        if (ownerHealth.Object == null)
        {
            return;
        }

        if (!ownerHealth.Object.HasStateAuthority)
        {
            return;
        }

        attackDamage = Mathf.Max(1, damage);
        attackActive = true;

        hitTargets.Clear();
        hitboxCollider.enabled = true;
    }

    public void EndAttack()
    {
        attackActive = false;
        hitTargets.Clear();

        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        if (!attackActive)
        {
            return;
        }

        if (ownerHealth == null)
        {
            return;
        }

        if (ownerHealth.Object == null)
        {
            return;
        }

        if (!ownerHealth.Object.HasStateAuthority)
        {
            return;
        }

        Hurtbox hurtbox = other.GetComponent<Hurtbox>();

        if (hurtbox == null)
        {
            return;
        }

        PlayerHealth targetHealth = hurtbox.OwnerHealth;

        if (targetHealth == null)
        {
            return;
        }

        if (targetHealth == ownerHealth)
        {
            return;
        }

        if (targetHealth.Object == null)
        {
            return;
        }

        if (!hitTargets.Add(targetHealth))
        {
            return;
        }

        targetHealth.RpcTakeDamage(attackDamage);

        Debug.Log(
            "Attack hit: " +
            targetHealth.gameObject.name +
            " Damage: " +
            attackDamage
        );
    }

    private void OnDisable()
    {
        attackActive = false;
        hitTargets.Clear();
    }
}