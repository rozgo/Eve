//---------------------------------------------------------------------------------------------------------------------     
using System;
using UnityEngine;
using System.Collections.Generic;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class AIStateDefend : AIState {
    //---------------------------------------------------------------------------------------------------------------------
    public AIStateDefend (
        List<Weapon> weapons,
        AIComms aiComms,
        Vector3 center,
        uint teamId,
        int instanceId ) {
//    DebugConsole.Assert(DebugChannel.AI, gameScene != null, "gameScene is null");
//    DebugConsole.Assert(DebugChannel.AI, weapons != null, "weapons is null");
    
        m_weapons = weapons;
        m_aiComms = aiComms;
        m_center = center;
        m_teamId = teamId;
        m_findTargetTimer = UnityEngine.Random.Range( 0.0f, 1.0f );
    }
    #if UNITY_EDITOR
    //---------------------------------------------------------------------------------------------------------------------
    public override void RealtimeUpdate () {
        foreach ( Weapon weapon in m_weapons ) {
//      weapon.DebugDraw();
        }
    }
    #endif
    //---------------------------------------------------------------------------------------------------------------------
    public override void Update ( float deltaTime ) {
        for ( int i = 0; i < m_weapons.Count; i++ ) {
            Weapon weapon = m_weapons[ i ];
            if ( weapon.IsReady ) {
                if ( weapon.Target != null ) {
                    if ( weapon.Target != null &&
                        weapon.Target.hitPoints == 0 ) {
                        weapon.StopFiring();
                        weapon.Target = null;
                    }
                }

                // get a target to fire at
                Health target = weapon.Target;
                if ( target == null ) {
                    m_findTargetTimer -= deltaTime;
                    if ( m_findTargetTimer < 0 ) {
                        Vector3 origFwd = Vector3.forward;
                        TurretWeapon turretWeapon = weapon as TurretWeapon;
                        if ( turretWeapon != null ) {
                            //TODO:What is this needed original forward?
                //              origFwd = turretWeapon.OriginalForward;
                        }

                        //TODO:Find nearest target somewhere else
            //            target = m_gameScene.FindNearestObject(weapon.Position, origFwd, m_teamId, weapon.TargetFlags, weapon.WeaponDefinition.RotationLimit, weapon.MinRange);
                        m_findTargetTimer = UnityEngine.Random.Range( 0.2f, 0.3f );
                    }
                }

                if ( target != null ) {
                    var targetNode = target.GetComponent<PathFindingNode>();
                    Vector3 to = targetNode.Center - ( m_aiComms.Position + m_center );
                    float distSqrd = to.sqrMagnitude;
                    float range = weapon.Range;
                    float minRange = weapon.MinRange;

                    if ( distSqrd < range * range &&
                    distSqrd > minRange * minRange ) {
                        weapon.Target = target;

                        TurretWeapon turretWeapon = weapon as TurretWeapon;

                        bool isAimed = true;
                        if ( turretWeapon != null ) {
                            turretWeapon.ResetIdleTimer();
                            isAimed = turretWeapon.IsAimed;
                        }

                        if ( isAimed ) {
                            weapon.Fire();
                        } else {
                            weapon.StopFiring();
                        }
                    } else {
                        weapon.Target = null;
                        weapon.StopFiring();

                        if ( distSqrd < range * range * 2 ) {
                            m_findTargetTimer = 0; // target is close, find a new target next frame
                        }
                    }
                }
            } else {
                // just fired so stay alert, find a new target next frame
                m_findTargetTimer = 0;
            }

            //TODO:Update here??
//      weapon.Update(deltaTime);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Enter ( StateArgs stateArgs ) {  
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Exit () {
        if ( m_weapons != null ) {
            foreach ( Weapon weapon in m_weapons ) {
                //TODO:Stop sound
//        weapon.StopSounds();
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private List<Weapon> m_weapons;
    private uint m_teamId;
    private float m_findTargetTimer;
    private Vector3 m_center;
    private AIComms m_aiComms;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

