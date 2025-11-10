using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class SamuraiBoss_AI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack, Block, Hurt, Idle, Dead }

    [Header("Referencias (¡Asignar!)")]
    public Enemy enemy;
    public Animator animator;
    public Transform player;
    public LayerMask playerLayer;
    public LayerMask whatIsGround;
    public GameObject healthBarObject; 

    [Header("Sensores (¡Asignar!)")]
    public Transform wallCheck;
    public Transform pitCheck;
    public Transform attackOrigin;
    public float checkRadius = 0.12f;

    [Header("Valores de Comportamiento")]
    public float detectionRadius = 6f;
    public float attackRadius = 1f;
    public float patrolSpeedMultiplier = 0.6f;
    public float attackCooldown = 1.2f; // Cooldown DESPUÉS de atacar o bloquear
    public float attackDelay = 0.1f;
    public float attackRange = 0.6f;
    public float blockDuration = 1.5f; // Cuánto dura un bloqueo
    public float hurtDuration = 0.5f; // Cuánto dura el "Hurt"
    
    // Estados Internos
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private State currentState = State.Patrol;
    private bool canAttack = true; // Controla el cooldown
    private bool isBlocking = false;
    private bool facingRight;
    private Vector3 baseScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale; 
        baseScale.x = Mathf.Abs(baseScale.x); 
        if (animator == null) animator = GetComponent<Animator>();
        if (enemy == null) enemy = GetComponent<Enemy>();
        if (attackOrigin == null) attackOrigin = transform;
        rb.freezeRotation = true;
        rb.isKinematic = false; 
        facingRight = (transform.localScale.x > 0);
        if (healthBarObject != null)
            healthBarObject.SetActive(false);
    }

    void Update()
    {
        // Si está en una acción (Bloqueando, Herido, Muerto), el "cerebro" se pausa.
        if (isBlocking || currentState == State.Hurt || currentState == State.Dead) return;
        
        if (player == null)
        {
            currentState = State.Patrol;
            if (healthBarObject != null && healthBarObject.activeSelf)
                healthBarObject.SetActive(false);
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (healthBarObject != null)
        {
            if (distToPlayer <= detectionRadius)
            {
                if (!healthBarObject.activeSelf)
                    healthBarObject.SetActive(true);
            }
            else 
            {
                if (healthBarObject.activeSelf)
                    healthBarObject.SetActive(false);
            }
        }

        // --- LÓGICA DE IA AGRESIVA ---
        if (distToPlayer <= attackRadius)
        {
            if (canAttack) // Si el cooldown terminó
            {
                currentState = State.Attack; // ¡Siempre ataca!
            }
            else // Si está en cooldown
            {
                currentState = State.Idle;
            }
        }
        else if (distToPlayer <= detectionRadius)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Patrol;
        }
        // --- FIN DE LA LÓGICA ---

        if (animator != null)
        {
            bool isMoving = (currentState == State.Patrol || currentState == State.Chase);
            if(ParameterExists(animator, "isRunning"))
                animator.SetBool("isRunning", isMoving);
            else if (ParameterExists(animator, "Idle"))
                animator.SetBool("Idle", !isMoving);
            
            animator.SetBool("IsBlocking", isBlocking);
        }
    }

    void FixedUpdate()
    {
        if (currentState == State.Dead) return;

        switch (currentState)
        {
            case State.Patrol: HandlePatrol(); break;
            case State.Chase: HandleChase(); break;
            case State.Attack: HandleAttack(); break;
            case State.Block: HandleBlock(); break; 
            case State.Hurt: HandleHurt(); break;
            case State.Idle:
            case State.Dead:
            default:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                break;
        }
    }
    
    // --- FUNCIONES DE ESTADO ---
    void HandlePatrol()
    {
        float spd = (enemy != null) ? enemy.speed * patrolSpeedMultiplier : 1f;
        bool pitDetected = (pitCheck != null) ? !Physics2D.OverlapCircle(pitCheck.position, checkRadius, whatIsGround) : false;
        bool wallDetected = (wallCheck != null) ? Physics2D.OverlapCircle(wallCheck.position, checkRadius, whatIsGround) : false;
        if (pitDetected || wallDetected)
            FlipFacing();
        
        float vx = facingRight ? spd : -spd;
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
    }

    void HandleChase()
    {
        if (player == null) return;
        float spd = (enemy != null) ? enemy.speed : 1f;
        LookAtPlayer();
        float vx = (facingRight ? 1f : -1f) * spd;
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
    }

    void HandleAttack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (canAttack)
        {
            LookAtPlayer(); 
            StartCoroutine(PerformAttack());
        }
    }

    void HandleBlock()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        LookAtPlayer();
    }
    
    void HandleHurt()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    
    // --- FUNCIONES PÚBLICAS (Llamadas por BossHealth) ---
    
    public void StartBlock()
    {
        currentState = State.Block;
        StartCoroutine(BlockRoutine());
    }

    public void GotHit()
    {
        currentState = State.Hurt;
        StartCoroutine(HurtRoutine());
    }
    
    public bool IsBlocking()
    {
        return isBlocking;
    }
    
    // --- CORUTINAS ---

    IEnumerator PerformAttack()
    {
        canAttack = false; // Inicia cooldown
        animator?.SetTrigger("Attack"); 
        yield return new WaitForSeconds(attackDelay);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackOrigin.position, attackRange, playerLayer);
        foreach (var c in hits)
        {
            if (c == null) continue;
            var ph = c.GetComponent<PlayerMovement>(); 
            if (ph != null)
                ph.TakeDamage(enemy != null ? enemy.damageToGive : 1);
        }
        
        StartCoroutine(ActionCooldownRoutine()); // Espera el cooldown
    }

    IEnumerator BlockRoutine()
    {
        isBlocking = true;
        canAttack = false; // Inicia cooldown
        
        yield return new WaitForSeconds(blockDuration); 
        
        isBlocking = false;
        currentState = State.Idle; // Vuelve a 'Idle' para re-evaluar
        
        StartCoroutine(ActionCooldownRoutine()); // Espera el cooldown
    }
    
    private IEnumerator HurtRoutine()
    {
        // Espera a que termine la animación de "Hurt"
        yield return new WaitForSeconds(hurtDuration);
        
        // Vuelve a 'Idle' para re-evaluar (¡y volver a atacar!)
        currentState = State.Idle; 
    }
    
    IEnumerator ActionCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // --- FUNCIONES DE AYUDA ---

    void LookAtPlayer()
    {
        if (player == null) return;
        float directionToPlayer = player.position.x - transform.position.x;
        if (directionToPlayer > 0.1f) { SetFacing(true); }
        else if (directionToPlayer < -0.1f) { SetFacing(false); }
    }

    void SetFacing(bool right)
    {
        if (facingRight == right) return;
        facingRight = right;
        transform.localScale = new Vector3(
            right ? baseScale.x : -baseScale.x, 
            baseScale.y,
            baseScale.z
        );
    }

    void FlipFacing()
    {
        SetFacing(!facingRight);
    }

    bool ParameterExists(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Transform center = (attackOrigin != null) ? attackOrigin : transform;
        Gizmos.DrawWireSphere(center.position, attackRange);
    }
}