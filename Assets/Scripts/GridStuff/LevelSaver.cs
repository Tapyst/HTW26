using UnityEngine;
using UnityEngine.Tilemaps;
public class LevelSaver : MonoBehaviour
{
    // public Tilemap map;
    // public TileBase[][] tiles;
    // void Start()
    // {
    //     SaveLevel();
    // }
    // public void SaveLevel()
    // {
    //     BoundsInt bounds = map.cellBounds;

    //     int width = bounds.size.x;
    //     int height = bounds.size.y;

    //     // Initialize jagged array
    //     tiles = new TileBase[width][];
    //     for (int x = 0; x < width; x++)
    //     {
    //         tiles[x] = new TileBase[height];
    //     }
    //     for (int x = 0; x < width; x++)
    //     {
    //         for (int y = 0; y < height; y++)
    //         {
    //             Vector3Int cellPos = new Vector3Int(
    //                 bounds.xMin + x,
    //                 bounds.yMin + y,
    //                 0
    //             );

    //             tiles[x][y] = map.GetTile(cellPos);
    //         }
    //     }
    //     Debug.Log("Level saved with dimensions: " + width + "x" + height);
    // }
}
