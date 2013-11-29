//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class AIStateControlledAttack : AIState {
    //---------------------------------------------------------------------------------------------------------------------
    public AIStateControlledAttack ( GotoVehicle go, 
                                 InWorldTargetSelectionManager inWorldTargetSelectionManager,
                                 TurretWeapon turretWeapon, 
                                 AIComms aiComms,
                                 int ownerInstanceId,
                                 float maxTurnSpeedDegrees,
                                 uint teamId ) {
//    DebugConsole.Assert(DebugChannel.AI, go != null, "go is null");
//    DebugConsole.Assert(DebugChannel.AI, gameScene != null, "gameScene is null");
//    DebugConsole.Assert(DebugChannel.AI, turretWeapon != null, "turretWeapon is null");
   
        m_goto = go;
        m_inWorldTargetSelectionManager = inWorldTargetSelectionManager;
        m_turretWeapon = turretWeapon;
        m_aiComms = aiComms;
        m_ownerInstanceId = ownerInstanceId;
        m_maxTurnSpeedDegrees = maxTurnSpeedDegrees;
        m_order = Order.GoToPosition;
        m_teamId = teamId;

        m_passiveTargetAITypes = new AIFlags[] { AIFlags.Defenses, AIFlags.Resources, AIFlags.Ground };

        m_marker = GameObject.CreatePrimitive( PrimitiveType.Cube );
        m_marker.transform.localScale = new Vector3( 0.5f, 0.5f, 0.5f );
        m_marker.SetActive( false );
    }
    #if UNITY_EDITOR
    //---------------------------------------------------------------------------------------------------------------------
    public override void RealtimeUpdate () {
//    m_turretWeapon.DebugDraw();
    }
    #endif
    //---------------------------------------------------------------------------------------------------------------------
    public override void Update ( float deltaTime ) {
        // update the turretWeapon first to determine if the hero itself should turn to help the turretWeapon aim
        //TODO:Updates here??
//    m_turretWeapon.Update(deltaTime);

        if ( m_turretWeapon.Target != null ) {
            HandleTarget( deltaTime );
        } else if ( m_goto.GotoState == Goto.State.INACTIVE ) {
            if ( m_order == Order.AttackModel ) {
                HandleAcquireNewTarget();
            } else if ( m_order == Order.GoToPosition ) {
                HandleGotoPositionIfOutOfRange();
            }
        }

        m_goto.Update( deltaTime );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void HandleGotoPositionIfOutOfRange () {
        Vector3 dist = m_aiComms.Position - GotoTargetPosition;
        if ( dist.sqrMagnitude > 4.0f ) {
            m_goto.Go( GotoTargetPosition, 0, null, null, GotoCallBack );
            m_marker.SetActive( true );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void HandleAcquireNewTarget () {
        // the player may be too busy to micro manage this hero
        // see if there is another target to attack in range

        foreach ( AIFlags flags in m_passiveTargetAITypes ) {

            var target = Health.FindNearestObject( m_aiComms.Position, 0, flags );

            if ( target != null ) {
                var targetNode = target.GetComponent<PathFindingNode>();

                bool inWeaponRange = PathHelper.IsWithinRange( m_turretWeapon.Range, m_aiComms.Position.x, m_aiComms.Position.z, targetNode );

                if ( inWeaponRange ) {
                    SetNewTarget( target );
                    break;
                }
            }
        }

        if ( m_turretWeapon.Target == null ) {
            // nothing in range - is there anything to attack not in range
            foreach ( AIFlags flags in m_passiveTargetAITypes ) {
                var target = Health.FindNearestObject( m_aiComms.Position, 0, flags );

                if ( target != null ) {
                    SetNewTarget( target );
                    break;
                }
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void HandleTarget ( float deltaTime ) {
        // find distance to target
        // if target is out of range then move until it is in range

        var targetNode = m_turretWeapon.Target.GetComponent<PathFindingNode>();
        bool inWeaponRange = PathHelper.IsWithinRange( m_turretWeapon.Range, m_aiComms.Position.x, m_aiComms.Position.z, targetNode );

        // if we've been ordered to attack a target in range then fire
        if ( m_order == Order.AttackModel || m_intermediateTarget ) {
            if ( inWeaponRange ) {
                m_goto.Stop();
            } else {
                bool enemyMoved = Vector3.Distance( m_cachedEnemyPosition, targetNode.Center ) > 2.0f;

                if ( m_goto.GotoState == Goto.State.INACTIVE ||
                enemyMoved ) {
                    m_cachedEnemyPosition = targetNode.Center;

                    m_goto.Go( m_cachedEnemyPosition, m_turretWeapon.Range - 2.0f, m_turretWeapon.Target, null, GotoCallBack );
                }
            }
        }

        UpdateWeaponTarget( deltaTime, inWeaponRange );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void UpdateWeaponTarget ( float deltaTime, bool targetInRange ) {
        if ( m_turretWeapon.Target != null ) {
            var targetNode = m_turretWeapon.Target.GetComponent<PathFindingNode>();
            Vector3 currentDir = m_aiComms.Rotation * Vector3.forward;
            Vector3 position = m_aiComms.Position;

            // if the turretWeapon has a turret, use that as the thing we're trying to align to the taget
            currentDir = m_turretWeapon.Forward;
            position = m_turretWeapon.Position;

            currentDir.y = 0;

            Vector3 toTarget = targetNode.Center - position;
            toTarget.y = 0;

            if ( m_goto.GotoState == Goto.State.INACTIVE ) {
                float maxTurnSpeedRad = m_maxTurnSpeedDegrees * Mathf.Deg2Rad;
                toTarget.Normalize();
                Vector3 resultDir = Vector3.RotateTowards( currentDir, toTarget, maxTurnSpeedRad * deltaTime, 0.0f );

                // use the calculated turret rotation and apply it to the hero

                float a = Vector3.Angle( resultDir, currentDir );
                Vector3 cross = Vector3.Cross( resultDir, currentDir );
                if ( cross.y > 0 ) {
                    a = -a;
                }
                Quaternion rotation = Quaternion.AngleAxis( a, Vector3.up );
                m_aiComms.Rotation *= rotation;
            }

            if ( targetInRange && m_turretWeapon.IsAimed ) {
                if ( m_turretWeapon.IsReady ) {
                    m_turretWeapon.Fire();
                }
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void GotoCallBack ( List<PathFindingNode> blockingModels, LinkedListNode<FinalPathNode> finalPathNode ) {
        m_intermediateTarget = false;
        for ( int i = 0; i < blockingModels.Count; i++ ) {
            if ( blockingModels[ i ].TeamId != m_teamId ) {
                var target = blockingModels[ i ].GetComponent<Health>();
                SetNewTarget( target );
                m_intermediateTarget = true;
                break;
            }
        }

        if ( finalPathNode != null ) {
            m_marker.transform.position = finalPathNode.Value.Position;
        }
    }

    #region IModelListener

    //---------------------------------------------------------------------------------------------------------------------
    public void OnModelChanged ( object sender, EventArgs e ) {

        //TODO: Events
//    if (e is ModelDestroyArgs)
//    {
//      IGameModelReadOnly gameModel = sender as IGameModelReadOnly;

//      if (m_turretWeapon.Target == gameModel)
//      {
//        SetNewTarget(null);
//      }
//    }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnAttach ( object sender ) {
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnDetach ( object sender ) {
    }

    #endregion

    //---------------------------------------------------------------------------------------------------------------------
    public override void Enter ( StateArgs stateArgs ) {
        AIStateAttackStateArgs attackStateArgs = stateArgs as AIStateAttackStateArgs;

        m_order = Order.None;
        GotoTargetPosition = m_aiComms.Position; // set a default for the platoon

        if ( attackStateArgs != null ) {
            if ( attackStateArgs.target != null ) {
                SetNewTarget( attackStateArgs.target );

                m_order = Order.AttackModel;

                m_marker.SetActive( false );
            } else {
                if ( m_intermediateTarget ) {
                    SetNewTarget( null );
                }

                m_goto.Go( attackStateArgs.position, 0, null, null, GotoCallBack );
                m_order = Order.GoToPosition;

                m_marker.SetActive( true );
                GotoTargetPosition = attackStateArgs.position;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Exit () {
        SetNewTarget( null );
        m_goto.Stop();
        //TODO:Why here?
//    m_turretWeapon.StopSounds();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override bool CallExitOnReEntry () {
        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected void SetNewTarget ( Health target ) {
        if ( target == m_turretWeapon.Target ) {
            return;
        }

        if ( m_order == Order.AttackModel ) {
            // we were sent to attack something or were on auto pilot, but we have a new target
            // stop where we are going (don't need to do this if we have a GoToPosition order)
            m_goto.Stop();
        }

        if ( m_turretWeapon.Target != null ) {
            m_inWorldTargetSelectionManager.UnRegisterAttackTarget( m_ownerInstanceId, m_turretWeapon.DPS );
//      m_turretWeapon.Target.Detach(this);
        }

        m_turretWeapon.Target = target;
        var targetNode = target.GetComponent<PathFindingNode>();

        if ( m_turretWeapon.Target != null ) {
            m_inWorldTargetSelectionManager.RegisterAttackTarget( m_ownerInstanceId, m_turretWeapon.DPS, m_turretWeapon.Target );
//      m_turretWeapon.Target.Attach(this);
            m_cachedEnemyPosition = targetNode.Center;
//      DebugConsole.Assert(DebugChannel.AI, m_turretWeapon.Target.Health > 0);
        } else {
            m_intermediateTarget = false;
            m_turretWeapon.StopFiring();
            //TODO:Stop sounds on stop firing
//      m_turretWeapon.StopSounds();
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public Vector3 GotoTargetPosition { get; private set; }

    private GotoVehicle m_goto;
    private InWorldTargetSelectionManager m_inWorldTargetSelectionManager;
    private TurretWeapon m_turretWeapon;
    private AIFlags m_preferredTargetType;
    private bool m_firstUpdate;
    private AIComms m_aiComms;
    private Vector3 m_cachedEnemyPosition;
    private int m_ownerInstanceId;
    private float m_maxTurnSpeedDegrees;
    private Order m_order;
    private AIFlags[] m_passiveTargetAITypes;
    private bool m_intermediateTarget;
    private uint m_teamId;

    private enum Order {
        None,
        GoToPosition,
        AttackModel,
    }

    private GameObject m_marker;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

