using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    // Ajusta la vida según la dificultad que quieras
    public int health = 50; 

    [Header("Lógica Metroidvania")]
    // Arrastra aquí la puerta, reja o muro que quieres desbloquear
    public GameObject barrierToUnlock;

    // Tiempo para que se ejecute la animación de muerte
    public float deathDelay = 2f; 

    private Animator anim;
    private bool isDead = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        
        // Asegura que la barrera esté activada al empezar
        if (barrierToUnlock != null)
        {
            barrierToUnlock.SetActive(true);
        }
    }

    // Esta función PÚBLICA será llamada por el script HeroKnight.cs
    public void TakeDamage(int damage)
    {
        // Si ya está muerto, no hagas nada
        if (isDead) return;

        health -= damage;
        Debug.Log("Boss recibió daño, vida restante: " + health);

        if (health <= 0)
        {
            Die();
        }
        else if (anim != null)
        {
            // Activa el trigger "Hurt" en el Animator del Boss
            anim.SetTrigger("Hit_Boss"); 
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("¡Boss derrotado!");

        // Desactivamos el collider para que no reciba más daño
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Activamos la animación de muerte
        if (anim != null)
        {
            anim.SetTrigger("Dead_Boss");
        }
        
        // Opcional: Desactiva la IA del Boss para que deje de moverse
        // (Reemplaza 'BossAI' con el nombre de tu script de IA)
        /*
        BossAI aiScript = GetComponent<BossAI>();
        if (aiScript != null) aiScript.enabled = false;
        */

        // Inicia la corutina para DESBLOQUEAR el camino
        StartCoroutine(UnlockPathAfterDelay());
    }

    private IEnumerator UnlockPathAfterDelay()
    {
        // 1. Espera el tiempo de la animación de muerte
        yield return new WaitForSeconds(deathDelay);
        
        // 2. Desactiva la barrera (¡Aquí ocurre la magia!)
        if (barrierToUnlock != null)
        {
            barrierToUnlock.SetActive(false);
        }
        
        // 3. Opcional: Destruye el cuerpo del boss un segundo después
        Destroy(gameObject, 1f); 
    }
}