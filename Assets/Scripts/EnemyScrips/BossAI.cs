using UnityEngine;

public class BossAI : MonoBehaviour
{
    public float speed = 2f;
    public float detectionRange = 8f; // rango para detectar al jugador
    public Transform player;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public Transform wallCheck;
    public float wallCheckDistance = 0.3f;
    public LayerMask wallLayer;

    public Vector2 zoneMinMax = new Vector2(-5f, 5f); // límites del área donde puede moverse (X min, X max)
    
    private Animator anim;
    private Rigidbody2D rb;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool isDead = false;

    private Vector3 startPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startPosition = transform.position; // punto de origen del boss
    }

    void Update()
    {
        if (isDead) return;

        // Revisar si el boss está tocando el suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Revisar si hay pared enfrente
        bool hittingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, wallLayer);

        // Revisar si el jugador está cerca
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Solo atacar si está dentro de la zona designada
            if (distanceToPlayer < detectionRange && 
                player.position.x > zoneMinMax.x + startPosition.x && 
                player.position.x < zoneMinMax.y + startPosition.x)
            {
                ChasePlayer(hittingWall);
            }
            else
            {
                ReturnToStart();
            }
        }
    }

    void ChasePlayer(bool hittingWall)
    {
        if (isGrounded && !hittingWall)
        {
            anim.SetBool("isWalking", true);
            float direction = player.position.x - transform.position.x;
            rb.linearVelocity = new Vector2(Mathf.Sign(direction) * speed, rb.linearVelocity.y);

            // Voltear sprite
            if (direction > 0 && !isFacingRight) Flip();
            else if (direction < 0 && isFacingRight) Flip();
        }
        else
        {
            anim.SetBool("isWalking", false);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void ReturnToStart()
    {
        float distance = Vector2.Distance(transform.position, startPosition);

        if (distance > 0.2f)
        {
            anim.SetBool("isWalking", true);
            float direction = startPosition.x - transform.position.x;
            rb.linearVelocity = new Vector2(Mathf.Sign(direction) * speed, rb.linearVelocity.y);

            if (direction > 0 && !isFacingRight) Flip();
            else if (direction < 0 && isFacingRight) Flip();
        }
        else
        {
            anim.SetBool("isWalking", false);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    // Llamado desde BossHealth cuando muere
    public void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isWalking", false);
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujar límites de zona
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(startPosition.x + zoneMinMax.x, startPosition.y, 0),
                        new Vector3(startPosition.x + zoneMinMax.y, startPosition.y, 0));
    }
}
