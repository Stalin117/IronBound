using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // --- Estados de la IA ---
    private enum AIState { Patrolling, Chasing, Attacking }
    [SerializeField] private AIState currentState = AIState.Patrolling;

    // --- Referencias a Componentes ---
    private Enemy enemyStats;
    private EnemyMovement enemyMovement;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;

    // --- Detección del Jugador ---
    [Header("Detección")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float detectionRange = 8f; // Rango para empezar a perseguir
    [SerializeField] private float attackRange = 1.5f;  // Rango para atacar

    // --- Lógica de Ataque ---
    [Header("Ataque")]
    [SerializeField] private Transform attackPoint; // Un objeto hijo que marca de dónde sale el ataque
    [SerializeField] private float attackRadius = 0.8f;
    [SerializeField] private float attackCooldown = 2f; // Tiempo de espera entre ataques
    private float currentCooldown = 0f;
    private bool isAttacking = false;

    void Awake()
    {
        // Obtenemos todos los componentes necesarios
        enemyStats = GetComponent<Enemy>();
        enemyMovement = GetComponent<EnemyMovement>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Buscamos al jugador por su etiqueta "Player"
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player == null || isAttacking) return; // Si no hay jugador o está atacando, no hace nada

        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }

        // --- Máquina de Estados ---
        switch (currentState)
        {
            case AIState.Patrolling:
                Patrol();
                break;
            case AIState.Chasing:
                Chase();
                break;
            case AIState.Attacking:
                Attack();
                break;
        }
    }

    private void Patrol()
    {
        enemyMovement.enabled = true;
        if (Vector2.Distance(transform.position, player.position) < detectionRange)
        {
            currentState = AIState.Chasing;
        }
    }

    private void Chase()
    {
        enemyMovement.enabled = false;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * enemyStats.speed, rb.linearVelocity.y);
            FlipTowardsPlayer();
        }
        else if (currentCooldown <= 0)
        {
            currentState = AIState.Attacking;
        }

        if (distanceToPlayer > detectionRange)
        {
            currentState = AIState.Patrolling;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void Attack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Attack"); // Lanza la animación de ataque
        currentCooldown = attackCooldown;
    }
    
    // MÉTODO LLAMADO DESDE LA ANIMACIÓN DE ATAQUE
    public void PerformAttack()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider2D playerHit in hitPlayers)
        {
            // Busca el script PlayerMovement en el objeto golpeado y le hace daño
            PlayerMovement playerScript = playerHit.GetComponent<PlayerMovement>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(enemyStats.damageToGive);
            }
        }
    }

    // MÉTODO LLAMADO AL FINAL DE LA ANIMACIÓN DE ATAQUE
    public void FinishAttack()
    {
        isAttacking = false;
        currentState = AIState.Chasing; // Vuelve a perseguir
    }

    private void FlipTowardsPlayer()
    {
        float directionToPlayer = player.position.x - transform.position.x;
        if ((directionToPlayer > 0 && transform.localScale.x < 0) || (directionToPlayer < 0 && transform.localScale.x > 0))
        {
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (attackPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}