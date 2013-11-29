//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
public class AIStateSupportingAttackArgs : StateArgs {
    public Hero Hero { get; set; }
}
//---------------------------------------------------------------------------------------------------------------------
public class AIStateSupportingAttack : AIStateAttack {
    //---------------------------------------------------------------------------------------------------------------------
    public AIStateSupportingAttack (
        Goto go,
        NavigationSystem navigationSystem,
        List<Weapon> weapons,
        AIComms aiComms,
        int ownerInstanceId,
        uint teamId,
        float turnSpeed,
        AIFlags preferredTargetType,
        InWorldTargetSelectionManager inWorldTargetSelectionManager,
        uint housingSpace )
    : base(
        go, 
        navigationSystem,
        weapons,
        aiComms,
        ownerInstanceId,
        teamId,
        turnSpeed,
        preferredTargetType,
        inWorldTargetSelectionManager ) {
        m_housingSpaceForThisUnit = housingSpace;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Update ( float deltaTime ) {
        if ( m_isSupporting ) {
            Weapon weapon = m_heroToSupport.GetComponent<Weapon>();
            if ( weapon.Target != m_cachedTarget ) {
                // the hero changed what it is attacking, acquire a new target
                SetNewTarget( null );
            }
        }

        base.Update( deltaTime );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Enter ( StateArgs stateArgs ) {
        AIStateSupportingAttackArgs sa = stateArgs as AIStateSupportingAttackArgs;

        if ( sa != null ) { // can be null if been revived
//      DebugConsole.Assert(DebugChannel.AI, sa.Hero != null);

            m_heroToSupport = sa.Hero;
            m_isSupporting = true;
        }

        m_gotoPositionCached = m_aiComms.Position;
        m_aiComms.Animation = AIComms.AnimationState.Salute;
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override void AcquireTarget () {
        var health = m_heroToSupport.GetComponent<Health>();
        var heroWeapon = m_heroToSupport.GetComponent<Weapon>();
        if ( health.hitPoints > 0 ) {
            // if the hero no longer has a target then force a repath
            // this is required, for example, when the tank finishes with an intermediate target that blocked its route
            bool forceRepath = m_cachedTarget != null && heroWeapon.Target == default(Health);

            m_cachedTarget = heroWeapon.Target;
            SetNewTarget( m_cachedTarget );

            if ( m_cachedTarget == null ) {
                Vector3 heroGotoPosition = m_heroToSupport.GetGotoTargetPosition();
                Vector3 to = m_gotoPositionCached - heroGotoPosition;
                to.y = 0;

                float r = m_heroToSupport.TileLength * SpaceConversion.MapTileSize * 0.6f;

                float salt = 8.0f;
                if ( to.sqrMagnitude > r * r + salt || forceRepath ) {
                    Vector3 heroPosition = m_heroToSupport.Position;
                    Vector3 heroTo = heroGotoPosition - heroPosition;
                    heroTo.y = 0;
                    heroTo.Normalize();

                    // the hero may have been blocked by an intermediate target that is now destroyed
                    // check for that case here and re start a goto, if needed

                    // if the distance > 0 or 
                    if ( heroTo.sqrMagnitude > 0.5f || forceRepath ) {
                        Quaternion q = Quaternion.LookRotation( heroTo );

                        r += UnityEngine.Random.Range( 0, 1.5f ); // add a little variation so they don't form a perfect circle

                        float theta = UnityEngine.Random.Range( -Mathf.PI * 0.25f, Mathf.PI * 1.25f ); // 0 is to the right
                        float x = r * Mathf.Cos( theta );
                        float z = r * Mathf.Sin( theta );

                        Vector3 randomPos = q * ( new Vector3( x, 0, z ) );

                        randomPos += heroGotoPosition;

                        m_gotoPositionCached = randomPos;
                        m_goto.Go( randomPos, 0, null, m_heroToSupport, null );
                    }
                }
            }
        } else {
            // no longer have a hero to support. default to base class behvaiour
            m_isSupporting = false;
            base.AcquireTarget();
        }
    }

    public override void Exit () {
        if ( m_heroToSupport != null ) {
            m_heroToSupport.RemoveFromPlatoon( m_housingSpaceForThisUnit );
        }
    }

    private Health m_cachedTarget;
    private bool m_isSupporting;
    private Vector3 m_gotoPositionCached;
    private uint m_housingSpaceForThisUnit;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

