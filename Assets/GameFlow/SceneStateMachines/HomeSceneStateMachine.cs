using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Blocks;
public class HomeSceneStateMachine : StateMachine {

  public enum HomeSceneStates{
    Init,
    Load,
    LoadEnvironment,
    LoadStructures,
    LoadUnits,
    LoadView,
    LoadUIView,
    UpdateStuff
  }

  public HomeSceneStateMachine():base(true){

    Dictionary<int, IState> states = new Dictionary<int, IState> ();

    states.Add((int)HomeSceneStates.Init, new HomeInitState (this));
    states.Add((int)HomeSceneStates.Load, new HomeLoadState (this));
    states.Add((int)HomeSceneStates.LoadEnvironment, new HomeLoadEnvironmentState (this));
    states.Add((int)HomeSceneStates.LoadStructures, new HomeLoadStructuresState (this));
    states.Add((int)HomeSceneStates.LoadUnits, new HomeLoadUnitsState (this));
    states.Add((int)HomeSceneStates.LoadView, new HomeLoadViewState (this));
    states.Add((int)HomeSceneStates.LoadUIView, new HomeLoadUIViewState (this));
    states.Add((int)HomeSceneStates.UpdateStuff, new HomeUpdateStuffState (this));
    base.SetStates (states);
    ChangeState ((int)HomeSceneStates.Init, new StateArgs ());
  }

  public GameObject environment;
  public NavigationSystem navigationSystem;
}

public class HomeInitState : IState
{
  public HomeInitState (HomeSceneStateMachine homeSceneStateMachine) : base ()
  {
    this.homeSceneStateMachine = homeSceneStateMachine;
  }

  public void Enter (StateArgs stateArgs)
  {
    Application.LoadLevel ("HomeScene");
  }

  public void Update (float deltaTime)
  {
    if (!Application.isLoadingLevel) {
      homeSceneStateMachine.ChangeState ((int)HomeSceneStateMachine.HomeSceneStates.Load, new StateArgs ());
    }
  }

  public void Exit ()
  { 
  }
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

  HomeSceneStateMachine homeSceneStateMachine;


}


public class HomeLoadState : IState
{
  public HomeLoadState (HomeSceneStateMachine homeSceneStateMachine) : base ()
  {
    this.homeSceneStateMachine = homeSceneStateMachine;
  }

  public void Enter (StateArgs stateArgs)
  {


    AnnotatedNode[,] annotatedNodes = new AnnotatedNode[SpaceConversion.MapTiles + 1, SpaceConversion.MapTiles + 1];
    for (int i = 0; i < SpaceConversion.MapTiles + 1; i++)
    {
      for (int j = 0; j < SpaceConversion.MapTiles + 1; j++)
      {
        annotatedNodes[i,j] = new AnnotatedNode();
      }
    }


    UnitTracker unitTracker = new UnitTracker();

    AStar solver = new AStar(annotatedNodes, unitTracker);
    DirectionalAStar solverDirectional = new DirectionalAStar(annotatedNodes);

    int maxAStarsNodesExploredPerFrame = 500;
    AStarScheduler aStarScheduler = new AStarScheduler(solver, solverDirectional, maxAStarsNodesExploredPerFrame);

    int [,] baseOutlineStructureIds = new int[SpaceConversion.MapTiles, SpaceConversion.MapTiles];
    ClearanceMap clearanceMap = new ClearanceMap(annotatedNodes, baseOutlineStructureIds);
    CostMap costMap = new CostMap(annotatedNodes);

    homeSceneStateMachine.navigationSystem =  new NavigationSystem(
      clearanceMap,
      costMap,
      aStarScheduler, 
      unitTracker,
      annotatedNodes,
      baseOutlineStructureIds);
  


  }

  public void Update (float deltaTime)
  {
    homeSceneStateMachine.ChangeState ((int)HomeSceneStateMachine.HomeSceneStates.LoadEnvironment, new StateArgs ());
  }

  public void Exit ()
  { 
  }
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

  HomeSceneStateMachine homeSceneStateMachine;

}


public class HomeLoadEnvironmentState : IState
{
  public HomeLoadEnvironmentState (HomeSceneStateMachine homeSceneStateMachine) : base ()
  {
    this.homeSceneStateMachine = homeSceneStateMachine;
  }

  public void Enter (StateArgs stateArgs)
  {
    Instantiator.Instantiate( "Common/SharedShaders", "Shaders", null, null );
    homeSceneStateMachine.environment = Instantiator.Instantiate( "HomeScene/Environment", "Environment", null, null ) as GameObject;
    homeSceneStateMachine.environment.GetComponentInChildren<ShadowGenerator> ().shadowCamera.enabled = false;
    homeSceneStateMachine.environment.GetComponentInChildren<ShadowGenerator> ().enabled = false;
  }

  public void Update (float deltaTime)
  {
    homeSceneStateMachine.ChangeState ((int)HomeSceneStateMachine.HomeSceneStates.LoadStructures, new StateArgs ());
  }

  public void Exit ()
  { 
  }
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

  HomeSceneStateMachine homeSceneStateMachine;

}

