using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sequencer : MonoBehaviour {
    void Start () {
        TestParallelSequence();
    }

    IEnumerator Task ( IEnumerator action ) {
        return action;
    }

    IEnumerator Sequence ( List<IEnumerator> tasks ) {
        foreach ( var task in tasks ) {
            while ( task.MoveNext() ) {
                yield return task.Current;
            }
        }
    }

    IEnumerator Parallel ( List<IEnumerator> tasks ) {
        var lanes = new List<IEnumerator>( tasks );
        while ( lanes.Count > 0 ) {
            for ( var i = 0; i < lanes.Count; ) {
                if ( lanes[ i ].MoveNext() ) {
                    yield return lanes[ i ].Current;
                    ++i;
                } else {
                    lanes.RemoveAt( i );
                }
            }
        }
    }

    void TestTask () {
        var task = Task( Counter( "A", 3 ) );
        StartCoroutine( task );
    }

    void TestSequence () {
        var tasks = new List<IEnumerator> {
            Task( Counter( "A", 3 ) ),
            Task( Counter( "B", 3 ) ),
        };
        StartCoroutine( Sequence( tasks ) );
    }

    void TestParallel () {
        var tasks = new List<IEnumerator> {
            Task( Counter( "A", 3 ) ),
            Task( Counter( "B", 3 ) ),
        };
        StartCoroutine( Parallel( tasks ) );
    }

    void TestParallelSequence () {
        var sequence0 = Sequence( new List<IEnumerator> {
            Task( Counter( "A", 3 ) ),
            Task( Counter( "B", 3 ) ),
        } );
        var sequence1 = Sequence( new List<IEnumerator> {
            Task( Counter( "C", 3 ) ),
            Task( Counter( "D", 3 ) ),
        } );
        var tasks = new List<IEnumerator> {
            sequence0,
            sequence1,
        };
        StartCoroutine( Parallel( tasks ) );
    }

    IEnumerator Counter ( string name, int seconds ) {
        while ( seconds > 0 ) {
            Debug.Log( name + ": " + seconds );
            --seconds;
            yield return new WaitForSeconds( 0.5f );
        }   
    }
}
