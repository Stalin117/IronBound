using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class BossHealth : MonoBehaviour
{
    // --- ¡¡LÓGICA DE VIDA CORREGIDA!! ---
    [Header("Health")]
    public int maxHealth = 50; // Pon aquí la vida máxima
    private int currentHealth; // La vida actual (privada)
    // ------------------------------------

    [Header("Lógica Metroidvania")]
    public GameObject barrierToUnlock;
    public float deathDelay = 2f; 

    [Header("UI")]
    public Image healthBarFill; 
    
    private Animator anim;
    private bool isDead = false;
    private bool isDamaged = false; 
    private Rigidbody2D rb;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); 
        
        // --- ¡¡INICIALIZADOR DE VIDA AÑADIDO!! ---
        currentHealth = maxHealth; // ¡Empieza con la vida al máximo!
        // ----------------------------------------
        
        if (barrierToUnlock != null)
        {
            barrierToUnlock.SetActive(true);
        }

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f; 
        }
    }

    // Esta función detecta tu arma (Tag "Weapon")
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return; 

        if (collision.CompareTag("Weapon") && !isDamaged)
        {
            // (Ajusta '2' al daño de tu arma si quieres)
            TakeDamage(2); 
        }
    }

    // Esta función maneja la lógica de recibir daño
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage; // Usa la variable 'currentHealth'
        isDamaged = true; 
        Debug.Log("Boss recibió daño, vida restante: " + currentHealth);

        if (healthBarFill != null)
        {
            // Usa 'currentHealth' y 'maxHealth'
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }

        StartCoroutine(Damager()); 

        if (currentHealth <= 0) // Comprueba 'currentHealth'
        {
            Die();
        }
        else if (anim != null)
        {
            anim.SetTrigger("Hurt"); 
        }
    }
 
    void Die()
    {
        if (isDead) return; 
        isDead = true;
        Debug.Log("¡Boss derrotado!");

        if (healthBarFill != null)
        {
            if (healthBarFill.transform.parent != null)
                healthBarFill.transform.parent.gameObject.SetActive(false); 
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        SamuraiBoss_AI aiScript = GetComponent<SamuraiBoss_AI>();
        if (aiScript != null)
        {
            aiScript.enabled = false; 
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true; // <-- Esta línea AHORA solo se ejecuta al morir
        }

        if (anim != null)
        {
            anim.SetTrigger("Death");
        }
     
        StartCoroutine(UnlockPathAfterDelay());
    }

    private IEnumerator UnlockPathAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay); 
        
        if (barrierToUnlock != null)
        {
            barrierToUnlock.SetActive(false);
        }
        
        Destroy(gameObject); 
    }

    IEnumerator Damager()
    {
        yield return new WaitForSeconds(0.3f); 
        isDamaged = false;
    }
}