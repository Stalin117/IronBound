using UnityEngine;
using System.Collections; // Necesario para las Corutinas

public class TutorialManager : MonoBehaviour
{
    [Header("Configuración")]
    public float showDuration = 8.0f; // Segundos que se mostrará (¡puedes cambiarlo!)

    // Esta es la variable "global" que nos dice si venimos del menú
    public static bool s_ShowTutorialOnLoad = false;

    void Start()
    {
        // Comprueba si la variable "global" está activada
        if (s_ShowTutorialOnLoad)
        {
            // ¡Sí! Inicia la secuencia.
            StartCoroutine(ShowTutorialSequence());
            
            // Resetea la variable para que no vuelva a salir si mueres y reinicias
            s_ShowTutorialOnLoad = false;
        }
        else
        {
            // Si no venimos del menú, nos aseguramos de que empiece oculto.
            gameObject.SetActive(false);
        }
    }

    // Esta es la secuencia automática de 8 segundos
    private IEnumerator ShowTutorialSequence()
    {
        // 1. Mostrar el panel y pausar el juego
        ShowPanel();
        
        // 2. Esperar el tiempo (usamos 'Realtime' porque el juego está pausado)
        yield return new WaitForSecondsRealtime(showDuration);
        
        // 3. Ocultar el panel
        HidePanel();
    }

    // --- Funciones Públicas (para los botones) ---

    // Esta función la llamará tu Menú de Pausa
    public void ShowPanel()
    {
        gameObject.SetActive(true); // Muestra este panel
        Time.timeScale = 0f; // ¡Pausa el juego!
    }

    // Esta función la llamará un botón "Cerrar" (opcional)
    public void HidePanel()
    {
        gameObject.SetActive(false); // Oculta este panel
        Time.timeScale = 1f; // ¡Reanuda el juego!
    }
}