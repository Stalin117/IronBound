using UnityEngine;
using System.Collections;

public class BossAI : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 2f;
    public float chaseRange = 6f;
    public float attackRange = 1.8f;
    public LayerMask groundLayer;

    [Header("Ataque")]
    public int damageToPlayer = 1;
    public float attackCooldown = 2f;
    private bool canAttack = true;

    [Header("Referencias")]
    public Transform player;
    public Transform groundCheck;
    public Transform wallCheck;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isFacingRight = true;
    private bool isDead = false;
    private BossHealth bossHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        bossHealth = GetComponent<BossHealth>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (bossHealth != null && bossHealth.health <= 0)
        {
            isDead = true;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (player == null || isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            if (canAttack)
                StartCoroutine(Attack());
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isWalking", false);
        }
        else if (distance <= chaseRange)
        {
            ChasePlayer();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isWalking", false);
        }
    }

    void ChasePlayer()
    {
        anim.SetBool("isWalking", true);

        if (player.position.x > transform.position.x)
        {
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
            if (!isFacingRight) Flip();
        }
        else
        {
            rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
            if (isFacingRight) Flip();
        }

        bool noGround = !Physics2D.Raycast(groundCheck.position, Vector2.down, 1f, groundLayer);
        bool wallAhead = Physics2D.Raycast(wallCheck.position, isFacingRight ? Vector2.right : Vector2.left, 0.3f, groundLayer);

        if (noGround || wallAhead)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    IEnumerator Attack()
    {
        canAttack = false;
        rb.linearVelocity = Vector2.zero;

        // Elegimos aleatoriamente Attack1 o Attack2
        int attackType = Random.Range(0, 2);
        string triggerName = attackType == 0 ? "Attack1" : "Attack2";
        anim.SetTrigger(triggerName);

        // Esperar al frame del golpe
        yield return new WaitForSeconds(0.5f);

        // Comprobar si el jugador sigue cerca
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange + 0.3f)
        {
            PlayerMovement playerScript = player.GetComponent<PlayerMovement>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damageToPlayer);
                Debug.Log($"⚔️ Boss realizó {triggerName} al jugador!");
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
