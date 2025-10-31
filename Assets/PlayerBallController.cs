using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkObject))]
public class PlayerBallController : NetworkBehaviour
{
    [Header("Ball / attach")]
    public Transform ballAttachPoint; // e.g. small empty under feet
    public float pickupRadius = 1.2f;

    [Header("Kick/Pass")]
    public float kickImpulse = 9f;
    public float passImpulse = 6f;
    public InputActionProperty kickAction; // assign via inspector or use PlayerInput binding
    public InputActionProperty passAction;

    NetworkBall nearbyBall;
    bool hasLocalControl = false; // whether this player is currently the controller locally

    void OnEnable()
    {
        // If using InputActionProperty fields, enable them
        if (kickAction.action != null) kickAction.action.Enable();
        if (passAction.action != null) passAction.action.Enable();
    }

    void OnDisable()
    {
        if (kickAction.action != null) kickAction.action.Disable();
        if (passAction.action != null) passAction.action.Disable();
    }

    void Update()
    {
        if (!IsOwner) return; // only local player reads input

        // allow taking control when close and press kick/pass (you can change to better pickup logic)
        if (nearbyBall != null && !hasLocalControl)
        {
            // Auto-request control when in contact and pressing kick OR pass (you may want a distinct pickup button)
            if (kickAction.action != null && kickAction.action.WasPressedThisFrame())
            {
                // Request control (server will transfer ownership)
                nearbyBall.RequestTakeControlServerRpc(this.NetworkObjectId);
                // Once server grants, the ball's NetworkObject will change owner; we detect that in NetworkBall.OnControllerChanged
                hasLocalControl = true; // optimistic local flag; final authority is server
            }

            if (passAction.action != null && passAction.action.WasPressedThisFrame())
            {
                nearbyBall.RequestTakeControlServerRpc(this.NetworkObjectId);
                hasLocalControl = true;
            }
        }

        // If we are controller/owner, provide local follow behavior (owner writes netPosition in NetworkBall)
        // Ownership transfer gives us ownership of the ball NetworkObject (NetworkObject.IsOwner on ball).
        // So we detect this by checking the ball's NetworkObject owner.
        if (nearbyBall != null && nearbyBall.NetworkObject != null)
        {
            // if this player's NetworkObjectId equals the ball.controllerNetId, and we are now owner of the ball,
            // we should locally snap/attach the ball to the attach point while kinematic.
            if (nearbyBall.controllerNetId.Value == this.NetworkObjectId && nearbyBall.IsOwner)
            {
                // local follow (set transform directly)
                Transform ballT = nearbyBall.transform;
                Rigidbody ballRb = nearbyBall.GetComponent<Rigidbody>();
                ballRb.isKinematic = true;
                ballT.position = ballAttachPoint.position;
                ballT.rotation = ballAttachPoint.rotation;

                // Kick input (release with velocity)
                if (kickAction.action != null && kickAction.action.WasPressedThisFrame())
                {
                    Vector3 releaseVel = (transform.forward + Vector3.up * 0.25f).normalized * kickImpulse;
                    nearbyBall.ReleaseServerRpc(releaseVel);
                    hasLocalControl = false;
                }

                // Pass input (we'll just shoot forward — you should modify to target teammate)
                if (passAction.action != null && passAction.action.WasPressedThisFrame())
                {
                    Vector3 releaseVel = (transform.forward + Vector3.up * 0.05f).normalized * passImpulse;
                    nearbyBall.ReleaseServerRpc(releaseVel);
                    hasLocalControl = false;
                }
            }
        }
    }

    // Simple proximity detection - you can replace this with trigger colliders for reliability
    void FixedUpdate()
    {
        if (!IsOwner) return;

        // Find closest ball in radius
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius);
        NetworkBall found = null;
        float best = float.MaxValue;
        foreach (var c in hits)
        {
            var nb = c.GetComponent<NetworkBall>();
            if (nb)
            {
                float d = Vector3.Distance(transform.position, nb.transform.position);
                if (d < best)
                {
                    best = d;
                    found = nb;
                }
            }
        }

        nearbyBall = found;
    }

    // Optional gizmo
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
        if (ballAttachPoint) Gizmos.DrawSphere(ballAttachPoint.position, 0.05f);
    }
}
