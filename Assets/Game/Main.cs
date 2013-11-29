using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {
    // Use this for initialization
    void Start () {
        gameStateMachine = new GameStateMachine ();
        Object.DontDestroyOnLoad (gameObject);
    }

    void Update () {
      gameStateMachine.Update (Time.deltaTime);
    }


  private GameStateMachine gameStateMachine;
  public GameObject instantiator;
}
