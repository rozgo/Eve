//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
public static class TileNeighbor {
    // names for each of the 8 surrounding nodes
    // 7 0 1
    // 6 + 2
    // 5 4 3
    public const int North = 0;
    public const int NorthEast = 1;
    public const int East = 2;
    public const int SouthEast = 3;
    public const int South = 4;
    public const int SouthWest = 5;
    public const int West = 6;
    public const int NorthWest = 7;
}
//---------------------------------------------------------------------------------------------------------------------
public class AnnotatedNode {
    public AnnotatedNode () {
        // 8 surrounding nodes - see above for int->direction map
        GameModels = new PathFindingNode[8];
    }
    // Units move along the edges of tiles, and can move between adjacent buildings
    // from the corner of this tile, which of the eight other corners are reachable by the unit
    public PathFindingNode[] GameModels { get; set; }
    // the amount of free space at this node. This is needed for objects that are more than 1x1
    public short Clearance { get; set; }
    // the game model that is occupying the center of this tile
    public PathFindingNode GameModel { get; set; }
    // the game model that stopped the clearance value being any larger. i.e. why we can't fit a 3x3 sized unit in this square
    public PathFindingNode BlockingGameModel { get; set; }
    // how much damage per second can the enemy inflict on this position
    public float DPS { get; set; }

    public short DpsTeamId { get; set; }
}
//---------------------------------------------------------------------------------------------------------------------

