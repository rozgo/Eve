//---------------------------------------------------------------------------------------------------------------------     
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class GotoVehicle : Goto {
    //---------------------------------------------------------------------------------------------------------------------
    public GotoVehicle (
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
        uint teamId )
    : base(
        navigationSystem,
        pathFindingInput,
        ownerInstanceId,
        aiComms,
        tileWidth,
        tileLength,
        maxTurnSpeed,
        maxSpeed,
        acceleration,
        stoppingDistance,
        teamId
    ) {
        m_noise = new Perlin( UnityEngine.Random.Range( 1, 1000 ) );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Go (
        Vector3 destination,
        float withinRangeDistance,
        PathFindingNode targetModel,
        Hero heroToAvoid, // ignored
        Action<List<PathFindingNode>, LinkedListNode<FinalPathNode>> pathFoundCallBack ) {
            SearchPriorityLevel priority = SearchPriorityLevel.Normal;
            m_pathFindingInput.UnitTileWidth = m_tileWidth;
            m_pathFindingInput.UnitTileLength = m_tileLength;
            m_pathFindingInput.TargetModel = targetModel;
            m_pathFindingInput.StopWithinDistanceFromTargetModel = withinRangeDistance;

            // time to turn 360 degrees
            //DebugConsole.Assert(DebugChannel.AI, m_maxTurnSpeed > 0, "turn speed must be positive");
            m_pathFindingInput.TurnCost = 360.0f / m_maxTurnSpeed;

            // the player controlled hero gets highest priority
            priority = SearchPriorityLevel.Immediate;

            Vector3 forwards = m_aiComms.Rotation * Vector3.forward;
            int x = Mathf.RoundToInt( forwards.x );
            int z = Mathf.RoundToInt( forwards.z );

            m_pathFindingInput.InitialDirection = ( byte )PathHelper.GetDirection( x, z );

            m_pathFindingInput.IsDirectional = true;
            m_pathFindingInput.HeuristicMultiplier = 1.1f;
            m_pathFindingInput.PostProcessStraighten = false; // vehicles have their own path smoothing (see below)
            m_pathFindingInput.CanDestroyBlockingBuildings = true;
            m_pathFindingInput.CanDestroyBlockingWalls = true;

            base.GoInternal( destination, pathFoundCallBack, priority );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override bool Update ( float deltaTime ) {
        Vector3 currentDir = m_aiComms.Rotation * Vector3.forward;
        currentDir.y = 0;

        if ( GotoState == State.INACTIVE ||
         ( GotoState == State.PATH_PENDING && !m_pathFindingOutputCurrent.Valid ) ) {
            // goto is done or we don't yet have a valid first path
            // come to a stop

            if ( m_speed != 0 ) {
                Vector3 prevPostition = m_aiComms.Position;

                m_speed -= deltaTime * m_acceleration * 4.0f; // TODO add breaking amount?

                if ( m_speed < 0 ) {
                    m_speed = 0;
                }

                m_aiComms.Position += currentDir * m_speed * deltaTime;
                m_aiComms.Speed = m_speed;

                int tileX, tileZ;
                SpaceConversion.GetMapTileFromWorldPosition( m_aiComms.Position, out tileX, out tileZ );
                if ( m_navigationSystem.AnnotatedNodes[ tileX, tileZ ].GameModel != null ) {
                    // entered an invalid position. hard stop.
                    m_aiComms.Position = prevPostition;
                    m_speed = 0;
                    m_aiComms.Speed = m_speed;
                }
            } else {
                m_speed = 0;
            }
            return false;
        }

        Vector3 toFinalDestination = m_aiComms.Position - m_pathFindingOutputCurrent.Destination;
        toFinalDestination.y = 0;
        float distToFinalDestination = toFinalDestination.magnitude;

        if ( GotoState == State.PATH_PENDING
         && distToFinalDestination < CloseEnough ) {
            // don't do anything until the A* finishes
            return false;
        }

        UpdateTarget( deltaTime );

        UpdateFutureTarget( deltaTime );

        Vector3 resultDir = Goto.TurnTowards( m_aiComms.Position, m_aiComms.Rotation, m_targetPosition, m_maxTurnSpeed * deltaTime * Mathf.Deg2Rad );
        m_aiComms.Rotation = Quaternion.LookRotation( resultDir );

        UpdateSpeed( deltaTime, currentDir, distToFinalDestination );

        SimulateSuspension( deltaTime );

        if ( distToFinalDestination < CloseEnough ) {
            Stop();
            return true;
        }

        // integrate
        m_aiComms.Position += currentDir * m_speed * deltaTime;
        m_aiComms.Speed = m_speed;

        return false;
    }

    private void UpdateSpeed ( float deltaTime, Vector3 currentDir, float distToFinalDestination ) {
        // base the default target speed on how well aligned we are with the target direction
        Vector3 toTarget = m_targetPosition - m_aiComms.Position;
        toTarget.y = 0;

        float angle = Vector3.Angle( currentDir, toTarget );
        float targetSpeed = ( ( 60.0f - angle ) / 60.0f ) * m_maxSpeed;

        // construct unreachable areas from our turn rate to determine if we need to slow down
        // see http://gamedev.stackexchange.com/questions/16810/projected-trajectory-of-a-vehicle/16814#16814

        Vector3 leftDir = m_aiComms.Rotation * Vector3.left;

        Vector3 futurePosition = m_aiComms.Position + currentDir * m_speed * deltaTime;

        // time to complete a full circle
        float fullCircleTime = 360 / m_maxTurnSpeed;
        float distanceTravelled = fullCircleTime * Math.Abs( m_speed );
        float radius = distanceTravelled / ( 2.0f * Mathf.PI );

        Vector3 circleLeft = futurePosition + leftDir * radius;
        Vector3 circleRight = futurePosition - leftDir * radius;

        Vector3 destToCircleLeft = m_targetPositionFuture - circleLeft;
        Vector3 destToCircleRight = m_targetPositionFuture - circleRight;
    
        if ( destToCircleLeft.magnitude < radius ||
         destToCircleRight.magnitude < radius ) {
            // this target cannot be reached, the vehicle will orbit, so it needs to slow down
      
            // from the link above, find the point perpendicular to our facing direction, equidistant from the target and our position
            float r1 = Vector3.Distance( m_aiComms.Position, m_targetPositionFuture );
            Vector3 chord1, chord2;

            PathHelper.FindCircleCircleIntersections( m_aiComms.Position.x, m_aiComms.Position.z, r1,
                m_targetPositionFuture.x, m_targetPositionFuture.z, r1,
                out chord1, out chord2 );

            Vector3 intersection;
            PathHelper.LinesIntersect( chord1, chord2, m_aiComms.Position - leftDir * r1, m_aiComms.Position + leftDir * r1, out intersection );

            float newRadius = Vector3.Distance( m_aiComms.Position, intersection );
            float newCirc = 2.0f * Mathf.PI * newRadius;

            targetSpeed = Mathf.Min( targetSpeed, newCirc / fullCircleTime );

//      if ((DebugConsole.debugChannelMask & (int)DebugChannel.AI) != 0)
//      {
//        Debug.DrawLine(chord1, chord2, Color.red, 0.1f);
//        PathHelper.DrawCircle(m_aiComms.Position, r1, 0.1f);
//        PathHelper.DrawCircle(m_targetPositionFuture, r1, 0.1f);
//        Debug.DrawLine(m_targetPositionFuture, intersection, Color.blue, 0.1f);
//      }
        }

        // adjust speed based and clamp
        if ( m_speed < targetSpeed ) {
            m_speed += m_acceleration * deltaTime;
        } else {
            float brakeAmount = Mathf.Max( 2.0f * m_speed - targetSpeed, 3.0f );

            m_speed -= brakeAmount * deltaTime;
        }

        m_speed = Mathf.Clamp( m_speed, 0, m_maxSpeed );
      
        // slow down to a stop if we're approaching the final waypoint and in 'stopping distance' range
        if ( distToFinalDestination < m_stoppingDistance ) {
//      if ((DebugConsole.debugChannelMask & (int)DebugChannel.AI) != 0)
//      {
            //Debug.DrawLine(m_aiComms.Position, m_pathFindingOutputCurrent.Destination, Color.white);
//      }

            float slowDownAmount = distToFinalDestination / m_stoppingDistance;
            m_speed = Mathf.Min( m_speed, slowDownAmount * m_maxSpeed );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override void AStarCompleteCallback () {
        base.AStarCompleteCallback();

        if ( m_pathFindingOutputCurrent.Path.Count > 1 ) {
            m_targetPosition = m_pathFindingOutputCurrent.Path.First.Next.Value.Position;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void UpdateTarget ( float deltaTime ) {
        if ( m_pathFindingOutputCurrent.Path.Count <= 1 ) {
            m_targetPosition = m_pathFindingOutputCurrent.Destination;
            return;
        }

        // make a copy of the target position in case we cannot reach the proposed target due to a structure blocking line of sight
        Vector3 cachedTargetPosition = m_targetPosition;

        Vector3 nodePosition = m_pathFindingOutputCurrent.Path.First.Next.Value.Position;
        Vector3 nodeToTarget = nodePosition - m_targetPosition;
        nodeToTarget.y = 0;
      
        float dist = nodeToTarget.magnitude;

        Vector3 moveThisFrame = nodeToTarget;
        moveThisFrame.Normalize();

        // target moves faster than the tank. adjust this for different smoothing results.
        float speedMultiplier = 1.5f;
        moveThisFrame *= deltaTime * m_speed * speedMultiplier;

        if ( dist <= deltaTime * m_maxSpeed * 2 ) {
            m_pathFindingOutputCurrent.Path.RemoveFirst();

            // todo UpdateTarget with remaining time

            nodeToTarget.Normalize();
            nodeToTarget *= dist;
        }

        m_targetPosition += moveThisFrame;
        m_targetPosition.y = 0;

//    if ((DebugConsole.debugChannelMask & (int)DebugChannel.AI) != 0)
//    {
//      Debug.DrawLine(m_targetPosition, m_targetPosition + Vector3.up * 10, Color.magenta, 0.01f);
//    }

        Vector3 heroToTarget = m_targetPosition - m_aiComms.Position;
        float heroDistToTarget = heroToTarget.sqrMagnitude;

        bool blocked = PathHelper.IsPathBlockedForVehicle(
                       m_aiComms.Position, 
                       m_targetPosition, 
                       m_navigationSystem.AnnotatedNodes, 
                       m_pathFindingInput.UnitTileWidth * SpaceConversion.MapTileSize, 
                       true );

        float angle = Vector3.Angle( heroToTarget, m_aiComms.Rotation * Vector3.forward );
        bool facingAway = angle > 90;

        // if the path is blocked or we're not facing it then revert to the cached position
        // the exception is if the hero is close. In that case, we have to force the target away
        // to stop the hero circling and being unable to reach the position. a hacky solution unfortunately.
        if ( ( blocked || facingAway ) && heroDistToTarget > 1.0f ) {
            m_targetPosition = cachedTargetPosition;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void UpdateFutureTarget ( float deltaTime ) {
//    if ((DebugConsole.debugChannelMask & (int)DebugChannel.AI) != 0)
//    {
//      //Debug.DrawLine(m_targetPositionFuture, m_targetPositionFuture + Vector3.up * 10, Color.cyan, 0.01f);
//    }

        if ( m_pathFindingOutputCurrent.Path.Count <= 1 ) {
            m_targetPositionFuture = m_pathFindingOutputCurrent.Destination;
            return;
        }

        float lookAheadDist = 2.0f;

        Vector3 nodePosition = m_pathFindingOutputCurrent.Path.First.Next.Value.Position;
        Vector3 nodeToTarget = nodePosition - m_targetPosition;
        nodeToTarget.y = 0;

        nodeToTarget.Normalize();
        m_targetPositionFuture = m_targetPosition + nodeToTarget * lookAheadDist;
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void SimulateSuspension ( float deltaTime ) {
        Vector3 position = m_aiComms.Position;
        var speedMultiplier = ( m_speed / m_maxSpeed ) * ( m_maxSpeed - m_speed );
        var noise = m_noise.Noise( position.x, position.y, position.z );
        var pitch = noise * speedMultiplier * m_noiseScale;
        var roll = noise * m_noiseScale / 2;
        m_aiComms.Rotation *= Quaternion.AngleAxis( pitch * deltaTime, Vector3.right );
        m_aiComms.Rotation *= Quaternion.AngleAxis( roll * deltaTime, Vector3.forward );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private float m_speed;
    // the target that the vehichle will drive towards
    // it will follow the path ahead of the vehicle creating a smoother path
    private Vector3 m_targetPosition;
    private Vector3 m_targetPositionFuture;
    private Perlin m_noise;
    private float m_noiseScale = 300f;
    private const float CloseEnough = 0.5f;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------


