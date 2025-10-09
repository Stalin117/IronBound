using UnityEngine;

public class ObjectDestroy : MonoBehaviour
{
    public float secondstoDestroy;
    private void Start()
    {
        Destroy(gameObject, secondstoDestroy);
    }
        
    
}
