//---------------------------------------------------------------------------------------------------------------------     
using System;
using UnityEngine;
//---------------------------------------------------------------------------------------------------------------------     


//---------------------------------------------------------------------------------------------------------------------     
public class AIStateExploreSky : AIState
{


  //---------------------------------------------------------------------------------------------------------------------     
  public AIStateExploreSky(Goto go, AIComms aiComms)
  {
//    DebugConsole.Assert(DebugChannel.AI, go != null, "need a goto to explore");

    m_goto = go;
    m_aiComms = aiComms;
  }


  //---------------------------------------------------------------------------------------------------------------------     
  public override void Update(float deltaTime)
  {
    Vector3 worldSpacePos = new Vector3(m_aiComms.Position.x, 5.0f, m_aiComms.Position.z);
    m_aiComms.Position = worldSpacePos;

    if (m_goto.GotoState == Goto.State.INACTIVE)
    {
      GotoRandomPosition();
    }
    else
    {
      m_goto.Update(deltaTime);
    }
  }


  //---------------------------------------------------------------------------------------------------------------------     
  public override void Enter(StateArgs stateArgs)
  {
    GotoRandomPosition();
  }

  //---------------------------------------------------------------------------------------------------------------------     
  public override void Exit()
  {
    m_goto.Stop();
  }

  //---------------------------------------------------------------------------------------------------------------------     
  private void GotoRandomPosition()
  {
    Vector3 randPos = new Vector3(UnityEngine.Random.Range(0, 30), 0, UnityEngine.Random.Range(0, 30));

    m_goto.Go(randPos, 0, null, null, null);
  }

  //---------------------------------------------------------------------------------------------------------------------     
  private Goto m_goto;
  private AIComms m_aiComms;

  //---------------------------------------------------------------------------------------------------------------------     
}
//---------------------------------------------------------------------------------------------------------------------     


