using UnityEngine;

namespace Filters
{

  [System.Serializable]
  public class PIDVector
  {
    public Vector3 setpoint;
    public float kp, ki, kd;
    Vector3 integral;
    Vector3 lastFeedback;

    public void Reset (Vector3 feedback)
    {
      integral = Vector3.zero;
      lastFeedback = feedback;
    }

    public Vector3 Compute (Vector3 feedback, float dt)
    {
      var proportional = setpoint - feedback;
      integral = integral + proportional * dt;
      var derivative = -(feedback - lastFeedback) / dt;
      lastFeedback = feedback;
      return kp * proportional + ki * integral + kd * derivative;
    }
  }

  [System.Serializable]
  public class PIDScalar
  {
    public float setpoint;
    public float kp, ki, kd;
    float integral;
    float lastFeedback;

    public void Reset (float feedback)
    {
      integral = 0;
      lastFeedback = feedback;
    }

    public float Compute (float feedback, float dt)
    {
      var proportional = setpoint - feedback;
      integral = integral + proportional * dt;
      var derivative = -(feedback - lastFeedback) / dt;
      lastFeedback = feedback;
      return kp * proportional + ki * integral + kd * derivative;
    }
  }

}
