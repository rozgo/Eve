using UnityEngine;
using System.Collections;

public class InitState : IState {
    public InitState ( GameStateMachine gameStateMachine ) : base () {
        this.gameStateMachine = gameStateMachine;
    }

    public void Enter ( StateArgs stateArgs ) {
        //Debug.Log ("init :enter "+ Time.time);
    }

    public void Update ( float deltaTime ) {
        //Debug.Log ("init :update "+ Time.time);
        gameStateMachine.ChangeState ( ( int )GameStateMachine.GameStates.Load, new StateArgs () );
    }

    public void Exit () {
        //Debug.Log ("init :exit "+ Time.time);
    }
    //TODO:javier, please let me erase the following methods.
    public bool IsReady () {
        return true;
    }

    public void RealtimeUpdate () {

    }

    public bool CallExitOnReEntry () {
        return true;
    }

    GameStateMachine gameStateMachine;

}