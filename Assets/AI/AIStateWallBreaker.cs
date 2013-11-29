//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class AIStateWallBreaker : AIState {
    //---------------------------------------------------------------------------------------------------------------------
    public AIStateWallBreaker (
        Goto go, 
        List<Weapon> weapons, 
        AIComms aiComms, 
        int ownerInstanceId,
        UnitsWallsCoordinator unitsWallsCoordinator, 
        float fuseTimer ) {
        m_goto = go;
        m_weapons = weapons;
        m_aiComms = aiComms;
        m_ownerInstanceId = ownerInstanceId;
        m_unitsWallsCoordinator = unitsWallsCoordinator;
        m_initialFuseTimer = fuseTimer;
        m_fuseTimer = fuseTimer;

        m_marker = GameObject.CreatePrimitive( PrimitiveType.Cube );
        m_marker.transform.localScale = new Vector3( 0.5f, 0.5f, 0.5f );
        m_marker.SetActive( false );

        // make sure we only need to get to a node within the diagonal distance to a wall + some salt
        float halfSquareDist = SpaceConversion.MapTileSize * 0.5f;
        s_getWithingRangeOfWallDist = ( halfSquareDist * halfSquareDist ) + ( halfSquareDist * halfSquareDist );
        s_getWithingRangeOfWallDist = Mathf.Sqrt( s_getWithingRangeOfWallDist );
        s_getWithingRangeOfWallDist += 0.1f;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Update ( float deltaTime ) {
        Weapon weapon = m_weapons[ 0 ];
        //TODO:Updates??
        //    weapon.Update(deltaTime);

        WallStructure target = GetWallTarget();

        if ( target == null ) {
            // no more walls (or all walls currently have a wall breaker attacking them)
            return;
        }

        if ( target != m_currentTarget ) {
            var targetNode = target.GetComponent<PathFindingNode>();
            m_currentTarget = target;
            m_done = false;
            m_fuseTimer = m_initialFuseTimer; // reset the fuse

            var health = m_currentTarget.GetComponent<Health>();

            m_goto.Go( targetNode.Center, s_getWithingRangeOfWallDist, health, null, null );

            m_marker.transform.position = targetNode.Center;
            m_marker.SetActive( true );
        }

        if ( m_done ) {
            bool facingCorrectDir = true;

            if ( m_aiComms.Animation != AIComms.AnimationState.Attack ) {
                var targetNode = m_currentTarget.GetComponent<PathFindingNode>();
                float angle = 0;
                Vector3 resultDir = Goto.TurnTowards( m_aiComms.Position, m_aiComms.Rotation, targetNode.Center, 360.0f * deltaTime * Mathf.Deg2Rad, true, ref angle );
                m_aiComms.Rotation = Quaternion.LookRotation( resultDir );

                if ( angle > 2 ) {
                    facingCorrectDir = false;
                }
            }

            if ( facingCorrectDir ) {
                m_aiComms.Animation = AIComms.AnimationState.Attack;
                m_fuseTimer -= deltaTime;

                if ( m_fuseTimer <= 0 ) {
                    // death and glory
                    //m_weapon.Target = m_currentTarget;
                    //m_weapon.Fire();

                    // TODO splash damage
                    var health = m_currentTarget.GetComponent<Health>();
                    health.DoDamage( 10000, m_ownerInstanceId );

                    // find a new wall target
                    Reset();

                    m_aiComms.Animation = AIComms.AnimationState.Idle;

                    return;
                }
            } else {
                // should be a turn animation
                m_aiComms.Animation = AIComms.AnimationState.Idle;
            }
      
            return;
        }

        if ( m_goto.GotoState == Goto.State.PATH_PENDING ) {
            m_done = false;
        } else {
            m_done = m_goto.Update( deltaTime );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Exit () {
        Reset();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Enter ( StateArgs stateArgs ) {
        m_aiComms.Animation = AIComms.AnimationState.Idle;
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void Reset () {
        m_unitsWallsCoordinator.RemoveWallTargetForWallbreaker( m_ownerInstanceId );

        m_goto.Stop();
        m_currentTarget = null;
        m_done = false;
        m_fuseTimer = m_initialFuseTimer;

        m_marker.SetActive( false );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private WallStructure GetWallTarget () {
        var target = m_unitsWallsCoordinator.GetWallTargetForWallbreaker( m_ownerInstanceId );
        if ( target != null ) {
            return target;
        }

        // do not have a target, let the fun begin

        if ( m_doneFirstSearch ) {
            target = GetWallTargetClosest();
        } else {
            target = GetWallTargetCardinalSearch();
        }

        if ( target != null ) {
            m_doneFirstSearch = true;
            m_unitsWallsCoordinator.SetWallTargetForWallbreaker( m_ownerInstanceId, target );
        }

        return target;
    }
    //---------------------------------------------------------------------------------------------------------------------
    private WallStructure GetWallTargetClosest () {
        int px, pz;
        SpaceConversion.GetMapNodeFromWorldPosition( m_aiComms.Position, out px, out pz );

        return m_unitsWallsCoordinator.FindNearestWall( px, pz );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private WallStructure GetWallTargetCardinalSearch () {
        // wall breaker AI in CoC is awful. Rather than copy and improve on it let's try a hopefully simple 
        // but powerful approach that keeps the player in full control

        // for each of the cardinal directions, find the closest wall
        // if none exists, try again from the four surrounding squares

        int px, pz;
        SpaceConversion.GetMapNodeFromWorldPosition( m_aiComms.Position, out px, out pz );

        int minDist = int.MaxValue;
        WallStructure target = GetWallInCardinalDirectionFromTile( px, pz, ref minDist );

        if ( target != null ) {
            return target;
        }

        // check the four surrounding squares and keep going until we've checked the entire map
        for ( int radius = 1; radius < SpaceConversion.MapTiles; radius++ ) {
            WallStructure temp = GetWallInCardinalDirectionFromTile( px + radius, pz + radius, ref minDist );

            if ( temp != null ) {
                target = temp;
            }

            temp = GetWallInCardinalDirectionFromTile( px - radius, pz + radius, ref minDist );

            if ( temp != null ) {
                target = temp;
            }

            temp = GetWallInCardinalDirectionFromTile( px + radius, pz - radius, ref minDist );

            if ( temp != null ) {
                target = temp;
            }

            temp = GetWallInCardinalDirectionFromTile( px - radius, pz - radius, ref minDist );

            if ( temp != null ) {
                target = temp;
            }

            if ( target != null ) {
                return target;
            }
        }

        return null;
    }
    //---------------------------------------------------------------------------------------------------------------------
    private WallStructure GetWallInCardinalDirectionFromTile ( int px, int pz, ref int minDist ) {
        if ( SpaceConversion.InMapBounds( px, pz ) == false ) {
            return null;
        }

        int counter = 0;
        WallStructure target = null;

        for ( int x = px + 1; x < SpaceConversion.MapTiles; x++ ) {
            counter++;
            if ( counter >= minDist ) {
                break;
            }

            WallStructure temp = m_unitsWallsCoordinator.Walls[ x, pz ];
            if ( temp != null ) {
                minDist = counter;
                target = temp;
                break;
            }
        }

        counter = 0;
        for ( int x = px - 1; x >= 0; x-- ) {
            counter++;
            if ( counter >= minDist ) {
                break;
            }

            WallStructure temp = m_unitsWallsCoordinator.Walls[ x, pz ];
            if ( temp != null ) {
                minDist = counter;
                target = temp;
                break;
            }
        }

        counter = 0;
        for ( int z = pz + 1; z < SpaceConversion.MapTiles; z++ ) {
            counter++;
            if ( counter >= minDist ) {
                break;
            }

            WallStructure temp = m_unitsWallsCoordinator.Walls[ px, z ];
            if ( temp != null ) {
                minDist = counter;
                target = temp;
                break;
            }
        }

        counter = 0;
        for ( int z = pz - 1; z >= 0; z-- ) {
            counter++;
            if ( counter >= minDist ) {
                break;
            }

            WallStructure temp = m_unitsWallsCoordinator.Walls[ px, z ];
            if ( temp != null ) {
                minDist = counter;
                target = temp;
                break;
            }
        }

        return target;
    }
    //---------------------------------------------------------------------------------------------------------------------
    private Goto m_goto;
    private List<Weapon> m_weapons;
    private AIComms m_aiComms;
    private UnitsWallsCoordinator m_unitsWallsCoordinator;
    private WallStructure m_currentTarget;
    private int m_ownerInstanceId;
    private bool m_done;
    private float m_fuseTimer;
    private float m_initialFuseTimer;
    private GameObject m_marker;
    private bool m_doneFirstSearch;
    private static float s_getWithingRangeOfWallDist;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------


