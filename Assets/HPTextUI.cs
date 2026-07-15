using TMPro;
using UnityEngine;

public class HPTextUI : MonoBehaviour
{
    public PlayerHealth targetHealth;
    public TextMeshProUGUI hpText;

    void Update()
    {
        if (targetHealth != null && hpText != null)
        {
            hpText.text = "Enemy HP : " + targetHealth.currentHP;
        }
    }
}