//---------------------------------------------------------------------------------------------------------------------     
using System;
using UnityEngine;
using System.Collections.Generic;
using Blocks;
//---------------------------------------------------------------------------------------------------------------------     


class AIStateLinearBombAttackStateArgs : StateArgs
{
  public Vector3 targetAttackPoint;
  public float movementSpeed;
}

//---------------------------------------------------------------------------------------------------------------------     
public class AIStateLinearBombAttack : AIState
{


  //---------------------------------------------------------------------------------------------------------------------     
  public AIStateLinearBombAttack(List<Weapon> weapons, AIComms aiComms)
  {
    m_weapons = weapons;
    m_aiComms = aiComms;
  }


  //---------------------------------------------------------------------------------------------------------------------     
  public override void Update(float deltaTime)
  {
    Weapon weapon = m_weapons[0];
    if (weapon.IsReady)
    {
      Vector3 to = m_targetAttackPoint - m_aiComms.Position;
      float dist = Mathf.Abs(to.x) + Mathf.Abs(to.z);

      if (dist < weapon.Range && dist > weapon.MinRange)
      {
        // Drop bomb from current position
        Vector3 bombTargetPos = m_aiComms.Position;
        bombTargetPos.y = m_targetAttackPoint.y;

        weapon.Fire(bombTargetPos);
      }
    }
    //TODO:Updating weapons again
//    weapon.Update(deltaTime);

    m_aiComms.Position = m_aiComms.Position + (m_exitDirection * m_attackArgs.movementSpeed * deltaTime);
    m_aiComms.Speed = m_attackArgs.movementSpeed;
  }


  //---------------------------------------------------------------------------------------------------------------------     
  public override void Enter(StateArgs stateArgs)
  {
    m_attackArgs = stateArgs as AIStateLinearBombAttackStateArgs;
    m_targetAttackPoint = m_attackArgs.targetAttackPoint;

    m_exitDirection = m_targetAttackPoint - m_aiComms.Position;
    m_exitDirection.y = 0.0f;
    m_exitDirection.Normalize();

    // desired direction to face
    Quaternion desired = Quaternion.LookRotation(m_exitDirection);
    m_aiComms.Rotation = desired;
  }


  //---------------------------------------------------------------------------------------------------------------------     
  public override void Exit()
  {
  }

  //---------------------------------------------------------------------------------------------------------------------     
  private List<Weapon> m_weapons;
  AIStateLinearBombAttackStateArgs m_attackArgs;
  private Vector3 m_targetAttackPoint;
  private Vector3 m_exitDirection;
  private AIComms m_aiComms;

  //---------------------------------------------------------------------------------------------------------------------       
}
//---------------------------------------------------------------------------------------------------------------------     

