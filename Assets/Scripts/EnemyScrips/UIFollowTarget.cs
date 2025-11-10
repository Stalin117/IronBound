using UnityEngine;

public class UIFollowTarget : MonoBehaviour
{
    private Transform target;
    public Vector3 offset = new Vector3(0, 0.75f, 0); // Ajusta este 'Y' para la altura
    private Quaternion fixedRotation;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        fixedRotation = transform.rotation; // Guarda la rotación original
    }

    void LateUpdate()
    {
        // Si el objetivo (enemigo) existe, sigue su posición + offset
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.rotation = fixedRotation; // ¡Mantiene la rotación fija!
        }
        else
        {
            // Si el enemigo murió y fue destruido, destrúyete a ti mismo
            Destroy(gameObject);
        }
    }
}