using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float viewRotateSpeed = 90.0f;
    public float resetSpeed = 5.0f;

    private CharacterController controller;
    private float verticalVelocity = 0.0f;
    private float gravity = -9.8f;

    private Quaternion defaultRotation;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        defaultRotation = transform.rotation;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ViewControl();
        }
        else
        {
            Move();
        }
    }

    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move = move.normalized;

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -1.0f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void ViewControl()
    {
        float x = Input.GetAxisRaw("Horizontal");

        transform.Rotate(Vector3.up * x * viewRotateSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.W))
        {
            transform.rotation = defaultRotation;
        }
    }
}