using System.Collections;
using UnityEngine;
using UnityEngine.UI; // ¡Importante! Añadir esto para la UI

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class SamuraiBoss_AI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack, Idle, Dead }

    [Header("Referencias (¡Asignar!)")]
    public Enemy enemy;
    public Animator animator;
    public Transform player;
    public LayerMask playerLayer;
    public LayerMask whatIsGround;
    
    // --- ¡NUEVA LÍNEA! ---
    public GameObject healthBarObject; // <-- Arrastra 'BossHealthBar_BG' aquí
    // --------------------

    [Header("Sensores (¡Asignar!)")]
    public Transform wallCheck;
    public Transform pitCheck;
    public Transform attackOrigin;
    public float checkRadius = 0.12f;

    [Header("Valores de Comportamiento")]
    public float detectionRadius = 6f;
    public float attackRadius = 1f;
    public float patrolSpeedMultiplier = 0.6f;
    public float attackCooldown = 1.2f;
    public float attackDelay = 0.1f;
    public float attackRange = 0.6f;

    // Estados Internos
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private State currentState = State.Patrol;
    private bool canAttack = true;
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
        rb.isKinematic = true; 
        
        facingRight = (transform.localScale.x > 0); // Asumiendo que base mira a DERECHA

        // --- ¡NUEVA LÍNEA! ---
        // Oculta la barra de vida al empezar
        if (healthBarObject != null)
            healthBarObject.SetActive(false);
        // --------------------
    }

    void Update()
    {
        if (currentState == State.Dead) return;
        
        // Si no hay jugador, oculta la barra y no hagas nada
        if (player == null)
        {
            currentState = State.Patrol;
            if (healthBarObject != null && healthBarObject.activeSelf)
                healthBarObject.SetActive(false);
            return;
        }

        // --- Lógica de Estados (Máquina de Estados) ---
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // --- ¡LÓGICA DE BARRA DE VIDA MODIFICADA! ---
        if (healthBarObject != null)
        {
            // Si el jugador está DENTRO del radio de detección
            if (distToPlayer <= detectionRadius)
            {
                // Y la barra está oculta, muéstrala
                if (!healthBarObject.activeSelf)
                    healthBarObject.SetActive(true);
            }
            else // Si el jugador está FUERA del radio
            {
                // Y la barra está visible, ocúltala
                if (healthBarObject.activeSelf)
                    healthBarObject.SetActive(false);
            }
        }
        // --- FIN DE LA LÓGICA ---

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

        // --- Control del Animator (Sin Cambios) ---
        if (animator != null)
        {
            bool isMoving = (currentState == State.Patrol || currentState == State.Chase);
            if(ParameterExists(animator, "isRunning"))
            {
                animator.SetBool("isRunning", isMoving);
            }
            else if (ParameterExists(animator, "Idle"))
            {
                animator.SetBool("Idle", !isMoving);
            }
        }
    }

    void FixedUpdate()
    {
        // (El resto de FixedUpdate, HandlePatrol, HandleChase, HandleAttack, 
        //  LookAtPlayer, PerformAttack, SetFacing, FlipFacing, 
        //  ParameterExists y OnDrawGizmosSelected... 
        //  ...SE QUEDAN EXACTAMENTE IGUAL QUE EN EL SCRIPT ANTERIOR)
        //  (Asegúrate de que la lógica de 'rb.MovePosition' esté ahí)

        if (currentState == State.Dead) return;

        switch (currentState)
        {
            case State.Patrol: HandlePatrol(); break;
            case State.Chase: HandleChase(); break;
            case State.Attack: HandleAttack(); break;
            case State.Idle:
            case State.Dead:
            default:
                break;
        }
    }

    void HandlePatrol()
    {
        float spd = (enemy != null) ? enemy.speed * patrolSpeedMultiplier : 1f;

        bool pitDetected = (pitCheck != null) ? !Physics2D.OverlapCircle(pitCheck.position, checkRadius, whatIsGround) : false;
        bool wallDetected = (wallCheck != null) ? Physics2D.OverlapCircle(wallCheck.position, checkRadius, whatIsGround) : false;

        if (pitDetected || wallDetected)
        {
            FlipFacing();
        }
        
        float vx = facingRight ? spd : -spd;
        float newPosX = rb.position.x + vx * Time.fixedDeltaTime;
        rb.MovePosition(new Vector2(newPosX, rb.position.y));
    }

    void HandleChase()
    {
        if (player == null) return;
        float spd = (enemy != null) ? enemy.speed : 1f;
        LookAtPlayer();
        float vx = (facingRight ? 1f : -1f) * spd;
        float newPosX = rb.position.x + vx * Time.fixedDeltaTime;
        rb.MovePosition(new Vector2(newPosX, rb.position.y));
    }

    void HandleAttack()
    {
        if (canAttack)
        {
            LookAtPlayer(); 
            StartCoroutine(PerformAttack());
        }
    }

    void LookAtPlayer()
    {
        if (player == null) return;
        float directionToPlayer = player.position.x - transform.position.x;

        if (directionToPlayer > 0.1f) { SetFacing(true); }
        else if (directionToPlayer < -0.1f) { SetFacing(false); }
    }

    IEnumerator PerformAttack()
    {
        canAttack = false;
        animator?.SetTrigger("Attack");
        yield return new WaitForSeconds(attackDelay);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackOrigin.position, attackRange, playerLayer);
        foreach (var c in hits)
        {
            if (c == null) continue;
            var ph = c.GetComponent<PlayerMovement>(); 
            if (ph != null)
            {
                ph.TakeDamage(enemy != null ? enemy.damageToGive : 1);
            }
        }
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void SetFacing(bool right)
    {
        if (facingRight == right) return;
        facingRight = right;

        // Lógica para sprites que miran a la DERECHA por defecto
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