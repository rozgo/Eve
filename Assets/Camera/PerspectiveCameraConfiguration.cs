using UnityEngine;
using System.Collections;

public class PerspectiveCameraConfiguration : MonoBehaviour {

  public FakeCamera gameCamera;
  public FakeCamera upperLimit;
  public FakeCamera lowerLimit;

  public Bounds m_lookAtLimits = new Bounds (new Vector3 (70, 0, 70), new Vector3 (250, 1, 250));
  public Bounds m_lookAtLimitsZoomIn = new Bounds(new Vector3(70, 0, 70), new Vector3(150, 3, 150));

}
