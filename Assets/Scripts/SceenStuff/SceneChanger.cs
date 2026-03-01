using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Tooltip(tooltip:"Name of the scene that the game should go to")]
    [SerializeField] string destination;
    [Tooltip(tooltip:"The location the player gets put at in the target scene")]
    [SerializeField] Vector2 destinationCordinates;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag =="Player")
        {
            other.gameObject.GetComponent<Respawn>().sceneTransitionSpawnLocation = destinationCordinates;
            SceneManager.LoadSceneAsync(destination);
        }
    }

}
