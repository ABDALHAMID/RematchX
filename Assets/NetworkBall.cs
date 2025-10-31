using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody), typeof(NetworkObject))]
public class NetworkBall : NetworkBehaviour
{
    // Controller: NetworkObjectId (0 = nobody)
    public NetworkVariable<ulong> controllerNetId = new(
        0UL,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // Owner (client) can write position/rotation for smooth replication
    public NetworkVariable<Vector3> netPosition = new(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<Quaternion> netRotation = new(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    [Header("Follow / smoothing")]
    public float followLerp = 25f;
    public float positionSendRate = 30f; // times/sec the owner writes netPosition

    Rigidbody rb;
    float sendTimer = 0f;
    ulong currentControllerCached = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        controllerNetId.OnValueChanged += OnControllerChanged;
        netPosition.OnValueChanged += OnNetPosChanged;
        netRotation.OnValueChanged += OnNetRotChanged;

        // initialize net vars on spawn
        if (IsServer)
        {
            netPosition.Value = transform.position;
            netRotation.Value = transform.rotation;
        }
    }

    void OnDestroy()
    {
        controllerNetId.OnValueChanged -= OnControllerChanged;
        netPosition.OnValueChanged -= OnNetPosChanged;
        netRotation.OnValueChanged -= OnNetRotChanged;
    }

    void OnControllerChanged(ulong oldId, ulong newId)
    {
        // Cache to speed up checks in Update
        currentControllerCached = newId;

        if (newId == 0)
        {
            // Released: enable physics on everyone (server will apply velocity on release)
            rb.isKinematic = false;
        }
        else
        {
            // Controlled: server or owner will set kinematic accordingly.
            // If this client is now owner, we'll set kinematic locally in Update loop.
        }
    }

    void OnNetPosChanged(Vector3 oldPos, Vector3 newPos)
    {
        // non-owners will smoothly interpolate to this position in Update()
    }

    void OnNetRotChanged(Quaternion oldRot, Quaternion newRot)
    {
        // non-owners interpolate for rotation
    }

    void Update()
    {
        // Owner writes netPosition periodically
        if (IsOwner && currentControllerCached != 0)
        {
            // When owning the NetworkObject, but only the controlling player should write position.
            // We'll still allow owner to write; Player script ensures only controller gets ownership.
            sendTimer += Time.deltaTime;
            if (sendTimer >= 1f / positionSendRate)
            {
                sendTimer = 0f;
                // write position/rotation (owner has write permission)
                netPosition.Value = transform.position;
                netRotation.Value = transform.rotation;
            }
        }
        else
        {
            // Non-owner: smooth to networked position
            if (!IsOwner)
            {
                // lerp to netPosition
                transform.position = Vector3.Lerp(transform.position, netPosition.Value, Mathf.Clamp01(followLerp * Time.deltaTime));
                transform.rotation = Quaternion.Slerp(transform.rotation, netRotation.Value, Mathf.Clamp01(followLerp * Time.deltaTime));
            }
        }
    }

    #region Server-Control API (RPCs)

    // Called by a client to request taking control. Allow non-owner to call.
    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeControlServerRpc(ulong playerNetId, ServerRpcParams rpcParams = default)
    {
        // SERVER VALIDATION:
        // - playerNetId must map to a spawned NetworkObject
        // - that object must be owned by the calling client (rpcParams) — prevents stealing.
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetId, out var playerNetObj))
        {
            Debug.LogWarning("RequestTakeControl: invalid playerNetId");
            return;
        }

        // Validate the caller actually owns playerNetObj
        if (playerNetObj.OwnerClientId != rpcParams.Receive.SenderClientId)
        {
            Debug.LogWarning("RequestTakeControl: caller does not own the player object");
            return;
        }

        // If ball already controlled -> reject or force release first
        if (controllerNetId.Value != 0)
        {
            Debug.LogWarning("RequestTakeControl: ball already controlled");
            return;
        }

        // Assign controller
        controllerNetId.Value = playerNetId;

        // Transfer ownership of this NetworkObject to the player's client so they can write netPosition
        NetworkObject.ChangeOwnership(playerNetObj.OwnerClientId);
        // Make ball kinematic on server to avoid conflicts (owner client will also set kinematic locally)
        rb.isKinematic = true;
    }

    // Called by the owner to release the ball with a velocity (kick/pass)
    [ServerRpc(RequireOwnership = false)]
    public void ReleaseServerRpc(Vector3 worldVelocity, ServerRpcParams rpcParams = default)
    {
        // Validate: only the controller's owner can release
        if (controllerNetId.Value == 0)
        {
            Debug.LogWarning("ReleaseServerRpc called but ball not controlled");
            return;
        }

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(controllerNetId.Value, out var controllerObj))
        {
            Debug.LogWarning("ReleaseServerRpc: controller object not found");
            return;
        }

        if (controllerObj.OwnerClientId != rpcParams.Receive.SenderClientId)
        {
            Debug.LogWarning("ReleaseServerRpc: caller is not the controller owner");
            return;
        }

        // Clear controller
        controllerNetId.Value = 0;

        // Transfer ownership back to server (server-client id = 0)
        NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);

        // Apply velocity on the server (authoritative)
        rb.isKinematic = false;
        rb.linearVelocity = worldVelocity;
        // Optionally set angular velocity, add impulse, etc.
    }

    #endregion
}
