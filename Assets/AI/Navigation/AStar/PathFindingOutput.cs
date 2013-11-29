//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
public class PathFindingOutput {
    public PathFindingOutput () {
        Path = new LinkedList<FinalPathNode>();
        ModelsPotentiallyBlockingAShorterPath = new List<PathFindingNode>();
        ModelsBlockingPath = new List<PathFindingNode>();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Clear () {
        // an in-progress search needs to be abandoned and restarted
        // clear out the working memory, so it is clean for a fresh search
        Path.Clear();
        ModelsPotentiallyBlockingAShorterPath.Clear();
        ModelsBlockingPath.Clear();
        PathFound = false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Copy ( PathFindingOutput copy ) {
        Clear();

        ModelsBlockingPath.AddRange( copy.ModelsBlockingPath );
        ModelsPotentiallyBlockingAShorterPath.AddRange( copy.ModelsPotentiallyBlockingAShorterPath );


        LinkedListNode<FinalPathNode> node = copy.Path.First;

        while ( node != null ) {
            Path.AddLast( node.Value );
            node = node.Next;
        }

        Valid = true;
        PathFound = copy.PathFound;
        Destination = copy.Destination;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void DebugDraw ( Color color ) {
//    if ((DebugConsole.debugChannelMask & (int)DebugChannel.AI) != 0)
//    {
//      LinkedListNode<FinalPathNode> current = Path.First;
//
//      while (current != null && current.Next != null)
//      {
//        if (current.Value.IsJumpNode)
//        {
//          Debug.DrawLine(current.Value.Position, current.Next.Value.Position,  Color.blue, 10.0f);
//        }
//        else
//        {
//          Debug.DrawLine(current.Value.Position, current.Next.Value.Position, color, 10.0f);
//        }
//        current = current.Next;
//      }
//    }
    }
    // the list of models that occupy nodes that were expanded
    // if these buildings are destroyed then a repath may find a more optimal path
    // WARNING these models could be deleted from the scene during an in-progress search
    public List<PathFindingNode> ModelsPotentiallyBlockingAShorterPath { get; set; }

    public List<PathFindingNode> ModelsBlockingPath { get; set; }

    public LinkedList<FinalPathNode> Path { get; set; }

    public Vector3 Destination { get; set; }

    public bool Valid;
    public bool PathFound;
}
