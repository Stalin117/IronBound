using UnityEngine;
using UnityEngine.SceneManagement; // ¡Importante!

public class SceneTransitionTrigger : MonoBehaviour
{
    // Escribe el nombre exacto de tu nueva escena de créditos aquí
    public string sceneToLoad = "CreditsScene"; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Comprueba si el objeto que entró es el jugador
        if (collision.CompareTag("Player"))
        {
            // Desactiva el control del jugador para que no se mueva
            HeroKnight playerScript = collision.GetComponent<HeroKnight>();
            if (playerScript != null)
            {
                playerScript.enabled = false;
            }
            
            // Carga la nueva escena de créditos
            Debug.Log("Cargando escena: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}