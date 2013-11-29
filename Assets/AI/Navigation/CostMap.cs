//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class CostMap {
    //---------------------------------------------------------------------------------------------------------------------
    public CostMap ( AnnotatedNode[,] annotatedNodes ) {
        m_annotatedNodes = annotatedNodes;
        m_isWall = new bool[SpaceConversion.MapTiles, SpaceConversion.MapTiles];
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void AddStructure ( PathFindingNode node ) {
        var structure = node.GetComponent<Structure>();

        if ( structure != default(Structure) ) {
            AddBuilding( node );
        } else {
            var wall = structure.GetComponent<WallStructure>();
            if ( wall != default(WallStructure) ) {
                AddWall( node, wall );
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void RemoveStructure ( Vector3 previousPositon, PathFindingNode node ) {

        var structure = node.GetComponent<Structure>();

        if ( structure != default(Structure) ) {
            RemoveBuilding( previousPositon, node );
        } else {
            var wall = structure.GetComponent<WallStructure>();
            if ( wall != default(WallStructure) ) {
                RemoveWall( node.Position );
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void AddToDPS ( Cannon cannon, PathFindingNode node ) {
        UpdateDPS( cannon, node, true );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void AddBuilding ( PathFindingNode node ) {
        UpdateBuildingNodes( node.Position, ( int )node.Width, ( int )node.Length, node );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void AddWall ( PathFindingNode node , WallStructure wall) {
        UpdateWallNodes( node.Position, wall );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void RemoveBuilding ( Vector3 previousPositon, PathFindingNode node ) {
        UpdateBuildingNodes( previousPositon, ( int )node.Width, ( int )node.Length, null );

        Cannon cannon = node.GetComponent<Cannon>();
        if ( cannon != null ) {
            UpdateDPS( cannon, node, false );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void RemoveWall ( Vector3 previousPosition ) {
        UpdateWallNodes( previousPosition, null );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void DebugDraw () {
        float scale = SpaceConversion.MapTileSize / 2.5f;
        float diag = Mathf.Sqrt( 2.0f ) / 2.5f;
        float duration = 0.1f;

        for ( int i = 0; i <= SpaceConversion.MapTiles; i++ ) {
          for (int j = 0; j <= SpaceConversion.MapTiles; j++) {
            Vector3 worldPos = SpaceConversion.GetWorldPositionFromMapTile (i, j);
            Debug.DrawLine (worldPos, worldPos + Vector3.forward * scale, m_annotatedNodes [i, j].GameModels [TileNeighbor.North] == null ? Color.white : Color.black, duration);
            Debug.DrawLine (worldPos, worldPos + Vector3.forward * diag + Vector3.right * diag, m_annotatedNodes [i, j].GameModels [TileNeighbor.NorthEast] == null ? Color.white : Color.black, duration);
            Debug.DrawLine (worldPos, worldPos + Vector3.right * scale, m_annotatedNodes [i, j].GameModels [TileNeighbor.East] == null ? Color.white : Color.black, duration);
            Debug.DrawLine (worldPos, worldPos - Vector3.forward * diag + Vector3.right * diag, m_annotatedNodes [i, j].GameModels [TileNeighbor.SouthEast] == null ? Color.white : Color.black, duration);
            Debug.DrawLine (worldPos, worldPos - Vector3.forward * scale, m_annotatedNodes [i, j].GameModels [TileNeighbor.South] == null ? Color.white : Color.black, duration);
            Debug.DrawLine (worldPos, worldPos - Vector3.forward * diag - Vector3.right * diag, m_annotatedNodes [i, j].GameModels [TileNeighbor.SouthWest] == null ? Color.white : Color.black, duration);
            Debug.DrawLine (worldPos, worldPos - Vector3.right * scale, m_annotatedNodes [i, j].GameModels [TileNeighbor.West] == null ? Color.white : Color.black, duration);
            Debug.DrawLine (worldPos, worldPos + Vector3.forward * diag - Vector3.right * diag, m_annotatedNodes [i, j].GameModels [TileNeighbor.NorthWest] == null ? Color.white : Color.black, duration);
            var gameModel = m_annotatedNodes[ i, j ].GameModel;
          }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void UpdateWallNodes ( Vector3 position, WallStructure wall ) {
        int x, z;

        var node = wall.GetComponent<PathFindingNode>();

        SpaceConversion.GetMapTileFromWorldPosition( position, out x, out z );

        m_isWall[ x, z ] = ( wall != default(WallStructure) );
        m_annotatedNodes[ x, z ].GameModel = node;

        m_annotatedNodes[ x, z ].GameModels[ TileNeighbor.NorthEast ] = node;

        if ( x < SpaceConversion.MapTiles - 1 ) {
            m_annotatedNodes[ x + 1, z ].GameModels[ TileNeighbor.NorthWest ] = node;

            if ( m_isWall[ x + 1, z ] ) {
                m_annotatedNodes[ x + 1, z ].GameModels[ TileNeighbor.North ] = node;

                if ( z < SpaceConversion.MapTiles - 1 ) {
                    m_annotatedNodes[ x + 1, z + 1 ].GameModels[ TileNeighbor.South ] = node;
                }
            }
        }

        if ( x < SpaceConversion.MapTiles - 1 && z < SpaceConversion.MapTiles - 1 ) {
            m_annotatedNodes[ x + 1, z + 1 ].GameModels[ TileNeighbor.SouthWest ] = node;
        }

        if ( z < SpaceConversion.MapTiles - 1 ) {
            m_annotatedNodes[ x, z + 1 ].GameModels[ TileNeighbor.SouthEast ] = node;

            if ( m_isWall[ x, z + 1 ] ) {
                m_annotatedNodes[ x, z + 1 ].GameModels[ TileNeighbor.East ] = node;

                if ( x < SpaceConversion.MapTiles - 1 ) {
                    m_annotatedNodes[ x + 1, z + 1 ].GameModels[ TileNeighbor.West ] = node;
                }
            }
        }

        if ( x > 0 ) {
            if ( m_isWall[ x - 1, z ] ) {
                m_annotatedNodes[ x, z ].GameModels[ TileNeighbor.North ] = node;

                if ( z < SpaceConversion.MapTiles - 1 ) {
                    m_annotatedNodes[ x, z + 1 ].GameModels[ TileNeighbor.South ] = node;
                }
            }
        }

        if ( z > 0 ) {
            if ( m_isWall[ x, z - 1 ] ) {
                m_annotatedNodes[ x, z ].GameModels[ TileNeighbor.East ] = node;

                if ( x < SpaceConversion.MapTiles - 1 ) {
                    m_annotatedNodes[ x + 1, z ].GameModels[ TileNeighbor.West ] = node;
                }
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void UpdateBuildingNodes ( Vector3 position, int width, int length, PathFindingNode node ) {
        int x, z;
        SpaceConversion.GetMapTileFromWorldPosition( position, out x, out z );

        for ( int i = 0; i < width; i++ ) {
            for ( int j = 0; j < length; j++ ) {
                m_annotatedNodes[ x + i, z + j ].GameModel = node;
            }
        }

        // everything inside the boundary is set to have no cost
        // this allows for the player moving a building ontop of a unit and it still being able to path out
        for ( int i = 1; i < width; i++ ) {
            for ( int j = 1; j < length; j++ ) {
                for ( int k = 0; k < 8; k++ ) {
                    m_annotatedNodes[ x + i, z + j ].GameModels[ k ] = null;
                }
            }
        }

        // bottom row
        for ( int i = 1; i < width; i++ ) {
            m_annotatedNodes[ x + i, z ].GameModels[ TileNeighbor.NorthWest ] = node;
            m_annotatedNodes[ x + i, z ].GameModels[ TileNeighbor.North ] = node;
            m_annotatedNodes[ x + i, z ].GameModels[ TileNeighbor.NorthEast ] = node;
        }

        // top row
        for ( int i = 1; i < width; i++ ) {
            m_annotatedNodes[ x + i, z + ( int )length ].GameModels[ TileNeighbor.SouthWest ] = node;
            m_annotatedNodes[ x + i, z + ( int )length ].GameModels[ TileNeighbor.South ] = node;
            m_annotatedNodes[ x + i, z + ( int )length ].GameModels[ TileNeighbor.SouthEast ] = node;
        }

        // left column
        for ( int j = 1; j < length; j++ ) {
            m_annotatedNodes[ x, z + j ].GameModels[ TileNeighbor.NorthEast ] = node;
            m_annotatedNodes[ x, z + j ].GameModels[ TileNeighbor.East ] = node;
            m_annotatedNodes[ x, z + j ].GameModels[ TileNeighbor.SouthEast ] = node;
        }

        // right column
        for ( int j = 1; j < length; j++ ) {
            m_annotatedNodes[ x + ( int )width, z + j ].GameModels[ TileNeighbor.NorthWest ] = node;
            m_annotatedNodes[ x + ( int )width, z + j ].GameModels[ TileNeighbor.West ] = node;
            m_annotatedNodes[ x + ( int )width, z + j ].GameModels[ TileNeighbor.SouthWest ] = node;
        }

        // corners
        m_annotatedNodes[ x, z ].GameModels[ TileNeighbor.NorthEast ] = node;
        m_annotatedNodes[ x, z + ( int )length ].GameModels[ TileNeighbor.SouthEast ] = node;
        m_annotatedNodes[ x + ( int )width, z + ( int )length ].GameModels[ TileNeighbor.SouthWest ] = node;
        m_annotatedNodes[ x + ( int )width, z ].GameModels[ TileNeighbor.NorthWest ] = node;
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void UpdateDPS ( Cannon cannon, PathFindingNode node, bool added ) {
        int x, z;
        SpaceConversion.GetMapTileFromWorldPosition( node.Center, out x, out z );
        float minR = ( cannon.MinRange / SpaceConversion.MapTileSize );
        float maxR = 1 + ( cannon.MaxRange / SpaceConversion.MapTileSize );

        int iMaxR = Mathf.RoundToInt( maxR );

        for ( int i = -iMaxR; i < iMaxR; i++ ) {
            for ( int j = -iMaxR; j < iMaxR; j++ ) {
                float d2 = Mathf.Sqrt( i * i + j * j );

                if ( d2 < maxR && d2 > minR ) {
                    // in the danger zone where this cannon will do some damage
                    if ( SpaceConversion.InMapBounds( x + i, z + j ) ) {
                        float dps = cannon.DPS;
                        if ( !added ) {
                            dps = -dps;
                        }

                        m_annotatedNodes[ x + i, z + j ].DPS += dps;
                        m_annotatedNodes[ x + i, z + j ].DpsTeamId = ( short )node.TeamId;
                    }
                }
            }
        }
    }

    AnnotatedNode[,] m_annotatedNodes;
    // if there is a wall at this map position
    bool[,] m_isWall;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

