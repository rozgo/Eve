using System;
using System.Linq;

//---------------------------------------------------------------------------------------------------------------------   
public abstract class Timer
{
  public bool Enabled { get; set; }

  public void Update()
  {
    if (Enabled)
    {
      OnUpdate();
    }
  }

  protected Timer()
  {
    Enabled = true;
  }

  protected abstract void OnUpdate();

//---------------------------------------------------------------------------------------------------------------------   
  protected void OnTick (EventArgs e)
  {
    if (tick != null) 
    {
      tick (this, e);
    }
    else
    {
//      DebugConsole.Assert(DebugChannel.Timer, false, "timer without listener");
    }
  }
  
 
//---------------------------------------------------------------------------------------------------------------------   
  TickEventHandler tick;
  public event TickEventHandler Tick
  {  
    add
    {
      if (tick == null || !tick.GetInvocationList().Contains(value))
      {
          tick += value;
      }
      else if (tick != null)
      {
//        DebugConsole.Assert(DebugChannel.Timer, false, "already added listener");
      }
    }
    remove
    {
      tick -= value;
    }
  }
    
//---------------------------------------------------------------------------------------------------------------------   
}
//---------------------------------------------------------------------------------------------------------------------   
