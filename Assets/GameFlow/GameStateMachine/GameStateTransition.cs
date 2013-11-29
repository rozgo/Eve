using UnityEngine;
using System.Collections;

public class TransitionState : IState
{

  public class TransitionStateArgs : StateArgs{
    public GameStateMachine.GameStates nextState;
    public StateArgs nextStateArgs;
  }

  public TransitionState (GameStateMachine gameStateMachine) : base ()
  {
    this.gameStateMachine = gameStateMachine;
  }

  public void Enter (StateArgs stateArgs)
  {
    TransitionStateArgs args = stateArgs as TransitionStateArgs;
    nextState = args.nextState;
    nextStateArgs = args.nextStateArgs;
    Debug.Log ("transition :enter "+ Time.time);
  }

  public void Update (float deltaTime)
  {
    Debug.Log ("transition :update "+ Time.time);
    gameStateMachine.ChangeState ((int)nextState, nextStateArgs);
  }

  public void Exit ()
  { 
    Debug.Log ("transition :exit "+ Time.time);
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

  GameStateMachine gameStateMachine;
  GameStateMachine.GameStates nextState;
  StateArgs nextStateArgs;

}