//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class AIStateAirUnitAttack : AIStateAttack {
    //---------------------------------------------------------------------------------------------------------------------
    public AIStateAirUnitAttack (
        Goto go,
        NavigationSystem navigationSystem,
        List<Weapon> weapons,
        AIComms aiComms,
        int ownerInstanceId,
        uint teamId,
        float turnSpeed,
        AIFlags preferredTargetType,
        InWorldTargetSelectionManager inWorldTargetSelectionManager ) :
        base(
            go,
            navigationSystem,
            weapons,
            aiComms,
            ownerInstanceId,
            teamId,
            turnSpeed,
            preferredTargetType,
            inWorldTargetSelectionManager ) {
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Update ( float deltaTime ) {
        Weapon weapon = m_weapons[ 0 ];

        if ( weapon.Target == null ) {
            AcquireTarget();
        }

        if ( weapon.Target != null ) {
            // if we've completed the goto and in range then fire
            var targetNode = weapon.Target.GetComponent<PathFindingNode>();
            bool gotoComplete = m_goto.GotoState == Goto.State.INACTIVE;

            Vector3 to = m_aiComms.Position - targetNode.Center;
            to.y = 0;
            bool inWeaponRange = Vector3.SqrMagnitude( to ) < weapon.Range * weapon.Range;

            if ( inWeaponRange && gotoComplete && weapon.IsAimed ) {
                m_goto.Stop();

                if ( weapon.IsReady ) {
                    weapon.Fire( targetNode.Center );
                }
            } else {
                weapon.StopFiring();
                if ( m_goto.GotoState == Goto.State.INACTIVE ) {
                    Vector3 offset = targetNode.Center - m_aiComms.Position;
                    offset.y = 0;
                    offset.Normalize();
                    offset *= weapon.Range * 0.9f; // ensure we fly within waepon range

                    Vector3 flyTo = targetNode.Center - offset;

                    if ( m_goto is GotoHelicopter ) {
                        flyTo.y = GotoHelicopter.PreferredHeight;
                    } else {
                        flyTo.y = GotoBlimp.PreferredHeight;
                    }

                    m_goto.Go( flyTo, 0, null, null, null );
                }
            }
        } else {
            weapon.StopFiring();
        }
        //TODO:Updates why here?
//    m_goto.Update(deltaTime);
//    weapon.Update(deltaTime);
    }
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

