using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetworkMove : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float jumpDuration = 0.2f;
    public float groundDetectionRaius = 0.5f;
    public float inertiaTime = 0.5f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private bool isJumping = false;
    private bool isJumpingAllwoed = true;
    private float jumpTimeCounter;
    private bool isGrounded;
    private float moveX;
    private float moveY;
    private Vector2 currentVelocity;
    private float inertiaCounter;
    private bool isSliding;

    private MultiPlayer playerInput;

    public override void OnNetworkSpawn()
    {
        Application.targetFrameRate = 120;
        rb = GetComponent<Rigidbody2D>();
        playerInput = new MultiPlayer();
        playerInput.Enable();
        playerInput.Player.Move.performed += SetMovement;
        playerInput.Player.Move.canceled += ctx => SetNoMovement();
    }


    void Update()
    {
        if (!IsOwner) return;
        ProcessInputs();
        CheckGrounded();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        MovePlayer();
        ActivateInertiaIfMovementWasEnded();
        SetNoMovementAfterInertiaEnds();
    }
    
    void ProcessInputs()
    {
        moveDirection = new Vector2(moveX, 0).normalized;

        if (moveY >= 0.5 && isGrounded)
        {
            isJumping = true;
            jumpTimeCounter = jumpDuration;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        if (moveY >= 0.5 && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                
                isJumping = false;
            }
        }
        
        if (moveY >= 0.5)
        {
            isJumping = false;
        }
    }

    void MovePlayer()
    {
        rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(transform.position, groundDetectionRaius, groundLayer);
    }

    public void SetMovement(InputAction.CallbackContext context)
    {
        SetValuesFromInputKnob(context);
        ResetInertiaCounterToInitialTime();
        IsInertiaIsAllowedToBePerformed(false);
    }

    private void SetNoMovement()
    {
        IsInertiaIsAllowedToBePerformed(true);
    }

    private void ResetInertiaCounterToInitialTime()
    {
        inertiaCounter = inertiaTime;
    }

    private void IsInertiaIsAllowedToBePerformed(bool value)
    {
        isSliding = value;
    }

    private void SetNoMovementAfterInertiaEnds()
    {
        if (inertiaCounter <= 0) 
        {
            moveX = 0;
            moveY = 0;
        }
    }

    private void ActivateInertiaIfMovementWasEnded()
    {
        if (isSliding && inertiaCounter >=0)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(0, rb.velocity.y), (inertiaTime - inertiaCounter) / inertiaTime);
            inertiaCounter -= Time.deltaTime;
        }
    }

    private void SetValuesFromInputKnob(InputAction.CallbackContext context)
    {
        Vector2 readValue = context.ReadValue<Vector2>();
        moveX = readValue.x;
        moveY = readValue.y;
    }
}