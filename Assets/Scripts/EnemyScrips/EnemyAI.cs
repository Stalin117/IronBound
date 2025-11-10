using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    // ¡ESTE SCRIPT MANEJA EL MOVIMIENTO Y EL ATAQUE!
    // Tu script "EnemyHealth.cs" maneja RECIBIR DAÑO.

    public enum State { Patrol, Chase, Attack, Idle }
    
    [Header("Referencias (¡Asignar!)")]
    public Enemy enemy;           // El script "Enemy" con las stats
    public Animator animator;         // El "Animator" del esqueleto
    public Transform player;          // Arrastra tu "HeroKnight" aquí
    public LayerMask playerLayer;     // Asigna la capa "Player"
    public LayerMask whatIsGround;    // Asigna la capa "Ground"

    [Header("Sensores (¡Asignar!)")]
    public Transform wallCheck;
    public Transform pitCheck;
    public Transform attackOrigin;    // Arrastra el "AttackPoint" del esqueleto
    public float checkRadius = 0.12f;

    [Header("Valores de Comportamiento")]
    public float detectionRadius = 6f;  // Rango Amarillo
    public float attackRadius = 1f;     // Rango Rojo
    public float patrolSpeedMultiplier = 0.6f;
    public float attackCooldown = 1.2f; // Tiempo entre ataques
    public float attackDelay = 0.1f;    // Espera para sincronizar anim
    public float attackRange = 0.6f;    // Rango del golpe (OverlapCircle)

    // Estados Internos
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector3 baseScale; // <--- AÑADE ESTA LÍNEA
    private State currentState = State.Patrol;
    private bool canAttack = true;
    
    // Asumimos que el sprite original mira a la DERECHA
    // por lo tanto, 'facingRight = true' es el estado inicial
    private bool facingRight = true; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        // boxCollider = GetComponent<BoxCollider2D>(); // <--- ELIMINA ESTA LÍNEA

        // --- AÑADE ESTAS 2 LÍNEAS AQUÍ ---
        baseScale = transform.localScale; 
        baseScale.x = Mathf.Abs(baseScale.x); 
        // ---------------------------------
        
        if (animator == null) animator = GetComponent<Animator>();
        if (enemy == null) enemy = GetComponent<Enemy>();
        if (attackOrigin == null) attackOrigin = transform;

        rb.freezeRotation = true;
        
        // Busca al jugador por Tag si no está asignado
        if (player == null) 
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) 
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("¡No se encontró al jugador! Asegúrate de que 'HeroKnight' tenga el Tag 'Player'.");
            }
        }

        // --- REEMPLAZA TU LÍNEA 'facingRight' ANTIGUA POR ESTA ---
        // Sincroniza la dirección inicial basándose en el 'scale'
        facingRight = (transform.localScale.x > 0);
        // -----------------------------------------------------
    }

    void Update()
    {
        // El script EnemyHealth se encarga de la muerte
        
        if (player == null) 
        {
            currentState = State.Patrol;
            return;
        }

        // --- Lógica de Estados ---
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= attackRadius) 
        {
            currentState = State.Attack;
        } 
        else if (distToPlayer <= detectionRadius) 
        {
            currentState = State.Chase;
        } 
        else 
        {
            currentState = State.Patrol;
        }
        
        // --- Control del Animator (Usa "Idle") ---
        if (animator != null) 
        {
            bool isMoving = (currentState == State.Patrol || currentState == State.Chase);
            animator.SetBool("Idle", !isMoving);
        }
    }

    void FixedUpdate()
    {
        // Ejecuta la lógica del estado actual
        switch (currentState) 
        {
            case State.Patrol:
                HandlePatrol();
                break;
            case State.Chase:
                HandleChase();
                break;
            case State.Attack:
                HandleAttack();
                break;
            case State.Idle:
            default:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                break;
        }
    }

    // --- LÓGICA DE ESTADOS ---

    void HandlePatrol()
    {
        float spd = (enemy != null) ? enemy.speed * patrolSpeedMultiplier : 1f;

        // Comprueba si hay un abismo o una pared
        bool pitDetected = (pitCheck != null) ? !Physics2D.OverlapCircle(pitCheck.position, checkRadius, whatIsGround) : false;
        bool wallDetected = (wallCheck != null) ? Physics2D.OverlapCircle(wallCheck.position, checkRadius, whatIsGround) : false;

        if (pitDetected || wallDetected) 
        {
            FlipFacing();
        }

        // Aplica velocidad
        float vx = facingRight ? spd : -spd;
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
    }

    void HandleChase()
    {
        if (player == null) { rb.linearVelocity = Vector2.zero; return; }
        float spd = (enemy != null) ? enemy.speed : 1f;
        
        LookAtPlayer(); // Gira hacia el jugador
        
        // Aplica velocidad
        rb.linearVelocity = new Vector2((facingRight ? 1f : -1f) * spd, rb.linearVelocity.y);
    }

    void HandleAttack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Detenerse

        // *** ↓↓↓ CORRECCIÓN 2 AQUÍ ↓↓↓ ***
        // Solo mira al jugador UNA VEZ, cuando decide atacar.
        if (canAttack) 
        {
            LookAtPlayer(); // MIRA AL JUGADOR
            StartCoroutine(PerformAttack());
        }
    }
    
    // --- FUNCIONES DE AYUDA ---

    // Gira al enemigo para mirar al jugador (con "zona muerta")
    void LookAtPlayer()
    {
        if (player == null) return;
        
        float directionToPlayer = player.position.x - transform.position.x;

        if (directionToPlayer > 0.1f) // Player está a la derecha
        {
            SetFacing(true); // Mirar derecha
        } 
        else if (directionToPlayer < -0.1f) // Player está a la izquierda
        {
            SetFacing(false); // Mirar izquierda
        }
    }

    // Ejecuta el ataque (animación y daño al jugador)
    IEnumerator PerformAttack()
    {
        canAttack = false;
        animator?.SetTrigger("Attack"); // Llama al Trigger "Attack" del Animator
        
        yield return new WaitForSeconds(attackDelay); 

        // Detecta al jugador en el punto de ataque
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackOrigin.position, attackRange, playerLayer);
        foreach (var c in hits)
        {
            if (c == null) continue;
            // Busca el script de vida del Héroe
            // NOTA: Tu script original buscaba 'PlayerMovement', asegúrate de que ese script tenga 'TakeDamage'
            var ph = c.GetComponent<PlayerMovement>(); // Asegúrate que este script exista en tu jugador
            if (ph != null)
            {
                // Le hace daño al jugador
                ph.TakeDamage(enemy != null ? enemy.damageToGive : 1);
            }
        }
        
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // Gira el sprite
    void SetFacing(bool right)
    {
        if (facingRight == right) return;
        facingRight = right;

        // Voltea el scale usando los valores guardados
        transform.localScale = new Vector3(
            right ? baseScale.x : -baseScale.x, // Si 'right' es true, usa X. Si es false, usa -X
            baseScale.y, 
            baseScale.z
        );

        // ¡Asegúrate de que YA NO HAY NADA aquí que toque 'sr.flipX'!
        // ¡Asegúrate de que YA NO HAY NADA aquí que toque 'boxCollider.offset'!
    }

    void FlipFacing()
    {
        SetFacing(!facingRight);
    }
    
    // Dibuja los círculos de rango en el Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Transform center = (attackOrigin != null) ? attackOrigin : transform;
        Gizmos.DrawWireSphere(center.position, attackRange);
    }
}