using UnityEngine;
using UnityEngine.SceneManagement;

public class Respawn : MonoBehaviour
{
    public Vector2 sceneTransitionSpawnLocation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
         // Subscribe to the event 
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
    // ALWAYS unsubscribe to prevent MissingReferenceException 
    SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameObject.transform.position = new Vector3(sceneTransitionSpawnLocation.x,sceneTransitionSpawnLocation.y,0);
    }

}
