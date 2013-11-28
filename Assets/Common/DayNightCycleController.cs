using UnityEngine;
using System.Collections;

public class DayNightCycleController : MonoBehaviour {
    public float hours;
//1 hour = 3600 seconds
    public float minutes;
//1 minute = 60 seconds
    public float seconds;
    public AnimationClip environmentClip;

    public static float DayTime {
        get;
        set;
    }

    void Start () {
        //Calculate and set duration by changing speed
        var duration = ( hours * 3600 ) + ( minutes * 60 ) + seconds;
        var newLength = duration / environmentClip.length;
        animation[ environmentClip.name ].speed = 1 / newLength;
    }

    void Update () {
        DayTime = ( ( animation[ environmentClip.name ].normalizedTime % 1 ) * 24 );
    }

    public void SetStartTime ( float startTime, bool isMultiplayer ) {
        //Set random start time if is attacking multiplayer
        if ( isMultiplayer ) {
            startTime = Random.Range( 0f, 1f );
        }
        //Set start time
        animation[ environmentClip.name ].time = animation[ environmentClip.name ].length * startTime;
    }
}
