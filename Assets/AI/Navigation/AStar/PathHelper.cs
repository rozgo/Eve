//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class PathHelper {
    //---------------------------------------------------------------------------------------------------------------------
    public static void ConvertFromClearanceMapPositionToWorldPosition ( LinkedList<FinalPathNode> list, uint unitWidth ) {
        LinkedListNode<FinalPathNode> current = list.First;
        while ( current != null ) {
            Vector3 adjustedPos = SpaceConversion.GetWorldPositionFromClearanceMapPosition( current.Value.Position, unitWidth );

            FinalPathNode newNode = new FinalPathNode( adjustedPos, current.Value.Direction, current.Value.IsPivotNode, current.Value.IsJumpNode );

            current.Value = newNode;

            current = current.Next;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static void Straighten ( LinkedList<FinalPathNode> list, AnnotatedNode[,] annotatedNodes, Hero heroToAvoid, bool canPathThroughDangerAreas ) {
//    DebugConsole.Assert(DebugChannel.AI, list.Count >= 2, "expecting at least 2 nodes in list - the start and end positions");
//    DebugConsole.Assert(DebugChannel.AI, list.First.Value.IsPivotNode, "first node expected to be a pivot");
//    DebugConsole.Assert(DebugChannel.AI, list.Last.Value.IsPivotNode, "last node expected to be a pivot");

        LinkedListNode<FinalPathNode> current = list.First;
        LinkedListNode<FinalPathNode> check = current.Next; 

        while ( check != null ) {
            bool mustKeep = current.Value.IsJumpNode;

            mustKeep |= IsPathBlockedForBiped( current.Value.Position, check.Value.Position, annotatedNodes, heroToAvoid, canPathThroughDangerAreas );

            if ( mustKeep ) {
                // we cannot straight line to this position, so the previous square cannot be ignored
                // unless it is the first node which is always added (see above)
                FinalPathNode pivot = new FinalPathNode(
                                  check.Previous.Value.Position,
                                  check.Previous.Value.Direction, 
                                  true,
                                  check.Previous.Value.IsJumpNode );

                check.Previous.Value = pivot;

                // start looking again from the previous tile
                current = check.Previous;
            }

            // advance to the next square
            check = check.Next;
        }

        // remove the non pivot nodes
        current = list.First.Next;

        while ( current != null ) {
            if ( current.Value.IsPivotNode == false ) {
                LinkedListNode<FinalPathNode> temp = current.Next;
                list.Remove( current );
                current = temp;
            } else {
                current = current.Next;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static bool IsPathBlockedForBiped ( Vector3 start, Vector3 end, AnnotatedNode[,] annotatedNodes, Hero heroToAvoid, bool canPathThroughDangerAreas ) {
        int endX, endZ;
        SpaceConversion.GetMapNodeFromWorldPosition( end, out endX, out endZ );

        int prevX, prevZ;
        SpaceConversion.GetMapNodeFromWorldPosition( start, out prevX, out prevZ );

        // for the current square, what is the furthest square we can move to in a straight line?
        foreach ( Vector3 p in GetPointsOnLine(start.x, start.z, end.x, end.z) ) {
            int x, z;
            SpaceConversion.GetMapNodeFromWorldPosition( p, out x, out z );

            if ( SpaceConversion.InMapBounds( x, z ) == false ) {
                continue;
            }

            if ( prevX == x && prevZ == z ) {
                continue;
            }

            if ( endX == x && endZ == z ) {
                return false;
            }

            int dir = GetDirection( x - prevX, z - prevZ );

            var model = annotatedNodes[ prevX, prevZ ].GameModels[ dir ];
            if ( model != null ) {
                return true;
            }

            // if we've crossed diagonally, also check the 2 cardinal directions
            int mult = ( z - prevZ ) * ( x - prevX );
      
            if ( mult != 0 ) {
                dir = GetDirection( 0, z - prevZ );
                model = annotatedNodes[ prevX, prevZ ].GameModels[ dir ];
                if ( model != null ) {
                    return true;
                }

                dir = GetDirection( x - prevX, 0 );
                model = annotatedNodes[ prevX, prevZ ].GameModels[ dir ];
                if ( model != null ) {
                    return true;
                }
            }

            AnnotatedNode endNode = annotatedNodes[ x, z ];
            if ( !canPathThroughDangerAreas && endNode.DPS > 0 ) {
                return true;
            }

            if ( heroToAvoid != null ) {
                if ( Vector3.SqrMagnitude( p - heroToAvoid.Position ) < 16 ) {
                    return true;
                }
            }

            prevX = x;
            prevZ = z;
        }
    
        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static bool IsPathBlockedForVehicle ( Vector3 start, Vector3 end, AnnotatedNode[,] annotatedNodes, float unitWidthWorld, bool canPathThroughDangerAreas ) {
        for ( float i = -unitWidthWorld * 0.5f; i <= unitWidthWorld * 0.5f; i += 0.99f ) {
            Vector3 direction = end - start;
            direction.Normalize();
            Vector3 cross = Vector3.Cross( direction, Vector3.up );
            cross *= i;

            // for the current square, what is the furthest square we can move to in a straight line?
            foreach ( Vector3 p in GetPointsOnLine(start.x + cross.x, start.z + cross.z, end.x + cross.x, end.z + cross.z) ) {
                int x, z;
                SpaceConversion.GetMapNodeFromWorldPosition( p, out x, out z );

                if ( SpaceConversion.InMapBounds( x, z ) == false ) {
                    continue;
                }

                AnnotatedNode endNode = annotatedNodes[ x, z ];

                if ( endNode.GameModel != null ) {
                    return true;
                }

                if ( !canPathThroughDangerAreas && endNode.DPS > 0 ) {
                    return true;
                }
            }
        }

        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    private static IEnumerable<Vector3> GetPointsOnLine ( float x0, float z0, float x1, float z1 ) {
        float dx = x1 - x0;
        float dz = z1 - z0;

        float fpoints = Mathf.Max( Mathf.Abs( dx ), Mathf.Abs( dz ) );
        int ipoints = Mathf.RoundToInt( fpoints );

        for ( int i = 0; i < ipoints; i++ ) {
            float progress = i / fpoints;
            yield return new Vector3( x0 + dx * progress, 0, z0 + dz * progress );
        }
        yield break;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static int GetDirection ( int x, int z ) {
//    DebugConsole.Assert(DebugChannel.AI, x != 0 || z != 0, string.Format("unexpected input {0},{1}", x, z) );
//    DebugConsole.Assert(DebugChannel.AI, x >= -1 && x <= 1, string.Format("unexpected input {0},{1}", x, z));
//    DebugConsole.Assert(DebugChannel.AI, z >= -1 && z <= 1, string.Format("unexpected input {0},{1}", x, z));

        if ( x == -1 ) {
            if ( z == -1 ) {
                return TileNeighbor.SouthWest;
            } else if ( z == 0 ) {
                return TileNeighbor.West;
            } else {
                return TileNeighbor.NorthWest;
            }
        } else if ( x == 0 ) {
            if ( z == -1 ) {
                return TileNeighbor.South;
            } else { // z == 1
                return TileNeighbor.North;
            }
        } else { // x == 1 
            if ( z == -1 ) {
                return TileNeighbor.SouthEast;
            } else if ( z == 0 ) {
                return TileNeighbor.East;
            } else {
                return TileNeighbor.NorthEast;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static void GetDirection ( int direction, out int x, out int z ) {
        x = 0;
        z = 0;

        switch ( direction ) {
            case TileNeighbor.SouthWest:
                x = -1;
                z = -1;
                break;
            case TileNeighbor.South:
                x = 0;
                z = -1;
                break;
            case TileNeighbor.SouthEast:
                x = 1;
                z = -1;
                break;
            case TileNeighbor.East:
                x = 1;
                z = 0;
                break;
            case TileNeighbor.NorthEast:
                x = 1;
                z = 1;
                break;
            case TileNeighbor.North:
                x = 0;
                z = 1;
                break;
            case TileNeighbor.NorthWest:
                x = -1;
                z = 1;
                break;
            case TileNeighbor.West:
                x = -1;
                z = 0;
                break;
        }

//    DebugConsole.Assert(DebugChannel.AI, x != 0 || z != 0, "direction not found");
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static int GetOctantsBetweenDirections ( int dir1, int dir2 ) {
        int octants = Math.Abs( dir1 - dir2 );

        return Math.Min( octants, 8 - octants );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static bool LinesIntersect ( Vector3 A, Vector3 B, Vector3 C, Vector3 D, out Vector3 I ) {
        I = Vector3.zero;
        Vector3 CmP = new Vector3( C.x - A.x, 0, C.z - A.z );
        Vector3 r = new Vector3( B.x - A.x, 0, B.z - A.z );
        Vector3 s = new Vector3( D.x - C.x, 0, D.z - C.z );

        float CmPxr = CmP.x * r.z - CmP.z * r.x;
        float CmPxs = CmP.x * s.z - CmP.z * s.x;
        float rxs = r.x * s.z - r.z * s.x;

        if ( CmPxr == 0f ) {
            // Lines are collinear, and so intersect if they have any overlap

            return ( ( C.x - A.x < 0f ) != ( C.x - B.x < 0f ) )
            || ( ( C.z - A.z < 0f ) != ( C.z - B.z < 0f ) );
        }

        if ( rxs == 0f )
            return false; // Lines are parallel.

        float rxsr = 1f / rxs;
        float t = CmPxs * rxsr;
        float u = CmPxr * rxsr;

        I = A + r * t;

        return ( t >= 0f ) && ( t <= 1f ) && ( u >= 0f ) && ( u <= 1f );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static int FindCircleCircleIntersections (
        float cx0, float cy0, float radius0,
        float cx1, float cy1, float radius1,
        out Vector3 intersection1, out Vector3 intersection2 ) {
        // Find the distance between the centers.
        float dx = cx0 - cx1;
        float dy = cy0 - cy1;
        double dist = Math.Sqrt( dx * dx + dy * dy );

        // See how manhym solutions there are.
        if ( dist > radius0 + radius1 ) {
            intersection1 = Vector3.zero;
            intersection2 = Vector3.zero;
            return 0;
        } else if ( dist < Math.Abs( radius0 - radius1 ) ) {
            intersection1 = Vector3.zero;
            intersection2 = Vector3.zero;
            return 0;
        } else if ( ( dist == 0 ) && ( radius0 == radius1 ) ) {
            intersection1 = Vector3.zero;
            intersection2 = Vector3.zero;
            return 0;
        } else {
            // Find a and h.
            double a = ( radius0 * radius0 -
                 radius1 * radius1 + dist * dist ) / ( 2 * dist );
            double h = Math.Sqrt( radius0 * radius0 - a * a );

            // Find P2.
            double cx2 = cx0 + a * ( cx1 - cx0 ) / dist;
            double cy2 = cy0 + a * ( cy1 - cy0 ) / dist;

            // Get the points P3.
            intersection1 = new Vector3(
                ( float )( cx2 + h * ( cy1 - cy0 ) / dist ),
                0,
                ( float )( cy2 - h * ( cx1 - cx0 ) / dist ) );
            intersection2 = new Vector3(
                ( float )( cx2 - h * ( cy1 - cy0 ) / dist ),
                0,
                ( float )( cy2 + h * ( cx1 - cx0 ) / dist ) );

            // See if we have 1 or 2 solutions.
            if ( dist == radius0 + radius1 )
                return 1;
            return 2;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static bool IsWithinRange ( float rangeWorld, float nodeX, float nodeZ, PathFindingNode gameModel ) {
        Structure structure = gameModel.GetComponent<Structure>();;
        Hero hero = gameModel.GetComponent<Hero>();

        float posX = 0;
        float posZ = 0;
        int width = 0;
        int length = 0;

        if ( structure != null ) {
            posX = gameModel.Position.x;
            posZ = gameModel.Position.z;
            width = ( int )( gameModel.Width * SpaceConversion.MapTileSize );
            length = ( int )( gameModel.Length * SpaceConversion.MapTileSize );
        } else if ( hero != null ) {
            width = ( int )( gameModel.TileWidth * SpaceConversion.MapTileSize ); 
            length = ( int )( gameModel.TileLength * SpaceConversion.MapTileSize );

            // heores position is their center so adjust to the corner
            posX = gameModel.Position.x - ( width * 0.5f );
            posZ = gameModel.Position.z - ( length * 0.5f );
        } else {
//      DebugConsole.Assert(DebugChannel.AI, false, "what is this gameModel?");
            return false;
        }
  

        float testX = -1;
        float testZ = -1;

        if ( nodeX < posX ) {
            if ( nodeZ < posZ ) {
                testX = posX;
                testZ = posZ;
            } else if ( nodeZ > posZ + length ) {
                testX = posX;
                testZ = posZ + length;
            } else {
                testX = posX;
                testZ = nodeZ;
            }
        } else if ( nodeX > posX + width ) {
            if ( nodeZ < posZ ) {
                testX = posX + width;
                testZ = posZ;
            } else if ( nodeZ > posZ + length ) {
                testX = posX + width;
                testZ = posZ + length;
            } else {
                testX = posX + width;
                testZ = nodeZ;
            }
        } else {
            if ( nodeZ < posZ ) {
                testX = nodeX;
                testZ = posZ;
            } else if ( nodeZ > posZ + length ) {
                testX = nodeX;
                testZ = posZ + length;
            } else {
                testX = nodeX;
                testZ = nodeZ;
            }
        }

        float dx = ( testX - nodeX ) * ( testX - nodeX );
        float dz = ( testZ - nodeZ ) * ( testZ - nodeZ );

        if ( dx + dz <= rangeWorld * rangeWorld ) {
            return true;
        }

        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public static void DrawCircle ( Vector3 p, float r, float time ) {
        int segments = 64;
        for ( int i = 0; i < segments; i++ ) {
            float radians = 2.0f * Mathf.PI * ( float )i / segments;
            float x1 = Mathf.Cos( radians ) * r;
            float z1 = Mathf.Sin( radians ) * r;

            radians = 2.0f * Mathf.PI * ( float )( ( i + 1 ) % segments ) / segments;
            float x2 = Mathf.Cos( radians ) * r;
            float z2 = Mathf.Sin( radians ) * r;

            Debug.DrawLine( p + new Vector3( x1, 0, z1 ), p + new Vector3( x2, 0, z2 ), Color.white, time ); 
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    // re-ordering of the 8 directions to be easily inded with the Atan2 calculation in IsPathBlockedForBiped
    private readonly static int[] s_headings = {
      TileNeighbor.East,
      TileNeighbor.NorthEast,
      TileNeighbor.North,
      TileNeighbor.NorthWest,
      TileNeighbor.West,
      TileNeighbor.SouthWest,
      TileNeighbor.South,
      TileNeighbor.SouthEast
    };

  //---------------------------------------------------------------------------------------------------------------------   
}
//---------------------------------------------------------------------------------------------------------------------   

