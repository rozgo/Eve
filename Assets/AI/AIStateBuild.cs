//---------------------------------------------------------------------------------------------------------------------     
using System;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class AIStateBuildArgs : StateArgs {
    public AIStateBuildArgs ( Health buildTarget ) {
        m_buildTarget = buildTarget;
    }

    public readonly Health m_buildTarget;
}
//---------------------------------------------------------------------------------------------------------------------
public class AIStateBuild : AIState {
    //---------------------------------------------------------------------------------------------------------------------
    public AIStateBuild ( Goto go ) {
//    DebugConsole.Assert(DebugChannel.AI, go != null, "need a goto to build");
        m_goto = go;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Update ( float deltaTime ) {
        m_goto.Update( deltaTime );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Enter ( StateArgs stateArgs ) {
        AIStateBuildArgs buildArgs = stateArgs as AIStateBuildArgs;
        m_buildTarget = buildArgs.m_buildTarget;
        var targetNode = m_buildTarget.GetComponent<PathFindingNode>();
//    m_buildTarget.Attach(this);
        m_goto.Go( targetNode.Position, 0, null, null, null );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Exit () {
//    m_buildTarget.Detach(this);
        m_goto.Stop();
    }

    #region IModelListener

    //---------------------------------------------------------------------------------------------------------------------
    public void OnModelChanged ( object sender, EventArgs e ) {
        //TODO:more events
//    PositionChangedEventArgs posChangedArgs = e as PositionChangedEventArgs;
//    if(posChangedArgs != null)
//    {
//      m_goto.Go(m_buildTarget.Position, 0, null, null, null);
//      return;
//    }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnAttach ( object sender ) {
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnDetach ( object sender ) {
    }

    #endregion

    //---------------------------------------------------------------------------------------------------------------------
    private Goto m_goto;
    Health m_buildTarget;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------


