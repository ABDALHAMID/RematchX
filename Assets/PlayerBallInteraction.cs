using UnityEngine;

public class PlayerBallInteraction : MonoBehaviour
{
    public Transform ballAttachPoint;
    public float kickForce = 50f;
    public float passForce = 6f;
    public KeyCode kickKey = KeyCode.Space;
    public KeyCode passKey = KeyCode.E;

    private BallController currentBall;

    void OnTriggerEnter(Collider other)
    {
        BallController ball = other.GetComponent<BallController>();
        if (ball && currentBall == null)
        {
            currentBall = ball;
            ball.TakeControl(transform, ballAttachPoint);
        }
    }

    void Update()
    {
        if (!currentBall) return;

        if (Input.GetKeyDown(kickKey))
        {
            Vector3 dir = transform.forward + Vector3.up * 0.3f;
            currentBall.ReleaseControl(dir * kickForce);
            currentBall = null;
        }

        else if (Input.GetKeyDown(passKey))
        {
            Vector3 dir = transform.forward;
            currentBall.ReleaseControl(dir * passForce);
            currentBall = null;
        }
    }
}
