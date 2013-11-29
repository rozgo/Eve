//---------------------------------------------------------------------------------------------------------------------     
using System;
using System.Collections.Generic;
using UnityEngine;
//---------------------------------------------------------------------------------------------------------------------     



//---------------------------------------------------------------------------------------------------------------------     
public enum AIStateType
{
  Unknown = 0,
  Dead,
  Idle,
  Attack,
  Explore,
  Defend,
  Upgrade,
  Train,
  Research,
  Repair,
  Build,
  GoHome,
  PatrolPoint,
  Victory,
  HangOut,
  Unconscious, // can be healed by a medic
  SupportingAttack, // attack whatever the hero is attacking
  BeingRevived,
  AwaitSupplyDrop,
}


//---------------------------------------------------------------------------------------------------------------------     
public class AIStateMachine : StateMachine, IDisposable
{
  //---------------------------------------------------------------------------------------------------------------------     
  public AIStateMachine(bool allowReEntry)
    : base (allowReEntry)
  {
  }

  public void Dispose()
  {
    if (ActiveState != null)
    {
      ActiveState.Exit();
    }
  }
}

//---------------------------------------------------------------------------------------------------------------------     
public class NoOpAIStateMachine : AIStateMachine
{
  public NoOpAIStateMachine()
    : base(false)
  { }

  public override void Update(float deltaTime)
  {
  }
}


//---------------------------------------------------------------------------------------------------------------------     


