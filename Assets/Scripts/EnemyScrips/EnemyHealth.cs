using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    Enemy enemy;
    public bool isDamaged;
    public GameObject deathEffect;
    Rigidbody2D rb;
    SpriteRenderer sprite;
    Blind blindEffect;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
        sprite = GetComponent<SpriteRenderer>();
        blindEffect = GetComponent<Blind>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon") && !isDamaged)
        {
            // Resta vida fija
            enemy.healthPoints -= 2;

            // Knockback según dirección del golpe
            if (collision.transform.position.x < transform.position.x)
            {
                rb.AddForce(new Vector2(enemy.knockbackForceX, enemy.knockbackForceY), ForceMode2D.Impulse);
            }
            else
            {
                rb.AddForce(new Vector2(-enemy.knockbackForceX, enemy.knockbackForceY), ForceMode2D.Impulse);
            }

            StartCoroutine(Damager());

            // Verificar si murió
            if (enemy.healthPoints <= 0)
            {
                if (deathEffect != null)
                {
                    Instantiate(deathEffect, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
            }
        }
    }

    IEnumerator Damager()
    {
        isDamaged = true;

        if (blindEffect != null && sprite != null)
        {
            sprite.material = blindEffect.Blink;
        }

        yield return new WaitForSeconds(0.3f);

        isDamaged = false;

        if (blindEffect != null && sprite != null)
        {
            sprite.material = blindEffect.original;
        }
    }
}

