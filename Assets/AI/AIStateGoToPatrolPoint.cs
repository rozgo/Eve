//---------------------------------------------------------------------------------------------------------------------     
using System;
using UnityEngine;
using System.Collections.Generic;
using Blocks;
//---------------------------------------------------------------------------------------------------------------------     

//---------------------------------------------------------------------------------------------------------------------
public class AIStateGoToPatrolPointArgs : StateArgs
{
  public AIStateGoToPatrolPointArgs()
  {
  }

  public PatrolPoint FirstPatrolPoint;
}

//---------------------------------------------------------------------------------------------------------------------     
public class AIStateGoToPatrolPoint : AIState
{
  // patrol points are a bit special as they can change the AI state directly
  public AIStateMachine AIStateMachine;

  //---------------------------------------------------------------------------------------------------------------------     
  public AIStateGoToPatrolPoint(Goto go, GameObject gameObjectWrapper)
  {
//    DebugConsole.Assert(DebugChannel.AI, go != null, "need a goto to AIStateGoToSpecialLocation");

    m_goto = go;
    m_gameObjectWrapper = gameObjectWrapper;
    m_aiStateHangOutArgs = new AIStateHangOutArgs();
  }


  //---------------------------------------------------------------------------------------------------------------------     
  public override void Update(float deltaTime)
  {
    bool done = false;

    if (m_first && m_goto.GotoState != Goto.State.INACTIVE)
    {
      // have a valid path to the first waypoint
      done = m_goto.Update(deltaTime);
    }
    else
    {
      // off the waypoint grid - just walk directly to the waypoint
      done = m_goto.UpdateGoToPosition(m_patrolPointPosition, ref deltaTime);
    }

    if (done)
    {
      if (m_patrolPoint.Next != null)
      {
        m_patrolPoint = m_patrolPoint.Next.GetComponent<PatrolPoint>() as PatrolPoint;

        Vector3 noise = UnityEngine.Random.insideUnitSphere;
        noise.y = 0;
        m_patrolPointPosition = m_patrolPoint.transform.position + noise;
      }
      else
      {
        if (m_patrolPoint.AttachToChildOf != null)
        {
          bool isBiped = m_goto is GotoBiped;

          if (isBiped) // only bipeds attach to the parent (helicopters, blimps fly above)
          {
            m_gameObjectWrapper.transform.parent = m_patrolPoint.AttachToChildOf.transform;
          }
        }

        if (string.IsNullOrEmpty(m_patrolPoint.ChangeAIState) == false)
        {
          switch (m_patrolPoint.ChangeAIState)
          {
            case "HangOut":
              {
//                DebugConsole.Assert(DebugChannel.AI, m_patrolPoint.AttachToChildOf.transform != null);

                m_aiStateHangOutArgs.ObjectWithHangOutArea = m_patrolPoint.AttachToChildOf.transform.FindChild("LandingCraft").gameObject;
                m_aiStateHangOutArgs.Structure = m_structure;
//                DebugConsole.Assert(DebugChannel.AI, m_aiStateHangOutArgs.ObjectWithHangOutArea != null);

                if (AIStateMachine != null)
                {
                  AIStateMachine.ChangeState((int)AIStateType.HangOut, m_aiStateHangOutArgs);
                }

                return;
              }
            default:
//              DebugConsole.Fail(DebugChannel.AI, "unknown AI state set in patrol point: " + m_patrolPoint.ChangeAIState);
              break;
          }
        }
      }

      m_first = false;
    }
  }


  //---------------------------------------------------------------------------------------------------------------------     
  public override void Enter(StateArgs stateArgs)
  {
    AIStateGoToPatrolPointArgs sl = stateArgs as AIStateGoToPatrolPointArgs;

//    DebugConsole.Assert(DebugChannel.AI, sl != null);

    m_first = true;

    m_patrolPoint = sl.FirstPatrolPoint;

    m_structure = (Structure)m_patrolPoint.Structure;

    if (m_goto is GotoHelicopter || m_goto is GotoBlimp)
    {
      while (m_patrolPoint.Next != null)
      {
        // flying things can go straight to the target
        m_patrolPoint = m_patrolPoint.Next.GetComponent<PatrolPoint>() as PatrolPoint;
      }
    }

    Vector3 noise = UnityEngine.Random.insideUnitSphere;
    noise.y = 0;
    m_patrolPointPosition = m_patrolPoint.transform.position + noise;

    if (m_goto is GotoHelicopter || m_goto is GotoBlimp)
    {
      // TODO
      m_patrolPointPosition.y = 15;
    }

    m_goto.Go(m_patrolPointPosition, 0, null, null, null);
  }

  //---------------------------------------------------------------------------------------------------------------------     
  public override void Exit()
  {
    m_goto.Stop();
  }


  //---------------------------------------------------------------------------------------------------------------------     
  private Goto m_goto;
  private AIStateGoHomeArgs m_goHomeArgs;
  private PatrolPoint m_patrolPoint;
  private Vector3 m_patrolPointPosition;
  private bool m_first;
  private GameObject m_gameObjectWrapper;

  // the structure that we're tryiing to get to
  private Structure m_structure;
  private AIStateHangOutArgs m_aiStateHangOutArgs;
}
//---------------------------------------------------------------------------------------------------------------------     


