//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class AIStateMedicAttack : AIStateAttack {
    // medics 'attack' by finding unconcious troops and doing negative damage to heal them
    //---------------------------------------------------------------------------------------------------------------------
    public AIStateMedicAttack (
        Goto go,
        NavigationSystem navigationSystem,
        List<Weapon> weapons,
        AIComms aiComms,
        int ownerInstanceId,
        uint teamId,
        float turnSpeed,
        AIFlags preferredTargetType,
        InWorldTargetSelectionManager inWorldTargetSelectionManager )
    : base( go,
             navigationSystem,
             weapons,
             aiComms,
             ownerInstanceId,
             teamId,
             turnSpeed,
             preferredTargetType,
             inWorldTargetSelectionManager
    ) {
        m_originalDropPosition = m_aiComms.Position;
        SpaceConversion.ClampToMapBounds( ref m_originalDropPosition.x, ref m_originalDropPosition.z );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Update ( float deltaTime ) {
        Weapon weapon = m_weapons[ 0 ];
        if ( weapon.Target != null ) {
            Unit unit = weapon.Target.GetComponent<Unit>();
            if ( unit != default(Unit) ) {
                if ( unit.MindState == Unit.Mind.Conscious ||
                unit.MindState == Unit.Mind.Dead ) {
                    // find another unit to heal
                    SetNewTarget( null );
                }
            }
        }

        base.Update( deltaTime );
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override void AcquireTarget () {
        // stop move, set the speed to 0 which changes the animation state in the UnitView
        m_goto.Stop();
        //TODO:Medic should know how to find a target
        Unit target = new Unit(); //m_gameScene.FindBestPatientForMedic(m_aiComms.Position, m_teamId);
        SetNewTarget( target.GetComponent<Health>() );

        if ( target == null ) {
            if ( m_goto.GotoState == Goto.State.INACTIVE ) {
                m_aiComms.Animation = AIComms.AnimationState.Idle;

                if ( Vector3.SqrMagnitude( m_aiComms.Position - m_originalDropPosition ) > 4 ) {
                    m_goto.Go( m_originalDropPosition, 0, null, null, null );
                } else {
                    Vector3 resultDir = Goto.TurnTowards( m_aiComms.Position, m_aiComms.Rotation, m_lastHelpPosition, 360.0f * 0.0333f * Mathf.Deg2Rad );
                    m_aiComms.Rotation = Quaternion.LookRotation( resultDir );
                }
            }
        } else {
            m_aiComms.Animation = AIComms.AnimationState.Attack;
            var targetNode = target.GetComponent<PathFindingNode>();
            m_lastHelpPosition = targetNode.Center;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private Vector3 m_originalDropPosition;
    private Vector3 m_lastHelpPosition;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

