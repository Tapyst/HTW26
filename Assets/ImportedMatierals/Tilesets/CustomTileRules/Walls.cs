using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "CustomTools/Custom Tiles/Wall Rule Tile")]
public class Walls : RuleTile<Walls.Neighbor> {
    [Tooltip("If enabled, the tile will connect to these tiles too when the mode is set to \"This\"")]
    public bool allwaysConnect;
    [Tooltip("Check itseft when the mode is set to \"any\"")]
    public bool checkSelf;
    [Tooltip("Wall tiles")]
    public TileBase[] wallTile;

    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int Any = 3;
        public const int Specified = 4;
        public const int Nothing = 5;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        switch (neighbor) {
            case Neighbor.This: return Check_This(tile);
            case Neighbor.NotThis: return Check_NotThis(tile);
            case Neighbor.Any: return Check_Any(tile);
            case Neighbor.Specified: return Check_Specified(tile);
            case Neighbor.Nothing: return Check_Nothing(tile);
        }
        return base.RuleMatch(neighbor, tile);
    }
    bool Check_This(TileBase tile)
    {
        if(!allwaysConnect) return tile ==this;
        else return wallTile.Contains(tile)||tile==this;
    }
    bool Check_NotThis(TileBase tile)
    {
        if(!allwaysConnect) return tile !=this;
        else return tile!=this&&!wallTile.Contains(tile);
    }
    bool Check_Any(TileBase tile)
    {
        if(checkSelf) return tile!=null;
        else return tile!=null&&tile!= this;
    }
        bool Check_Specified(TileBase tile)
    {
        return wallTile.Contains(tile);
    }
    bool Check_Nothing(TileBase tile)
    {
        return tile==null;
    }
}

