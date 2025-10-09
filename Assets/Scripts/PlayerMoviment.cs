using UnityEngine;
using System.Collections;   // Necesario para IEnumerator
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 12f;

    [Header("Health")]
    public int health;
    public int maxHealth = 3;
    public Image healthImg;
    private bool isImmune;
    public float immunityTime = 1f;  // segundos de inmunidad tras recibir daño

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    private Rigidbody2D rb;
    private Animator anim;

    [Header("UI")]
    public GameObject gameOverImg;

    [Header("Muerte por caída")]
    public float fallDeathY = -10f; // si el jugador cae por debajo de este valor, muere

    public bool isDead;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = maxHealth;   // vida inicial

        if (gameOverImg != null)
            gameOverImg.SetActive(false);

        Time.timeScale = 1; // asegurar que el juego no está pausado
        isDead = false;
    }

    void Update()
    {
        if (isDead) return;

        // Movimiento lateral
        float moveX = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);

        // Revisar si está en el suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Actualizar la barra de vida
        if (healthImg != null)
            healthImg.fillAmount = (float)health / maxHealth;

        if (health > maxHealth)
            health = maxHealth;

        // 🔥 Muerte por caída
        if (transform.position.y < fallDeathY)
        {
            DieByFall();
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;

        if (health > 0)
        {
            anim.SetTrigger("Hurt");  // animación de recibir daño
            StartCoroutine(Immunity()); // activar inmunidad temporal
        }
        else
        {
            Die();
        }
    }

    void DieByFall()
    {
        health = 0;
        anim.SetTrigger("Death");
        Die();
    }

    void Die()
    {
        isDead = true;
        Time.timeScale = 0; // pausar el juego
        if (gameOverImg != null)
            gameOverImg.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !isImmune)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            int damageToGive = enemy != null ? enemy.damageToGive : 1;
            TakeDamage(damageToGive);
        }
    }

    IEnumerator Immunity()
    {
        isImmune = true;
        yield return new WaitForSeconds(immunityTime);
        isImmune = false;
    }
}
