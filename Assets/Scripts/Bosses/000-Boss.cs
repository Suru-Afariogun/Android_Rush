
using System.Collections;
using UnityEngine;

public class Boss000 : MonoBehaviour
{
    [Header("References")]
    public Transform groundCheck;               // small transform at boss feet
    public LayerMask groundLayer;
    public Transform spawnPoint; 
    public CharacterAnimator characterAnimator;               // optional, not required

    [Header("Tuning")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public float jumpHorizontalSpeed = 4f;      // when jumping toward a target
    public float attackCooldown = 1.2f;
    public int maxHealth = 50;
    public int damage = 10;

    // Distances (tweak these)
    [Header("Attack Distances")]
    public float groundAttackADistance = 1.5f;  // Attack A: very close
    public float groundAttackBDistance = 3.0f;  // Attack B: bit farther (calf kick)
    public float airAttackADistance = 2.0f;     // Air A: close in air (forward punch)
    public float airAttackBDistance = 4.0f;     // Air B: dive range (aims down)

    [Header("Dive (Air B)")]
    public float diveSpeed = 18f;               // overall speed of dive
    public float diveVerticalBias = -1f;        // ensures downward component (negative)
    public float diveDuration = 1.0f;           // length of dive state before recover if not landed

    [Header("Follow-jump (far)")]
    public float followJumpDelay = 2f;          // wait time when far before jumping to last known pos

    [Header("Ground Check")]
    public float groundCheckRadius = 0.12f;

    // internal
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 originalScale;

    private float attackTimer = 0f;
    private int currentHealth;
    private Vector2 playerLastKnownPos;

    // states
    private bool isGrounded = true;
    private bool isRecovering = false;
    private bool isHurt = false;
    private bool isAttacking = false;
    private bool isJumping = false;
    private bool isDiving = false;
    private Coroutine followJumpCoroutine = null;

    // player find retry
    private float findRetryInterval = 0.5f;
    private float findRetryTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        originalScale = transform.localScale;
        currentHealth = maxHealth;
    }

    void Start()
    {
        TryFindPlayer();
    }

    void Update()
    {
        // re-acquire player if missing
        if (player == null)
        {
            findRetryTimer -= Time.deltaTime;
            if (findRetryTimer <= 0f)
            {
                findRetryTimer = findRetryInterval;
                TryFindPlayer();
            }
            return;
        }

        // update basic timers and states
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        // update grounded state
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = false; //Default to false if groundCheck not assigned
            Debug.LogWarning("Boss000: groundCheck is not assigned!");
        }
        Debug.Log($"Grounded: {isGrounded}");
        characterAnimator?.SetGrounded(isGrounded);

        // do nothing if recovering/hurt/attacking/dive in progress
        if (isRecovering || isHurt || isAttacking || isDiving) return;

        // record player's last known position constantly for dive targeting
        playerLastKnownPos = player.position;

        float horizontalDist = Mathf.Abs(player.position.x - transform.position.x);
        float totalDist = Vector2.Distance(transform.position, player.position);
        float verticalDiff = player.position.y - transform.position.y;

        // ------------------------
        // Priority rules (per your spec)
        // 1) If player in range for downward kick (Air B) -> prepare jump/dive
        // 2) If player is in the air and close enough -> jump/punish with Air A
        // 3) If player is far -> wait followJumpDelay seconds, then jump to last-known-pos
        // 4) Otherwise choose grounded attacks by proximity (A very close, B less close)
        // ------------------------

        // CASE: If boss is grounded and player is high enough and within air-dive distance: prepare jump+dive
        // if (isGrounded && verticalDiff < 0.5f == false) // not strict, keep for normal cases
        // {
        //     // detect air-dive desire: player is on ground & within airAttackBDistance (we target from air)
        //     if (Mathf.Abs(player.position.y - transform.position.y) <= 1.5f)
        //     {
        //         // no vertical priority here (player near same height) - handled later
        //     }
        // }

        // 1) If player is on ground and within airAttackBDistance horizontally and verticalDiff is not high (we will jump then dive)
        if (isGrounded && !isJumping && !isDiving && horizontalDist <= airAttackBDistance && Mathf.Abs(verticalDiff) < 1.0f)
        {
            // If we are in dive window: boss should jump and perform air B (downward kick)
            // but only do this if player is not too far horizontally (sensible dive range)
            if (horizontalDist <= airAttackBDistance && attackTimer <= 0f)
            {
                StartCoroutine(JumpThenDiveAt(playerLastKnownPos));
                return;
            }
        }

