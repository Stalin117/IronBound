using UnityEngine;

public class UIcamara : MonoBehaviour
{
    [Tooltip("Assign the player's Transform here. If left empty, the script will try to find a GameObject with tag 'Player'.")]
    public Transform Player;
    public float xPos;
    public float yPos;

    public float zPos;
    void Start()
    {
        if (Player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                Player = p.transform;
                Debug.Log("UIcamara: auto-assigned Player by tag 'Player'.");
            }
            else
            {
                Debug.LogWarning("UIcamara: 'Player' Transform not assigned and no GameObject with tag 'Player' found.");
                return;
            }
        }

        transform.position = new Vector3(Player.position.x + xPos, Player.position.y + yPos, zPos);
    }

    void Update()
    {
        if (Player == null)
            return;

        transform.position = new Vector3(Player.position.x + xPos, Player.position.y + yPos, zPos);

    }
}
