using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    float speed;
    Rigidbody2D rb;

    public bool isWalker = true;
    public bool walksRight = true;

    public Transform wallCheck;
    public Transform pitCheck;
    public Transform groundCheck;             // <-- vuelvo a agregar GroundCheck
    public float detectionRadius = 0.2f;
    public float groundCheckRadius = 0.12f;   // radio separado para GroundCheck
    public LayerMask whatIsGround;

    [HideInInspector] public bool isGround;   // público solo para debug si lo quieres ver
    private Vector3 initialScale;
    private bool alreadyFlipped;

    void Start()
    {
        speed = GetComponent<Enemy>().speed;
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;
    }

    void Update()
    {
        // --- comprobaciones seguras: evita NullReference si no asignaste algo ---
        bool pitDetected = false;
        bool wallDetected = false;

        if (pitCheck != null)
            pitDetected = !Physics2D.OverlapCircle(pitCheck.position, detectionRadius, whatIsGround);

        if (wallCheck != null)
            wallDetected = Physics2D.OverlapCircle(wallCheck.position, detectionRadius, whatIsGround);

        // Ground check (vuelve a añadirse)
        if (groundCheck != null)
            isGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        else
            isGround = false;

        // Flip cuando detecta pared o hueco (y no lo haya hecho ya)
        if ((pitDetected || wallDetected) && !alreadyFlipped)
        {
            Flip();
            alreadyFlipped = true;
        }

        // desbloquea flip cuando ya no detecta nada
        if (!pitDetected && !wallDetected)
            alreadyFlipped = false;
    }

    private void FixedUpdate()
    {
        if (!isWalker) return;

        // opcional: si querés que el enemigo no se mueva en el aire, chequeamos isGround
        // si preferís que camine igual aunque esté en el aire (ej. empujado), comentá la siguiente línea:
        // if (!isGround) return;

        if (walksRight)
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
    }

    private void Flip()
    {
        walksRight = !walksRight;

        transform.localScale = new Vector3(
            walksRight ? Mathf.Abs(initialScale.x) : -Mathf.Abs(initialScale.x),
            initialScale.y,
            initialScale.z
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheck.position, detectionRadius);
        }
        if (pitCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pitCheck.position, detectionRadius);
        }
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
