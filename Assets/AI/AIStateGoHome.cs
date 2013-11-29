//---------------------------------------------------------------------------------------------------------------------     
using System;
using UnityEngine;
using System.Collections.Generic;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class AIStateGoHomeArgs : StateArgs {
    public AIStateGoHomeArgs () {
    }

    public AIStateGoHomeArgs ( Health homeBuilding ) {
        m_homeBuilding = homeBuilding;
    }

    public Health m_homeBuilding;
}
//---------------------------------------------------------------------------------------------------------------------
public class AIStateGoHome : AIState {
    //---------------------------------------------------------------------------------------------------------------------
    public AIStateGoHome ( Goto go ) {
//    DebugConsole.Assert(DebugChannel.AI, go != null, "need a goto to go home");
        m_goto = go;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Update ( float deltaTime ) {
        m_goto.Update( deltaTime );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Enter ( StateArgs stateArgs ) {
        AIStateGoHomeArgs goHomeArgs = stateArgs as AIStateGoHomeArgs;
        m_homeBuilding = goHomeArgs.m_homeBuilding;
//    m_homeBuilding.Attach(this);
        var homeNode = m_homeBuilding.GetComponent<PathFindingNode>();
        m_goto.Stop();
        m_goto.Go( homeNode.Center, 1, m_homeBuilding, null, null );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public override void Exit () {
//    m_homeBuilding.Detach(this);
        m_goto.Stop();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnModelChanged ( object sender, EventArgs e ) {
//    BuildingModel builderHome = sender as BuildingModel;
//    if (builderHome != null)
//    {
//      PositionChangedEventArgs posChangedArgs = e as PositionChangedEventArgs;
//
//      if (posChangedArgs != null)
//      {
//        m_goto.Go(m_homeBuilding.Center, 1, m_homeBuilding, null, null);
//      }
//    }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnAttach ( object sender ) {
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnDetach ( object sender ) {
    }
    //---------------------------------------------------------------------------------------------------------------------
    private Goto m_goto;
    private Health m_homeBuilding;
}
//---------------------------------------------------------------------------------------------------------------------


