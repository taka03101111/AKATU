using UnityEngine;

public class PointItem : MonoBehaviour
{
    public float rotateSpeed = 90.0f;
    public float floatHeight = 0.25f;
    public float floatSpeed = 2.0f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Rotate(
            Vector3.up * rotateSpeed * Time.deltaTime,
            Space.World
        );

        float yOffset =
            Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.position =
            startPosition + new Vector3(0.0f, yOffset, 0.0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerPointCounter pointCounter =
            other.GetComponent<PlayerPointCounter>();

        if (pointCounter == null)
        {
            pointCounter =
                other.GetComponentInParent<PlayerPointCounter>();
        }

        if (pointCounter == null)
        {
            Debug.LogWarning("PlayerPointCounterがPlayerに付いていません");
            return;
        }

        pointCounter.TryGetPoint(this);
    }
}