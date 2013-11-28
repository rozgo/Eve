﻿using UnityEngine;
using System.Collections;

public class ShadowGenerator : MonoBehaviour
{


  void Start ()
  {
    shadowCamera.SetReplacementShader(shadowShader, null);
    gameCamera = Camera.main;
    ground = new Plane(Vector3.up,new Vector3(40f,0f,40f));
  }

  void Update ()
  {
    float enter = 0;
    var ray = new Ray (gameCamera.transform.position, gameCamera.transform.forward);
    ground.Raycast (ray, out enter);

    var hit = ray.GetPoint (enter);
    transform.parent.position = hit;





    var calculatedTime = (time + offset) % 1; 
    var incidenceAngleStartIndex = Mathf.FloorToInt (calculatedTime * (incidenceAngle.Length));
    var incidenceAngleSecondIndex = (incidenceAngleStartIndex + 1) % (incidenceAngle.Length);
    var x = Mathf.Lerp (incidenceAngle[incidenceAngleStartIndex], incidenceAngle[incidenceAngleSecondIndex], (calculatedTime*incidenceAngle.Length)%1);
    var y = Mathf.Lerp (0, 360, calculatedTime);

    transform.eulerAngles = new Vector3 (x, y, transform.eulerAngles.z);
    var angles = plane.transform.rotation.eulerAngles;
    var pivotAngles = transform.rotation.eulerAngles;
    plane.transform.eulerAngles = new Vector3 (angles.x, pivotAngles.y, angles.z);

    var newScale = 1f / (Mathf.Cos ((pivotAngles.x)*Mathf.PI/180f) / Mathf.Tan ((90-pivotAngles.x)*Mathf.PI/180f));
    plane.transform.localScale = new Vector3 (plane.transform.localScale.x, plane.transform.localScale.y, newScale );


    cameraSize = Mathf.Lerp (minCameraSize, maxCameraSize, ((gameCamera.transform.position.y - lowerLimit) / (upperLimit * 0.6f - lowerLimit)));

    var innerPlaneScale = new Vector3 (cameraSize / 5f, 1, cameraSize / 5f);

    innerPlane.transform.localScale = innerPlaneScale;
    shadowCamera.orthographicSize = cameraSize;


  }


//---------------------------------------------------------------------------------------------------------------------

  [Range (0.0f, 1.0f)]
  public float time;
  [Range (0.0f, 1.0f)]
  public float offset;

  public float[] incidenceAngle;

  public GameObject plane;
  public GameObject innerPlane;// used for scaling.
  public Shader shadowShader;
  public Camera shadowCamera;
  private Camera gameCamera;

  private Plane ground;
  private float cameraSize = 20f;

  private float maxCameraSize = 65f;
  private float minCameraSize = 25f;


  public float lowerLimit = 20;
  public float upperLimit = 40;
  //---------------------------------------------------------------------------------------------------------------------


}
//---------------------------------------------------------------------------------------------------------------------
