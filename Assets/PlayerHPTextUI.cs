using TMPro;
using UnityEngine;

public class PlayerHPTextUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public TextMeshProUGUI hpText;

    void Update()
    {
        if (playerHealth != null && hpText != null)
        {
            hpText.text = "Player HP : " + playerHealth.currentHP;
        }
    }
}