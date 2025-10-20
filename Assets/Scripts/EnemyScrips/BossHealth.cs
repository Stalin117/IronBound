using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("Vida y daño")]
    public int health = 50;
    public int damageTaken = 2; // Daño que recibe por golpe del jugador
    public bool isDamaged = false;

    [Header("Lógica Metroidvania")]
    public GameObject barrierToUnlock;
    public float deathDelay = 2f;

    [Header("Feedback visual")]
    public float damageCooldown = 0.3f;
    public GameObject deathEffect;
    public Material blinkMaterial;
    private Material originalMaterial;

    // Referencias internas
    private Animator anim;
    private bool isDead = false;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;

    void Start()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (sprite != null)
            originalMaterial = sprite.material;

        if (barrierToUnlock != null)
            barrierToUnlock.SetActive(true);
    }

    // ==========================================
    // ⚔️ Detecta colisiones con el arma del jugador
    // ==========================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon") && !isDamaged && !isDead)
        {
            // Llamamos a TakeDamage con el daño base del jugador
            TakeDamage(damageTaken);

            // Knockback dependiendo del lado del golpe
            if (rb != null)
            {
                if (collision.transform.position.x < transform.position.x)
                    rb.AddForce(new Vector2(4f, 2f), ForceMode2D.Impulse);
                else
                    rb.AddForce(new Vector2(-4f, 2f), ForceMode2D.Impulse);
            }
        }
    }

    // ==========================================
    // ❤️ Recibir daño (público por si el HeroKnight lo llama)
    // ==========================================
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        Debug.Log($"Boss recibió daño, vida restante: {health}");

        if (health <= 0)
        {
            Die();
        }
        else
        {
            if (anim != null)
                anim.SetTrigger("Hit_Boss");

            StartCoroutine(DamageBlink());
        }
    }

    // ==========================================
    // 💀 Muerte del jefe
    // ==========================================
    void Die()
    {
        isDead = true;
        Debug.Log("¡Boss derrotado!");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (anim != null)
            anim.SetTrigger("Dead_Boss");

        // Efecto visual de muerte
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // Desactiva IA si existe
        BossAI aiScript = GetComponent<BossAI>();
        if (aiScript != null)
            aiScript.enabled = false;

        StartCoroutine(UnlockPathAfterDelay());
    }

    // ==========================================
    // ✨ Efecto visual al recibir daño
    // ==========================================
    IEnumerator DamageBlink()
    {
        isDamaged = true;

        if (sprite != null && blinkMaterial != null)
            sprite.material = blinkMaterial;

        yield return new WaitForSeconds(damageCooldown);

        if (sprite != null)
            sprite.material = originalMaterial;

        isDamaged = false;
    }

    // ==========================================
    // 🚪 Desbloquear el camino después de morir
    // ==========================================
    private IEnumerator UnlockPathAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);

        if (barrierToUnlock != null)
            barrierToUnlock.SetActive(false);

        Destroy(gameObject, 1f);
    }
}
