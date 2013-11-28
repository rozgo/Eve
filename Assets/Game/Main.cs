using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {

	// Use this for initialization
	void Start () {
    Instantiator.Instantiate ("Common/SharedShaders","Environment",null,null);
    Instantiator.Instantiate ("HomeScene/Environment","Environment",null,null);

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
