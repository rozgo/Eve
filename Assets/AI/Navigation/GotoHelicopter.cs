//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class GotoHelicopter : Goto {
    public static readonly float PreferredHeight = 10.0f;
    //---------------------------------------------------------------------------------------------------------------------
    public GotoHelicopter (
        NavigationSystem navigationSystem,
        PathFindingInput pathFindingInput,
        List<Weapon> weapons,
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
        m_weapons = weapons;
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
        m_destination = position;

        if ( m_speed < speed ) {
            m_speed += m_acceleration * deltaTime;
        }

        Vector3 to = position - m_aiComms.Position;
        float dist = to.magnitude;

        to.Normalize();
        UpdateOrientation( to, deltaTime );

        if ( dist < m_stoppingDistance ) {
            float closeEnough = ( m_maxSpeed * deltaTime );
            if ( dist <= closeEnough ) {
                m_speed = 0;
                return true;
            }

            float slowDownAmount = dist / m_stoppingDistance;
            m_speed = Mathf.Min( m_speed, slowDownAmount * m_maxSpeed );
        }

        m_aiComms.Position += to * deltaTime * m_speed;
        m_aiComms.Speed = m_speed;

        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void UpdateOrientation ( Vector3 direction, float deltaTime ) {
        // this is crying out for using angluar velocity

        if ( m_firstUpdate ) {
            m_firstUpdate = false;
            m_heading = m_aiComms.Rotation;
        } else {
            Vector3 desired = m_destination - m_aiComms.Position;
            Weapon weapon = m_weapons[ 0 ];
            if ( weapon.Target != null ) {
                desired = weapon.Target.Center - m_aiComms.Position;
            }

            desired.y = 0;
            desired.Normalize();
            if ( desired.sqrMagnitude > 0 ) {
                Quaternion idealHeading = Quaternion.LookRotation( desired, Vector3.up );
                float angle = Quaternion.Angle( idealHeading, m_heading );
                float delta = Math.Min( 1.0f, angle / 5.0f );
                m_heading = Quaternion.RotateTowards( m_heading, idealHeading, delta );
            }
        }

        m_aiComms.Rotation = m_heading;

        float dotFwd = Vector3.Dot( direction, m_aiComms.Rotation * Vector3.forward );
        float dotRight = Vector3.Dot( direction, m_aiComms.Rotation * Vector3.right );

        float speedMult = m_speed / m_maxSpeed;

        float pitch = dotFwd * speedMult * 20.0f;
        float roll = dotRight * speedMult * 20.0f;

        m_aiComms.Rotation *= Quaternion.AngleAxis( pitch, Vector3.right );
        m_aiComms.Rotation *= Quaternion.AngleAxis( -roll, Vector3.forward );
    }
    //---------------------------------------------------------------------------------------------------------------------
    Vector3 m_destination;
    Quaternion m_heading;
    float m_speed;
    bool m_firstUpdate = true;
    List<Weapon> m_weapons;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

