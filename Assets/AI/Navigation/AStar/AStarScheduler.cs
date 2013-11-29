using System;
using System.Collections;
using System.Collections.Generic;

//---------------------------------------------------------------------------------------------------------------------
public class AStarScheduler {
    //---------------------------------------------------------------------------------------------------------------------
    public AStarScheduler ( AStar solver, DirectionalAStar solverDirectional, int maxAStarsNodesExploredPerFrame ) {
//    DebugConsole.Assert(DebugChannel.AI, solver != null);
//    DebugConsole.Assert(DebugChannel.AI, maxAStarsNodesExploredPerFrame > 0);
    
        m_solver = solver;
        m_solverDirectional = solverDirectional;
        m_maxAStarsNodesExploredPerFrame = maxAStarsNodesExploredPerFrame;

        m_requests = new List<List<Request>>();
        for ( int i = 0; i < ( int )SearchPriorityLevel.NumPriorites; i++ ) {
            m_requests.Add( new List<Request>() );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Schedule ( PathFindingInput input, PathFindingOutput output, SearchPriorityLevel priority, int ownerId ) {
        // remove any previously scheduled AStars
        RemoveTask( ownerId );

        int index = ( int )priority;

        Request req = new Request() {
            OwnerId = ownerId,
            Input = input,
            Output = output,
            Priority = priority,
            InProgress = false
        };

        m_requests[ index ].Add( req );

        // clear out any in-progress searches in lower priority queues
        while ( ++index < ( int )SearchPriorityLevel.NumPriorites ) {
            for ( int j = 0; j < m_requests[ index ].Count; j++ ) {
                Request request = m_requests[ index ][ j ];

                if ( request.InProgress ) {
                    request.InProgress = false;
                    request.Output.Clear();
                }
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void UnSchedule ( int ownerId ) {
        RemoveTask( ownerId );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void RemoveTask ( int ownerId ) {
        for ( int i = 0; i < ( int )SearchPriorityLevel.NumPriorites; i++ ) {
            for ( int j = 0; j < m_requests[ i ].Count; j++ ) {
                if ( m_requests[ i ][ j ].OwnerId == ownerId ) {
                    m_requests[ i ].RemoveAt( j );
                    return;
                }
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Update ( float deltaTime ) {    
        int nodesExploredThisFrame = 0;
        int units = 0;

        for ( int r = 0; r < m_requests.Count; r++ ) { // each of the priorities
            List<Request> requests = m_requests[ r ];

            // an element is removed every iteration so do not increment i
            for ( int i = 0; i < requests.Count; ) { // each request in each priority list
                bool completed = false;

                if ( requests[ i ].InProgress ) {
                    if ( requests[ i ].Input.IsDirectional ) {
                        completed = m_solverDirectional.ContinueSearch( ref nodesExploredThisFrame, m_maxAStarsNodesExploredPerFrame );
                    } else {
                        completed = m_solver.ContinueSearch( ref nodesExploredThisFrame, m_maxAStarsNodesExploredPerFrame );
                    }
                } else {
                    if ( requests[ i ].Input.IsDirectional ) {
                        completed = m_solverDirectional.NewSearch( requests[ i ].Input, requests[ i ].Output, ref nodesExploredThisFrame, m_maxAStarsNodesExploredPerFrame );
                    } else {
                        completed = m_solver.NewSearch( requests[ i ].Input, requests[ i ].Output, ref nodesExploredThisFrame, m_maxAStarsNodesExploredPerFrame );
                    }
                }

                if ( completed == false ) {
                    // resume this search next frame
                    // copy over a new request with InProgress set to true
                    requests[ i ] = new Request() {
                        OwnerId = requests[ i ].OwnerId,
                        Input = requests[ i ].Input,
                        Output = requests[ i ].Output,
                        Priority = requests[ i ].Priority,
                        InProgress = true
                    };
                    return;
                }

                units++;

                Action callback = requests[ i ].Input.Callback;

                requests.RemoveAt( i );

                // some callbacks may schedule a new search so make sure to call the callbacks after the task has been removed
                callback();
            }
        }
    }

    struct Request {
        public int OwnerId { get; set; }

        public bool InProgress { get; set; }

        public SearchPriorityLevel Priority { get; set; }

        public PathFindingInput Input { get; set; }

        public PathFindingOutput Output { get; set; }
    }
    //---------------------------------------------------------------------------------------------------------------------
    private List<List<Request>> m_requests;
    private AStar m_solver;
    private DirectionalAStar m_solverDirectional;
    private readonly int m_maxAStarsNodesExploredPerFrame;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------


