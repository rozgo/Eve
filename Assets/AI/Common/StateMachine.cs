//---------------------------------------------------------------------------------------------------------------------     
using System;
using System.Collections.Generic;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public abstract class StateMachine {
    public int ActiveStateId { get; private set; }

    public IState ActiveState { get; private set; }
    //---------------------------------------------------------------------------------------------------------------------
    public StateMachine ( bool allowReEntry ) {    
        ActiveStateId = DEFAULT;
        AllowReEntry = allowReEntry;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void SetStates ( Dictionary<int, IState> states ) {
        m_states = states;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public virtual void Update ( float deltaTime ) {
        ActiveState.Update( deltaTime );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public virtual void RealtimeUpdate () {
        ActiveState.RealtimeUpdate();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public virtual void ChangeState ( int newState, StateArgs stateArgs ) { 
//    DebugConsole.Assert(DebugChannel.StateMachine, newState > DEFAULT, "invalid state change request");

        if ( HasState( newState ) == false ) {
//      DebugConsole.Assert(DebugChannel.StateMachine, false, "Unexpected game state change. Forget to add this state to the statemachine?");
            return;
        }
    
        if ( AllowReEntry == false &&
             newState == ActiveStateId ) {
//      DebugConsole.Assert(DebugChannel.StateMachine, false, "Unexpected game state change: same state");
            return;
        }

        if ( m_states.ContainsKey( ActiveStateId ) ) {
            if ( ActiveStateId != newState ||
                 ActiveState.CallExitOnReEntry() ) {
                ActiveState.Exit();
            }
        }
    
        // set the new active state
        ActiveStateId = newState;
        ActiveState = m_states[ ActiveStateId ];
    
        // and initialize it
        ActiveState.Enter( stateArgs );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public bool HasState ( int state ) {
        return m_states.ContainsKey( state );
    }

    protected Dictionary<int, IState> m_states;
    private bool AllowReEntry;
    private const int DEFAULT = -1;
}
