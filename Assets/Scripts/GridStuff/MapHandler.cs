using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
public class MapHandler : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile[] tiles;
    public List<Vector3Int> controllerPositions = new List<Vector3Int>();
    void Start()
    {

        SearchControllerMoveable();
    }
    void Update()
    {
        InputHandler();
    }
    public void InputHandler()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            moveTiles(Vector3Int.up);
            
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            moveTiles(Vector3Int.left);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            moveTiles(Vector3Int.down);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            moveTiles(Vector3Int.right);
        }
    }
    public void moveTiles(Vector3Int direction)
    {
        for (int i = 0; i < controllerPositions.Count; i++)
        {
            Vector3Int pos = controllerPositions[i];
            if (findTile(pos).controllerMoveable) {
                    Vector3Int newPos = pos + direction;
                    Tile t = findTile(newPos);
                    if (!(t.hasCollision))
                    {
                        tilemap.SetTile(newPos, tilemap.GetTile(pos));
                        tilemap.SetTile(pos, null); 
                        controllerPositions.RemoveAt(i);
                        controllerPositions.Insert(i, newPos);
                    }
                    else if (t.isMoveable) {
                        if (tryMoveTile(newPos, direction))
                        {
                            tilemap.SetTile(newPos, tilemap.GetTile(pos));
                            tilemap.SetTile(pos, null); 
                            controllerPositions.RemoveAt(i);
                            controllerPositions.Insert(i, newPos);
                        }
                    }
                    
            } else {
                Debug.Log("Error");
            }
        } 
    }
    public bool tryMoveTile(Vector3Int position, Vector3Int direction)
    {
        Vector3Int newPos = position + direction;
        Tile t = findTile(newPos);
        if (!(t.hasCollision == true))
        {
            tilemap.SetTile(newPos, tilemap.GetTile(position));
            tilemap.SetTile(position, null); 
            return true;
        } else if (t.isMoveable == true) {
            if (tryMoveTile(newPos, direction))
            {
                tilemap.SetTile(newPos, tilemap.GetTile(position));
                tilemap.SetTile(position, null); 
                return true;
            }
        }
        return false;
    }
    public Tile findTile(Vector3Int position)
    {
        TileBase tile = tilemap.GetTile(position);
        foreach (Tile t in tiles)
        {
            if (tile == t.tileBase)
            {
                return t;
            }
        }
        return tiles[0];
    }
    public void SearchControllerMoveable()
    {
        controllerPositions.Clear();
        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;
        Debug.Log("Tilemap bounds: " + bounds);
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile<TileBase>(position);
                if (findTile(position).controllerMoveable == true)
                {
                    controllerPositions.Add(position);
                }
            }
        }
    }
}
