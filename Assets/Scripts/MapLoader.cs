using UnityEngine;
using UnityEngine.Tilemaps;
public class MapLoader : MonoBehaviour
{
    public LevelSaver[] tileMap;
    void Start()
    {
        if (tileMap == null || tileMap.Length == 0)
        {
            Debug.LogWarning("No LevelSavers assigned in MapLoader!");
        } else
        {
            tileMap[0].tileMap.gameObject.SetActive(true);
        }
    }   
}

