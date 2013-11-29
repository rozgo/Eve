//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class ClearanceMap {
    // see http://aigamedev.com/open/tutorials/clearance-based-pathfinding/
    // Units have different radii and so some can take paths that others cannot
    // For example, a Unit that is 3x3 will not be able to fit through a 1x1 gap
    //---------------------------------------------------------------------------------------------------------------------
    public ClearanceMap ( AnnotatedNode[,] annotatedNodes, int[,] baseOutlineStructureIds ) {
        AnnotatedNodes = annotatedNodes;
	
        m_baseOutlineStructureIds = baseOutlineStructureIds;
        m_instanceIdToBlockedSqauresMap = new Dictionary<int, List<short>>();
        m_isReady = false;

        BaseOutlineStructureIdsView = new BaseOutlineView( SpaceConversion.MapTiles, SpaceConversion.MapTiles, m_baseOutlineStructureIds );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Update ( float deltaTime ) {
        BaseOutlineStructureIdsView.Update( deltaTime );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void RecalulateBaseOutline () {
        BaseOutlineStructureIdsView.Recalculate();
    }
    //---------------------------------------------------------------------------------------------------------------------
    //Reset timer to show buildings boundaries
    public void ResetBaseOutlineTimer () {
        BaseOutlineStructureIdsView.ResetTimer();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void CalculateClearance () {
//    DebugConsole.Assert(DebugChannel.AI, m_isReady == false, "CalculateClearance should only be called once on scene start up");

        for ( int i = 0; i < SpaceConversion.MapTiles; i++ ) {
            for ( int j = 0; j < SpaceConversion.MapTiles; j++ ) {
                CalculateClearance( i, j );
            }
        }
    
        RecalulateBaseOutline();
        m_isReady = true;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public bool GetOpenSquareNearToPosition ( int maxDimensionInGridSquares, Vector3 position, out int x, out int z ) {
//    DebugConsole.Assert(DebugChannel.AI, m_isReady, "ClearanceMap not ready - has CalculateClearance been called?");
//    DebugConsole.Assert(DebugChannel.AI, maxDimensionInGridSquares <= m_maxClearance, "increase m_maxClearance");

        x = 0;
        z = 0;
        int rootX, rootZ;
        SpaceConversion.GetMapTileFromWorldPosition( position, out rootX, out rootZ );
        int max = Math.Max( SpaceConversion.MapTiles, SpaceConversion.MapTiles ) * 2; // rootX, rootZ can be negative
        int size = maxDimensionInGridSquares;

        // traverse the grid by creating ever bigger concentric 'rings' around the initial position
        // this is somewhat inefficient as there are no early-outs for hitting the map boundaries

        for ( int length = 2; length < max; length += 2 ) {
            //XXXO
            //OOOO
            //OOOO
            //OOOO
            for ( int i = 0; i < length; i++ ) {
                x = rootX + i;
                z = rootZ;

                if ( CheckClearance( ref x, ref z, size ) ) {
                    return true;
                }
            }

            //OOOX
            //OOOX
            //OOOX
            //OOOO
            for ( int i = 0; i < length; i++ ) {
                x = rootX + length;
                z = rootZ + i;

                if ( CheckClearance( ref x, ref z, size ) ) {
                    return true;
                }
            }

            //OOOO
            //OOOO
            //OOOO
            //OXXX
            for ( int i = 1; i <= length; i++ ) {
                x = rootX + i;
                z = rootZ + length;

                if ( CheckClearance( ref x, ref z, size ) ) {
                    return true;
                }
            }

            //OOOO
            //XOOO
            //XOOO
            //XOOO
            for ( int i = 1; i <= length; i++ ) {
                x = rootX;
                z = rootZ + i;

                if ( CheckClearance( ref x, ref z, size ) ) {
                    return true;
                }
            }

            // move to the next corner, R
            //ROOOO
            //OOOOO
            //OOOOO
            //OOOOO
            //OOOOO
            rootX -= 1;
            rootZ -= 1;
        }

//    DebugConsole.Assert(DebugChannel.AI, false, "No more room on map!");
        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Destroy () {
        BaseOutlineStructureIdsView.Destroy();
    }
    //---------------------------------------------------------------------------------------------------------------------
    private bool CheckClearance ( ref int x, ref int z, int size ) {
        if ( x < 0 || x + size >= SpaceConversion.MapTiles || z < 0 || z + size >= SpaceConversion.MapTiles ) {
            // out of bounds
            return false;
        }

        if ( AnnotatedNodes[ x, z ].Clearance >= size ) {
            return true;
        }

        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void AddGameModel ( int x, int z, PathFindingNode model ) {
        if ( model.AnchoredInScene ) {
            // this model is not a placeable model
            return;
        }

        SetSquares( x, z, model );

        if ( m_isReady ) {
            // structure added during game
            // update just the local clearance map
            UpdateClearance( x, z, model.Width, model.Length );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void UpdateGameModel ( int prevX, int prevZ, int x, int z, PathFindingNode model ) {
        if ( model.AnchoredInScene ) {
            // this model is not a placeable model
            return;
        }
        if ( !( model is Trap ) ) {
            RemoveGameModel( prevX, prevZ, model );
            AddGameModel( x, z, model );
        }

        UpdateClearance( prevX, prevZ, model.Width, model.Length );
        UpdateClearance( x, z, model.Width, model.Length );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void RemoveGameModel ( int posX, int posZ, PathFindingNode node ) {
        if ( node.AnchoredInScene ) {
            // this model is not a placeable model
            return;
        }

//    DebugConsole.Assert(DebugChannel.AI, m_isReady, "ClearanceMap not ready - has CalculateClearance been called?");
//    DebugConsole.Assert(DebugChannel.AI, m_instanceIdToBlockedSqauresMap.ContainsKey(model.InstanceId), "m_instanceIdToBlockedSqauresMap does not contain " + model.Definition.NameInternal);

        if ( m_instanceIdToBlockedSqauresMap.ContainsKey( node.InstanceId ) == false ) {
            return;
        }

        // clear which squares this building is on top of
        ClearSquares( node, posX, posZ, ( int )node.Width, ( int )node.Length );

        // recalculate the clearance for each of the squares that clearance was restricted by this building
        List<short> squaresBlockedByThisObject = m_instanceIdToBlockedSqauresMap[ node.InstanceId ];

        foreach ( int squareId in squaresBlockedByThisObject ) {
            int x, z;
            SpaceConversion.GetTileCoordinatesFromId( squareId, out x, out z );

            CalculateClearance( x, z );
        }

        // finally, remove this building from the map
        m_instanceIdToBlockedSqauresMap.Remove( node.InstanceId );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void DebugDraw () {
        Color[] cols = new Color[6] {
            Color.black,
            Color.red,
            new Color( 1, 0.5f, 0 ),
            Color.yellow,
            Color.green,
            Color.white
        };


      for (int i = 0; i < SpaceConversion.MapTiles; i++)
      {
        for (int j = 0; j < SpaceConversion.MapTiles; j++)
        {
          Vector3 worldPos = SpaceConversion.GetWorldPositionFromMapTile(i, j);

          Debug.DrawLine(worldPos, worldPos + Vector3.up * AnnotatedNodes[i, j].Clearance, cols[AnnotatedNodes[i, j].Clearance], 0.1f);

          if (AnnotatedNodes[i, j].BlockingGameModel != null)
          {
            Debug.DrawLine(worldPos, AnnotatedNodes[i, j].BlockingGameModel.Center, Color.blue, 0.1f);
          }
        }
      }
    
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void UpdateClearance ( int x, int z, uint width, uint length ) {
        for ( int i = x - m_maxClearance; i < x + width; i++ ) {
            if ( i < 0 || i >= SpaceConversion.MapTiles ) {
                continue;
            }

            for ( int j = z - m_maxClearance; j < z + length; j++ ) {
                if ( j < 0 || j >= SpaceConversion.MapTiles ) {
                    continue;
                }

                CalculateClearance( i, j );
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void SetSquares ( int posX, int posZ, PathFindingNode model ) {
        int width = ( int )model.Width;
        int length = ( int )model.Length;
  
        if ( posX < 0 || posZ < 0 || posX + width >= SpaceConversion.MapTiles || posZ + length >= SpaceConversion.MapTiles ) {
//      DebugConsole.Assert(DebugChannel.AI, false, String.Format("trying to place object off map {0},{1}, {2}", posX, posZ, model.Definition.Id));
            return;
        }

        // similarly for pathfinding but allow border
        int borderForPathFinding = 0;// Math.Max(1, width - 1);
        for ( int i = posX; i < posX + width - borderForPathFinding; i++ ) {
            for ( int j = posZ; j < posZ + length - borderForPathFinding; j++ ) {
//        DebugConsole.Assert(DebugChannel.AI, null == AnnotatedNodes[i, j].GameModel || AnnotatedNodes[i, j].GameModel == model, String.Format("clearance map error: AnnotatedNodes overwriting occupied square {0},{1}", i, j));
                AnnotatedNodes[ i, j ].GameModel = model;
            }
        }

        // lastly keep track of the base outline which is an additional area surrounding the building
        for ( int i = posX - outlineWidth; i < posX + width + outlineWidth; i++ ) {
            if ( i < 0 || i >= SpaceConversion.MapTiles ) {
                continue;
            }

            for ( int j = posZ - outlineWidth; j < posZ + length + outlineWidth; j++ ) {
                if ( j < 0 || j >= SpaceConversion.MapTiles ) {
                    continue;
                }

                m_baseOutlineStructureIds[ i, j ]++;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void ClearSquares ( PathFindingNode model, int posX, int posZ, int width, int length ) {
        for ( int i = posX; i < posX + width; i++ ) {
            for ( int j = posZ; j < posZ + length; j++ ) {
                if ( model != AnnotatedNodes[ i, j ].GameModel ) {
//          DebugConsole.Assert(DebugChannel.AI, false, "clearance map error: clearing unexpected square");
                }

                AnnotatedNodes[ i, j ].GameModel = null;
                AnnotatedNodes[ i, j ].Clearance = m_maxClearance;
            }
        }

        // clear the base outline also
        for ( int i = posX - outlineWidth; i < posX + width + outlineWidth; i++ ) {
            if ( i < 0 || i >= SpaceConversion.MapTiles ) {
                continue;
            }

            for ( int j = posZ - outlineWidth; j < posZ + length + outlineWidth; j++ ) {
                if ( j < 0 || j >= SpaceConversion.MapTiles ) {
                    continue;
                }

                m_baseOutlineStructureIds[ i, j ] -= 1;

//        DebugConsole.Assert(DebugChannel.AI, m_baseOutlineStructureIds[i, j] >= 0, "error with base outline");
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private bool ProcessSquare ( int x, int z, int tx, int tz, short radius ) {
        var model = AnnotatedNodes[ tx, tz ].GameModel;

        if ( model != null ) {
            if (model.AnchoredInScene ) {
                // this model is not a placeable model
                return false;
            }

            // hit a building :(
            AnnotatedNodes[ x, z ].Clearance = radius;
            AnnotatedNodes[ x, z ].BlockingGameModel = model;

            if ( m_instanceIdToBlockedSqauresMap.ContainsKey( model.InstanceId ) == false ) {
                m_instanceIdToBlockedSqauresMap.Add( model.InstanceId, new List<short>() );
            }

            m_instanceIdToBlockedSqauresMap[ model.InstanceId ].Add( SpaceConversion.GetTileIdFromCoordinates( x, z ) );

            return true;
        }

        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void CalculateClearance ( int x, int z ) {
        short radius = 0;

        while ( radius <= m_maxClearance ) {
            for ( int i = 0; i <= radius && i + x < SpaceConversion.MapTiles && radius + z < SpaceConversion.MapTiles; i++ ) {
                if ( ProcessSquare( x, z, i + x, radius + z, radius ) ) {
                    return;
                }
            }

            for ( int j = 0; j < radius && j + z < SpaceConversion.MapTiles && radius + x < SpaceConversion.MapTiles; j++ ) {
                if ( ProcessSquare( x, z, radius + x, j + z, radius ) ) {
                    return;
                }
            }

            radius++;
        }

        if ( x >= 0 && x < SpaceConversion.MapTiles && z >= 0 && z < SpaceConversion.MapTiles ) {
            AnnotatedNodes[ x, z ].Clearance = m_maxClearance;
            AnnotatedNodes[ x, z ].BlockingGameModel = null;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public AnnotatedNode[,] AnnotatedNodes { get; private set; }

    private int[,] m_baseOutlineStructureIds;
    private BaseOutlineView BaseOutlineStructureIdsView;
    private const int m_maxClearance = 5;
    private const int outlineWidth = 1;
    // a dictionary of all the squares whose clearance was restricted by the structure (indetified via its InstanceId)
    // this is used for a fast way to update the clearance map when a structure is destroyed
    private Dictionary<int, List<short>> m_instanceIdToBlockedSqauresMap;
    // for debug assert
    private bool m_isReady;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

