
//---------------------------------------------------------------------------------------------------------------------   
using UnityEngine;
using System.Collections;
using System;
//---------------------------------------------------------------------------------------------------------------------   


//---------------------------------------------------------------------------------------------------------------------   
public class CountupTimerEventArgs : EventArgs
{
  public uint TotalSecondsElapsed { get; set; }
  public uint SecondsElapsedSinceLastTick { get; set; }
}


//---------------------------------------------------------------------------------------------------------------------   
public class CountupTimer : Timer
{
  public uint TotalSecondsElapsed
  {
    get
    {
      return (uint)(DateTime.UtcNow - m_startedTimeStamp).TotalSeconds;
    }
  }
//---------------------------------------------------------------------------------------------------------------------   
 public CountupTimer (DateTime startedTimeStamp)
 {
    Reset(startedTimeStamp);
    m_event = new CountupTimerEventArgs();
 }
 
  
//---------------------------------------------------------------------------------------------------------------------   
  public void Reset(DateTime startedTimeStamp)
  {
    m_startedTimeStamp = startedTimeStamp;
    m_previousTotalSecondsElapsed = 0;
  }
  
  
//---------------------------------------------------------------------------------------------------------------------   
  protected override void OnUpdate () 
  {
    m_event.TotalSecondsElapsed = TotalSecondsElapsed;
    
    if (m_event.TotalSecondsElapsed != m_previousTotalSecondsElapsed)
    {   
      m_event.SecondsElapsedSinceLastTick = m_event.TotalSecondsElapsed - m_previousTotalSecondsElapsed;

      OnTick(m_event);

      m_previousTotalSecondsElapsed = m_event.TotalSecondsElapsed;
    }
  }
 

//---------------------------------------------------------------------------------------------------------------------     
  DateTime m_startedTimeStamp;
  uint m_previousTotalSecondsElapsed;
  CountupTimerEventArgs m_event;

//---------------------------------------------------------------------------------------------------------------------     
}
//---------------------------------------------------------------------------------------------------------------------     
