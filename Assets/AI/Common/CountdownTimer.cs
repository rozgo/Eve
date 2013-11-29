//---------------------------------------------------------------------------------------------------------------------   
using UnityEngine;
using System.Collections;
using System;
//---------------------------------------------------------------------------------------------------------------------   


//---------------------------------------------------------------------------------------------------------------------   
public class CountdownTimerEventArgs : EventArgs
{
  public int SecondsRemaining { get; set; }
}


//---------------------------------------------------------------------------------------------------------------------   
public class CountdownTimer : Timer
{
  public int SecondsRemaining
  {
    get
    {
      int secondsSinceStart = (int)Math.Floor((DateTime.UtcNow - m_startedTimeStamp).TotalSeconds);

      return m_initialSecondsRemaining - secondsSinceStart;
    }
  }

//---------------------------------------------------------------------------------------------------------------------   
  public CountdownTimer (DateTime startedTimeStamp, int initialSecondsRemaining)
  {
    Reset(startedTimeStamp, initialSecondsRemaining);
    m_event = new CountdownTimerEventArgs();
  }
  
  
//---------------------------------------------------------------------------------------------------------------------   
  public void Reset(DateTime startedTimeStamp, int initialSecondsRemaining)
  {
    m_startedTimeStamp = startedTimeStamp;
    m_initialSecondsRemaining = initialSecondsRemaining;
    m_previousSecondsRemaining = initialSecondsRemaining;
  }
  
  
//---------------------------------------------------------------------------------------------------------------------   
  protected override void OnUpdate () 
  {
    m_event.SecondsRemaining = SecondsRemaining;
    
    if (m_event.SecondsRemaining != m_previousSecondsRemaining)
    {   
      OnTick(m_event);
      m_previousSecondsRemaining = m_event.SecondsRemaining;
    }
  }
  

//---------------------------------------------------------------------------------------------------------------------     
  DateTime m_startedTimeStamp;
  int m_initialSecondsRemaining;
  int m_previousSecondsRemaining;
  CountdownTimerEventArgs m_event;

//---------------------------------------------------------------------------------------------------------------------     
}
//---------------------------------------------------------------------------------------------------------------------     