        // 2) If player is airborne and close enough for Air A punish
        if (isGrounded && !isJumping && !isDiving && Mathf.Abs(verticalDiff) > 0.6f && horizontalDist <= airAttackADistance && attackTimer <= 0f)
        {
            // boss jumps and performs AirA (forward punch in air)
            StartCoroutine(JumpThenPerformAirA());
            return;
        }

        // 3) If player is far: initiate follow-jump after delay
        if (isGrounded && totalDist > groundAttackBDistance)
        {
            // far scenario: wait then jump to player's last known pos
            if (followJumpCoroutine == null)
            {
                followJumpCoroutine = StartCoroutine(DelayedFollowJump(playerLastKnownPos, followJumpDelay));
            }
            // still chase while waiting
            Chase();
            return;
        }

        // 4) Grounded attack selection by proximity
        if (isGrounded && totalDist <= groundAttackADistance && attackTimer <= 0f)
        {
            // Attack A: very close (both on ground)
            Debug.Log("Within Attack A range");
            GroundAttackA();
            return;
        }
        else if (isGrounded && totalDist <= groundAttackBDistance && attackTimer <= 0f)
        {
            // Attack B: calf kick (both on ground), less close
            Debug.Log("Within Attack B range");
            GroundAttackB();
            return;
        }

