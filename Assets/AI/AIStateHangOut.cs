//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;
//---------------------------------------------------------------------------------------------------------------------   

//---------------------------------------------------------------------------------------------------------------------
public class AIStateHangOutArgs : StateArgs
{
  public GameObject ObjectWithHangOutArea;
  public Structure Structure;
}


//---------------------------------------------------------------------------------------------------------------------   
public class AIStateHangOut : AIState
{

  //---------------------------------------------------------------------------------------------------------------------   
  public AIStateHangOut(Goto go, AIComms aiComms)
  {
    m_goto = go;
    m_aiComms = aiComms;
  }

  //---------------------------------------------------------------------------------------------------------------------   
  public override void Update(float deltaTime)
  {
    if (m_saluting)
    {
      if (m_goto is GotoBiped)
      {
        Vector3 resultDir = Goto.TurnTowards(m_aiComms.Position, m_aiComms.Rotation, Camera.main.transform.position, 180.0f * deltaTime * Mathf.Deg2Rad);
        m_aiComms.Rotation = Quaternion.LookRotation(resultDir);
      }
    }
    else
    {
      if (!m_arrived)
      {
        m_arrived = m_goto.UpdateGoToPosition(m_positionToMoveTo, m_walkSpeed, ref deltaTime);

        if (m_arrived)
        {
          m_aiComms.Speed = 0;
        }
      }
      else
      {
        m_waitTime -= deltaTime;
        if (m_waitTime < 0)
        {
          FindNewPosition(5);
        }
      }
    }
  }

  //---------------------------------------------------------------------------------------------------------------------   
  public override void Enter(StateArgs stateArgs)
  {
//    DebugConsole.Assert(DebugChannel.AI, stateArgs is AIStateHangOutArgs);

    AIStateHangOutArgs sh = stateArgs as AIStateHangOutArgs;
    if (sh != null)
    {
      m_bounds.center = sh.ObjectWithHangOutArea.transform.position;
      Transform t = sh.ObjectWithHangOutArea.transform.FindChild("HangOutArea");
      if (t != null)
      {
        m_bounds.extents = new Vector3(t.localScale.x, 0, t.localScale.z);
        m_bounds.center = t.position;
      }

      FindNewPosition(0);

      m_homeBuilding = sh.Structure;
//      m_homeBuilding.Attach(this);
    }
  }


  //---------------------------------------------------------------------------------------------------------------------   
  public void FindNewPosition(int minTime)
  {
    float height = m_bounds.center.y;
    float multiplier = 1;
    if (m_goto is GotoHelicopter || m_goto is GotoBlimp)
    {
      if (m_goto is GotoHelicopter)
      {
        height = GotoHelicopter.PreferredHeight;
      }
      else
      {
        height = GotoBlimp.PreferredHeight;
      }

      multiplier = 8;
    }

    m_positionToMoveTo.x = m_bounds.center.x + UnityEngine.Random.Range(-m_bounds.extents.x * 0.5f * multiplier, m_bounds.extents.x * 0.5f * multiplier);
    m_positionToMoveTo.y = height;
    m_positionToMoveTo.z = m_bounds.center.z + UnityEngine.Random.Range(-m_bounds.extents.z * 0.5f * multiplier, m_bounds.extents.z * 0.5f * multiplier);

    m_waitTime = UnityEngine.Random.Range(minTime, 10);

    m_arrived = false;
    m_walkSpeed = 2.0f * UnityEngine.Random.Range(0.75f, 1.25f);
  }


  //---------------------------------------------------------------------------------------------------------------------
  public override void Exit()
  {
//    m_homeBuilding.Detach(this);
    m_goto.Stop();
  }

  //---------------------------------------------------------------------------------------------------------------------
  public void OnModelChanged(object sender, EventArgs e)
  {
    //TODO:Handle events
//    Structure home = sender as Structure;
//    if (home == m_homeBuilding)
//    {
//      StructureSelectedEventArgs selected = e as StructureSelectedEventArgs;
//
//      if (selected != null)
//      {
//        if (selected.selected)
//        {
//          m_aiComms.Animation = AIComms.AnimationState.Salute;
//          m_saluting = true;
//          m_goto.Stop();
//        }
//        else
//        {
//          m_aiComms.Animation = AIComms.AnimationState.Idle;
//          m_saluting = false;
//        }
//      }
//    }
  }

  //---------------------------------------------------------------------------------------------------------------------
  public void OnAttach(object sender)
  {
  }

  //---------------------------------------------------------------------------------------------------------------------
  public void OnDetach(object sender)
  {
  }

  //---------------------------------------------------------------------------------------------------------------------   
  private Vector3 m_positionToMoveTo;
  private Bounds m_bounds;
  private Goto m_goto;
  private float m_waitTime;
  private bool m_arrived;
  private float m_walkSpeed;
  private AIComms m_aiComms;
  private Structure m_homeBuilding;
  private bool m_saluting;
  //---------------------------------------------------------------------------------------------------------------------   
}
//---------------------------------------------------------------------------------------------------------------------   

