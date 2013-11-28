//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public static class SpaceConversion {
    // the waypoint graph for the AI is a grid, MapTiles * MapTiles tiles
    // each tile is of size MapTileSize meters (or Unity units)
    // each tile has four corners or nodes
    // human characters move from node to node
    // tanks move from tile to tile and use a ClearanceMap (see ClearanceMap file) to determine if they can fit though small gaps
    // x-------x
    // |  2x2  |
    // | tile  |
    // x-------x <-- A node
    //---------------------------------------------------------------------------------------------------------------------
    public static void GetMapTileFromWorldPosition ( Vector3 worldPosition, out int x, out int z ) {
        x = Mathf.FloorToInt( worldPosition.x / MapTileSize );
        z = Mathf.FloorToInt( worldPosition.z / MapTileSize );

        x = Mathf.Clamp( x, 0, MapTiles - 1 );
        z = Mathf.Clamp( z, 0, MapTiles - 1 );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static Vector3 GetWorldPositionFromMapTile ( int x, int z ) {
        return new Vector3( ( x * MapTileSize ), 0, ( z * MapTileSize ) );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static void GetMapNodeFromWorldPosition ( Vector3 worldPosition, out int x, out int z ) {
        x = Mathf.RoundToInt( worldPosition.x / MapTileSize );
        z = Mathf.RoundToInt( worldPosition.z / MapTileSize );

        x = Mathf.Clamp( x, 0, MapTiles - 1 );
        z = Mathf.Clamp( z, 0, MapTiles - 1 );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static Vector3 GetWorldPositionFromClearanceMapPosition ( Vector3 worldPosition, uint tileWidth ) {
        // see ClearanceMap
        return new Vector3( worldPosition.x += tileWidth * 0.5f * MapTileSize, worldPosition.y, worldPosition.z += tileWidth * 0.5f * MapTileSize );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static Vector3 GetClearanceMapPositionFromWorldPosition ( Vector3 worldPosition, uint tileWidth ) {
        // see ClearanceMap
        return new Vector3( worldPosition.x -= tileWidth * 0.5f * MapTileSize, worldPosition.y, worldPosition.z -= tileWidth * 0.5f * MapTileSize );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static short GetTileIdFromCoordinates ( int x, int z ) {
        return ( short )( ( x * MapTiles ) + z );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static void GetTileCoordinatesFromId ( int id, out int x, out int z ) {
        x = id / MapTiles;
        z = id % MapTiles;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static bool InMapBounds ( int x, int z ) {
        return x >= 0 && z >= 0 && x < MapTiles && z < MapTiles;
    }

    public static void ClampToMapBounds ( ref int x, ref int z ) {
        x = Mathf.Clamp( x, 0, MapTiles - 1 );
        z = Mathf.Clamp( z, 0, MapTiles - 1 );
    }

    public static void ClampToMapBounds ( ref float x, ref float z ) {
        x = Mathf.Clamp( x, 0, ( MapTiles - 1 ) * MapTileSize );
        z = Mathf.Clamp( z, 0, ( MapTiles - 1 ) * MapTileSize );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public const float MapTileSize = 2.0f;
    public const int MapTiles = 42;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

