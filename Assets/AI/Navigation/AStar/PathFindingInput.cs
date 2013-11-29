//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public enum SearchPriorityLevel {
    // higher to lower
    Immediate = 0,
    Normal,
    Low,
    NumPriorites
}

public struct FinalPathNode {
    public FinalPathNode ( Vector3 position, byte direction, bool isPivot, bool isJump ) {
        Position = position;
        Direction = direction;
        IsPivotNode = isPivot;
        IsJumpNode = isJump;
    }

    public Vector3 Position;
    public byte Direction;
    // nodes that cannot be removed after the nodes are straightened
    public bool IsPivotNode;
    // nodes are created at the intersection of walls that units can jump over
    public bool IsJumpNode;
}
//---------------------------------------------------------------------------------------------------------------------
public class PathFindingInput {
    public PathFindingInput () {
    }
    //---------------------------------------------------------------------------------------------------------------------
    public Action Callback { get; set; }

    public int StartTileX { get; set; }

    public int StartTileZ { get; set; }

    public int TargetTileX { get; set; }

    public int TargetTileZ { get; set; }

    public float MaxSpeedInTileSpace { get; set; }

    public bool PostProcessStraighten { get; set; }
    // troops can break through buildings, units can break trough walls
    public bool CanDestroyBlockingBuildings { get; set; }

    public bool CanDestroyBlockingWalls { get; set; }
    // how quickly we can destroy a structure that is blocking the path
    public float HitPointsPerSecond { get; set; }

    public float HitPointsPerSecondPreferred { get; set; }

    public AIFlags PreferredTargetAITypeFlags { get; set; }

    public uint TeamId { get; set; }

    public Health Target { get; set; }

    public Hero HeroToAvoid { get; set; }
    // 1 or less will give the shortest path, more than 1 may produce a non optimal path but will be faster to calulcate
    public float HeuristicMultiplier { get; set; }
    // stop searching if we get within this range of the target
    public float StopWithinDistanceFromTargetModel { get; set; }
    // wall breakers, for example, don't care if there are other units in their target square. Other units do care and try not to overcrowd a square
    // or worse still all stand on top of each other.
    public bool IgnoreOthersOccupyingSquare { get; set; }
    // how much units should avoid danger areas - areas around cannon models, for example
    public float AvoidDangerAreaModifier { get; set; }
    // vehicle specific
    public byte InitialDirection { get; set; }
    // 0-7 for the 8 directions
    public uint UnitTileWidth { get; set; }
    // width of the object measured in grid squares
    public uint UnitTileLength { get; set; }
    // length of the object measured in grid squares
    public float TurnCost { get; set; }
    // time to turn 360 degrees
    public bool IsDirectional { get; set; }
}
//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------

