using TMPro;
using UnityEngine;

public class PlayerHPTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;

    private PlayerHealth targetHealth;

    void Start()
    {
        if (hpText == null)
        {
            hpText = GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        if (targetHealth == null)
        {
            FindLocalPlayerHealth();
        }

        UpdateHPText();
    }

    public void SetPlayerHealth(PlayerHealth playerHealth)
    {
        if (playerHealth == null)
        {
            return;
        }

        if (playerHealth.Object != null && !playerHealth.Object.HasStateAuthority)
        {
            return;
        }

        targetHealth = playerHealth;
        UpdateHPText();
    }

    private void FindLocalPlayerHealth()
    {
        PlayerHealth[] healths = FindObjectsOfType<PlayerHealth>();

        foreach (PlayerHealth health in healths)
        {
            if (health == null)
            {
                continue;
            }

            if (!health.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (health.Object != null && health.Object.HasStateAuthority)
            {
                targetHealth = health;
                return;
            }
        }
    }

    private void UpdateHPText()
    {
        if (hpText == null)
        {
            return;
        }

        if (targetHealth == null)
        {
            hpText.text = "HP : - / -";
            return;
        }

        hpText.text = "HP : " + targetHealth.CurrentHP + " / " + targetHealth.MaxHP;
    }
}