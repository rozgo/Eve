//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public interface IIndexedObject {
    int Index { get; set; }
}
//---------------------------------------------------------------------------------------------------------------------
public class PathNode : IComparer<PathNode>, IIndexedObject {
    //---------------------------------------------------------------------------------------------------------------------
    public PathNode ( byte inX, byte inZ, byte inDirection ) {
        X = inX;
        Z = inZ;
        Direction = inDirection;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public int Compare ( PathNode x, PathNode z ) {
        if ( x.F < z.F ) {
            return -1;
        } else if ( x.F > z.F ) {
            return 1;
        }

        return 0;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static readonly PathNode Comparer = new PathNode( 0, 0, 0 );
    // 20 bytes
    // TODO possible use shorts to further reduce memory footprint
    public float G { get; set; }

    public float H { get; set; }

    public float F { get; set; }

    public int Index { get; set; }

    public byte X { get; set; }

    public byte Z { get; set; }

    public byte Direction { get; set; }
    //---------------------------------------------------------------------------------------------------------------------
    public override string ToString () {
        return string.Format( "G:{0}, H:{1}, F:{2}, I:{3}, X:{4}, Z:{5}, D:{6} {7}",
            G, H, F, Index, X, Z, Direction, Environment.NewLine );
    }
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------
