//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
//---------------------------------------------------------------------------------------------------------------------   


//---------------------------------------------------------------------------------------------------------------------   
public class AIStateVictory : AIState
{

  //---------------------------------------------------------------------------------------------------------------------   
  public AIStateVictory(AIComms aiComms, Goto go, float turnSpeed, bool isBiped, bool isAirUnit)
  {
    m_aiComms = aiComms;
    m_goto = go;
    m_turnSpeed = turnSpeed;
    m_isBiped = isBiped;
    m_isAirUnit = isAirUnit;
  }


  //---------------------------------------------------------------------------------------------------------------------     
  public override void Update(float deltaTime)
  {
    if (m_isBiped)
    {
      Vector3 resultDir = Goto.TurnTowards(m_aiComms.Position, m_aiComms.Rotation, Camera.main.transform.position, m_turnSpeed * deltaTime * Mathf.Deg2Rad);
      m_aiComms.Rotation = Quaternion.LookRotation(resultDir);
    }
    else if (m_isAirUnit)
    {
      // air units fly off the map
      m_goto.Update(deltaTime);
    }
  }


  //---------------------------------------------------------------------------------------------------------------------     
  public override void Enter(StateArgs stateArgs)
  {
    if (m_isBiped)
    {
      m_aiComms.Animation = AIComms.AnimationState.Victory;
    }
    else if (m_isAirUnit)
    {
      float r1 = UnityEngine.Random.Range(-10, 10);
      float r2 = UnityEngine.Random.Range(-10, 10);

      Vector3 offMap = new Vector3(-80 + r1, 10, -80 + r2);
      m_goto.Go(offMap, 0, null, null, null);
    }
  }


  //---------------------------------------------------------------------------------------------------------------------     
  public override void Exit()
  {
  }

  AIComms m_aiComms;
  float m_turnSpeed;
  bool m_isBiped;
  bool m_isAirUnit;
  Goto m_goto;
  //---------------------------------------------------------------------------------------------------------------------   
}
//---------------------------------------------------------------------------------------------------------------------   

