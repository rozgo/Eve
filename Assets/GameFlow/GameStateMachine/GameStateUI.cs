using UnityEngine;
using System.Collections;

public class UIState : IState {
    public UIState ( GameStateMachine gameStateMachine ) : base () {
        this.gameStateMachine = gameStateMachine;
    }

    public void Enter ( StateArgs stateArgs ) {
        //Debug.Log ("UIState :enter "+ Time.time);
    }

    public void Update ( float deltaTime ) {
        //Debug.Log ("UIState :update "+ Time.time);
        gameStateMachine.ChangeState ( ( int )GameStateMachine.GameStates.HomeScene, new StateArgs () );
    }

    public void Exit () {
        //Debug.Log ("UIState :exit "+ Time.time);
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