//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using Blocks;
//---------------------------------------------------------------------------------------------------------------------   

//---------------------------------------------------------------------------------------------------------------------   
public class AIStateBeingRevived : AIState
{
  public static readonly float TimeUntilRevivedStart = 5.0f; // when to start playing the revived animation
  public static readonly float TimeUntilRevivedEnd = 8.0f; // when revived animation is done and ready to return to attack

  //---------------------------------------------------------------------------------------------------------------------   
  public AIStateBeingRevived(AIComms aiComms)
  {
    m_aiComms = aiComms;
  }

  //---------------------------------------------------------------------------------------------------------------------   
  public override void Enter(StateArgs stateArgs)
  {
    m_timer = 0;
    m_playedAnimation = false;
  }

  //---------------------------------------------------------------------------------------------------------------------
  public override void Update(float deltaTime)
  {
    m_timer += deltaTime;

    if (!m_playedAnimation &&
        m_timer > TimeUntilRevivedStart)
    {
      m_aiComms.Animation = AIComms.AnimationState.Revived;
      m_playedAnimation = true;
    }
  }

  //---------------------------------------------------------------------------------------------------------------------
  private AIComms m_aiComms;
  private float m_timer;
  private bool m_playedAnimation;

  //---------------------------------------------------------------------------------------------------------------------   
}
//---------------------------------------------------------------------------------------------------------------------   

