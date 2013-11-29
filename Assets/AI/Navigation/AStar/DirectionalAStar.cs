//---------------------------------------------------------------------------------------------------------------------     

//---------------------------------------------------------------------------------------------------------------------     
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
// see http://www.gamasutra.com/view/feature/3096/toward_more_realistic_pathfinding.php
//---------------------------------------------------------------------------------------------------------------------
public class DirectionalAStar : AStarBase {
    //---------------------------------------------------------------------------------------------------------------------
    public DirectionalAStar ( AnnotatedNode[,] annotatedNodes )
    : base() {
        m_annotatedNodes = annotatedNodes;
        m_searchSpace = new PathNode[SpaceConversion.MapTiles * SpaceConversion.MapTiles * 8];
        m_cameFrom = new PathNode[SpaceConversion.MapTiles * SpaceConversion.MapTiles * 8];

        for ( byte direction = 0; direction < 8; direction++ ) {
            for ( byte x = 0; x < SpaceConversion.MapTiles; x++ ) {
                for ( byte y = 0; y < SpaceConversion.MapTiles; y++ ) {
//          DebugConsole.Assert(DebugChannel.AI, m_annotatedNodes[x, y] != null, "gameMap tile is null");
                    m_searchSpace[ Flatten( x, y, direction ) ] = new PathNode( x, y, direction );
                }
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override void Clear () {
        base.Clear();

        Array.Clear( m_cameFrom, 0, m_cameFrom.Length );
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override PathNode GetStartNode () {
        return m_searchSpace[ Flatten( m_input.StartTileX, m_input.StartTileZ, m_input.InitialDirection ) ];
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override PathNode GetEndNode () {
        return m_searchSpace[ Flatten( m_input.TargetTileX, m_input.TargetTileZ, 0 ) ];
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override PathNode GetCameFrom ( PathNode pathNode ) {
        return m_cameFrom[ Flatten( pathNode.X, pathNode.Z, pathNode.Direction ) ];
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override void SetCameFrom ( PathNode node, PathNode neighbor ) {
        m_cameFrom[ Flatten( neighbor.X, neighbor.Z, neighbor.Direction ) ] = node;
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override float EvaluateNeighborCost ( PathNode node, PathNode neighbour, bool isEndNode ) {
        AnnotatedNode annotatedNode = m_annotatedNodes[ neighbour.X, neighbour.Z ];

        float timeToDestroyBlockingModel = 0;
        bool blockingModelIsWall = false;

        if ( annotatedNode.GameModel != null ) {
            // there is a model at this position
            m_output.ModelsPotentiallyBlockingAShorterPath.Add( annotatedNode.GameModel );

            if ( m_input.TeamId == annotatedNode.GameModel.TeamId ||
            m_input.CanDestroyBlockingBuildings == false ||
            m_input.HitPointsPerSecond <= 0 ) {
                // we can't destroy our own models, so just have to ignore this node
                return float.MaxValue;
            }

            // look at the neighbour's node model to see the cost to destroy it
            if ( ( m_input.PreferredTargetAITypeFlags & annotatedNode.GameModel.AITypeFlags ) != 0 ) {
                // great, this is our preferred target so we can destroy this model quicker
                timeToDestroyBlockingModel = annotatedNode.GameModel.Health / ( annotatedNode.GameModel.DamageDoneToMePerSecond + m_input.HitPointsPerSecondPreferred );
            } else {
                timeToDestroyBlockingModel = annotatedNode.GameModel.Health / ( annotatedNode.GameModel.DamageDoneToMePerSecond + m_input.HitPointsPerSecond );
            }

            blockingModelIsWall = annotatedNode.GameModel.GetComponent<WallStructure>() != default(WallStructure);
        }

        if ( annotatedNode.Clearance < m_input.UnitTileWidth ) {
            // the unit is too big to fit in this square. as above but for the blocking model

            if ( annotatedNode.BlockingGameModel == null ) {
                // blocked by the side of the map
                return float.MaxValue;
            }

            {
                if ( m_input.TeamId == annotatedNode.BlockingGameModel.TeamId ||
                m_input.CanDestroyBlockingBuildings == false ||
                m_input.HitPointsPerSecond <= 0 ) {
                    // we can't destroy our own models, so just have to completely ignore this node
                    return float.MaxValue;
                }

                if ( ( m_input.PreferredTargetAITypeFlags & annotatedNode.BlockingGameModel.AITypeFlags ) != 0 ) {
                    // great, this is our preferred target so we can destroy this model quicker
                    timeToDestroyBlockingModel += annotatedNode.BlockingGameModel.Health / ( annotatedNode.BlockingGameModel.DamageDoneToMePerSecond + m_input.HitPointsPerSecondPreferred );
                } else {
                    timeToDestroyBlockingModel += annotatedNode.BlockingGameModel.Health / ( annotatedNode.BlockingGameModel.DamageDoneToMePerSecond + m_input.HitPointsPerSecond );
                }

                blockingModelIsWall |= annotatedNode.BlockingGameModel.GetComponent<WallStructure>() != default(WallStructure);
            }
        }

        float turnPenalty = 0;

        bool nodeInManoeuvreZone = InManoeuvreZone( node );
        bool neighbourInManoeuvreZone = InManoeuvreZone( neighbour );

        if ( nodeInManoeuvreZone && neighbourInManoeuvreZone ) {
            // find the cost of turning to this neighbour
            int octants = PathHelper.GetOctantsBetweenDirections( node.Direction, neighbour.Direction );
//      DebugConsole.Assert(DebugChannel.AI, octants <= 4 && octants >= 0);

            if ( octants > 1 ) {
                // turning more than 45 degrees

#if TURN_LENGTH_CHECK
        // check there is room to turn around
        if (octants == 4)
        {
          // hold on there, sonny. Trying to do a full 180 degree turn.
          if (m_input.UnitTileLength > annotatedNode.Clearance)
          {
            // the unit is too long to turn 180 in this square
            return float.MaxValue;
          }
        }
#endif

                turnPenalty = ( m_input.TurnCost * 0.125f ) * octants; // *0.125 to get the cost per octant
            } else if ( octants > 0 ) {
                turnPenalty = 0.25f;
            }
        } else if ( !nodeInManoeuvreZone && neighbourInManoeuvreZone ) {
            // penalty for leaving the manoeuvre zone and re-entering it
            turnPenalty = 10 + ( m_input.TurnCost * 0.125f ) * 4;
        }

        // this reduces the time taken to destroy a building so the hero will prefer to destroy a building rather than
        // take an albeit faster route that is much longer
        // this helps speed up the A* search and is more realistic in a battle scenario, i think
        if ( !blockingModelIsWall ) {
            timeToDestroyBlockingModel *= 0.25f;
        } else {
            // walls tend to be attached to other walls so there may be more to destroy. This number basically tunes how much a tank prefers to destroy a wall rather than find an optimal path
            timeToDestroyBlockingModel *= 2.0f; 
        }

        float timeToNeighbor = NeighborDistance( node, neighbour ) / m_input.MaxSpeedInTileSpace;

        float dps = annotatedNode.DPS * m_input.AvoidDangerAreaModifier;
    
        if ( m_input.TeamId == annotatedNode.DpsTeamId ) {
            // on the same team as the building guarding this square, so this tile isn't actually a danger
            dps = 0;
        }

        float cost = timeToNeighbor + turnPenalty + timeToDestroyBlockingModel + dps;

        return cost;
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override void SetBlockingModels ( PathNode previous, PathNode currentNode ) {
        var blocker = m_annotatedNodes[ previous.X, previous.Z ].GameModel;

        if ( blocker != null ) {
            WallStructure wall = blocker.GetComponent<WallStructure>();
            bool myWall = wall != default(WallStructure) && m_input.TeamId == blocker.TeamId;

            if ( !myWall ) {
                if ( m_output.ModelsBlockingPath.Contains( blocker ) == false ) {
                    m_output.ModelsBlockingPath.Add( blocker );
                }
            }
        }

        if ( m_annotatedNodes[ previous.X, previous.Z ].Clearance < m_input.UnitTileWidth ) {
            blocker = m_annotatedNodes[ previous.X, previous.Z ].BlockingGameModel;

            if ( blocker != null ) {
                WallStructure wall = blocker.GetComponent<WallStructure>();
                bool myWall = wall != default(WallStructure) && m_input.TeamId == blocker.TeamId;

                if ( !myWall ) {
                    if ( m_output.ModelsBlockingPath.Contains( blocker ) == false ) {
                        m_output.ModelsBlockingPath.Add( blocker );
                    }
                }
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected bool InManoeuvreZone ( PathNode node ) {
        // directional A* is really expensive. The best thing that it gives is the initial manouvre that avoids turning on the spot
        // it isn't necesary after that
        // So let's limit it to just  a 'initial manouvre zone' of nxn nodes
        int dx = Mathf.Abs( node.X - m_input.StartTileX );
        int dz = Mathf.Abs( node.Z - m_input.StartTileZ );

        if ( dx < 5 && dz < 5 ) {
            return true;
        }

        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override void FindNeighbourNodes ( PathNode node ) {
        if ( InManoeuvreZone( node ) ) {
            FindNeighbourNodesWithDirection( node );
        } else {
            FindNeighbourNodesNoDirection( node );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void FindNeighbourNodesWithDirection ( PathNode inAround ) {
        int x = inAround.X;
        int z = inAround.Z;

        if ( ( x > 0 ) && ( z > 0 ) ) {
            m_neighborNodes[ TileNeighbor.SouthWest ] = m_searchSpace[ Flatten( x - 1, z - 1, TileNeighbor.SouthWest ) ];
        } else {
            m_neighborNodes[ TileNeighbor.SouthWest ] = null;
        }

        if ( z > 0 ) {
            m_neighborNodes[ TileNeighbor.South ] = m_searchSpace[ Flatten( x, z - 1, TileNeighbor.South ) ];
        } else {
            m_neighborNodes[ TileNeighbor.South ] = null;
        }

        if ( ( x < SpaceConversion.MapTiles - 1 ) && ( z > 0 ) ) {
            m_neighborNodes[ TileNeighbor.SouthEast ] = m_searchSpace[ Flatten( x + 1, z - 1, TileNeighbor.SouthEast ) ];
        } else {
            m_neighborNodes[ TileNeighbor.SouthEast ] = null;
        }

        if ( x < SpaceConversion.MapTiles - 1 ) {
            m_neighborNodes[ TileNeighbor.East ] = m_searchSpace[ Flatten( x + 1, z, TileNeighbor.East ) ];
        } else {
            m_neighborNodes[ TileNeighbor.East ] = null;
        }

        if ( ( x < SpaceConversion.MapTiles - 1 ) && ( z < SpaceConversion.MapTiles - 1 ) ) {
            m_neighborNodes[ TileNeighbor.NorthEast ] = m_searchSpace[ Flatten( x + 1, z + 1, TileNeighbor.NorthEast ) ];
        } else {
            m_neighborNodes[ TileNeighbor.NorthEast ] = null;
        }

        if ( z < SpaceConversion.MapTiles - 1 ) {
            m_neighborNodes[ TileNeighbor.North ] = m_searchSpace[ Flatten( x, z + 1, TileNeighbor.North ) ];
        } else {
            m_neighborNodes[ TileNeighbor.North ] = null;
        }

        if ( ( x > 0 ) && ( z < SpaceConversion.MapTiles - 1 ) ) {
            m_neighborNodes[ TileNeighbor.NorthWest ] = m_searchSpace[ Flatten( x - 1, z + 1, TileNeighbor.NorthWest ) ];
        } else {
            m_neighborNodes[ TileNeighbor.NorthWest ] = null;
        }

        if ( x > 0 ) {
            m_neighborNodes[ TileNeighbor.West ] = m_searchSpace[ Flatten( x - 1, z, TileNeighbor.West ) ];
        } else {
            m_neighborNodes[ TileNeighbor.West ] = null;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void FindNeighbourNodesNoDirection ( PathNode node ) {
        int x = node.X;
        int z = node.Z;

        if ( ( x > 0 ) && ( z > 0 ) )
            m_neighborNodes[ TileNeighbor.SouthWest ] = m_searchSpace[ Flatten( x - 1, z - 1, 0 ) ];
        else
            m_neighborNodes[ TileNeighbor.SouthWest ] = null;

        if ( z > 0 )
            m_neighborNodes[ TileNeighbor.South ] = m_searchSpace[ Flatten( x, z - 1, 0 ) ];
        else
            m_neighborNodes[ TileNeighbor.South ] = null;

        if ( ( x < SpaceConversion.MapTiles - 1 ) && ( z > 0 ) )
            m_neighborNodes[ TileNeighbor.SouthEast ] = m_searchSpace[ Flatten( x + 1, z - 1, 0 ) ];
        else
            m_neighborNodes[ TileNeighbor.SouthEast ] = null;

        if ( x < SpaceConversion.MapTiles - 1 )
            m_neighborNodes[ TileNeighbor.East ] = m_searchSpace[ Flatten( x + 1, z, 0 ) ];
        else
            m_neighborNodes[ TileNeighbor.East ] = null;

        if ( ( x < SpaceConversion.MapTiles - 1 ) && ( z < SpaceConversion.MapTiles - 1 ) )
            m_neighborNodes[ TileNeighbor.NorthEast ] = m_searchSpace[ Flatten( x + 1, z + 1, 0 ) ];
        else
            m_neighborNodes[ TileNeighbor.NorthEast ] = null;

        if ( z < SpaceConversion.MapTiles - 1 )
            m_neighborNodes[ TileNeighbor.North ] = m_searchSpace[ Flatten( x, z + 1, 0 ) ];
        else
            m_neighborNodes[ TileNeighbor.North ] = null;

        if ( ( x > 0 ) && ( z < SpaceConversion.MapTiles - 1 ) )
            m_neighborNodes[ TileNeighbor.NorthWest ] = m_searchSpace[ Flatten( x - 1, z + 1, 0 ) ];
        else
            m_neighborNodes[ TileNeighbor.NorthWest ] = null;

        if ( x > 0 )
            m_neighborNodes[ TileNeighbor.West ] = m_searchSpace[ Flatten( x - 1, z, 0 ) ];
        else
            m_neighborNodes[ TileNeighbor.West ] = null;
    }
    //---------------------------------------------------------------------------------------------------------------------
    private int Flatten ( int a, int b, int c ) {
        // AOT does not support 3 dimensional arrays so this function is just to convert from 3d to 1d array
        return a + ( b * SpaceConversion.MapTiles ) + ( c * SpaceConversion.MapTiles * SpaceConversion.MapTiles );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private PathNode[] m_searchSpace;
    private PathNode[] m_cameFrom;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

