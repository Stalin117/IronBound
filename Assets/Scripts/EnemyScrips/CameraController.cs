using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("Assign the player's Transform here. If left empty, the script will try to find a GameObject with tag 'Player'.")]
    public Transform player; // Referencia al jugador
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Awake()
    {
        // If player not assigned in inspector, try to find by tag to avoid UnassignedReferenceException
        if (player == null)
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("CameraController: auto-assigned player by tag 'Player'.");
            }
            else
            {
                Debug.LogWarning("CameraController: 'player' is not assigned in the inspector and no GameObject with tag 'Player' was found. Please assign it to avoid errors.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
            return; // avoid NullReferenceException if player still isn't assigned

        transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
    }
}