public class HomeLoadStructuresState : IState
{
  public HomeLoadStructuresState (HomeSceneStateMachine homeSceneStateMachine) : base ()
  {
    this.homeSceneStateMachine = homeSceneStateMachine;
  }

  public void Enter (StateArgs stateArgs)
  {
    //Buildings/NuclearPowerPlant_L5
    //Buildings/Wall_L1
    var building = Instantiator.Instantiate ("Buildings/NuclearPowerPlant_L5","nuclear stuff",null,null,null);
    var buildingPathfindingNode = building.AddComponent<PathFindingNode>();
    buildingPathfindingNode.Width = 3;
    buildingPathfindingNode.Length = 3;
    var pos = new Vector3(40f,0f,40f);
    buildingPathfindingNode.Position = pos;
    building.transform.position = pos + new Vector3(3 * 0.5f * SpaceConversion.MapTileSize, 0, 3 * 0.5f * SpaceConversion.MapTileSize);
    building.AddComponent<Structure>();

    homeSceneStateMachine.navigationSystem.AddGameModel (buildingPathfindingNode);

//    homeSceneStateMachine.navigationSystem.UpdateGameModelPosition (pos, pos, buildingPathfindingNode);
//    for(int i = 0;i<40;i++){
//      for (int j = 0; j < 40; j++) {
//        var pos = SpaceConversion.GetWorldPositionFromMapTile (i,j);
//
//
//        var building = Instantiator.Instantiate ("Buildings/Wall_L1","wall",null,null,null);
//        var buildingPathfindingNode = building.AddComponent<PathFindingNode> ();
//        buildingPathfindingNode.Length = 3;
//        buildingPathfindingNode.Width = 3;
//        building.transform.position = pos;
//        building.AddComponent<Structure> ();
//        buildingPathfindingNode.Position = pos;
//
//        homeSceneStateMachine.navigationSystem.AddGameModel (buildingPathfindingNode);
//
//        homeSceneStateMachine.navigationSystem.UpdateGameModelPosition (pos, pos, buildingPathfindingNode);
//
//
//
//      }
//    }





  }

  public void Update (float deltaTime)
  {
    homeSceneStateMachine.ChangeState ((int)HomeSceneStateMachine.HomeSceneStates.LoadUnits, new StateArgs ());
  }

  public void Exit ()
  { 
  }
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

  HomeSceneStateMachine homeSceneStateMachine;

}


public class HomeLoadUnitsState : IState
{
  public HomeLoadUnitsState (HomeSceneStateMachine homeSceneStateMachine) : base ()
  {
    this.homeSceneStateMachine = homeSceneStateMachine;
  }

  public void Enter (StateArgs stateArgs)
  {

  }

  public void Update (float deltaTime)
  {
    homeSceneStateMachine.ChangeState ((int)HomeSceneStateMachine.HomeSceneStates.LoadView, new StateArgs ());
  }

  public void Exit ()
  { 
  }
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

  HomeSceneStateMachine homeSceneStateMachine;

}


public class HomeLoadViewState : IState
{
  public HomeLoadViewState (HomeSceneStateMachine homeSceneStateMachine) : base ()
  {
    this.homeSceneStateMachine = homeSceneStateMachine;
  }

  public void Enter (StateArgs stateArgs)
  {

    GameObject.Instantiate (Resources.Load ("GameCamera"));
    GameObject.Instantiate (Resources.Load ("PerspectiveCameraEditor"));
    homeSceneStateMachine.environment.GetComponentInChildren<ShadowGenerator> ().shadowCamera.enabled = true;
    homeSceneStateMachine.environment.GetComponentInChildren<ShadowGenerator> ().enabled = true;
  }

  public void Update (float deltaTime)
  {
    homeSceneStateMachine.ChangeState ((int)HomeSceneStateMachine.HomeSceneStates.LoadUIView, new StateArgs ());
  }

  public void Exit ()
  { 
  }
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

  HomeSceneStateMachine homeSceneStateMachine;

}


public class HomeLoadUIViewState : IState
{
  public HomeLoadUIViewState (HomeSceneStateMachine homeSceneStateMachine) : base ()
  {
    this.homeSceneStateMachine = homeSceneStateMachine;
  }

  public void Enter (StateArgs stateArgs)
  {

  }

  public void Update (float deltaTime)
  {
    homeSceneStateMachine.ChangeState ((int)HomeSceneStateMachine.HomeSceneStates.UpdateStuff, new StateArgs ());
  }

  public void Exit ()
  { 
  }
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

  HomeSceneStateMachine homeSceneStateMachine;

}


public class HomeUpdateStuffState : IState
{
  public HomeUpdateStuffState (HomeSceneStateMachine homeSceneStateMachine) : base ()
  {
    this.homeSceneStateMachine = homeSceneStateMachine;
  }

  public void Enter (StateArgs stateArgs)
  {

  }

  public void Update (float deltaTime)
  {
    homeSceneStateMachine.navigationSystem.Update (deltaTime);
  }

  public void Exit ()
  { 
  }
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

  HomeSceneStateMachine homeSceneStateMachine;

}









