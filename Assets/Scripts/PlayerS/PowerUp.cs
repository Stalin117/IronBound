using UnityEngine;
using System.Collections;

public class PowerUp : MonoBehaviour
{
    public PowerUpType type;

    // --- ¡NUEVA VARIABLE AQUÍ! ---
    [Header("Valores del PowerUp")]
    public int healAmount = 1; // Cantidad de vida que cura (por defecto 1)
    // --- FIN DE LA NUEVA VARIABLE ---

    [Header("Respawn")]
    public float respawnTime = 10f; 

    private SpriteRenderer sprite;
    private Collider2D col;
    private bool isHidden = false; 

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public void PickUp()
    {
        if (isHidden)
        {
            return;
        }
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        isHidden = true;
        sprite.enabled = false;
        col.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        sprite.enabled = true;
        col.enabled = true;
        isHidden = false;
    }
}