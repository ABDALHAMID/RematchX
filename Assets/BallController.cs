using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    public float maxSpeed = 20f;
    public float dragWhenControlled = 5f;
    public Transform playerAttachPoint;

    private Rigidbody rb;
    private bool isControlled = false;
    private Transform controllingPlayer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (isControlled && controllingPlayer)
        {
            Vector3 targetPos = playerAttachPoint.position;
            Vector3 direction = targetPos - transform.position;
            rb.linearVelocity = direction * 10f;

            rb.linearDamping = dragWhenControlled;
        }
        else
        {
            rb.linearDamping = 0.5f;
            if (rb.linearVelocity.magnitude > maxSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    public void TakeControl(Transform player, Transform attachPoint)
    {
        controllingPlayer = player;
        playerAttachPoint = attachPoint;
        isControlled = true;
    }

    public void ReleaseControl(Vector3 kickForce)
    {
        isControlled = false;
        rb.AddForce(kickForce, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision other)
    {
        if (isControlled && other.relativeVelocity.magnitude > 3f)
        {
            isControlled = false;
        }
    }
}
