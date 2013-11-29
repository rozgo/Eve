//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
// keeps track of which unit is heading to which tile
public class UnitTracker {
    //---------------------------------------------------------------------------------------------------------------------
    public UnitTracker () {
        m_unitsGoingToTile = new List<int>[SpaceConversion.MapTiles, SpaceConversion.MapTiles];
        m_unitToTileIdMap = new Dictionary<int, int>();

        for ( int i = 0; i < SpaceConversion.MapTiles; i++ ) {
            for ( int j = 0; j < SpaceConversion.MapTiles; j++ ) {
                m_unitsGoingToTile[ i, j ] = new List<int>();
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void RegisterUnitGoingToTile ( int x, int z, int instanceId ) {
        UnRegisterUnitGoingToTile( instanceId );

//    DebugConsole.Assert(DebugChannel.AI, m_unitsGoingToTile[x, z].Contains(instanceId) == false);

        m_unitsGoingToTile[ x, z ].Add( instanceId );
        m_unitToTileIdMap.Add( instanceId, SpaceConversion.GetTileIdFromCoordinates( x, z ) );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void UnRegisterUnitGoingToTile ( int instanceId ) {
        if ( m_unitToTileIdMap.ContainsKey( instanceId ) == true ) {
            int tileId = m_unitToTileIdMap[ instanceId ];

            int x, z;
            SpaceConversion.GetTileCoordinatesFromId( tileId, out x, out z );

//      DebugConsole.Assert(DebugChannel.AI, m_unitsGoingToTile[x, z].Contains(instanceId));

            m_unitsGoingToTile[ x, z ].Remove( instanceId );
            m_unitToTileIdMap.Remove( instanceId );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public int GetNumberOfUnitsGoingToTile ( int x, int z ) {
        return m_unitsGoingToTile[ x, z ].Count;
    }
    //---------------------------------------------------------------------------------------------------------------------
    private List<int>[,] m_unitsGoingToTile;
    private Dictionary<int, int> m_unitToTileIdMap;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

