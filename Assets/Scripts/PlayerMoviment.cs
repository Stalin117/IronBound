using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// (El enum PowerUpType ahora está en HeroKnight.cs,
//  pero no hace daño dejarlo aquí también)
// public enum PowerUpType { ExtraJump, HealthUp }

public class PlayerMovement : MonoBehaviour
{
    // --- VARIABLES DE MOVIMIENTO Y SALTO ELIMINADAS ---
    // (Ya no están aquí para no competir con HeroKnight.cs)

    [Header("Health")]
    public int health;
    public int maxHealth = 3;
    public Image healthImg;
    private bool isImmune;
    public float immunityTime = 1f;

    // --- VARIABLES DE DOBLE SALTO ELIMINADAS ---

    private Animator anim;
    // --- AÑADIDO: Referencia al script principal ---
    private HeroKnight heroKnightScript; 

    [Header("UI")]
    public GameObject gameOverImg;

    [Header("Muerte por caída")]
    public float fallDeathY = -10f;

    public bool isDead;

    void Start()
    {
        // --- MODIFICADO: Solo obtiene los componentes que necesita ---
        anim = GetComponent<Animator>();
        heroKnightScript = GetComponent<HeroKnight>(); // Obtiene el script "Cerebro"
        health = maxHealth;

        if (gameOverImg != null)
            gameOverImg.SetActive(false);

        Time.timeScale = 1;
        isDead = false;
    }

    // --- MODIFICADO: Update() ahora solo maneja la vida y la caída ---
    void Update()
    {
        if (isDead) return;

        // --- LÓGICA DE MOVIMIENTO Y SALTO ELIMINADA ---

        // Actualiza la barra de vida
        if (healthImg != null)
            healthImg.fillAmount = (float)health / maxHealth;

        if (health > maxHealth)
            health = maxHealth;

        // Muerte por caída
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
            anim.SetTrigger("Hurt"); // Llama a la animación de "Hurt"
            StartCoroutine(Immunity());
        }
        else
        {
            Die();
        }
    }

    void DieByFall()
    {
        health = 0;
        anim.SetTrigger("Death"); // Llama a la animación de "Death"
        Die();
    }

    void Die()
    {
        isDead = true;
        Time.timeScale = 0;
        if (gameOverImg != null)
            gameOverImg.SetActive(true);
    }

    // --- MODIFICADO: OnTriggerEnter2D ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Lógica para RECIBIR daño por choque
        if (collision.CompareTag("Enemy") || collision.CompareTag("Boss") && !isImmune)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            int damageToGive = enemy != null ? enemy.damageToGive : 1;
            TakeDamage(damageToGive);
        }
        // Lógica para RECOGER power-ups
        else if (collision.CompareTag("PowerUp"))
        {
            PowerUp powerUp = collision.GetComponent<PowerUp>();
            if (powerUp != null)
            {
                // Llama a la función correspondiente
                if (powerUp.type == PowerUpType.ExtraJump)
                {
                    // Llama a la función en HeroKnight.cs
                    heroKnightScript.ActivatePowerUp(powerUp.type);
                }
                else if (powerUp.type == PowerUpType.HealthUp)
                {
                    // Llama a la función local en este script
                    ActivatePowerUp(powerUp.type);
                }
                
                // Le dice al power-up que se oculte
                powerUp.PickUp();
            }
        }
    }

    // --- MODIFICADO: ActivatePowerUp ahora solo maneja la vida ---
    public void ActivatePowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.HealthUp:
                health += 1;
                Debug.Log("¡Vida extra!");
                break;
        }
    }

    // --- CORUTINA DE DOBLE SALTO ELIMINADA ---
    // (Ahora está en HeroKnight.cs)

    IEnumerator Immunity()
    {
        isImmune = true;
        yield return new WaitForSeconds(immunityTime);
        isImmune = false;
    }
}