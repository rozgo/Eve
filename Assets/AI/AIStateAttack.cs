//---------------------------------------------------------------------------------------------------------------------     
using System;
using UnityEngine;
using System.Collections.Generic;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
class AIStateAttackStateArgs : StateArgs {
    public Health target;
    public Vector3 position;
}
//---------------------------------------------------------------------------------------------------------------------
public class AIStateAttack : AIState {
    //---------------------------------------------------------------------------------------------------------------------
    public AIStateAttack (
        Goto go,
        NavigationSystem navigationSystem, 
        List<Weapon> weapons, 
        AIComms aiComms,
        int ownerInstanceId,
        uint teamId,
        float turnSpeed,
        AIFlags preferredTargetType,
        InWorldTargetSelectionManager inWorldTargetSelectionManager ) {
        m_goto = go;
        m_navigationSystem = navigationSystem;
        m_weapons = weapons;
        m_aiComms = aiComms;
        m_ownerInstanceId = ownerInstanceId;
        m_teamId = teamId;
        m_turnSpeed = turnSpeed;
        m_preferredTargetType = preferredTargetType;
        m_inWorldTargetSelectionManager = inWorldTargetSelectionManager;
    }
    #if UNITY_EDITOR
    //---------------------------------------------------------------------------------------------------------------------
    public override void RealtimeUpdate () {
        Weapon weapon = m_weapons[ 0 ];
//    weapon.DebugDraw();
    }
    #endif
    //---------------------------------------------------------------------------------------------------------------------
    public override void Update ( float deltaTime ) {
        Weapon weapon = m_weapons[ 0 ];

        if ( weapon.Target == null ) {
            AcquireTarget();

            // reset the attack point
            if ( weapon.Target != null ) {
                var node = weapon.Target.GetComponent<PathFindingNode>();
                m_attackPoint = node.Center;
            }
        }

        if ( weapon.Target != null ) {
            // if we've completed the goto and in range then fire
            bool gotoComplete = m_goto.GotoState == Goto.State.INACTIVE;

            bool inWeaponRange = false;

            //TODO:Get Weapon target inrange
//      if (weapon.Target is StructureModel || weapon.Target is HeroModel)
//      {
//        inWeaponRange = PathHelper.IsWithinRange(weapon.Range, m_aiComms.Position.x, m_aiComms.Position.z, weapon.Target);
//      }
//      else
//      {
//        Vector3 to = m_aiComms.Position - weapon.TargetCenter;
//        to.y = 0;
//        inWeaponRange = Vector3.SqrMagnitude(to) < weapon.Range * weapon.Range;
//      }
      
            if ( inWeaponRange && gotoComplete ) {
                m_goto.Stop();
                m_aiComms.Animation = AIComms.AnimationState.Attack;

                TurretWeapon turretWeapon = weapon as TurretWeapon;
                if ( turretWeapon != null ) {
                    if ( turretWeapon.IsReady && turretWeapon.IsAimed ) {
                        turretWeapon.Fire( m_attackPoint );
                    }
                } else {
                    // turn towards the target
                    // either we're infantry or some other unit without a turreted weapon
                    var targetNode = weapon.Target.GetComponent<PathFindingNode>();
                    float maxDegreesDelta = m_turnSpeed * deltaTime;
                    float angle = 0;
                    Vector3 resultDir = Goto.TurnTowards( m_aiComms.Position, m_aiComms.Rotation, targetNode.Center, maxDegreesDelta * Mathf.Deg2Rad, true, ref angle );
                    m_aiComms.Rotation = Quaternion.LookRotation( resultDir );

                    if ( angle < maxDegreesDelta ) {
                        if ( weapon.IsReady ) {
                            weapon.Fire( m_attackPoint );
                        }
                    } else {
                        weapon.StopFiring();
                    }
                }
            } else {
                weapon.StopFiring();
                if ( m_goto.GotoState == Goto.State.INACTIVE ) {
                    m_navigationSystem.UnRegisterUnitGoingToTile( m_ownerInstanceId );

                    var node = weapon.Target.GetComponent<PathFindingNode>();
                    m_attackPoint = node.Position;
                    var structure = node.GetComponent<Structure>();

                    if ( structure != null ) {
                        int side = UnityEngine.Random.Range( 0, 4 );

                        switch ( side ) {
                            case 0:
                                {
                                    m_attackPoint.x += UnityEngine.Random.Range( 0, structure.width * SpaceConversion.MapTileSize );
                                    break;
                                }
                            case 1:
                                {
                                    m_attackPoint.z += UnityEngine.Random.Range( 0, structure.length * SpaceConversion.MapTileSize );
                                    break;
                                }
                            case 2:
                                {
                                    m_attackPoint.x += UnityEngine.Random.Range( 0, structure.width * SpaceConversion.MapTileSize );
                                    m_attackPoint.z += structure.length * SpaceConversion.MapTileSize;
                                    break;
                                }
                            case 3:
                                {
                                    m_attackPoint.x += structure.width * SpaceConversion.MapTileSize;
                                    m_attackPoint.z += UnityEngine.Random.Range( 0, structure.length * SpaceConversion.MapTileSize );
                                    break;
                                }
                            default:
//                DebugConsole.Assert(DebugChannel.AI, false);
                                break;
                        }
                    }
          
                    m_goto.Go( m_attackPoint, weapon.Range - 1.0f, weapon.Target, m_heroToSupport, GotoCallBack );
                }
            }
        } else {
            weapon.StopFiring();
            m_inWorldTargetSelectionManager.UnRegisterAttackTarget( m_ownerInstanceId, weapon.DPS );
        }

        //TODO:Why are we updating here?
//    m_goto.Update(deltaTime);
//    weapon.Update(deltaTime);
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected virtual void AcquireTarget () {
        Health target = null;
        if ( m_preferredTargetType != AIFlags.None ) {
            target = Health.FindNearestObject( m_aiComms.Position, m_teamId, m_preferredTargetType );
        }

        if ( target == null ) {
            // no preferred target found (either we dont have a preference or there is none of that type on the map)
            // find a target that our weapon can shoot at
            Weapon weapon = m_weapons[ 0 ];
            target = Health.FindNearestObject( m_aiComms.Position, m_teamId, weapon.TargetFlags );
        }

        SetNewTarget( target );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void GotoCallBack ( List<PathFindingNode> blockingModels, LinkedListNode<FinalPathNode> finalPathNode ) {
        for ( int i = 0; i < blockingModels.Count; i++ ) {
            if ( blockingModels[ i ].TeamId != m_teamId ) {
                var target = blockingModels[ i ].GetComponent<Health>();
                SetNewTarget( target );
                m_goto.Stop();
                break;
            }
        }
    }

    #region IModelListener

    //---------------------------------------------------------------------------------------------------------------------
    //TODO:Delete this event/model stuff
    //  public void OnModelChanged(object sender, EventArgs e)
    //  {
    //    if (e is ModelDestroyArgs)
    //    {
    //      SetNewTarget(null);
    //    }
    //  }
    //
    //  //---------------------------------------------------------------------------------------------------------------------
    //  public void OnAttach(object sender)
    //  {
    //  }
    //
    //  //---------------------------------------------------------------------------------------------------------------------
    //  public void OnDetach(object sender)
    //  {
    //  }

    #endregion

    //---------------------------------------------------------------------------------------------------------------------
    public override void Enter ( StateArgs stateArgs ) {
        AIStateAttackStateArgs attackStateArgs = stateArgs as AIStateAttackStateArgs;

        if ( attackStateArgs != null ) {
            SetNewTarget( attackStateArgs.target );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Exit () {
        SetNewTarget( null );

        m_goto.Stop();
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected void SetNewTarget ( Health target ) {
        Weapon weapon = m_weapons[ 0 ];
        if ( target == weapon.Target ) {
            return;
        }

        m_goto.Stop();

        if ( weapon.Target != null ) {
            m_inWorldTargetSelectionManager.UnRegisterAttackTarget( m_ownerInstanceId, weapon.DPS );

            //TODO: Detaching?? rewrite this
            {
//        weapon.Target.Detach(this);
            }
        }

        if ( weapon.Target != null ) {
            m_inWorldTargetSelectionManager.RegisterAttackTarget( m_ownerInstanceId, weapon.DPS, weapon.Target );
//      weapon.Target.Attach(this);
        } else {
            weapon.StopFiring();
            //TODO:Weapon should stop sounds
//      weapon.StopSounds();
        }
    }
    //---------------------------------------------------------------------------------------------------------------------

    protected List<Weapon> m_weapons { get; private set; }

    protected AIComms m_aiComms { get; private set; }

    protected uint m_teamId { get; private set; }

    protected Goto m_goto { get; private set; }

    protected Hero m_heroToSupport { get; set; }

    private Vector3 m_attackPoint;
    private Vector3 m_attackFromPosition;
    private AIFlags m_preferredTargetType;
    private NavigationSystem m_navigationSystem;
    private int m_ownerInstanceId;
    private float m_turnSpeed;
    private InWorldTargetSelectionManager m_inWorldTargetSelectionManager;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

