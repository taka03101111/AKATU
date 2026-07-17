using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerHealth : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField]
    private int maxHP = 100;

    private PlayerMovement playerMovement;

    [Networked]
    public int CurrentHP { get; set; }

    [Networked]
    public NetworkBool IsDead { get; set; }

    public int currentHP
    {
        get
        {
            return CurrentHP;
        }
    }

    public int MaxHP
    {
        get
        {
            return maxHP;
        }
    }

    public override void Spawned()
    {
        playerMovement = GetComponent<PlayerMovement>();

        if (Object.HasStateAuthority)
        {
            CurrentHP = maxHP;
            IsDead = false;

            PlayerHPTextUI hpUI =
                FindObjectOfType<PlayerHPTextUI>();

            if (hpUI != null)
            {
                hpUI.SetPlayerHealth(this);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        if (Object == null)
        {
            Debug.LogError(
                "PlayerHealth is not attached to a NetworkObject."
            );

            return;
        }

        RpcTakeDamage(damage);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcTakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        if (IsDead)
        {
            return;
        }

        CurrentHP = Mathf.Max(
            0,
            CurrentHP - damage
        );

        if (CurrentHP <= 0)
        {
            IsDead = true;

            if (playerMovement != null)
            {
                playerMovement.ApplyDamageReaction(true);
            }

            Debug.Log(
                "Player dead. HP: " +
                CurrentHP
            );

            return;
        }

        if (playerMovement != null)
        {
            playerMovement.ApplyDamageReaction(false);
        }

        Debug.Log(
            "Player damage: " +
            damage +
            " HP: " +
            CurrentHP
        );
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        RpcHeal(amount);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcHeal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (IsDead)
        {
            return;
        }

        CurrentHP = Mathf.Min(
            maxHP,
            CurrentHP + amount
        );

        Debug.Log(
            "Player heal: " +
            amount +
            " HP: " +
            CurrentHP
        );
    }
}