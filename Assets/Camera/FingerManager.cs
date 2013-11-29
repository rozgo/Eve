using UnityEngine;
using System.Collections;

public class FingerManager : MonoBehaviour {
    public struct TouchHit {
        public Collider Collider;
        public TouchData TouchData;
        public float TouchDownTime;
    }

    public GameObject touchListener;
    public LayerMask mask = ~( int )0;
    TouchHit[] touchHits = new TouchHit[5];
    Vector3 lastMousePosition;
    float distance = 200;
    string scrollAxisName = "Mouse ScrollWheel";

    void FingerBegin ( TouchData evt ) {
        touchHits[ evt.fingerId ].Collider = null;
        Ray ray = camera.ScreenPointToRay( evt.position );
        RaycastHit hit;
        if ( Physics.Raycast( ray.origin, ray.direction, out hit, distance, mask ) ) {
            touchHits[ evt.fingerId ].Collider = hit.collider;
            touchHits[ evt.fingerId ].TouchData.worldPosition = hit.point;
            hit.collider.SendMessage( "OnFingerBegin", null, SendMessageOptions.DontRequireReceiver );      
        }

        touchListener.SendMessage( "OnFingerBegin", touchHits[ evt.fingerId ], SendMessageOptions.DontRequireReceiver );
    }

    void FingerMove ( TouchData evt ) {
        Ray ray = camera.ScreenPointToRay( evt.position );
        RaycastHit hit;
        if ( Physics.Raycast( ray.origin, ray.direction, out hit, distance, mask ) ) {
            touchHits[ evt.fingerId ].Collider = hit.collider;
            touchHits[ evt.fingerId ].TouchData.worldPosition = hit.point;
        } else {
            touchHits[ evt.fingerId ].Collider = null;
        }

        touchListener.SendMessage( "OnFingerMove", touchHits[ evt.fingerId ], SendMessageOptions.DontRequireReceiver );
    }

    void FingerEnd ( TouchData evt ) {
        Ray ray = camera.ScreenPointToRay( evt.position );
        RaycastHit hit;

        if ( Physics.Raycast( ray.origin, ray.direction, out hit, distance, mask ) ) {
            touchHits[ evt.fingerId ].Collider = hit.collider;
            touchHits[ evt.fingerId ].TouchData.worldPosition = hit.point;
        } else {
            touchHits[ evt.fingerId ].Collider = null;
        }

        touchListener.SendMessage( "OnFingerEnd", touchHits[ evt.fingerId ], SendMessageOptions.DontRequireReceiver );
    }

    void Update () {
        for ( int t = 0; t < Input.touchCount; ++t ) {
            Touch evt = Input.GetTouch( t );
            if ( evt.fingerId < 0 || evt.fingerId >= touchHits.Length ) {
                continue;
            }

            if ( touchHits[ evt.fingerId ].TouchDownTime > -1.0f ) {
                touchHits[ evt.fingerId ].TouchDownTime += Time.deltaTime;
            }

            if ( evt.phase == TouchPhase.Began ) {
                touchHits[ evt.fingerId ].TouchData.fingerId = evt.fingerId;
                touchHits[ evt.fingerId ].TouchData.position = evt.position;
                touchHits[ evt.fingerId ].TouchData.phase = evt.phase;
                touchHits[ evt.fingerId ].TouchDownTime = 0.0f;
                FingerBegin( touchHits[ evt.fingerId ].TouchData );
            } else if ( evt.phase == TouchPhase.Moved ) {
                touchHits[ evt.fingerId ].TouchData.fingerId = evt.fingerId;
                touchHits[ evt.fingerId ].TouchData.position = evt.position;
                touchHits[ evt.fingerId ].TouchData.phase = evt.phase;
                FingerMove( touchHits[ evt.fingerId ].TouchData );
            } else if ( evt.phase == TouchPhase.Ended || evt.phase == TouchPhase.Canceled ) {
                touchHits[ evt.fingerId ].TouchData.fingerId = evt.fingerId;
                touchHits[ evt.fingerId ].TouchData.position = evt.position;
                touchHits[ evt.fingerId ].TouchData.phase = evt.phase;
                FingerEnd( touchHits[ evt.fingerId ].TouchData );
                touchHits[ evt.fingerId ].TouchDownTime = -1.0f; 
            }
        }

        #if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
        if ( touchHits[ 0 ].TouchDownTime > -1.0f )
            touchHits[ 0 ].TouchDownTime += Time.deltaTime;
        float scroll = Input.GetAxis( scrollAxisName );
        for ( int mouseButton = 0; mouseButton < 3; mouseButton++ ) {
            if ( Input.GetMouseButtonDown( mouseButton ) ) {
                touchHits[ 0 ].TouchDownTime = 0.0f;
                FingerBegin( MouseToTouch( TouchPhase.Began ) );
            } else if ( Input.GetMouseButton( mouseButton ) ) {
                if ( Vector3.Distance( lastMousePosition, Input.mousePosition ) > 0.2f )
                    FingerMove( MouseToTouch( TouchPhase.Moved ) );
                touchHits[ 0 ].TouchDownTime += Time.deltaTime;
                lastMousePosition = Input.mousePosition;
            } else if ( Input.GetMouseButtonUp( mouseButton ) ) {
                FingerEnd( MouseToTouch( TouchPhase.Ended ) );
                touchHits[ 0 ].TouchDownTime = -1.0f;
            } else if ( scroll != 0f ) {
                touchListener.SendMessage( "OnScroll", scroll, SendMessageOptions.DontRequireReceiver );
            }
        }
        #endif
    }

    TouchData MouseToTouch ( TouchPhase tPhase ) {
        touchHits[ 0 ].TouchData.fingerId = 0;
        touchHits[ 0 ].TouchData.position = Input.mousePosition;
        touchHits[ 0 ].TouchData.phase = tPhase;
        if ( tPhase == TouchPhase.Began )
            lastMousePosition = touchHits[ 0 ].TouchData.position;

        return touchHits[ 0 ].TouchData;
    }
}

public struct TouchData {
    public TouchData ( int fingerId, Vector3 position, TouchPhase phase ) {
        this.fingerId = fingerId;
        this.position = position;
        this.phase = phase;
        this.worldPosition = Vector3.zero;
    }

    public int fingerId;
    public Vector3 position;
    public Vector3 worldPosition;
    public TouchPhase phase;
}
