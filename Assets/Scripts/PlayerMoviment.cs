using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Health")]
    public int health;
    public int maxHealth = 3; // ¡Asegúrate de poner 21 aquí en el Inspector!
    public Image healthImg;
    private bool isImmune;
    public float immunityTime = 1f;

    private Animator anim;
    // --- Referencia al script "Cerebro" ---
    private HeroKnight heroKnightScript; 

    [Header("UI")]
    public GameObject gameOverImg;

    [Header("Muerte por caída")]
    public float fallDeathY = -10f;

    public bool isDead;

    void Start()
    {
        anim = GetComponent<Animator>();
        heroKnightScript = GetComponent<HeroKnight>(); // Obtiene el script "Cerebro"
        
        // Si 'maxHealth' es 3 pero en el Inspector pusiste 21, 'health' será 21.
        // Si 'health' es 0, se setea al 'maxHealth' del Inspector.
        if (health <= 0) 
        {
            health = maxHealth;
        }

        if (gameOverImg != null)
            gameOverImg.SetActive(false);

        Time.timeScale = 1;
        isDead = false;
    }

    void Update()
    {
        if (isDead) return;

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

    // --- ¡¡FUNCIÓN TAKEDAMAGE() TOTALMENTE CORREGIDA!! ---
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        // --- 1. COMPROBACIÓN DE BLOQUEO ---
        // Lee la variable pública del script HeroKnight
        if (heroKnightScript != null && heroKnightScript.m_blocking)
        {
            Debug.Log("¡Ataque bloqueado!");
            anim.SetTrigger("Block"); // Re-activa la animación de bloqueo por si acaso
            return; // ¡No recibe daño!
        }
        // --- FIN DE LA COMPROBACIÓN ---


        // --- 2. COMPROBACIÓN DE I-FRAMES (CORREGIDA) ---
        // Si no está bloqueando, comprueba si es invencible
        if (isImmune) return;
        // --- FIN DE LA CORRECCIÓN ---


        // --- 3. RECIBIR DAÑO ---
        health -= damageAmount;
        isImmune = true; // Se vuelve invencible AHORA
        Debug.Log("Jugador recibe daño. Vida: " + health);

        if (health > 0)
        {
            anim.SetTrigger("Hurt"); // Llama a la animación de "Hurt"
            StartCoroutine(Immunity()); // Inicia el *temporizador* de inmunidad
        }
        else
        {
            Die();
        }
    }
    // --- FIN DE LA FUNCIÓN TAKEDAMAGE ---

    void DieByFall()
    {
        health = 0;
        anim.SetTrigger("Death"); 
        Die();
    }

    void Die()
    {
        isDead = true;
        Time.timeScale = 0;
        if (gameOverImg != null)
            gameOverImg.SetActive(true);
        
        // Desactiva el control del jugador
        if (heroKnightScript != null)
            heroKnightScript.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Lógica para RECIBIR daño por choque (TOCAR al enemigo)
        if (collision.CompareTag("Enemy") && !isImmune)
        {
            // Esta función AHORA comprobará si estás bloqueando
            TakeDamage(1); // O puedes leer 'enemy.damageToGive'
        }
        
        // Lógica de PowerUp (Sin Cambios)
        else if (collision.CompareTag("PowerUp"))
        {
            PowerUp powerUp = collision.GetComponent<PowerUp>();
            if (powerUp != null)
            {
                bool pickedUp = false; 

                if (powerUp.type == PowerUpType.ExtraJump)
                {
                    heroKnightScript.ActivatePowerUp(powerUp.type);
                    pickedUp = true; 
                }
                else if (powerUp.type == PowerUpType.HealthUp)
                {
                    pickedUp = ActivatePowerUp(powerUp.type, powerUp.healAmount); 
                }
                
                if (pickedUp)
                {
                    powerUp.PickUp();
                }
            }
        }
    }

    // Función de PowerUp (Sin Cambios)
    public bool ActivatePowerUp(PowerUpType type, int amountToHeal) 
    {
        switch (type)
        {
            case PowerUpType.HealthUp:
                if (health < maxHealth) 
                {
                    health += amountToHeal; 
                    if (health > maxHealth)
                    {
                        health = maxHealth;
                    }
                    Debug.Log("¡Vida recuperada! Salud actual: " + health);
                    return true; 
                }
                else
                {
                    Debug.Log("¡Vida ya está al máximo!");
                    return false; 
                }
        }
        return false; 
    }

    // Corutina de Inmunidad (Sin Cambios)
    IEnumerator Immunity()
    {
        yield return new WaitForSeconds(immunityTime);
        isImmune = false;
    }
}