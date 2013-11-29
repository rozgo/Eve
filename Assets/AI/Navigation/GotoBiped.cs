//---------------------------------------------------------------------------------------------------------------------     
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class GotoBiped : Goto {
    //---------------------------------------------------------------------------------------------------------------------
    public GotoBiped (
        NavigationSystem navigationSystem,
        PathFindingInput pathFindingInput,
        UnitsWallsCoordinator unitsWallsCoordinator,
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
        m_unitsWallsCoordinator = unitsWallsCoordinator;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Go (
        Vector3 destination, 
        float withinRangeDistance, 
        PathFindingNode targetModel,
        Hero heroToAvoid, 
        Action<List<PathFindingNode>, 
        LinkedListNode<FinalPathNode>> pathFoundCallBack ) {
            m_pathFindingInput.HeuristicMultiplier = 1.5f;
            m_pathFindingInput.PostProcessStraighten = true;
            m_pathFindingInput.TeamId = m_teamId;
            m_pathFindingInput.TargetModel = targetModel;
            m_pathFindingInput.HeroToAvoid = heroToAvoid;
            m_pathFindingInput.CanDestroyBlockingBuildings = false;
            m_pathFindingInput.CanDestroyBlockingWalls = true;

            m_pathFindingInput.StopWithinDistanceFromTargetModel = withinRangeDistance;

            m_timeSinceLastJump = 10.0f;
            m_isJumping = false;

            base.GoInternal( destination, pathFoundCallBack, SearchPriorityLevel.Normal );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override bool Update ( float deltaTime ) {
        if ( GotoState == State.INACTIVE ||
         ( GotoState == State.PATH_PENDING && !m_pathFindingOutputCurrent.Valid ) ) {
            // goto is done or we don't yet have a valid first path
            return false;
        }
  
        if ( m_pathFindingOutputCurrent.Path.Count == 0 ) {
            m_aiComms.Animation = AIComms.AnimationState.Idle;

            if ( GotoState == State.ACTIVE ) {
                Stop();
            }

            return true;
        }
    
        Vector3 current = m_pathFindingOutputCurrent.Path.First.Value.Position;

        bool done = UpdateGoToPosition( current, ref deltaTime );

        if ( done ) {
            // pop off the target that we just got to

            m_pathFindingOutputCurrent.Path.RemoveFirst();

            if ( m_pathFindingOutputCurrent.Path.Count == 0 ) {
                if ( GotoState == State.ACTIVE ) {
                    Stop();
                }

                m_aiComms.Animation = AIComms.AnimationState.Idle;

                return true;
            }

            current = m_pathFindingOutputCurrent.Path.First.Value.Position;
            UpdateGoToPosition( current, ref deltaTime );
        } else {
            if ( m_pathFindingOutputCurrent.Path.First.Value.IsJumpNode ) {
                Vector3 toWall = m_aiComms.Position - current;
                toWall.y = 0;
                bool closeToWall = toWall.sqrMagnitude < 2.0f;

                if ( closeToWall ) {
                    if ( !m_isJumping ) {
                        m_aiComms.Animation = AIComms.AnimationState.Jump;
                        m_isJumping = true;
                        m_timeSinceLastJump = 0;

                        m_pathFindingOutputCurrent.Path.First.Value = new FinalPathNode( m_pathFindingOutputCurrent.Path.First.Value.Position,
                            m_pathFindingOutputCurrent.Path.First.Value.Direction,
                            m_pathFindingOutputCurrent.Path.First.Value.IsPivotNode,
                            false );
                    } else {
                        m_aiComms.Animation = AIComms.AnimationState.Locomote;
                        m_isJumping = false;
                    }
                }
            }
        }

        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override bool UpdateGoToPosition ( Vector3 position, float speed, ref float deltaTime ) {
        Vector3 to = position - m_aiComms.Position;
        to.y = 0;

        float distSqrd = to.sqrMagnitude;
        float closeEnoughSqrd = ( speed * deltaTime ) * ( speed * deltaTime );

        if ( distSqrd <= closeEnoughSqrd ) {
            if ( speed > 0 ) {
                float time = to.magnitude / speed;
                deltaTime -= time;
            } else {
                deltaTime = 0;
            }

            return true;
        }

        Vector3 resultDir = Goto.TurnTowards( m_aiComms.Position, m_aiComms.Rotation, position, 360.0f * deltaTime * Mathf.Deg2Rad );
        m_aiComms.Rotation = Quaternion.LookRotation( resultDir );

        to.Normalize();

        // adjust speed if jumping and dictate when the jump should end
        m_timeSinceLastJump += deltaTime;
        if ( m_timeSinceLastJump < 0.8f ) {
            speed = 2.5f;
        } else if ( m_isJumping ) {
            m_isJumping = false;
            m_aiComms.Animation = AIComms.AnimationState.Locomote;
        }

        m_aiComms.Position += to * deltaTime * speed;
        m_aiComms.Speed = speed;

        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Stop () {
        m_unitsWallsCoordinator.UnRegisterBlockingWallList( this, m_pathFindingOutputCurrent.ModelsPotentiallyBlockingAShorterPath );

        base.Stop();
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected override void AStarCompleteCallback () {
        // unregister current list, so we don't repath when a wall is destroyed that we dont care about
        m_unitsWallsCoordinator.UnRegisterBlockingWallList( this, m_pathFindingOutputCurrent.ModelsPotentiallyBlockingAShorterPath );

        base.AStarCompleteCallback();

        // if any of the walls that the A* touched are destroyed then it may be optimal to repath when this happens
        m_unitsWallsCoordinator.RegisterBlockingWallList( this, m_pathFindingOutputCurrent.ModelsPotentiallyBlockingAShorterPath );

        // anim state change needs to happen here, in case Stop() was called while the route was being calculated, and so resetting the animation state to idle 
        m_aiComms.Animation = AIComms.AnimationState.Locomote;
    }

    private float m_timeSinceLastJump;
    private bool m_isJumping;
    private UnitsWallsCoordinator m_unitsWallsCoordinator;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------


