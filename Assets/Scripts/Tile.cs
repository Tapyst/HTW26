using UnityEngine;
using UnityEngine.Tilemaps;
[CreateAssetMenu(fileName = "Tile", menuName = "Scriptable Objects/Tile")]
public class Tile : ScriptableObject
{
    public TileBase tileBase;
    public string methodName = "name";
    public string description = "description";
    public bool isMoveable = false;
    public bool isBreakable = false;
    public bool hasCollision = true;
    public bool controllerMoveable = false;
}
