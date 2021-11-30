using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class MHRuleTile : HexagonalRuleTile<MHRuleTile.Neighbor> {
    public TileBase specificTile;

    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int SpecificTile = 3;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        switch (neighbor) {
            case Neighbor.SpecificTile: return tile == specificTile;
        }
        return base.RuleMatch(neighbor, tile);
    }
}