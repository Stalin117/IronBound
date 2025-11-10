using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player; // Arrastra tu 'HeroKnight' aquí

    [Header("Posición de la Cámara (Offsets)")]
    public float offsetY = 1.5f; // Sube esto para ver más el suelo
    public float offsetX = 1f;   // Sube esto para mover la cámara a la derecha
    
    [Header("Suavizado")]
    public float smoothSpeed = 0.3f; // Qué tan rápido sigue la cámara (más bajo = más lento)

    private Vector3 velocity = Vector3.zero;

    // Usamos LateUpdate para asegurarnos de que el jugador
    // ya se ha movido este frame.
    void LateUpdate()
    {
        // Si no hay jugador asignado, no hacer nada.
        if (player == null)
        {
            return;
        }

        // 1. Calcula la posición a la que la cámara QUIERE ir
        Vector3 desiredPosition = new Vector3(
            player.position.x + offsetX,  // Posición del jugador + offset en X
            player.position.y + offsetY,  // Posición del jugador + offset en Y
            transform.position.z          // Mantiene la posición Z original de la cámara
        );

        // 2. Mueve la cámara suavemente a esa posición
        transform.position = Vector3.SmoothDamp(
            transform.position, // Desde dónde se mueve
            desiredPosition,    // Hacia dónde se mueve
            ref velocity,       // (Variable interna para el cálculo)
            smoothSpeed         // El tiempo que tarda en llegar
        );
    }
}