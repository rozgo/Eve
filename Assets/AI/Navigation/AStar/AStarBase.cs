//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public abstract class AStarBase {
    //---------------------------------------------------------------------------------------------------------------------
    public AStarBase () {
        m_closedSet = new OpenCloseMap( ( byte )SpaceConversion.MapTiles, ( byte )SpaceConversion.MapTiles );
        m_openSet = new OpenCloseMap( ( byte )SpaceConversion.MapTiles, ( byte )SpaceConversion.MapTiles );
        m_runtimeGrid = new OpenCloseMap( ( byte )SpaceConversion.MapTiles, ( byte )SpaceConversion.MapTiles );
        m_orderedOpenSet = new PriorityQueue<PathNode>( PathNode.Comparer );
        m_neighborNodes = new PathNode[8];
        m_path = new LinkedList<PathNode>();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public bool NewSearch ( PathFindingInput input, PathFindingOutput output, ref int nodesExploredThisFrame, int maxAllowedNodesToExplore ) {
//    DebugConsole.Assert(DebugChannel.AI, input.MaxSpeedInTileSpace > 0, "unexpected speed");

        m_input = input;
        m_output = output;
        m_output.Clear();

        if ( m_input.StartTileX < 0 || m_input.StartTileZ < 0 || m_input.StartTileX >= SpaceConversion.MapTiles || m_input.StartTileZ >= SpaceConversion.MapTiles ||
         m_input.TargetTileX < 0 || m_input.TargetTileZ < 0 || m_input.TargetTileX >= SpaceConversion.MapTiles || m_input.TargetTileZ >= SpaceConversion.MapTiles ) {
//      DebugConsole.Log(DebugChannel.AI, "fixing up route end points. trying to seach out of bounds at : (" + m_input.StartTileX + "," + m_input.StartTileZ + ") to (" + m_input.TargetTileX + "," + m_input.TargetTileZ + ")");

            int x = m_input.StartTileX;
            int z = m_input.StartTileZ;
            SpaceConversion.ClampToMapBounds( ref x, ref z );
            m_input.StartTileX = x;
            m_input.StartTileZ = z;

            x = m_input.TargetTileX;
            z = m_input.TargetTileZ;
            SpaceConversion.ClampToMapBounds( ref x, ref z );
            m_input.TargetTileX = x;
            m_input.TargetTileZ = z;
        }

        if ( m_input.StartTileX == m_input.TargetTileX &&
         m_input.StartTileZ == m_input.TargetTileZ ) {
            output.PathFound = true;
            return true;
        }

        Clear();

        PathNode endNode = GetEndNode();
        PathNode startNode = GetStartNode();

        startNode.G = 0;
        startNode.H = Heuristic( startNode, endNode, m_input.HeuristicMultiplier );
        startNode.F = startNode.H;

        m_openSet.Add( startNode );
        m_orderedOpenSet.Push( startNode );
        m_runtimeGrid.Add( startNode );

        return ContinueSearch( ref nodesExploredThisFrame, maxAllowedNodesToExplore );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public bool ContinueSearch ( ref int nodesExploredThisFrame, int maxAllowedNodesToExplore ) {
        if ( m_input == null ) {
            // unit has been destroyed?
            return true;
        }

        PathNode endNode = GetEndNode();

        while ( !m_openSet.IsEmpty ) {
            if ( nodesExploredThisFrame++ >= maxAllowedNodesToExplore ) {
                // the caller can call ContinueSearch next frame to resume the search
                return false;
            }

            PathNode node = m_orderedOpenSet.Pop();

            bool isEndNode = ( node.X == m_input.TargetTileX && node.Z == m_input.TargetTileZ );

            if ( m_input.StopWithinDistanceFromTargetModel > 0 &&
            m_input.TargetModel != null &&
            ( m_input.TargetModel is Structure || m_input.TargetModel is Hero ) ) {
                // convert the map tile location to the world position, adjusting for the clearance map offset 
                // TODO function naming is confusing here
                Vector3 clearanceMapPos = SpaceConversion.GetWorldPositionFromMapTile( node.X, node.Z );
                Vector3 worldPos = SpaceConversion.GetWorldPositionFromClearanceMapPosition( clearanceMapPos, m_input.UnitTileWidth );

                bool inRange = PathHelper.IsWithinRange( m_input.StopWithinDistanceFromTargetModel, worldPos.x, worldPos.z, m_input.TargetModel );

                if ( inRange ) {
                    if ( m_input.IgnoreOthersOccupyingSquare ) { // some units don't care about spreading out when attacking
                        isEndNode = true;
                    } else {
                        bool occupied = IsNodeOccupiedByOthers( node );

                        if ( !occupied ) {
                            isEndNode = true;
                        }
                    }
                }
            }

            if ( isEndNode ) {
                ReconstructPath( node );

                CreateFinalPath();

                m_output.PathFound = true;

                return true;
            }

            m_openSet.Remove( node );
            m_closedSet.Add( node );

            FindNeighbourNodes( node ); // TODO this adds nodes already in the closed set, so potential optimization here

            for ( int i = 0; i < 8; i++ ) {
                PathNode neighbour = m_neighborNodes[ i ];
                Boolean tentative_is_better;

                if ( neighbour == null ) {
                    continue;
                }

                if ( m_closedSet.Contains( neighbour ) ) {
                    continue;
                }

                float neighborCost = EvaluateNeighborCost( node, neighbour, isEndNode );

                if ( neighborCost == float.MaxValue ) {
                    continue;
                }

                float tentative_g_score = m_runtimeGrid[ node ].G + neighborCost;
                bool wasAdded = false;

                if ( !m_openSet.Contains( neighbour ) ) {
                    m_openSet.Add( neighbour );

                    tentative_is_better = true;
                    wasAdded = true;
                } else if ( tentative_g_score < m_runtimeGrid[ neighbour ].G ) {
                    tentative_is_better = true;
                } else {
                    tentative_is_better = false;
                }

                if ( tentative_is_better ) {
                    SetCameFrom( node, neighbour );

                    if ( !m_runtimeGrid.Contains( neighbour ) ) {
                        m_runtimeGrid.Add( neighbour );
                    }

                    m_runtimeGrid[ neighbour ].G = tentative_g_score;
                    m_runtimeGrid[ neighbour ].H = Heuristic( neighbour, endNode, m_input.HeuristicMultiplier );
                    m_runtimeGrid[ neighbour ].F = m_runtimeGrid[ neighbour ].G + m_runtimeGrid[ neighbour ].H;
                    m_runtimeGrid[ neighbour ].Direction = neighbour.Direction;

                    if ( wasAdded ) {
                        m_orderedOpenSet.Push( neighbour );
                    } else {
                        m_orderedOpenSet.Update( neighbour );
                    }

//          if ((DebugConsole.debugChannelMask & (int)DebugChannel.AI) != 0)
//          {
//            Debug.DrawLine(SpaceConversion.GetWorldPositionFromMapTile(node.X, node.Z),
//                  SpaceConversion.GetWorldPositionFromMapTile(node.X, node.Z) + Vector3.up,
//                  Color.black, 2.5f);
//
//            Debug.DrawLine(SpaceConversion.GetWorldPositionFromMapTile(node.X, node.Z),
//                  SpaceConversion.GetWorldPositionFromMapTile(neighbour.X, neighbour.Z) + Vector3.up * neighborCost / 100.0f,
//                  Color.white, 2.5f);
//          }
                }
            }
        }

        m_output.PathFound = false;
        return true;
    }

    private void DrawPath () {
#if UNITY_EDITOR
//    if ((DebugConsole.debugChannelMask & (int)DebugChannel.AI) != 0)
//    {
        //if (m_input.IsDirectional)
//      {
        /*LinkedListNode<FinalPathNode> fpNode = m_output.Path.First;
        int cost = 0;
        while (fpNode != null && fpNode.Next != null)
        {
          FinalPathNode fpNode1 = fpNode.Value;
          FinalPathNode fpNode2 = fpNode.Next.Value;

          int tx1, tz1, tx2, tz2;
          SpaceConversion.GetMapTileFromWorldPositionForUnit(fpNode1.Position, 0, out tx1, out tz1);
          SpaceConversion.GetMapTileFromWorldPositionForUnit(fpNode2.Position, 0, out tx2, out tz2);

          AnnotatedNode annotatedNode1 = m_annotatedNodes[tx1, tz1];

          int oldCost = cost;
          cost += NeighborCost(annotatedNode1, tx1, tz1, tx2, tz2);

          Debug.DrawLine(fpNode1.Position + Vector3.up * oldCost, fpNode2.Position + Vector3.up * cost, Color.white, 5.0f);

          fpNode = fpNode.Next;
        }*/
//      }
//    }
#endif
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected virtual void Clear () {
        Array.Clear( m_neighborNodes, 0, m_neighborNodes.Length );
        m_closedSet.Clear();
        m_openSet.Clear();
        m_runtimeGrid.Clear();
        m_orderedOpenSet.Clear();
        m_path.Clear();
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected virtual float NeighborDistance ( PathNode inStart, PathNode inEnd ) {
        int diffX = Math.Abs( inStart.X - inEnd.X );
        int diffY = Math.Abs( inStart.Z - inEnd.Z );

        switch ( diffX + diffY ) {
            case 1:
                {
                    return 1;
                }
            case 2:
                {
                    return SQRT_2;
                }
            case 0:
                {
//          DebugConsole.Assert(DebugChannel.AI, false);
                    return 0;
                }
            default:
                {
//          DebugConsole.Assert(DebugChannel.AI, false);
                    return 0;
                }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected virtual float Heuristic ( PathNode inStart, PathNode inEnd, float muliplier ) {
        // see here:
        // http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html

        int dx = Math.Abs( inStart.X - inEnd.X );
        int dy = Math.Abs( inStart.Z - inEnd.Z );

        // if the multiplier slightly over estimates the cost of diagonals and adjacent squares then
        // this is not guaranteed to give the perfect shortest path,
        // but it will be a good path, and likely fewer nodes will be expanded and so is generally faster to compute.

        float D = 1.0f * muliplier;
        float D2 = SQRT_2 * muliplier;

        // the Chebyshev distance allowing diagonols
        return D * ( dx + dy ) + ( D2 - 2 * D ) * Math.Min( dx, dy );
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected abstract void FindNeighbourNodes ( PathNode inAround );

    protected abstract void SetBlockingModels ( PathNode previous, PathNode currentNode );

    protected abstract PathNode GetStartNode ();

    protected abstract PathNode GetEndNode ();

    protected abstract float EvaluateNeighborCost ( PathNode node, PathNode neighbour, bool isEndNode );

    protected abstract PathNode GetCameFrom ( PathNode pathNode );

    protected abstract void SetCameFrom ( PathNode node, PathNode neighbor );

    protected virtual bool IsNodeOccupiedByOthers ( PathNode node ) {
        return false;
    }

    protected virtual Vector3 AdjustFinalPostion ( PathNode node, Vector3 position ) {
        return position;
    }

    protected virtual bool IsJumpNode ( PathNode previous, PathNode currentNode ) {
        return false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void ReconstructPath ( PathNode currentNode ) {
        PathNode previousNode = GetCameFrom( currentNode );

        if ( previousNode != null ) {
            ReconstructPath( previousNode );
        }

        m_path.AddLast( currentNode );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void CreateFinalPath () {
        LinkedListNode<PathNode> llnode = m_path.First;

        while ( llnode != null ) {
            PathNode node = llnode.Value;

            Vector3 position = new Vector3();
            bool isJump = false;
            byte direction = node.Direction;


            if ( llnode.Next == null ) {
                // last node 
                // add a little variation to keep units spaced out
                position = SpaceConversion.GetWorldPositionFromMapTile( ( int )node.X, ( int )node.Z );

                Vector3 adjustedPosition = AdjustFinalPostion( node, position );

                // check that this is still a valid position
                if ( m_input.StopWithinDistanceFromTargetModel > 0 &&
                m_input.TargetModel != null &&
                ( m_input.TargetModel is Structure || m_input.TargetModel is Hero ) ) {
                    bool inRange = PathHelper.IsWithinRange( m_input.StopWithinDistanceFromTargetModel, adjustedPosition.x, adjustedPosition.z, m_input.TargetModel );

                    if ( inRange ) {
                        position = adjustedPosition;
                    }
                }

                SetBlockingModels( node, null );
            } else {
                // not the last

                // is it a jump node?
                isJump = IsJumpNode( node, llnode.Next.Value );

                if ( isJump ) {
                    // a wall to jump over
                    // merge the two nodes and create a single jump node
                    float x = 0.5f * ( node.X + llnode.Next.Value.X );
                    float z = 0.5f * ( node.Z + llnode.Next.Value.Z );

                    // do the conversion back to world space
                    x *= SpaceConversion.MapTileSize;
                    z *= SpaceConversion.MapTileSize;

                    position = new Vector3( x, 0, z );

                    // skip ahead a node
                    llnode = llnode.Next;
                } else {
                    position = SpaceConversion.GetWorldPositionFromMapTile( node.X, node.Z );

                    SetBlockingModels( node, llnode.Next.Value );
                }
            }

            FinalPathNode fpn = new FinalPathNode( position, direction, isJump/* jump nodes are also pivot nodes and cannot be straigthened away*/, isJump );

            m_output.Path.AddLast( fpn );

            llnode = llnode.Next;
        }

        DrawPath();
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected OpenCloseMap m_closedSet;
    protected OpenCloseMap m_openSet;
    protected PriorityQueue<PathNode> m_orderedOpenSet;
    protected OpenCloseMap m_runtimeGrid;
    protected PathNode[] m_neighborNodes;
    protected PathFindingInput m_input;
    protected PathFindingOutput m_output;
    protected AnnotatedNode[,] m_annotatedNodes;
    protected LinkedList<PathNode> m_path;
    protected static readonly float SQRT_2 = ( float )Math.Sqrt( 2.0f );
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

