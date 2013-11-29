using UnityEngine;
using System.Collections;

public class BattlePvEState : IState
{
  public BattlePvEState (GameStateMachine gameStateMachine) : base ()
  {

  }

  public void Enter (StateArgs stateArgs)
  {

  }

  public void Update (float deltaTime)
  {

  }

  public void Exit ()
  {

  }
  //TODO:javier, please let me erase the following methods.
  public bool IsReady ()
  {
    return true;
  }

  public void RealtimeUpdate ()
  {

  }

  public bool CallExitOnReEntry ()
  {
    return true;
  }
}