// using UnityEngine;

// public class Boss000 : MonoBehaviour
// {
//     [Header("Boss Settings")]
//     public float moveSpeed = 6f;
//     public float attackRange = 3f;
//     public float attackCooldown = 2f;
//     public int maxHealth = 50;
//     public int damage = 10;

//     private Transform player;
//     private Rigidbody2D rb;
//     private Animator anim;

//     private float attackTimer;
//     private int currentHealth;

//     void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         anim = GetComponent<Animator>();
//         player = GameObject.FindWithTag("Player")?.transform;
//         currentHealth = maxHealth;
//     }

//     void Update()
//     {
//         if (player == null) return;

//         float distance = Vector2.Distance(transform.position, player.position);

//         if (distance <= attackRange)
//         {
//             Attack();
//         }
//         else
//         {
//             Chase();
//         }

//         attackTimer -= Time.deltaTime;
//     }

//     void Chase()
//     {
//         Vector2 direction = (player.position - transform.position).normalized;
//         rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

//         // Flip sprite depending on direction
//         if (direction.x != 0)
//             transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);

//         anim.SetBool("IsMoving", true);
//     }

//     void Attack()
//     {
//         rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Stop moving when attacking
//         anim.SetBool("IsMoving", false);

//         if (attackTimer <= 0f)
//         {
//             attackTimer = attackCooldown;
//             anim.SetTrigger("Attack");
//             // TODO: Add actual damage logic (player health)
//         }
//     }

//     public void TakeDamage(int damage)
//     {
//         currentHealth -= damage;
//         anim.SetTrigger("Hurt");

//         if (currentHealth <= 0)
//             Die();
//     }

//     void Die()
//     {
//         anim.SetTrigger("Die");
//         rb.linearVelocity = Vector2.zero;
//         Destroy(gameObject, 1f); // Delay to allow death animation
//     }
// }



using UnityEngine;

public class Boss000 : MonoBehaviour
{
    [Header("Boss Settings")]
    public float moveSpeed = 6f;
    public float attackRange = 3f;
    public float attackCooldown = 2f;
    public int maxHealth = 50;
    public int damage = 10;

    private Transform player;
    private Rigidbody2D rb;
    // private Animator anim; // animations commented out for now

    private float attackTimer;
    private int currentHealth;
    private Vector3 originalScale;

    // How often (seconds) we re-try FindWithTag if player not found yet.
    private float findRetryInterval = 0.5f;
    private float findRetryTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // anim = GetComponent<Animator>();
        originalScale = transform.localScale;
        currentHealth = maxHealth;

        TryFindPlayer();

        if (player == null)
            Debug.LogWarning("⚠️ Boss000: Player not found in Start(). Will retry in Update().");
    }

    void Update()
    {
        // If player isn't found yet, try again periodically (useful if player is spawned later)
        if (player == null)
        {
            findRetryTimer -= Time.deltaTime;
            if (findRetryTimer <= 0f)
            {
                findRetryTimer = findRetryInterval;
                TryFindPlayer();
            }
            return; // Wait until player exists to run AI
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
            Attack();
        else
            Chase();

        attackTimer -= Time.deltaTime;
    }

    // Try to find a GameObject with tag "Player"
    void TryFindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            Debug.Log("✅ Boss000: Player found at runtime.");
        }
    }

    void Chase()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        // Flip sprite but preserve original scale size (so editor resizing is preserved)
        if (direction.x != 0f)
        {
            transform.localScale = new Vector3(
                Mathf.Sign(direction.x) * Mathf.Abs(originalScale.x),
                originalScale.y,
                originalScale.z
            );
        }

        // anim?.SetBool("IsMoving", true); // commented out until animations exist
        // debug line:
        Debug.Log("🏃 Boss chasing player. Direction.x: " + direction.x + " Velocity: " + rb.linearVelocity.x);
    }

    void Attack()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        // anim?.SetBool("IsMoving", false);

        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            // anim?.SetTrigger("Attack");
            Debug.Log("⚔️ Boss000 attacks (debug).");
            // TODO: apply damage to player here
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        // anim?.SetTrigger("Hurt");
        Debug.Log("Boss000 took " + amount + " damage. HP left: " + currentHealth);
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        // anim?.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        Debug.Log("💀 Boss000 defeated (debug).");
        Destroy(gameObject, 1f);
    }
}

