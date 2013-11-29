//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public abstract class Goto {
    public enum State {
        INACTIVE,
        ACTIVE,
        PATH_PENDING,
        // a goto has been requested but not yet completed
    }
    //---------------------------------------------------------------------------------------------------------------------
    public Goto (
        NavigationSystem navigationSystem, 
        PathFindingInput pathFindingInput,
        int ownerInstanceId,
        AIComms aiComms,
        uint tileWidth,
        uint tileLength,
        float maxTurnSpeed,
        float maxSpeed,
        float acceleration,
        float stoppingDistance,
        uint teamId ) {
        m_navigationSystem = navigationSystem;
        m_pathFindingInput = pathFindingInput;

        m_ownerInstanceId = ownerInstanceId;
        m_aiComms = aiComms;
        m_tileWidth = tileWidth;
        m_tileLength = tileLength;
        m_maxTurnSpeed = maxTurnSpeed;
        m_maxSpeed = maxSpeed;
        m_acceleration = acceleration;
        m_stoppingDistance = stoppingDistance;
        m_teamId = teamId;
        m_pathFindingInput.Callback = new Action( AStarCompleteCallback );

        m_pathFindingOutputPending = new PathFindingOutput();
        m_pathFindingOutputCurrent = new PathFindingOutput();

        GotoState = State.INACTIVE;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public abstract void Go (
        Vector3 destination, 
        float withinRangeDistance, 
        PathFindingNode targetModel, 
        Hero heroToAvoid, 
        Action<List<PathFindingNode>, LinkedListNode<FinalPathNode>> callBack );
    //---------------------------------------------------------------------------------------------------------------------
    public abstract bool Update ( float deltaTime );
    //---------------------------------------------------------------------------------------------------------------------
    public bool UpdateGoToPosition ( Vector3 position, ref float deltaTime ) {
        return UpdateGoToPosition( position, m_maxSpeed, ref deltaTime );
    }

    public virtual bool UpdateGoToPosition ( Vector3 position, float speed, ref float deltaTime ) {
        return true;
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected void GoInternal ( Vector3 destination, Action<List<PathFindingNode>, LinkedListNode<FinalPathNode>> pathFoundCallBack, SearchPriorityLevel priority ) {
        m_pathFindingOutputPending.Destination = destination;
        m_pathFoundCallBack = pathFoundCallBack;

        int startX, startZ;
        Vector3 startPosition = SpaceConversion.GetClearanceMapPositionFromWorldPosition( m_aiComms.Position, m_tileWidth );
        SpaceConversion.GetMapNodeFromWorldPosition( startPosition, out startX, out startZ );

        int targetX, targetZ;
        destination = SpaceConversion.GetClearanceMapPositionFromWorldPosition( destination, m_tileWidth );
        SpaceConversion.GetMapNodeFromWorldPosition( destination, out targetX, out targetZ );

        m_pathFindingInput.StartTileX = Mathf.Clamp( startX, 0, SpaceConversion.MapTiles - 1 );
        m_pathFindingInput.StartTileZ = Mathf.Clamp( startZ, 0, SpaceConversion.MapTiles - 1 );
        m_pathFindingInput.TargetTileX = Mathf.Clamp( targetX, 0, SpaceConversion.MapTiles - 1 );
        m_pathFindingInput.TargetTileZ = Mathf.Clamp( targetZ, 0, SpaceConversion.MapTiles - 1 );

        m_navigationSystem.ScheduleAStar( m_pathFindingInput, m_pathFindingOutputPending, priority, m_ownerInstanceId );

        GotoState = State.PATH_PENDING;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void RePath () {
        Go( m_pathFindingOutputPending.Destination, m_pathFindingInput.StopWithinDistanceFromTargetModel, m_pathFindingInput.TargetModel, m_pathFindingInput.HeroToAvoid, m_pathFoundCallBack );
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected virtual void AStarCompleteCallback () {

        // copy over the output from the pending output
        m_pathFindingOutputCurrent.Copy( m_pathFindingOutputPending );

        m_pathFindingOutputCurrent.ModelsPotentiallyBlockingAShorterPath = m_pathFindingOutputCurrent.ModelsPotentiallyBlockingAShorterPath.Distinct().ToList();

        if ( m_pathFindingOutputCurrent.Path.Count > 1 ) { 
            // check that a path was found
            // get a copy of the final node to use to patch up below
            FinalPathNode last = m_pathFindingOutputCurrent.Path.Last.Value;
            FinalPathNode first = m_pathFindingOutputCurrent.Path.First.Value;

            // remove the terminal nodes and replace with the start position and destination position
            m_pathFindingOutputCurrent.Path.RemoveFirst();

            if ( m_pathFindingInput.StopWithinDistanceFromTargetModel == 0 ) { // don't remove the last one if we are able to stop enroute
                if ( m_pathFindingInput.UnitTileWidth > 1 ) { // and don't remove if we can't potentially fit  
                    m_pathFindingOutputCurrent.Path.RemoveLast();
                }
            }

            Vector3 startPosition = SpaceConversion.GetClearanceMapPositionFromWorldPosition( m_aiComms.Position, m_tileWidth );
            FinalPathNode start = new FinalPathNode( startPosition, m_pathFindingInput.InitialDirection, true, first.IsJumpNode );
            m_pathFindingOutputCurrent.Path.AddFirst( start );

            if ( m_pathFindingInput.StopWithinDistanceFromTargetModel == 0 && m_pathFindingInput.UnitTileWidth <= 1 ) {
                FinalPathNode end = new FinalPathNode( m_pathFindingOutputCurrent.Destination, last.Direction, true, last.IsJumpNode );
                m_pathFindingOutputCurrent.Path.AddLast( end );
            } else {
                FinalPathNode end = new FinalPathNode( last.Position, last.Direction, true, last.IsJumpNode );
                m_pathFindingOutputCurrent.Path.RemoveLast();
                m_pathFindingOutputCurrent.Path.AddLast( end );
            }

            PathHelper.ConvertFromClearanceMapPositionToWorldPosition( m_pathFindingOutputCurrent.Path, m_tileWidth );

            if ( m_pathFindingInput.PostProcessStraighten ) {
//        DebugConsole.Assert(DebugChannel.AI, m_pathFindingInput.IsDirectional == false, "path straightening only supported for non-direcitonal search");
                PathHelper.Straighten( m_pathFindingOutputCurrent.Path, m_navigationSystem.AnnotatedNodes, m_pathFindingInput.HeroToAvoid, m_pathFindingInput.AvoidDangerAreaModifier == 0 );
            }

            m_pathFindingOutputCurrent.DebugDraw( Color.green );

        }

        if ( m_pathFindingOutputCurrent.PathFound == true ) {
            GotoState = State.ACTIVE;

            if ( m_pathFindingOutputCurrent.Path.Count > 0 ) {
                int x, z;
                SpaceConversion.GetMapTileFromWorldPosition( m_pathFindingOutputCurrent.Path.Last.Value.Position, out x, out z ); // tile is used here to keep in sync with the node<->world calc in AStar
                m_navigationSystem.RegisterUnitGoingToTile( x, z, m_ownerInstanceId );
            }
        } else {
            GotoState = State.INACTIVE;
        }

        if ( m_pathFoundCallBack != null ) {
            m_pathFoundCallBack( m_pathFindingOutputCurrent.ModelsBlockingPath, m_pathFindingOutputCurrent.Path.Last );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public virtual void Stop () {
        if ( GotoState == State.PATH_PENDING ) {
            m_navigationSystem.UnScheduleAStar( m_ownerInstanceId );
        }

        GotoState = State.INACTIVE;

        m_aiComms.Speed = 0;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static Vector3 TurnTowards ( Vector3 currentPosition, Quaternion currectRotation, Vector3 target, float maxRadiansDelta ) {
        float unused = 0;
        return TurnTowards( currentPosition, currectRotation, target, maxRadiansDelta, false, ref unused );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static Vector3 TurnTowards ( Vector3 currentPosition, Quaternion currectRotation, Vector3 target, float maxRadiansDelta, bool calculateAngle, ref float angle ) {
        Vector3 currentDir = currectRotation * Vector3.forward;
        currentDir.y = 0;

        Vector3 toTarget = target - currentPosition;
        toTarget.y = 0;
        toTarget.Normalize();

        Vector3 resultDir = currentDir;

        if ( toTarget.sqrMagnitude > 0 ) {
            resultDir = Vector3.RotateTowards( currentDir, toTarget, maxRadiansDelta, 0.0f );

            if ( calculateAngle ) {
                angle = Vector3.Angle( resultDir, toTarget );
            }
        }

        return resultDir;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public State GotoState { get; protected set; }
    //---------------------------------------------------------------------------------------------------------------------
    protected NavigationSystem m_navigationSystem;
    protected PathFindingInput m_pathFindingInput;
    // having two copies allows a goto to continue on when another one has been queued up
    // this removes the noticable stops when units repath
    protected PathFindingOutput m_pathFindingOutputPending;
    protected PathFindingOutput m_pathFindingOutputCurrent;
    protected int m_ownerInstanceId;
    protected AIComms m_aiComms;
    protected uint m_tileWidth;
    protected uint m_tileLength;
    protected float m_maxTurnSpeed;
    protected SearchPriorityLevel m_priority;
    protected float m_maxSpeed;
    protected float m_acceleration;
    protected float m_stoppingDistance;
    protected uint m_teamId;
    private Action OnDone;
    private Action<List<PathFindingNode>, LinkedListNode<FinalPathNode>> m_pathFoundCallBack;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

