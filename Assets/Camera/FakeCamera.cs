using UnityEngine;
using System.Collections;

public class FakeCamera : MonoBehaviour {


  public float fov = 60f;



  void OnDrawGizmos() {
    Gizmos.color = Color.white;
    Matrix4x4 temp = Gizmos.matrix;
    Gizmos.matrix = Matrix4x4.TRS (transform.position, transform.rotation, Vector3.one);
    Gizmos.DrawFrustum (Vector3.zero, fov, 100f, 0.3f, 1f);
    Gizmos.matrix = temp;
  }



}
