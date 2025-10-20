using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Añadido para el enum

// --- AÑADIDO: El enum del PowerUp ---
public enum PowerUpType { ExtraJump, HealthUp }

public class HeroKnight : MonoBehaviour {

    [SerializeField] float      m_speed = 4.0f;
    [SerializeField] float      m_jumpForce = 7.5f;
    [SerializeField] float      m_rollForce = 6.0f;
    [SerializeField] bool       m_noBlood = false;
    [SerializeField] GameObject m_slideDust;

    // --- AÑADIDO: Variables de Ataque y Doble Salto ---
    [Header("Attack Logic")]
    [SerializeField] Transform  m_attackPoint; // Asigna esto en el Inspector
    [SerializeField] float      m_attackRange = 0.5f;
    [SerializeField] int        m_attackDamage = 1;
    [SerializeField] LayerMask  m_attackableLayer; // Asigna esto (Enemy, Boss)

    [Header("Double Jump")]
    [SerializeField] float      m_doubleJumpForceMultiplier = 0.8f;
    [SerializeField] float      m_doubleJumpDuration = 10f; // Tiempo que dura el power-up
    private int                 m_maxJumps = 1;
    private int                 m_jumpCount = 0;
    private Coroutine           m_doubleJumpCoroutine;
    // --- FIN DE AÑADIDOS ---

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_HeroKnight   m_groundSensor;
    private Sensor_HeroKnight   m_wallSensorR1;
    private Sensor_HeroKnight   m_wallSensorR2;
    private Sensor_HeroKnight   m_wallSensorL1;
    private Sensor_HeroKnight   m_wallSensorL2;
    private bool                m_isWallSliding = false;
    private bool                m_grounded = false;
    private bool                m_rolling = false;
    private int                 m_facingDirection = 1;
    private int                 m_currentAttack = 0;
    private float               m_timeSinceAttack = 0.0f;
    private float               m_delayToIdle = 0.0f;
    private float               m_rollDuration = 8.0f / 14.0f;
    private float               m_rollCurrentTime;


    // Use this for initialization
    void Start ()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();

        // --- AÑADIDO: Asignar Attack Point si no está en el Inspector ---
        // (Es mejor si lo asignas manualmente en el Inspector)
        if (m_attackPoint == null)
        {
            // Asume que tienes un hijo llamado "Weapon"
            // m_attackPoint = transform.Find("Weapon"); 
        }
    }

    // Update is called once per frame
    void Update ()
    {
        m_timeSinceAttack += Time.deltaTime;

        if(m_rolling)
            m_rollCurrentTime += Time.deltaTime;

        if(m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
            // --- AÑADIDO: Resetear contador de saltos al aterrizar ---
            m_jumpCount = 0;
        }

        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        float inputX = Input.GetAxis("Horizontal");

        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }
            
        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        if (!m_rolling )
            m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);

        m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);

        m_isWallSliding = (m_wallSensorR1.State() && m_wallSensorR2.State()) || (m_wallSensorL1.State() && m_wallSensorL2.State());
        m_animator.SetBool("WallSlide", m_isWallSliding);

        if (Input.GetKeyDown("e") && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }
            
        else if (Input.GetKeyDown("q") && !m_rolling)
            m_animator.SetTrigger("Hurt");

        // --- MODIFICADO: Ataque (Clic Izquierdo) ---
        else if(Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;

            if (m_currentAttack > 3)
                m_currentAttack = 1;

            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            m_animator.SetTrigger("Attack" + m_currentAttack);

            // --- AÑADIDO: Llamar a la función de hacer daño ---
            DealDamage();

            m_timeSinceAttack = 0.0f;
        }
        // --- FIN MODIFICADO ---

        else if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }

        else if (Input.GetMouseButtonUp(1))
            m_animator.SetBool("IdleBlock", false);

        else if (Input.GetKeyDown("left shift") && !m_rolling && !m_isWallSliding)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
        }
            
        // --- MODIFICADO: Lógica de Salto (Espacio) ---
        else if (Input.GetKeyDown("space") && !m_rolling)
        {
            // Comprueba si aún tenemos saltos (normal o doble)
            if (m_jumpCount < m_maxJumps)
            {
                if (m_jumpCount == 0) // Si es el primer salto
                {
                    m_animator.SetTrigger("Jump");
                    m_grounded = false;
                    m_animator.SetBool("Grounded", m_grounded);
                }

                // Aplica la fuerza (normal o doble)
                float forceToApply = (m_jumpCount == 0) ? m_jumpForce : m_jumpForce * m_doubleJumpForceMultiplier;
                m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, forceToApply);
                
                m_jumpCount++; // Incrementa el contador de saltos

                if (m_groundSensor != null)
                {
                    m_groundSensor.Disable(0.2f);
                }
            }
        }
        // --- FIN MODIFICADO ---

        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }

        else
        {
            m_delayToIdle -= Time.deltaTime;
                if(m_delayToIdle < 0)
                    m_animator.SetInteger("AnimState", 0);
        }
    }

    // Animation Events
    void AE_SlideDust()
    {
        // ... (Tu código de SlideDust no se toca) ...
    }

    // --- AÑADIDO: Nueva Función para Hacer Daño ---
    void DealDamage()
    {
        // 1. Detecta TODO lo que esté en la capa "atacable"
        Collider2D[] allHits = Physics2D.OverlapCircleAll(m_attackPoint.position, m_attackRange, m_attackableLayer);

        // 2. Recorre todo lo que golpeó
        foreach(Collider2D hit in allHits)
        {
            // --- COMPROBACIÓN 1: ¿Es un enemigo normal? ---
            // (¡IMPORTANTE! Reemplaza "Enemy" si tu script de vida
            // del esqueleto se llama diferente, ej. "EnemyHealth")
            Enemy enemyScript = hit.GetComponent<Enemy>(); 
            if (enemyScript != null)
            {
                // (Reemplaza "TakeDamage" si la función se llama diferente)
                // enemyScript.TakeDamage(m_attackDamage); 
                Debug.Log("Golpeaste a un enemigo normal");
            }

            // --- COMPROBACIÓN 2: ¿Es un Boss? ---
            // (Necesitarás crear el script "BossHealth.cs")
            BossHealth bossScript = hit.GetComponent<BossHealth>();
            if (bossScript != null)
            {
                bossScript.TakeDamage(m_attackDamage);
            }
        }
    }

    // --- AÑADIDO: Funciones de Power-Up (Llamadas por PlayerMovement.cs) ---
    
    // Esta función es PÚBLICA para que PlayerMovement.cs pueda llamarla
    public void ActivatePowerUp(PowerUpType type)
    {
        if (type == PowerUpType.ExtraJump)
        {
            if (m_doubleJumpCoroutine != null)
            {
                StopCoroutine(m_doubleJumpCoroutine);
            }
            m_doubleJumpCoroutine = StartCoroutine(DoubleJumpPowerUpRoutine());
        }
        // El tipo "HealthUp" será manejado por PlayerMovement.cs
    }
    
    private IEnumerator DoubleJumpPowerUpRoutine()
    {
        m_maxJumps = 2; // Otorga 2 saltos
        Debug.Log("¡Doble Salto Activado! (Temporal)");
        yield return new WaitForSeconds(m_doubleJumpDuration);
        m_maxJumps = 1; // Revierte a 1 salto
        Debug.Log("¡Efecto de Doble Salto terminado!");
        m_doubleJumpCoroutine = null;
    }
    // --- FIN DE AÑADIDOS ---
}