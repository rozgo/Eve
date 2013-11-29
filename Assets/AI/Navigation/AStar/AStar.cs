//---------------------------------------------------------------------------------------------------------------------     

//---------------------------------------------------------------------------------------------------------------------     
//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class AStar : AStarBase {
    //---------------------------------------------------------------------------------------------------------------------
    public AStar ( AnnotatedNode[,] annotatedNodes, UnitTracker unitTracker )
    : base() {
        m_annotatedNodes = annotatedNodes;
        m_unitTracker = unitTracker;
        m_searchSpace = new PathNode[SpaceConversion.MapTiles, SpaceConversion.MapTiles];
        m_cameFrom = new PathNode[SpaceConversion.MapTiles, SpaceConversion.MapTiles];

        for ( byte x = 0; x < SpaceConversion.MapTiles; x++ ) {
            for ( byte y = 0; y < SpaceConversion.MapTiles; y++ ) {
//        DebugConsole.Assert(DebugChannel.AI, m_annotatedNodes[x, y] != null, "gameMap tile is null");
                m_searchSpace[ x, y ] = new PathNode( x, y, 0 );
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override void Clear () {
        base.Clear();

        System.Array.Clear( m_cameFrom, 0, m_cameFrom.Length );
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override PathNode GetStartNode () {
        return m_searchSpace[ m_input.StartTileX, m_input.StartTileZ ];
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override PathNode GetEndNode () {
        return m_searchSpace[ m_input.TargetTileX, m_input.TargetTileZ ];
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override float EvaluateNeighborCost ( PathNode node, PathNode neighbour, bool isEndNode ) {
        float timeToDestroyBlockingModel = 0;
        float jumpCost = 0;

        AnnotatedNode annotatedNode = m_annotatedNodes[ node.X, node.Z ];
        int diffX = neighbour.X - node.X;
        int diffY = neighbour.Z - node.Z;
        int direction = PathHelper.GetDirection( diffX, diffY );

        if ( annotatedNode.GameModels[ direction ] != null ) {
            // there is a model blocking us
            m_output.ModelsPotentiallyBlockingAShorterPath.Add( annotatedNode.GameModels[ direction ] );

            var pathNode = annotatedNode.GameModels[ direction ];

            var wall = pathNode.GetComponent<WallStructure>();

            if ( wall != null && m_input.TeamId == pathNode.TeamId ) {
                // we can jump over it, just add a minor cost
                jumpCost = 4;
            }
            else{
                Structure building = pathNode.GetComponent<Structure>();

                bool canDestroyModel = m_input.CanDestroyBlockingBuildings && ( building != null );
                canDestroyModel |= m_input.CanDestroyBlockingWalls && ( wall != null );

                if ( m_input.TeamId == annotatedNode.GameModels[ direction ].TeamId ||
                canDestroyModel == false ||
                m_input.HitPointsPerSecond <= 0 ) {
                    // we can't destroy this model, so we'll just have to ignore this node
                    return float.MaxValue;
                }

                // look at the neighbour's node model to see the cost to destroy it
                if ( ( m_input.PreferredTargetAITypeFlags & annotatedNode.GameModels[ direction ].AITypeFlags ) != 0 ) {
                    // great, this is our preferred target so we can destroy this model quicker
                    timeToDestroyBlockingModel = annotatedNode.GameModels[ direction ].Health / ( annotatedNode.GameModels[ direction ].DamageDoneToMePerSecond + m_input.HitPointsPerSecondPreferred );
                } else {
                    timeToDestroyBlockingModel = annotatedNode.GameModels[ direction ].Health / ( annotatedNode.GameModels[ direction ].DamageDoneToMePerSecond + m_input.HitPointsPerSecond );
                }
            }
        }


        float closeToOwnHeroCost = 0;

        if ( m_input.HeroToAvoid != null ) {
            float x = neighbour.X * SpaceConversion.MapTileSize;
            float z = neighbour.Z * SpaceConversion.MapTileSize;
            Vector3 hpos = m_input.HeroToAvoid.Position;
            float dx = hpos.x - x;
            float dz = hpos.z - z;
            float d2 = dx * dx + dz * dz;
            if ( d2 < 16 ) {
                closeToOwnHeroCost = 10;
            }
        }

        float neighborDistance = NeighborDistance( node, neighbour );
        float timeToNeighbor = neighborDistance / m_input.MaxSpeedInTileSpace;

        float dps = annotatedNode.DPS * m_input.AvoidDangerAreaModifier;

        if ( m_input.TeamId == annotatedNode.DpsTeamId ) {
            // on the same team as the building guarding this square, so this tile isn't actually a danger
            dps = 0;
        }

        float totalCost = timeToDestroyBlockingModel + timeToNeighbor + closeToOwnHeroCost + dps + jumpCost;

        return totalCost;
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override bool IsNodeOccupiedByOthers ( PathNode node ) {
        // only allow a fixed number of units into this node
        return m_unitTracker.GetNumberOfUnitsGoingToTile( node.X, node.Z ) >= 3;
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override Vector3 AdjustFinalPostion ( PathNode node, Vector3 position ) {
        // give the units some controlled randomness to their final position
        int count = m_unitTracker.GetNumberOfUnitsGoingToTile( node.X, node.Z );
        Vector3 adjusted = position;
    
        // check to see if there is something in this square and only spread out to where there is room
        AnnotatedNode annotatedNode = m_annotatedNodes[ node.X, node.Z ];
        bool top = annotatedNode.GameModels[ TileNeighbor.North ] == null;
        bool right = annotatedNode.GameModels[ TileNeighbor.East ] == null;
        bool topRight = annotatedNode.GameModels[ TileNeighbor.NorthEast ] == null;

        if ( top && right && topRight ) {
            switch ( count ) {
                case 0:
                    {
                        adjusted.x += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.1f, 0.4f );
                        adjusted.z += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.1f, 0.4f );
                        break;
                    }
                case 1:
                    {
                        adjusted.x += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.1f, 0.4f );
                        adjusted.z += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.6f, 0.9f );
                        break;
                    }
                case 2:
                    {
                        adjusted.x += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.6f, 0.9f );
                        adjusted.z += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.1f, 0.4f );
                        break;
                    }
                case 3:
                    {
                        adjusted.x += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.6f, 0.9f );
                        adjusted.z += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.6f, 0.9f );
                        break;
                    }
                default:
                    adjusted.x += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.1f, 0.9f );
                    adjusted.z += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.1f, 0.9f );
                    break;
            }
        } else if ( top ) {
            switch ( count ) {
                case 0:
                    {
                        adjusted.z += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.1f, 0.4f );
                        break;
                    }
                case 1:
                    {
                        adjusted.z += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.6f, 0.9f );
                        break;
                    }
                case 2:
                    {
                        adjusted.z += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.1f, 0.4f );
                        break;
                    }
                case 3:
                    {
                        adjusted.z += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.6f, 0.9f );
                        break;
                    }
                default:
                    adjusted.z += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.1f, 0.9f );
                    break;
            }
        } else if ( right ) {
            switch ( count ) {
                case 0:
                    {
                        adjusted.x += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.1f, 0.4f );
                        break;
                    }
                case 1:
                    {
                        adjusted.x += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.6f, 0.9f );
                        break;
                    }
                case 2:
                    {
                        adjusted.x += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.1f, 0.4f );
                        break;
                    }
                case 3:
                    {
                        adjusted.x += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.6f, 0.9f );
                        break;
                    }
                default:
                    adjusted.x += SpaceConversion.MapTileSize * UnityEngine.Random.Range( 0.1f, 0.9f );
                    break;
            }
        }

        return adjusted;
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override void FindNeighbourNodes ( PathNode node ) {
        FindNeighbourNodesNoDirection( node );
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override bool IsJumpNode ( PathNode previous, PathNode currentNode ) {
        int diffX = currentNode.X - previous.X;
        int diffY = currentNode.Z - previous.Z;
        int direction = PathHelper.GetDirection( diffX, diffY );

        var blocker = m_annotatedNodes[ previous.X, previous.Z ].GameModels[ direction ];

        if ( blocker != null ) {
            var wall = blocker.GetComponent<WallStructure>();

            if ( wall != null && m_input.TeamId == blocker.TeamId ) {
                return true;
            }
        }

        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override void SetBlockingModels ( PathNode previous, PathNode currentNode ) {
        if ( currentNode == null ) {
            return;
        }

        int diffX = currentNode.X - previous.X;
        int diffY = currentNode.Z - previous.Z;
        int direction = PathHelper.GetDirection( diffX, diffY );

        var blocker = m_annotatedNodes[ previous.X, previous.Z ].GameModels[ direction ];

        if ( blocker != null ) {
            var wall = blocker.GetComponent<WallStructure>();
            bool myWall = wall != null && m_input.TeamId == blocker.TeamId;

            if ( !myWall ) {
                if ( m_output.ModelsBlockingPath.Contains( blocker ) == false ) {
                    m_output.ModelsBlockingPath.Add( blocker );
                }
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override PathNode GetCameFrom ( PathNode pathNode ) {
        return m_cameFrom[ pathNode.X, pathNode.Z ];
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override void SetCameFrom ( PathNode node, PathNode neighbor ) {
        m_cameFrom[ neighbor.X, neighbor.Z ] = node;
    }
    //---------------------------------------------------------------------------------------------------------------
    private void FindNeighbourNodesNoDirection ( PathNode node ) {
        int x = node.X;
        int z = node.Z;

        if ( ( x > 0 ) && ( z > 0 ) )
            m_neighborNodes[ TileNeighbor.SouthWest ] = m_searchSpace[ x - 1, z - 1 ];
        else
            m_neighborNodes[ TileNeighbor.SouthWest ] = null;

        if ( z > 0 )
            m_neighborNodes[ TileNeighbor.South ] = m_searchSpace[ x, z - 1 ];
        else
            m_neighborNodes[ TileNeighbor.South ] = null;

        if ( ( x < SpaceConversion.MapTiles - 1 ) && ( z > 0 ) )
            m_neighborNodes[ TileNeighbor.SouthEast ] = m_searchSpace[ x + 1, z - 1 ];
        else
            m_neighborNodes[ TileNeighbor.SouthEast ] = null;

        if ( x < SpaceConversion.MapTiles - 1 )
            m_neighborNodes[ TileNeighbor.East ] = m_searchSpace[ x + 1, z ];
        else
            m_neighborNodes[ TileNeighbor.East ] = null;

        if ( ( x < SpaceConversion.MapTiles - 1 ) && ( z < SpaceConversion.MapTiles - 1 ) )
            m_neighborNodes[ TileNeighbor.NorthEast ] = m_searchSpace[ x + 1, z + 1 ];
        else
            m_neighborNodes[ TileNeighbor.NorthEast ] = null;

        if ( z < SpaceConversion.MapTiles - 1 )
            m_neighborNodes[ TileNeighbor.North ] = m_searchSpace[ x, z + 1 ];
        else
            m_neighborNodes[ TileNeighbor.North ] = null;

        if ( ( x > 0 ) && ( z < SpaceConversion.MapTiles - 1 ) )
            m_neighborNodes[ TileNeighbor.NorthWest ] = m_searchSpace[ x - 1, z + 1 ];
        else
            m_neighborNodes[ TileNeighbor.NorthWest ] = null;

        if ( x > 0 )
            m_neighborNodes[ TileNeighbor.West ] = m_searchSpace[ x - 1, z ];
        else
            m_neighborNodes[ TileNeighbor.West ] = null;
    }

    private PathNode[,] m_searchSpace;
    private PathNode[,] m_cameFrom;
    private UnitTracker m_unitTracker;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

