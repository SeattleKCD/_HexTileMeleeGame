using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class HexTileData : ScriptableObject
{
    public TileBase[] tiles;

    public int movementCost;
    public bool isWalkable;
}