        // default: chase (only if not in any special state)
        if (!isAttacking && !isJumping && !isDiving)
        {
            Chase();
        }
    }

    // -----------------------------
    // FIND PLAYER
    // -----------------------------
    void TryFindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            Debug.Log("✅ Boss000: Player found.");
        }
    }

    // -----------------------------
    // CHASE & StartMoving transition
    // -----------------------------
    void Chase()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        // trigger transition animation when starting to move
        characterAnimator?.SetMoving(true);

        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Sign(direction.x) * Mathf.Abs(originalScale.x),
                originalScale.y, originalScale.z);
        }
    }

    // -----------------------------
    // GROUND ATTACKS
    // -----------------------------
    void GroundAttackA()
    {
        isAttacking = true;
        attackTimer = attackCooldown;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        characterAnimator?.TriggerGroundAttackA();

        Debug.Log("Ground Attack A (very close) triggered.");
        // Attack hitbox should be spawned by an AnimationEvent inside the clip
        StartCoroutine(EndAttackAfter(0.6f));
    }

    void GroundAttackB()
    {
        isAttacking = true;
        attackTimer = attackCooldown;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        characterAnimator?.TriggerGroundAttackB();

        Debug.Log("Ground Attack B (calf kick) triggered.");
        StartCoroutine(EndAttackAfter(0.8f));
    }

    // -----------------------------
    // AIR ATTACKS helper coroutines
    // -----------------------------
    IEnumerator JumpThenPerformAirA()
    {
        // Prepare to jump
        isJumping = true;
        isAttacking = true;
        attackTimer = attackCooldown;

        characterAnimator?.SetMoving(true); // transition to jump

        // compute horizontal direction toward player
        float dx = playerLastKnownPos.x - transform.position.x;
        float hdir = Mathf.Sign(dx);
        rb.linearVelocity = new Vector2(hdir * jumpHorizontalSpeed, jumpForce);

        // small delay to become airborne
        yield return new WaitForSeconds(0.12f);

        // now perform AirA
        characterAnimator?.TriggerAirAttackA();
        Debug.Log("Air Attack A triggered (forward air punch).");

        // let the animation/hitbox play out
        yield return new WaitForSeconds(0.6f);

        // recover on landing - let physics handle landing detection elsewhere
        isJumping = false;
        isAttacking = false;
        isDiving = false;
    }

    IEnumerator JumpThenDiveAt(Vector2 targetPos)
    {
        // Jump up a bit toward the player's last known x position then dive
        isJumping = true;
        isDiving = true;
        isAttacking = true;
        attackTimer = attackCooldown;

        characterAnimator?.SetMoving(true);

        float dx = targetPos.x - transform.position.x;
        float hSign = Mathf.Sign(dx);
        rb.linearVelocity = new Vector2(hSign * jumpHorizontalSpeed, jumpForce);

        // give a small hang time to reach an apex
        yield return new WaitForSeconds(0.12f);

        // compute 45 degree down vector toward target's last pos
        Vector2 directionToTarget = (targetPos - (Vector2)transform.position).normalized;
        // ensure downward component
        directionToTarget.y = Mathf.Min(directionToTarget.y, -0.1f);
        directionToTarget = directionToTarget.normalized;

        // make sure a significant downward component exists (force approx -0.7)
        Vector2 diveVec = new Vector2(directionToTarget.x, -Mathf.Abs(directionToTarget.y));
        diveVec = diveVec.normalized;

        rb.linearVelocity = diveVec * diveSpeed;

        characterAnimator?.TriggerAirAttackB();
        Debug.Log("Air Attack B (downward 45° kick) launched toward: " + targetPos);

        // wait for dive duration or until collide/land (we still use the coroutine timer to reset)
        float timer = 0f;
        while (timer < diveDuration)
        {
            // if landed mid-dive, break early
            if (isGrounded)
                break;
            timer += Time.deltaTime;
            yield return null;
        }

        // ended dive: go to recover
        isDiving = false;
        isJumping = false;
        isAttacking = false;
        Recover();

        yield break;
    }

    // delayed follow-jump (far) - record last known pos at delay start
    IEnumerator DelayedFollowJump(Vector2 startingPlayerPos, float delay)
    {
        // ensure only one follow-jump coroutine runs
        Vector2 targetPos = startingPlayerPos;
        float timer = 0f;
        while (timer < delay)
        {
            // keep updating targetPos to player's latest position while waiting
            if (player != null) targetPos = player.position;
            timer += Time.deltaTime;
            yield return null;
        }

        followJumpCoroutine = null; // clear reference
        // After delay, perform a jump toward targetPos
        StartCoroutine(JumpThenPerformAirAorBDependingOnPosition(targetPos));
        yield break;
    }

    IEnumerator JumpThenPerformAirAorBDependingOnPosition(Vector2 targetPos)
    {
        // similar logic as JumpThenPerformAirA: choose A if airborne close; B if landing dive desired
        isJumping = true;
        isAttacking = true;
        attackTimer = attackCooldown;

        characterAnimator?.SetMoving(true);

        float dx = targetPos.x - transform.position.x;
        float hSign = Mathf.Sign(dx);
        rb.linearVelocity = new Vector2(hSign * jumpHorizontalSpeed, jumpForce);

        yield return new WaitForSeconds(0.12f);

        // choose based on whether target is airborne near us or on ground near us
        float horDist = Mathf.Abs(targetPos.x - transform.position.x);
        float vertDiff = targetPos.y - transform.position.y;

        if (vertDiff > 0.5f && horDist <= airAttackADistance)
        {
            characterAnimator?.TriggerAirAttackA();
            Debug.Log("Follow-jump: performed AirA on target.");
        }
        else if (horDist <= airAttackBDistance)
        {
            // dive
            Vector2 t = targetPos;
            StartCoroutine(JumpThenDiveAt(t));
            // JumpThenDiveAt handles resetting states
            yield break;
        }
        else
        {
            // default to AirA if nothing else
            characterAnimator?.TriggerAirAttackA();
            Debug.Log("Follow-jump defaulted to AirA.");
        }

        yield return new WaitForSeconds(0.6f);

        isJumping = false;
        isAttacking = false;
        isDiving = false;
        yield break;
    }

    // -----------------------------
    // RECOVER
    // -----------------------------
    public void Recover()
    {
        if (isRecovering) return;
        isRecovering = true;
        rb.linearVelocity = Vector2.zero;
        characterAnimator?.TriggerRecover();
        Debug.Log("Recover triggered.");
        Invoke(nameof(EndRecover), 0.5f);
    }

    void EndRecover()
    {
        isRecovering = false;
    }

    // -----------------------------
    // HURT & DIE (HURT & DIE animation calls intentionally left commented out)
    // -----------------------------
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;
        currentHealth -= amount;
        Debug.Log("Boss takes " + amount + " damage. HP left: " + currentHealth);

        // intentionally NOT calling Hurt animation until you provide the clip
        // if (anim != null) anim.SetTrigger("Hurt");

        isHurt = true;
        rb.linearVelocity = Vector2.zero;
        Invoke(nameof(EndHurt), 0.25f);

        if (currentHealth <= 0)
            Die();
    }

    void EndHurt() { isHurt = false; }

    void Die()
    {
        // intentionally NOT calling Die animation until you provide the clip
        // if (anim != null) anim.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        Debug.Log("Boss died!");
        Destroy(gameObject, 1f);
    }

    // -----------------------------
    // Small helpers
    // -----------------------------
    IEnumerator EndAttackAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

       // Optional: show attack ranges as debug spheres
       Gizmos.color = Color.red;
       Gizmos.DrawWireSphere(transform.position, groundAttackADistance);
       Gizmos.color = Color.magenta;
       Gizmos.DrawWireSphere(transform.position, groundAttackBDistance);
       Gizmos.color = Color.cyan;
       Gizmos.DrawWireSphere(transform.position, airAttackBDistance);
       Gizmos.color = Color.green;
       Gizmos.DrawWireSphere(transform.position, airAttackADistance);
    }
}
