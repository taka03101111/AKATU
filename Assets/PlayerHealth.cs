using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerHealth : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField]
    private int maxHP = 100;

    [Header("Respawn")]
    [SerializeField]
    private float respawnDelay = 2.0f;

    private PlayerMovement playerMovement;
    private Coroutine respawnRoutine;

    [Networked]
    public int CurrentHP { get; set; }

    [Networked]
    public NetworkBool IsDead { get; set; }

    public int MaxHP
    {
        get
        {
            return maxHP;
        }
    }

    public int currentHP
    {
        get
        {
            return CurrentHP;
        }
    }

    public override void Spawned()
    {
        playerMovement = GetComponent<PlayerMovement>();

        if (Object.HasStateAuthority)
        {
            CurrentHP = maxHP;
            IsDead = false;
        }

        if (Object.HasStateAuthority)
        {
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
            Debug.LogError("PlayerHealth is not attached to a NetworkObject.");
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
            Die();
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

    private void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        if (playerMovement != null)
        {
            playerMovement.ApplyDamageReaction(true);
        }

        Debug.Log("Player dead. HP: " + CurrentHP);

        if (respawnRoutine == null)
        {
            respawnRoutine =
                StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        Respawn();
    }

    private void Respawn()
    {
        if (!Object.HasStateAuthority)
        {
            respawnRoutine = null;
            return;
        }

        PlayerRespawnPoint respawnPoint =
            GetComponent<PlayerRespawnPoint>();

        if (respawnPoint != null)
        {
            respawnPoint.Respawn();
        }
        else
        {
            Debug.LogWarning("PlayerRespawnPointがありません。StartPointに戻れません。");
        }

        CurrentHP = maxHP;
        IsDead = false;

        respawnRoutine = null;

        Debug.Log("復活しました。HP: " + CurrentHP);
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