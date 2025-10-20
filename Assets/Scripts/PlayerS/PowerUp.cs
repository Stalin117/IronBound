using UnityEngine;
using System.Collections;

public class PowerUp : MonoBehaviour
{
    // Esta es la variable que el jugador lee para saber qué tipo de power-up es
    public PowerUpType type;

    [Header("Respawn")]
    public float respawnTime = 10f; // Tiempo en segundos para reaparecer

    private SpriteRenderer sprite;
    private Collider2D col;
    private bool isHidden = false; // Un "seguro" para evitar que se tome dos veces

    void Start()
    {
        // Obtenemos los componentes al inicio
        sprite = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    // Esta es la nueva función pública que el JUGADOR llamará
    public void PickUp()
    {
        // Si ya estamos ocultos (esperando reaparecer), no hacemos nada.
        if (isHidden)
        {
            return;
        }

        // Si no estamos ocultos, iniciamos la corutina para ocultarnos
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        // 1. Marcamos como oculto y desactivamos
        isHidden = true;
        sprite.enabled = false;
        col.enabled = false;

        // 2. Esperamos el tiempo de respawn
        yield return new WaitForSeconds(respawnTime);

        // 3. Volvemos a aparecer y marcamos como visible
        sprite.enabled = true;
        col.enabled = true;
        isHidden = false;
    }
}