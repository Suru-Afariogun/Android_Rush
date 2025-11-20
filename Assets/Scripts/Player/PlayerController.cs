using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterAnimator characterAnimator;
    public Transform groundCheck;
    public LayerMask groundLayer;
    
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float groundCheckRadius = 0.12f;
    
    [Header("Combat")]
    public float attackCooldown = 1.2f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool wasMoving = false;
    private float attackTimer = 0f;
    private bool isAttacking = false;

    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable()
{
    if (!PauseMenu.isPaused)
    {
        controls.Player.Enable();
    }
    controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
    controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    controls.Player.Jump.performed += ctx => TryJump();
    
    // Attack inputs
    controls.Player.AttackA.performed += ctx => TryAttackA();
    controls.Player.AttackB.performed += ctx => TryAttackB();
}

   void OnDisable()
{
    if (controls != null)
        controls.Player.Disable();
}

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
{
    // Disable player input when paused
    if (PauseMenu.isPaused && controls.Player.enabled)
    {
        controls.Player.Disable();
    }
    // Re-enable player input when unpaused
    else if (!PauseMenu.isPaused && !controls.Player.enabled)
    {
        controls.Player.Enable();
    }
}

    void Update()
    {
        // Update attack timer
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;
        
        // Update grounded state (like boss does)
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            // Fallback to collision-based detection if groundCheck not assigned
            // (keep existing OnCollisionEnter/Exit logic as backup)
        }
        
        characterAnimator?.SetGrounded(isGrounded);
        
        // Don't move if attacking
        if (!isAttacking)
        {
            MovePlayer();
        }
        
        ApplyBetterJump();
    }

    void MovePlayer()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        if (moveInput.x != 0)
            sr.flipX = moveInput.x < 0;
        
        // Update animation based on movement
        bool isMoving = Mathf.Abs(moveInput.x) > 0.1f;
        
        // Only call SetMoving when state actually changes (to avoid spamming triggers)
        if (isMoving != wasMoving)
        {
            characterAnimator?.SetMoving(isMoving);
            wasMoving = isMoving;
        }
        // But always keep the bool updated in case animator needs it
        else if (characterAnimator != null && characterAnimator.anim != null)
        {
            characterAnimator.anim.SetBool("IsMoving", isMoving);
        }
    }
    
    void TryAttackA()
    {
        if (attackTimer > 0f || isAttacking) return;
        
        isAttacking = true;
        attackTimer = attackCooldown;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // Stop horizontal movement
        
        if (isGrounded)
        {
            characterAnimator?.TriggerGroundAttackA();
        }
        else
        {
            characterAnimator?.TriggerAirAttackA();
        }
        
        // End attack after animation duration
        Invoke(nameof(EndAttack), 0.6f);
    }
    
    void TryAttackB()
    {
        if (attackTimer > 0f || isAttacking) return;
        
        isAttacking = true;
        attackTimer = attackCooldown;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // Stop horizontal movement
        
        if (isGrounded)
        {
            characterAnimator?.TriggerGroundAttackB();
        }
        else
        {
            characterAnimator?.TriggerAirAttackB();
        }
        
        // End attack after animation duration
        Invoke(nameof(EndAttack), 0.8f);
    }
    
    void EndAttack()
    {
        isAttacking = false;
    }

    void TryJump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            // Trigger jump animation (uses StartMoving trigger like boss does)
            characterAnimator?.SetMoving(true);
        }
    }

    void ApplyBetterJump()
    {
        if (rb == null) return;

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
