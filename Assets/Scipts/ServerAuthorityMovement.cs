using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ServerAuthorityMovement : NetworkBehaviour
{
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private NetworkVariable<float> positionX = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<float> positionY = new NetworkVariable<float>();

    private MultiPlayer playerInput;
    private float oldMoveXLocalVariable;
    private float oldMoveYLocalVariable;


    public override void OnNetworkSpawn()
    {
        playerInput = new MultiPlayer();
        playerInput.Enable();
        playerInput.Player.Move.performed += SetValuesFromInputKnob;
        playerInput.Player.Move.canceled += ctx => SetNoMovement();
    }

    private void Update()
    {
        if (IsServer)
        {
            UpdateServer();
        }
    }

    private void UpdateServer()
    {
        transform.position = new Vector3(transform.position.x + positionX.Value, transform.position.y + positionY.Value);
    }

    private void SetValuesFromInputKnob(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            float moveXLocalVariable;
            float moveYLocalVariable;

            Vector2 readValue = context.ReadValue<Vector2>();

            moveXLocalVariable = readValue.x * walkSpeed;
            moveYLocalVariable = readValue.y * walkSpeed;

            if (oldMoveYLocalVariable != moveYLocalVariable || oldMoveXLocalVariable != moveXLocalVariable)
            {
                oldMoveYLocalVariable = moveYLocalVariable;
                oldMoveXLocalVariable = moveXLocalVariable;
                UpdateClientPositionServerRpc(moveYLocalVariable, moveXLocalVariable);
            }
        }
    }

    [ServerRpc]
    public void UpdateClientPositionServerRpc(float moveYLocalVariable, float moveXLocalVariable)
    {
        positionX.Value = moveXLocalVariable;
        positionY.Value = moveYLocalVariable;
    }

    private void SetNoMovement()
    {
        if (IsOwner)
        {
            UpdateClientPositionServerRpc(0, 0);
        }
    }
}