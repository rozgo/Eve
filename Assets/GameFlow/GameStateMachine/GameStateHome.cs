using UnityEngine;
using System.Collections;

public class HomeState : IState
{
  public HomeState (GameStateMachine gameStateMachine) : base ()
  {
    this.gameStateMachine = gameStateMachine;
  }

  public void Enter (StateArgs stateArgs)
  {
    Debug.Log ("HomeState :enter "+ Time.time);
    homeSceneStateMachine = new HomeSceneStateMachine ();

  }

  public void Update (float deltaTime)
  {
    homeSceneStateMachine.Update (deltaTime);
  }

  public void Exit ()
  { 
    Debug.Log ("HomeState :exit "+ Time.time);
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
  HomeSceneStateMachine homeSceneStateMachine;
}