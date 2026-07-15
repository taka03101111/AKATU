using UnityEngine;

public class SelfDamageTest : MonoBehaviour
{
    public PlayerHealth playerHealth;

    void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }
    }
}