//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class OpenCloseMap {
    //---------------------------------------------------------------------------------------------------------------------
    private PathNode[] m_Map;

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int Count { get; private set; }
    //---------------------------------------------------------------------------------------------------------------------
    public PathNode this [ int x, int z, int direction ] {
        get {
            return m_Map[ Flatten( x, z, direction ) ];
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public PathNode this [ PathNode Node ] {
        get {
            return m_Map[ Flatten( Node.X, Node.Z, Node.Direction ) ];
        }

    }
    //---------------------------------------------------------------------------------------------------------------------
    public bool IsEmpty {
        get {
            return Count == 0;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public OpenCloseMap ( byte inWidth, byte inHeight ) {
        m_Map = new PathNode[inWidth * inHeight * 8];
        Width = inWidth;
        Height = inHeight;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Add ( PathNode inValue ) {

#if DEBUG
        PathNode item = m_Map[ Flatten( inValue.X, inValue.Z, inValue.Direction ) ];
//    DebugConsole.Assert(DebugChannel.AI, item == null);
#endif

        Count++;
        m_Map[ Flatten( inValue.X, inValue.Z, inValue.Direction ) ] = inValue;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public bool Contains ( PathNode inValue ) {
        PathNode item = m_Map[ Flatten( inValue.X, inValue.Z, inValue.Direction ) ];

        if ( item == null )
            return false;

        return true;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Remove ( PathNode inValue ) {
        Count--;
        m_Map[ Flatten( inValue.X, inValue.Z, inValue.Direction ) ] = null;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Clear () {
        Count = 0;
        Array.Clear( m_Map, 0, m_Map.Length );
    }

    private int Flatten ( int a, int b, int c ) {
        return a + ( b * Width ) + ( c * Width * Height );
    }
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

