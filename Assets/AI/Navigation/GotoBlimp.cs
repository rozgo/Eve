//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class GotoBlimp : Goto {
    public static readonly float PreferredHeight = 15.0f;
    //---------------------------------------------------------------------------------------------------------------------
    public GotoBlimp ( NavigationSystem navigationSystem,
                   PathFindingInput pathFindingInput,
                   int ownerInstanceId,
                   AIComms aiComms,
                   uint width,
                   uint length,
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
        width,
        length,
        maxTurnSpeed * UnityEngine.Random.Range( 0.9f, 1.1f ), // just to add a little variation
        maxSpeed * UnityEngine.Random.Range( 0.9f, 1.1f ),
        acceleration,
        stoppingDistance,
        teamId
    ) {
        m_initialRotation = aiComms.Rotation;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Go (
        Vector3 destination,
        float withinRangeDistance,
        PathFindingNode targetModel,
        Hero heroToAvoid,
        Action<List<PathFindingNode>,
        LinkedListNode<FinalPathNode>> pathFoundCallBack ) {
            m_destination = destination;
            GotoState = State.ACTIVE;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override bool Update ( float deltaTime ) {
        bool done = UpdateGoToPosition( m_destination, ref deltaTime );
        if ( done ) {
            if ( GotoState == State.ACTIVE ) {
                Stop();
            }
            return true;
        }

        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override bool UpdateGoToPosition ( Vector3 position, float speed, ref float deltaTime ) {
        float angle = 0;
        Vector3 resultDir = Goto.TurnTowards( m_aiComms.Position, m_aiComms.Rotation, position, m_maxTurnSpeed * deltaTime * Mathf.Deg2Rad, true, ref angle );
        m_aiComms.Rotation = Quaternion.LookRotation( resultDir );

        Vector3 currentDir = m_aiComms.Rotation * Vector3.forward;
        currentDir.y = 0;

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

        Vector3 destToCircleLeft = position - circleLeft;
        Vector3 destToCircleRight = position - circleRight;

        if ( destToCircleLeft.magnitude < radius ||
         destToCircleRight.magnitude < radius ) {
            m_speed = 0;
        } else if ( m_speed < speed ) {
            m_speed += m_acceleration * deltaTime;
        }

        Vector3 to = position - m_aiComms.Position;
        float dist = to.magnitude;
        float closeEnough = ( m_maxSpeed * deltaTime );

        if ( dist < m_stoppingDistance ) {
            if ( dist <= closeEnough ) {
                m_speed = 0;
                return true;
            }

            float slowDownAmount = dist / m_stoppingDistance;
            m_speed = Mathf.Min( m_speed, slowDownAmount * m_maxSpeed );
        }

        to.Normalize();
        m_aiComms.Position += to * deltaTime * m_speed;
        m_aiComms.Speed = m_speed;

        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    Vector3 m_destination;
    float m_speed;
    Quaternion m_initialRotation;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

