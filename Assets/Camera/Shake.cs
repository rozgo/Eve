using UnityEngine;
using System.Collections;

public class Shake {

  public float ScaleFactor { get; set; }
  public float Speed { get; set; }
  public float MaxHeight { get; set; }
  public float Span { get; set; }

  //---------------------------------------------------------------------------------------------------------------------   

  public Shake ( Camera camera )
  {
    m_camera = camera;
    ScaleFactor = 20f;
    Speed = 20f;
    MaxHeight = 100f;
    Span = 0.25f;
    m_perlin = new Perlin(UnityEngine.Random.Range(1,1000));
  }

  //---------------------------------------------------------------------------------------------------------------------   

  public void OnExplosion ( Vector3 origin )
  {
    var transform = m_camera.transform;
    var r = origin - transform.position;
    m_axis = r - Vector3.Dot (r, transform.forward) * transform.forward;
    m_scale = (1 - (r.magnitude/MaxHeight)) * ScaleFactor;
    m_limitTime = Time.time + Span;
  }

  //---------------------------------------------------------------------------------------------------------------------   

  public Vector3 Offset ()
  {
    var time = Time.time;

    if ( time > m_limitTime ) 
    {
      return Vector3.zero;
    }

    var offset = m_axis.normalized * m_perlin.Noise( time * Speed );
    offset.y = 0;

    return offset * m_scale;
  }

  //---------------------------------------------------------------------------------------------------------------------   

  private Camera m_camera;
  private Perlin m_perlin;
  private float m_scale = 0;
  private float m_limitTime = 0;
  private Vector3 m_axis = Vector3.zero;

}
