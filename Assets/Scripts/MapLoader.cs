using UnityEngine;
using UnityEngine.Tilemaps;
public class MapLoader : MonoBehaviour
{
//     public LevelSaver tiles;
//     public Tilemap map;
//     void Start()
//     {
//         LoadLevel();
//     }
//     public void LoadLevel()
//     {
//         if (tiles == null || tiles.Length == 0)
//         {
//             Debug.LogWarning("No saved level data to load.");
//             return;
//         }

//         map.ClearAllTiles();

//         int width = tiles.Length;
//         //int height = tiles[0].Length;

//         // Rebuild tiles into the Tilemap
//         for (int x = 0; x < width; x++)
//         {
//             for (int y = 0; y < height; y++)
//             {
//                 TileBase tile = tiles[x][y];
//                 if (tile == null) continue;

//                 Vector3Int cellPos = new Vector3Int(
//                     map.cellBounds.xMin + x,
//                     map.cellBounds.yMin + y,
//                     0
//                 );

//                 map.SetTile(cellPos, tile);
//             }
//         }

//         Debug.Log("Level loaded from 2D tile array.");
//     }
}

