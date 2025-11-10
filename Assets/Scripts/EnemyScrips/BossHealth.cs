using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 50; 
    private int currentHealth; 

    // --- ¡NUEVAS VARIABLES DE BLOQUEO! ---
    [Header("Block Logic")]
    [Tooltip("Probabilidad (de 0 a 1) de bloquear un solo ataque")]
    [Range(0, 1)]
    public float blockChance = 0.2f; // 20%
    
    [Tooltip("Cuántos golpes seguidos activan un bloqueo garantizado")]
    public int spamBlockThreshold = 3; // Bloquea al 3er golpe
    
    [Tooltip("Tiempo (en seg) para reiniciar el conteo de spam")]
    public float spamTimeWindow = 1.5f; 
    
    private int spamCounter = 0;
    private float spamTimer = 0f;
    // -----------------------------------

    [Header("Lógica Metroidvania")]
    public GameObject barrierToUnlock;
    public float deathDelay = 2f; 
    
    [Header("UI")]
    public Image healthBarFill; 
    
    private Animator anim;
    private bool isDead = false;
    private bool isDamaged = false; // (I-Frames)
    private Rigidbody2D rb;
    private SamuraiBoss_AI aiScript; 

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); 
        aiScript = GetComponent<SamuraiBoss_AI>(); 
        currentHealth = maxHealth; 
        
        if (barrierToUnlock != null)
            barrierToUnlock.SetActive(true);

        if (healthBarFill != null)
            healthBarFill.fillAmount = 1f; 
    }

    // --- ¡NUEVO! Conteo del Spam ---
    void Update()
    {
        // Si el contador de spam está activo, réstale tiempo
        if (spamTimer > 0)
        {
            spamTimer -= Time.deltaTime;
            if (spamTimer <= 0)
            {
                spamCounter = 0; // Se acabó el tiempo, reinicia el conteo
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return; 

        if (collision.CompareTag("Weapon") && !isDamaged)
        {
            TakeDamage(2); 
        }
    }

    // --- ¡¡LÓGICA DE TAKEDAMAGE() MEJORADA!! ---
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // 1. COMPROBACIÓN DE BLOQUEO
        // Si el jefe ya está bloqueando (isBlocking) o ya está herido (isDamaged), ignora el golpe.
        if (isDamaged || (aiScript != null && aiScript.IsBlocking()))
        {
            return; 
        }

        // 2. CONTEO DE SPAM Y DECISIÓN DE BLOQUEO
        spamTimer = spamTimeWindow; // Reinicia el tiempo de spam
        spamCounter++; // Añade un golpe al contador

        bool guaranteedBlock = (spamCounter >= spamBlockThreshold);
        bool luckyBlock = (Random.Range(0f, 1f) < blockChance);

        if (guaranteedBlock || luckyBlock)
        {
            Debug.Log("¡Ataque del jugador bloqueado por el Boss!");
            spamCounter = 0; // Reinicia el conteo
            
            if (aiScript != null)
                aiScript.StartBlock(); // ¡Llama al "cerebro" para que bloquee!
            
            return; // No recibe daño
        }

        // 3. RECIBIR DAÑO (Si no bloqueó)
        currentHealth -= damage; 
        isDamaged = true; // Activa I-Frames
        Debug.Log("Boss recibió daño, vida restante: " + currentHealth);

        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;

        StartCoroutine(Damager()); // Inicia temporizador de I-Frames

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (anim != null)
        {
            anim.SetTrigger("Hurt"); 
            
            // Le dice al "cerebro" (IA) que ha sido golpeado
            if (aiScript != null)
                aiScript.GotHit(); 
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

        if (aiScript != null)
            aiScript.enabled = false; 

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true; 
        }

        if (anim != null)
            anim.SetTrigger("Death");
     
        StartCoroutine(UnlockPathAfterDelay());
    }

    private IEnumerator UnlockPathAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay); 
        
        if (barrierToUnlock != null)
        {
            Animator barrierAnimator = barrierToUnlock.GetComponent<Animator>();
            if (barrierAnimator != null)
            {
                barrierAnimator.SetTrigger("Vanish");
                // TODO: Reemplaza '1.5f' por la variable 'barrierAnimationTime' si la creaste
                yield return new WaitForSeconds(1.5f); 
            }
            barrierToUnlock.SetActive(false); 
        }
        
        Destroy(gameObject); 
    }

    IEnumerator Damager()
    {
        yield return new WaitForSeconds(0.3f); // Duración de I-Frames
        isDamaged = false;
    }
}