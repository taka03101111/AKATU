using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Hurtbox : MonoBehaviour
{
    [SerializeField]
    private PlayerHealth ownerHealth;

    public PlayerHealth OwnerHealth
    {
        get
        {
            return ownerHealth;
        }
    }

    private void Awake()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;

        if (ownerHealth == null)
        {
            ownerHealth = GetComponentInParent<PlayerHealth>();
        }
    }
}