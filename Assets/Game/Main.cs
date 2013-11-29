using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {
    void Start() {
        gameStateMachine = new GameStateMachine();
        Object.DontDestroyOnLoad(gameObject);
    }

    void Update() {
        gameStateMachine.Update(Time.deltaTime);
    }

    private GameStateMachine gameStateMachine;
    public GameObject instantiator;
}
