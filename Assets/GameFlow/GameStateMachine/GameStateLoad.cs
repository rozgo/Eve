using UnityEngine;
using System.Collections;

public class LoadState : IState {
    public LoadState ( GameStateMachine gameStateMachine ) : base () {
        this.gameStateMachine = gameStateMachine;
    }

    public void Enter ( StateArgs stateArgs ) {
        //Debug.Log ("LoadState :enter "+ Time.time);
        Definitions.Get().StartCoroutine( Definitions.Get().LoadDefinitions( () => {
            gameStateMachine.ChangeState ( ( int )GameStateMachine.GameStates.UI, new StateArgs () );
        } ) );
    }

    public void Update ( float deltaTime ) {
    }

    public void Exit () {
        //Debug.Log ("LoadState :exit "+ Time.time);
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