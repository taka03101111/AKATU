using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPTextUI : MonoBehaviour
{
    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private TextMeshProUGUI hpText;

    [SerializeField]
    private Image hpFillImage;

    public void SetPlayerHealth(PlayerHealth health)
    {
        playerHealth = health;
        RefreshUI();
    }

    private void Update()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (playerHealth == null)
        {
            return;
        }

        int currentHP = playerHealth.currentHP;
        int maxHP = playerHealth.MaxHP;

        if (hpText != null)
        {
            hpText.text =
                "Player HP : " +
                currentHP +
                " / " +
                maxHP;
        }

        if (hpFillImage != null)
        {
            float hpRate = 0.0f;

            if (maxHP > 0)
            {
                hpRate =
                    (float)currentHP /
                    (float)maxHP;
            }

            hpFillImage.fillAmount =
                Mathf.Clamp01(hpRate);
        }
    }
}