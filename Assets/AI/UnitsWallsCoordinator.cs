//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class UnitsWallsCoordinator {
    // the Units and Walls Coordinator
    // i) keeps track of which wall breakers are targetting which walls
    // if a wall is destroyed then update the target for any wall breakers that were targetting it
    // if a wall breaker is killed then free up that wall as a potential target again
    // ii) coordinates between walls and units, so that if a wall is destryoed or moved,
    // and that wall was touched during the unit's last A* search then it forces the unit to repath
    // This helps make Units follow optimal paths when they are en route and the scene's wall state changes
    //---------------------------------------------------------------------------------------------------------------------
    public UnitsWallsCoordinator () {
        m_wallBreakerToWallTargetMap = new Dictionary<int, WallStructure>();
        m_openList = new List<int>();
        m_closed = new bool [SpaceConversion.MapTiles, SpaceConversion.MapTiles];
        Walls = new WallStructure[SpaceConversion.MapTiles, SpaceConversion.MapTiles];
        m_blockingWallToUnitsMap = new Dictionary<WallStructure, List<Goto>>();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnModelChanged ( object sender, EventArgs e ) {
        //TODO://Handle events
//    Wall wallCausingUnitsToRepath = null;
//
//    if (e is ModelDestroyArgs)
//    {
//      WallModel wall = sender as WallModel;
//      if (wall != null)
//      {
//        List<int> toRemove = new List<int>();
//        foreach (KeyValuePair<int, WallModel> pair in m_wallBreakerToWallTargetMap)
//        {
//          if (pair.Value.InstanceId == wall.InstanceId)
//          {
//            toRemove.Add(pair.Key);
//          }
//        }
//
//        foreach (int key in toRemove)
//        {
//          m_wallBreakerToWallTargetMap.Remove(key);
//        }
//
//        int x, z;
//        SpaceConversion.GetMapTileFromWorldPosition(wall.Position, out x, out z);
//
//        Walls[x, z] = null;
//
//        wallCausingUnitsToRepath = wall;
//      }
//    }
//    else if (e is PositionChangedEventArgs)
//    { 
//       WallModel wall = sender as WallModel;
//       if (wall != null)
//       {
//         wallCausingUnitsToRepath = wall;
//       }
//    }
//
//    if (wallCausingUnitsToRepath != null)
//    {
//      for (int i = 0; i < m_blockingWallToUnitsMap[wallCausingUnitsToRepath].Count; i++)
//      {
//        Goto g = m_blockingWallToUnitsMap[wallCausingUnitsToRepath][i];
//
//        g.RePath();
//      }
//    }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnAttach ( object sender ) {
//    WallModel wall = sender as WallModel;
//    DebugConsole.Assert(DebugChannel.AI, wall != null, "expected wall model");
//
//    if (wall != null)
//    {
//      int x, z;
//      SpaceConversion.GetMapTileFromWorldPosition(wall.Position, out x, out z);
//
//      Walls[x, z] = wall;
//
//      m_blockingWallToUnitsMap.Add(wall, new List<Goto>());
//    }
    }
    //---------------------------------------------------------------------------------------------------------------------

    //---------------------------------------------------------------------------------------------------------------------
    public WallStructure GetWallTargetForWallbreaker ( int wallbreakerId ) {
        if ( m_wallBreakerToWallTargetMap.ContainsKey( wallbreakerId ) ) {
            return m_wallBreakerToWallTargetMap[ wallbreakerId ];
        }
        return null;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void SetWallTargetForWallbreaker ( int wallBreakerId, WallStructure wallModel ) {
        m_wallBreakerToWallTargetMap.Add( wallBreakerId, wallModel );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public bool IsFree ( WallStructure wallModel ) {
        return ( m_wallBreakerToWallTargetMap.ContainsValue( wallModel ) == false );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void RemoveWallTargetForWallbreaker ( int wallBreakerId ) {
        m_wallBreakerToWallTargetMap.Remove( wallBreakerId );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public WallStructure FindNearestWall ( int tileX, int tileZ ) {
        m_openList.Clear();
        Array.Clear( m_closed, 0, m_closed.Length );

        m_openList.Add( SpaceConversion.GetTileIdFromCoordinates( tileX, tileZ ) );
        m_closed[ tileX, tileZ ] = true;

        while ( m_openList.Count > 0 ) {
            int id = m_openList[ 0 ];
            m_openList.RemoveAt( 0 );

            int x, z;
            SpaceConversion.GetTileCoordinatesFromId( id, out x, out z );

            if ( Walls[ x, z ] != null && IsFree( Walls[ x, z ] ) ) {
                return Walls[ x, z ];
            }

            // add neghbours
            if ( x > 0 ) {
                if ( m_closed[ x - 1, z ] == false ) {
                    m_openList.Add( SpaceConversion.GetTileIdFromCoordinates( x - 1, z ) );
                    m_closed[ x - 1, z ] = true;
                }
            }
            if ( x < SpaceConversion.MapTiles - 1 ) {
                if ( m_closed[ x + 1, z ] == false ) {
                    m_openList.Add( SpaceConversion.GetTileIdFromCoordinates( x + 1, z ) );
                    m_closed[ x + 1, z ] = true;
                }
            }

            if ( z > 0 ) {
                if ( m_closed[ x, z - 1 ] == false ) {
                    m_openList.Add( SpaceConversion.GetTileIdFromCoordinates( x, z - 1 ) );
                    m_closed[ x, z - 1 ] = true;
                }
            }
            if ( z < SpaceConversion.MapTiles - 1 ) {
                if ( m_closed[ x, z + 1 ] == false ) {
                    m_openList.Add( SpaceConversion.GetTileIdFromCoordinates( x, z + 1 ) );
                    m_closed[ x, z + 1 ] = true;
                }
            }
        }

        return null;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void UnRegisterBlockingWallList ( Goto g, List<PathFindingNode> oldlist ) {
        foreach ( var node in oldlist ) {
            var wall = node.GetComponent<WallStructure>();
            if ( wall != default(WallStructure) ) {
                m_blockingWallToUnitsMap[ wall ].Remove( g );
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void RegisterBlockingWallList ( Goto g, List<PathFindingNode> newlist ) {
        foreach ( var node in newlist ) {
            var wall = node.GetComponent<WallStructure>();
            if ( wall != default(WallStructure) ) {
//        DebugConsole.Assert(DebugChannel.AI, m_blockingWallToUnitsMap[wall].Contains(g) == false, "expected to not have Unit in list");
                if ( m_blockingWallToUnitsMap[ wall ].Contains( g ) == false ) {
                    m_blockingWallToUnitsMap[ wall ].Add( g );
                }
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public WallStructure[,] Walls { get; private set; }
    //---------------------------------------------------------------------------------------------------------------------
    private List<int> m_openList;
    private bool[,] m_closed;
    private Dictionary<int, WallStructure> m_wallBreakerToWallTargetMap;
    // If a wall is destroyed or moved then Unit that was blocked by that Wall on its previous Goto
    // may now be able to find a more optimal path. // This dictionary keeps track of that map
    private Dictionary<WallStructure, List<Goto>> m_blockingWallToUnitsMap;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

