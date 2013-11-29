//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;
//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
// the public interface to everything navigation
public class NavigationSystem {
    //---------------------------------------------------------------------------------------------------------------------
    public NavigationSystem (
        ClearanceMap clearanceMap,
        CostMap costMap,
        AStarScheduler aStarScheduler,
        UnitTracker unitTracker,
        AnnotatedNode[,] annotatedNodes,
        int[,] baseOutlineStructureIds ) {
        m_clearanceMap = clearanceMap;
        m_costMap = costMap;
        m_AStarScheduler = aStarScheduler;
        m_unitTracker = unitTracker;
        AnnotatedNodes = annotatedNodes;
        BaseOutlineStructureIds = baseOutlineStructureIds;
		
        // create a dummy first object that will weight new building creation towards the center of the map
        m_totalObjects = 1;
        m_baseTotalXPositions = ( int )( SpaceConversion.MapTiles / 2.0f );
        m_baseTotalZPositions = ( int )( SpaceConversion.MapTiles / 2.0f );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Update ( float deltaTime ) {
        Profiler.BeginSample( "NavigationSystem.Update" );

        Profiler.BeginSample( "m_AStarScheduler.Update" );
        m_AStarScheduler.Update( deltaTime );
        Profiler.EndSample();

        Profiler.BeginSample( "m_clearanceMap.Update" );
        m_clearanceMap.Update( deltaTime );
        Profiler.EndSample();

        Profiler.EndSample();

        DebugDraw();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void DebugDraw () {
    Debug.Log ("se esta llamando eso ");
        m_clearanceMap.DebugDraw();
        m_costMap.DebugDraw();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Destroy () {
        m_clearanceMap.Destroy();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnModelChanged ( object sender, EventArgs e ) {
        //TODO:Reimplement events
//    PositionChangedEventArgs sp = e as PositionChangedEventArgs;
//    if (sp != null && sp.IsBlocked == false)
//    {
//      Structure model = sender as Structure;
//      UpdateGameModelPosition(sp.PrevPosition, sp.Position, model);
//    }
//
//    StructureSelectedEventArgs ss = e as StructureSelectedEventArgs;
//    if (ss != null && ss.selected == false)
//    {
//      m_clearanceMap.RecalulateBaseOutline();
//    }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnAttach ( object sender ) {
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnDetach ( object sender ) {
    }
    //---------------------------------------------------------------------------------------------------------------------
    public bool IsBlocked ( PathFindingNode model, int x, int z ) {
        return IsBlocked( model.Width, model.Length, x, z, model.InstanceId );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public bool IsBlocked ( uint width, uint length, int x, int z ) {
        return IsBlocked( width, length, x, z, null );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private bool IsBlocked ( uint width, uint length, int x, int z, int? instanceId ) {
        for ( int i = x; i < x + width; i++ ) {
            for ( int j = z; j < z + length; j++ ) {
                if ( i >= SpaceConversion.MapTiles || j >= SpaceConversion.MapTiles || i < 0 || j < 0 ) {
                    return true;
                }

                PathFindingNode gameModel = m_clearanceMap.AnnotatedNodes[ i, j ].GameModel;

                if ( gameModel != null ) {
                    if ( instanceId.HasValue == false ) {
                        // there is a game model at this location and it is not the one passed in as nothing was passed in
                        return true;
                    }

                    if ( gameModel.InstanceId != instanceId ) {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void AddGameModel ( PathFindingNode model ) {
        int x, z;
        SpaceConversion.GetMapTileFromWorldPosition( model.Position, out x, out z );

        m_clearanceMap.AddGameModel( x, z, model );
        m_costMap.AddStructure( model );

        m_totalObjects++;
        m_baseTotalXPositions += x;
        m_baseTotalZPositions += z;
    }

    public void UpdateDPSInCostMap ( Cannon cannon, PathFindingNode node) {
        m_costMap.AddToDPS( cannon, node );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void UpdateGameModelPosition ( Vector3 previous, Vector3 current, PathFindingNode node ) {
        int prevX, prevZ;
        SpaceConversion.GetMapTileFromWorldPosition( previous, out prevX, out prevZ );
        int x, z;
        SpaceConversion.GetMapTileFromWorldPosition( current, out x, out z );
    Debug.Log (x + " "+ z);
        m_clearanceMap.UpdateGameModel( prevX, prevZ, x, z, node );

        var trap = node.GetComponent<Trap>();

        if ( trap == default(Trap) ) {
            m_costMap.RemoveStructure( previous, node );
            m_costMap.AddStructure( node );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void RemoveGameModel ( PathFindingNode node ) {
        int x, z;
        SpaceConversion.GetMapTileFromWorldPosition( node.Position, out x, out z );
        m_clearanceMap.RemoveGameModel( x, z, node );
        m_costMap.RemoveStructure( node.Position, node );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public PathFindingNode GetStructureAtPosition ( int x, int z ) {
        return AnnotatedNodes[ x, z ].GameModel;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void CalculateClearanceMap () {
        m_clearanceMap.CalculateClearance();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void ResetBaseOutlineTimer () {
        m_clearanceMap.ResetBaseOutlineTimer();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public bool CalculatePositionForNewBaseStructre ( int maxDimension, out int x, out int z ) {
        int baseCenterX = m_baseTotalXPositions / m_totalObjects;
        int baseCenterZ = m_baseTotalZPositions / m_totalObjects;
        Vector3 position = new Vector3( baseCenterX, 0, baseCenterZ );

        return m_clearanceMap.GetOpenSquareNearToPosition( maxDimension, position, out x, out z );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public bool GetOpenSquareNearToPosition ( int maxDimension, Vector3 position, out int x, out int z ) {
        return m_clearanceMap.GetOpenSquareNearToPosition( maxDimension, position, out x, out z );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void ScheduleAStar ( PathFindingInput input, PathFindingOutput output, SearchPriorityLevel priority, int ownerId ) {
        m_AStarScheduler.Schedule( input, output, priority, ownerId );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void UnScheduleAStar ( int ownerId ) {
        m_AStarScheduler.UnSchedule( ownerId );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void RegisterUnitGoingToTile ( int x, int z, int instanceId ) {
        m_unitTracker.RegisterUnitGoingToTile( x, z, instanceId );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void UnRegisterUnitGoingToTile ( int instanceId ) {
        m_unitTracker.UnRegisterUnitGoingToTile( instanceId );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public AnnotatedNode[,] AnnotatedNodes { get; private set; }

    public int [,] BaseOutlineStructureIds { get; private set; }
    //---------------------------------------------------------------------------------------------------------------------
    private ClearanceMap m_clearanceMap;
    private CostMap m_costMap;
    private AStarScheduler m_AStarScheduler;
    private UnitTracker m_unitTracker;
    private int m_totalObjects;
    private int m_baseTotalXPositions;
    private int m_baseTotalZPositions;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

