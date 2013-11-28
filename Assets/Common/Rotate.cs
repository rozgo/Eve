using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {

	public Vector3 velocity;
	Vector3 eulers;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		eulers += velocity * Time.deltaTime;
		transform.rotation = Quaternion.Euler(eulers);
	}
}
