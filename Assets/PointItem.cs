using UnityEngine;

public class PointItem : MonoBehaviour
{
    public GameRuleManager gameRuleManager;

    public float rotateSpeed = 90.0f;
    public float floatHeight = 0.25f;
    public float floatSpeed = 2.0f;

    private Vector3 startPosition;
    private bool collected = false;
    private Collider pointCollider;

    void Start()
    {
        startPosition = transform.position;
        pointCollider = GetComponent<Collider>();
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);

        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = startPosition + new Vector3(0, yOffset, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            if (gameRuleManager != null)
            {
                gameRuleManager.GetPoint(this);
            }
        }
    }

    public void Collect()
    {
        if (collected)
        {
            return;
        }

        collected = true;

        if (pointCollider != null)
        {
            pointCollider.enabled = false;
        }

        Debug.Log(gameObject.name + " を取得しました");
    }

    public bool IsCollected()
    {
        return collected;
    }
}