using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 3;
    public int currentHP;
    public Transform startPoint;

    private CharacterController controller;

    void Start()
    {
        currentHP = maxHP;
        controller = GetComponent<CharacterController>();
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        Debug.Log(gameObject.name + " HP: " + currentHP);

        if (currentHP <= 0)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        currentHP = maxHP;

        if (startPoint != null)
        {
            if (controller != null)
            {
                controller.enabled = false;
                transform.position = startPoint.position;
                controller.enabled = true;
            }
            else
            {
                transform.position = startPoint.position;
            }
        }

        Debug.Log(gameObject.name + " がスタート地点に戻りました");
    }
}