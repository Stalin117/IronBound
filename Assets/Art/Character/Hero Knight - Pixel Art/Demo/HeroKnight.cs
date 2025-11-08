using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public enum PowerUpType { ExtraJump, HealthUp }

public class HeroKnight : MonoBehaviour {

    [SerializeField] float      m_speed = 4.0f;
    [SerializeField] float      m_jumpForce = 7.5f;
    [SerializeField] float      m_rollForce = 6.0f;
    [SerializeField] bool       m_noBlood = false;
    [SerializeField] GameObject m_slideDust;
    
    // --- VARIABLES DE 'Combat' ELIMINADAS ---
    // (Ya no se necesitan, el scale volteará el objeto 'Weapon' automáticamente)

    [Header("Physics")] 
    [SerializeField] BoxCollider2D m_feetCollider; // Collider con 'HighFriction'

    [Header("Double Jump")]
    [SerializeField] float      m_doubleJumpForceMultiplier = 0.8f;
    [SerializeField] float      m_doubleJumpDuration = 10f; 
    private int                 m_maxJumps = 1;
    private int                 m_jumpCount = 0;
    private Coroutine           m_doubleJumpCoroutine;

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

    public bool m_blocking = false; 
    private Vector3 m_baseScale; // <-- NUEVO: Para guardar la escala original

    void Start ()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();

        // --- LÓGICA DE 'Start' CORREGIDA ---
        m_baseScale = transform.localScale; // Guarda la escala (ej: 1.1131, 1.1131)
        m_facingDirection = (transform.localScale.x > 0) ? 1 : -1;
    }

    void Update ()
    {
        m_timeSinceAttack += Time.deltaTime;

        if(m_rolling)
            m_rollCurrentTime += Time.deltaTime;

        if(m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

        
        // Lógica de Suelo (Detecta el ESTADO actual)
        if (m_groundSensor.State())
        {
            if (!m_grounded)
            {
                m_grounded = true;
                m_animator.SetBool("Grounded", m_grounded);
                m_jumpCount = 0;
            }
        }
        else // Si el sensor NO detecta suelo
        {
            if (m_grounded)
            {
                m_grounded = false;
                m_animator.SetBool("Grounded", m_grounded);
            }
        }

        // Activar/Desactivar el collider de agarre (pies) CADA FRAME
        if (m_feetCollider != null)
        {
            if (m_grounded)
                m_feetCollider.enabled = true; // Agarre activado
            else 
                m_feetCollider.enabled = false; // Agarre desactivado
        }


        float inputX = Input.GetAxis("Horizontal");

        // --- Lógica de Giro (¡REEMPLAZADA!) ---
        if (inputX > 0.1f)
        {
            SetFacing(1); // Llama a la nueva función de giro
        }
        else if (inputX < -0.1f)
        {
            SetFacing(-1); // Llama a la nueva función de giro
        }
        
        
        // --- Lógica de Paredes (¡SIMPLIFICADA!) ---
        // Comprueba los sensores del lado al que estás mirando
        bool wallInFront = (m_facingDirection == 1) ? 
                            (m_wallSensorR1.State() || m_wallSensorR2.State()) : 
                            (m_wallSensorL1.State() || m_wallSensorL2.State());


        // Lógica de Movimiento
        if (!m_rolling && !m_blocking)
        {
            float currentVerticalVelocity = m_body2d.linearVelocity.y;
            
            // CASO 1: Estás empujando contra una pared (adelante)
            if ( (inputX > 0.1f && m_facingDirection == 1 && wallInFront) || (inputX < -0.1f && m_facingDirection == -1 && wallInFront) )
            {
                if (currentVerticalVelocity > 0)
                    currentVerticalVelocity = 0; // "Mata" el salto si chocas subiendo
                
                m_body2d.linearVelocity = new Vector2(0, currentVerticalVelocity);
            }
            // CASO 2: No hay pared
            else
            {
                m_body2d.linearVelocity = new Vector2(inputX * m_speed, currentVerticalVelocity);
            }
        }
        else if (m_blocking)
        {
            m_body2d.linearVelocity = new Vector2(0, m_body2d.linearVelocity.y);
        }

        
        m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);
        // La lógica de WallSlide ahora usa 'wallInFront'
        m_isWallSliding = wallInFront && !m_grounded; 
        m_animator.SetBool("WallSlide", m_isWallSliding);

        
        // --- Inputs (sin cambios) ---
        if (Input.GetKeyDown("e") && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }
        else if (Input.GetKeyDown("q") && !m_rolling)
            m_animator.SetTrigger("Hurt");
        else if(Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;
            if (m_currentAttack > 3)
                m_currentAttack = 1;
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;
            m_animator.SetTrigger("Attack" + m_currentAttack);
            m_timeSinceAttack = 0.0f;
        }
        else if (Input.GetMouseButtonDown(1) && !m_rolling && m_grounded)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
            m_blocking = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            m_animator.SetBool("IdleBlock", false);
            m_blocking = false;
        }
        else if (Input.GetKeyDown("left shift") && !m_rolling && !m_isWallSliding)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
        }
        else if (Input.GetKeyDown("space") && !m_rolling)
        {
            if (m_jumpCount < m_maxJumps)
            {
                if (m_jumpCount == 0)
                {
                    m_animator.SetTrigger("Jump");
                }
                float forceToApply = (m_jumpCount == 0) ? m_jumpForce : m_jumpForce * m_doubleJumpForceMultiplier;
                m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, forceToApply);
                m_jumpCount++; 
                if (m_groundSensor != null)
                {
                    m_groundSensor.Disable(0.2f);
                }
            }
        }
        else if (Mathf.Abs(inputX) > Mathf.Epsilon && !m_blocking)
        {
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        else if (!m_blocking)
        {
            m_delayToIdle -= Time.deltaTime;
                if(m_delayToIdle < 0)
                    m_animator.SetInteger("AnimState", 0);
        }
    }

    // --- ¡NUEVA FUNCIÓN DE GIRO! ---
    void SetFacing(int direction)
    {
        // Si ya estamos mirando en esa dirección, no hacer nada
        if (m_facingDirection == direction) return;
        
        m_facingDirection = direction;
        
        // Voltea el scale del GameObject principal
        transform.localScale = new Vector3(
            m_baseScale.x * direction, // Voltea X
            m_baseScale.y,
            m_baseScale.z
        );
    }

    // Eventos de Animación (Sin Cambios)
    void AE_SlideDust() { }

    // Lógica de Power-Up (Sin Cambios)
    public void ActivatePowerUp(PowerUpType type)
    {
        if (type == PowerUpType.ExtraJump)
        {
            if (m_doubleJumpCoroutine != null)
                StopCoroutine(m_doubleJumpCoroutine);
            m_doubleJumpCoroutine = StartCoroutine(DoubleJumpPowerUpRoutine());
        }
    }
    
    private IEnumerator DoubleJumpPowerUpRoutine()
    {
        m_maxJumps = 2; 
        yield return new WaitForSeconds(m_doubleJumpDuration);
        m_maxJumps = 1; 
        m_doubleJumpCoroutine = null;
    }
}