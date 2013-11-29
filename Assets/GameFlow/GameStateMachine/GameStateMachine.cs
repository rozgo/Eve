using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GameStateMachine : StateMachine {

  public enum GameStates{
    Init,
    Load,
    UI,
    HomeScene,
    WorldMap,
    NationMap,
    BattlePvE,
    BattlePvP,
    Transition
  }

  public GameStateMachine():base(true){

    Dictionary<int, IState> states = new Dictionary<int, IState> ();

    states.Add((int)GameStates.Init, new InitState (this));
    states.Add((int)GameStates.Load, new LoadState (this));
    states.Add((int)GameStates.UI, new UIState (this));
    states.Add((int)GameStates.HomeScene, new HomeState (this));
    states.Add((int)GameStates.WorldMap, new WorldMapState (this));
    states.Add((int)GameStates.NationMap, new NationMapState (this));
    states.Add((int)GameStates.BattlePvE, new BattlePvEState (this));
    states.Add((int)GameStates.BattlePvP, new BattlePvPState (this));
    states.Add((int)GameStates.Transition, new TransitionState (this));
    base.SetStates (states);
    ChangeState ((int)GameStates.Init, new StateArgs ());
  }



}


