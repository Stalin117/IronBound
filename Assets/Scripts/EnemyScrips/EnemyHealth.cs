using UnityEngine;
using System.Collections;
using UnityEngine.UI; // <-- ¡Añadido!

public class EnemyHealth : MonoBehaviour
{
    Enemy enemy;
    public bool isDamaged;
    public GameObject deathEffect;
    Rigidbody2D rb;
    SpriteRenderer sprite;
    Blind blindEffect;

    // --- Referencias de Barra de Vida ---
    [Header("Health Bar")]
    public GameObject healthBarPrefab; // <-- Arrastra tu Prefab 'EnemyHealthBar' aquí
    private Image healthBarFill;
    private float maxHealth;
    private GameObject healthBarInstance;
    private UIFollowTarget healthBarFollow;
    // ----------------------------------

    private Animator anim;
    private EnemyAI aiScript; 
    private Collider2D col;  
    private bool isDead = false;
    

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
        sprite = GetComponent<SpriteRenderer>();
        blindEffect = GetComponent<Blind>();

        anim = GetComponent<Animator>();
        aiScript = GetComponent<EnemyAI>(); 
        col = GetComponent<Collider2D>(); 
        
        // --- Añadido en Start ---
        maxHealth = enemy.healthPoints; // Guarda la vida máxima
        
        // Crea la barra de vida, pero mantenla oculta
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBarFill = healthBarInstance.transform.Find("HealthBar_BG/HealthBar_Fill").GetComponent<Image>();
            healthBarFollow = healthBarInstance.GetComponent<UIFollowTarget>();
            
            if (healthBarFollow != null)
                healthBarFollow.SetTarget(this.transform); // Dile a la barra que te siga
            
            healthBarInstance.SetActive(false); // Oculta la barra
        }
        // -------------------------
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon") && !isDamaged && !isDead)
        {
            enemy.healthPoints -= 2; 
            
            // --- Lógica de Barra de Vida ---
            if (healthBarInstance != null)
            {
                // Si es el primer golpe, muestra la barra
                if (!healthBarInstance.activeSelf)
                {
                    healthBarInstance.SetActive(true);
                }
                
                // Actualiza el relleno
                healthBarFill.fillAmount = enemy.healthPoints / maxHealth;
            }
            // -----------------------------
            
            if (collision.transform.position.x < transform.position.x)
                rb.AddForce(new Vector2(enemy.knockbackForceX, enemy.knockbackForceY), ForceMode2D.Impulse);
            else
                rb.AddForce(new Vector2(-enemy.knockbackForceX, enemy.knockbackForceY), ForceMode2D.Impulse);
            
            if (enemy.healthPoints <= 0)
            {
                Die(); 
            }
            else
            {
                StartCoroutine(Damager()); 
            }
        }
    }

    void Die()
    {
        if (isDead) return; 
        isDead = true;

        if (aiScript != null)
            aiScript.enabled = false;

        if (col != null)
            col.enabled = false;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; 
            rb.isKinematic = true; 
            rb.constraints = RigidbodyConstraints2D.FreezeAll; 
        }

        if (anim != null)
            anim.SetTrigger("Death"); 

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        
        // La barra de vida se destruirá sola gracias al script 'UIFollowTarget'
        
        Destroy(gameObject, 2f); 
    }

    IEnumerator Damager()
    {
        isDamaged = true;

        if (blindEffect != null && sprite != null)
            sprite.material = blindEffect.Blink;

        yield return new WaitForSeconds(0.15f); // 0.15f recomendado

        isDamaged = false;

        if (blindEffect != null && sprite != null)
            sprite.material = blindEffect.original;
    }
}