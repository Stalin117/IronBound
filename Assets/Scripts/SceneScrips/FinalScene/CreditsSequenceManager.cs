using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // ¡¡NECESARIO!!

public class CreditsSequenceManager : MonoBehaviour
{
    [Header("Referencias")]
    public HeroKnight player; 
    public GameObject escapeMessageObject; 
    public Animator creditsAnimator; 
    
    [Header("Configuración")]
    public float walkSpeed = 2.0f;
    public float messageDisplayTime = 4f;
    
    // --- ¡NUEVAS VARIABLES AQUÍ! ---
    [Header("Finalización")]
    public float creditsDuration = 60f; // ¡Pon aquí cuánto duran tus créditos en segundos!
    public string mainMenuSceneName = "MainMenu"; // Nombre de tu escena de Menú
    public string creditsTriggerName = "StartCredits"; 
    // --------------------------------

    private bool canWalk = false; 
    private Animator playerAnim; // Guardamos el animator del jugador
    private Rigidbody2D playerRb; // Guardamos el Rigidbody del jugador

    void Start()
    {
        // 1. Guardar referencias
        if (player != null)
        {
            playerAnim = player.GetComponent<Animator>();
            playerRb = player.GetComponent<Rigidbody2D>();
        
            // 2. Desactivar control del jugador
            player.enabled = false; 
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.enabled = false;
        }

        // 3. Iniciar la secuencia
        StartCoroutine(CreditsSequence());
    }

    private IEnumerator CreditsSequence()
    {
        // 1. Configurar estado inicial
        if (escapeMessageObject != null)
            escapeMessageObject.SetActive(true); 
        if (creditsAnimator != null)
            creditsAnimator.gameObject.SetActive(false); 
        
        canWalk = false; 

        // 2. Esperar a que el jugador lea el mensaje
        yield return new WaitForSeconds(messageDisplayTime);

        // 3. Ocultar mensaje, mostrar créditos e iniciar animación
        if (escapeMessageObject != null)
            escapeMessageObject.SetActive(false);
        if (creditsAnimator != null)
        {
            creditsAnimator.gameObject.SetActive(true);
            creditsAnimator.SetTrigger(creditsTriggerName);
        }

        // 4. Preparar al jugador para caminar
        StartPlayerAutoWalk();
        canWalk = true; 

        // --- ¡NUEVO! ---
        // 5. Esperar a que terminen los créditos
        yield return new WaitForSeconds(creditsDuration);

        // 6. Volver al Menú Principal
        Debug.Log("Créditos terminados. Volviendo al Menú Principal.");
        SceneManager.LoadScene(mainMenuSceneName);
        // -----------------
    }

    void StartPlayerAutoWalk()
    {
        if (player == null) return;

        // Ponerlo a mirar a la derecha
        player.transform.localScale = new Vector3(
            Mathf.Abs(player.transform.localScale.x),
            player.transform.localScale.y,
            player.transform.localScale.z
        );
    }

    // --- ¡FIXEDUPDATE() MODIFICADO! ---
    // (Arregla el bug de "deslizamiento")
    void FixedUpdate()
    {
        // Si el jugador no existe, no hacer nada
        if (playerRb == null) return;

        // Si la secuencia dio permiso...
        if (canWalk)
        {
            // Mover el Rigidbody
            playerRb.linearVelocity = new Vector2(walkSpeed, playerRb.linearVelocity.y);

            // ¡Forzar la animación de correr!
            if (playerAnim != null)
                playerAnim.SetInteger("AnimState", 1);
        }
        else // Si aún no puede caminar (mostrando el mensaje)
        {
            // Asegurarse de que esté quieto
            playerRb.linearVelocity = new Vector2(0, playerRb.linearVelocity.y);

            // ¡Forzar la animación de Idle!
            if (playerAnim != null)
                playerAnim.SetInteger("AnimState", 0);
        }
    }
}