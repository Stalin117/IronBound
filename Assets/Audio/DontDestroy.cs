using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    void Awake()
    {
        // Busca si ya existe un objeto con este tag
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Music");

        // Si ya existe más de uno, destruye este
        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        // Si no, ponle el tag y no lo destruyas
        this.gameObject.tag = "Music";
        DontDestroyOnLoad(this.gameObject);
    }
}