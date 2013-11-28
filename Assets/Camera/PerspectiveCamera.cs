//---------------------------------------------------------------------------------------------------------------------   
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class PerspectiveCamera : MonoBehaviour {
    public void Start () {

 
        PerspectiveCameraInputListener listener = new PerspectiveCameraInputListener( this );
        m_cameraInputHandler.listeners.Add( listener );

     
        m_lookAtLimits = m_sceneConfiguration.m_lookAtLimits;
        m_lookAtLimitsZoomIn = m_sceneConfiguration.m_lookAtLimitsZoomIn;

        m_minHeight = m_sceneConfiguration.lowerLimit.transform.position.y;
        m_maxHeight = m_sceneConfiguration.upperLimit.transform.position.y * 0.6f; // TODO remove the *0.6 - a temp fixup until we get a bigger environment

        m_lookAt = new Vector3( 20, 0, 20 );
        /*
    [
      {
        "Platform":"WindowsEditor",
        "LookAtLowPassFactor" : 5.0,
        "EyeLowPassFactor" : 20.0,
        "ZoomDeltaLimit" : 200.0,
        "ZoomDeltaMultiplier" : 20.0,
        "EyeCtrlKp" : 20.0,
        "EyeCtrlKi" : 0.0,
        "EyeCtrlKd" : 0.0
      },
      {
        "Platform":"OSXEditor",
        "LookAtLowPassFactor" : 5.0,
        "EyeLowPassFactor" : 10.0,
        "ZoomDeltaLimit" : 2.0,
        "ZoomDeltaMultiplier" : 1.0,
        "EyeCtrlKp" : 16.0,
        "EyeCtrlKi" : 0.0,
        "EyeCtrlKd" : 0.0
      },
      {
        "Platform":"Default",
        "LookAtLowPassFactor" : 5.0,
        "EyeLowPassFactor" : 10.0,
        "ZoomDeltaLimit" : 100.0,
        "ZoomDeltaMultiplier" : 0.05,
        "EyeCtrlKp" : 16.0,
        "EyeCtrlKi" : 0.0,
        "EyeCtrlKd" : 0.0
      }
    ]

*/



  

        eyeCtrl.kp = 16f; //configurationByPlatform.EyeCtrlKp;
        eyeCtrl.ki = 0f; //configurationByPlatform.EyeCtrlKi;
        eyeCtrl.kd = 0f;//configurationByPlatform.EyeCtrlKd;
        lookAtLowPassFactor = 5f;
        eyeLowPassFactor = 10f;
        zoomDeltaMultiplier = 0.05f;
        zoomDeltaLimit = 100f;

        var height = Eye().transform.position.y;

        m_lowerFov = m_sceneConfiguration.lowerLimit.fov;
        m_upperFov = m_sceneConfiguration.upperLimit.fov;

        m_lowerRotation = m_sceneConfiguration.lowerLimit.transform.rotation;
        m_upperRotation = m_sceneConfiguration.upperLimit.transform.rotation;

        var normalizedHeight = ( ( height - m_minHeight ) / ( m_maxHeight - m_minHeight ) );
        Eye().fieldOfView = Mathf.Lerp( m_lowerFov, m_upperFov, normalizedHeight );

        Eye().transform.rotation = Quaternion.Lerp( m_lowerRotation, m_upperRotation, normalizedHeight );

        stochasticEye = new GameObject( "StochasticEye" ).transform;
        stochasticEye.position = Eye().transform.position;
        eyeCtrl.Reset( stochasticEye.position );
        m_shake = new Shake( Eye() );

        m_lookAt = ScreenToGroundFromViewportPoint( m_viewCenter );


    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Zoom ( float deltaZoom ) {
        deltaZoom = Mathf.Clamp( deltaZoom, -zoomDeltaLimit, zoomDeltaLimit );
        stochasticEye.position -= Eye().transform.forward * deltaZoom * zoomDeltaMultiplier;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void DragSelection ( Vector2 unused, bool alreadyHandled ) {
        if ( alreadyHandled ) {
            // m_dragging = 0;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    Camera Eye () {
        return Camera.main;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void DoSelection () {
        Debug.Log( "do selection" );
        if ( m_zooming == 0 ) {
            m_lookAtAnchor = ScreenToGroundFromScreenPoint( touchPosition() );
            m_dragging = 1;
            dragTimer = 0f;
            m_fingerSpeed = Vector3.zero;
            onDrag = (float dt ) => {
                var groundPos = ScreenToGroundFromScreenPoint( touchPosition() );
                Debug.Log( "drag selection" );
                var viewPortPoint = Eye().WorldToViewportPoint( groundPos ); 
                // low pass filter.
                m_fingerSpeed = Vector3.Lerp( m_fingerSpeed, ( m_lastFingerPosition - viewPortPoint ) / dt, 0.75f );
                m_lastFingerPosition = viewPortPoint;

                var delta = m_lookAtAnchor - groundPos;
                stochasticEye.position += delta;
                m_lookAt = ScreenToGroundFromViewportPoint( m_viewCenter );
                m_lastTouch = groundPos;
                m_lastTouch = Eye().WorldToViewportPoint( groundPos );
                m_lastTouch.z = 0f;
                dragTimer += dt;
            };
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnPressFinished () {
        Debug.Log( "on press finished" );
        m_dragging = 0;
    }
    //---------------------------------------------------------------------------------------------------------------------
    Vector3 ClampToBounds ( Vector3 position, Bounds bounds ) {
        position.x = Mathf.Max( bounds.center.x - bounds.extents.x, position.x );
        position.x = Mathf.Min( bounds.center.x + bounds.extents.x, position.x );
        position.y = Mathf.Max( bounds.center.y - bounds.extents.y, position.y );
        position.y = Mathf.Min( bounds.center.y + bounds.extents.y, position.y );
        position.z = Mathf.Max( bounds.center.z - bounds.extents.z, position.z );
        position.z = Mathf.Min( bounds.center.z + bounds.extents.z, position.z );
        return position;
    }
    //---------------------------------------------------------------------------------------------------------------------
    Rect ClampViewAreaToBounds ( Rect viewArea, Bounds bounds ) {
        var position = Vector2.zero;
        var clampedViewArea = new Rect( viewArea );
        var maxX = Mathf.Max( bounds.center.x + bounds.extents.x, viewArea.xMax );
        var minX = Mathf.Min( bounds.center.x - bounds.extents.x, viewArea.xMin );
        var maxY = Mathf.Max( bounds.center.z + bounds.extents.z, viewArea.yMax );
        var minY = Mathf.Min( bounds.center.z - bounds.extents.z, viewArea.yMin );

        position.x = position.x + ( bounds.center.x + bounds.extents.x ) - maxX;
        position.x = position.x + ( bounds.center.x - bounds.extents.x ) - minX;

        position.y = position.y + ( bounds.center.z + bounds.extents.z ) - maxY;
        position.y = position.y + ( bounds.center.z - bounds.extents.z ) - minY;

        clampedViewArea.center = clampedViewArea.center + position;

        return clampedViewArea;
    }
    //---------------------------------------------------------------------------------------------------------------------
    Rect CalculateCameraViewArea ( Camera camera, Plane ground, Vector3 position ) {
        var frustumHeight = 2.0f * 20f * Mathf.Tan( camera.fieldOfView * 0.5f * Mathf.Deg2Rad );
        var frustumWidth = frustumHeight * camera.aspect;

        var basePoint = position + camera.transform.forward * 20f;

        var pointA = basePoint - camera.transform.up * frustumHeight * 0.5f - camera.transform.right * frustumWidth * 0.5f;
        var pointB = basePoint - camera.transform.up * frustumHeight * 0.5f + camera.transform.right * frustumWidth * 0.5f;
        var pointC = basePoint + camera.transform.up * frustumHeight * 0.5f + camera.transform.right * frustumWidth * 0.5f;
        var pointD = basePoint + camera.transform.up * frustumHeight * 0.5f - camera.transform.right * frustumWidth * 0.5f;

        /*    rays  local to camera.
C-------D
|       |
|       |
A-------B
*/

        var rayA = new Ray( position, ( pointA - position ).normalized );
        var rayB = new Ray( position, ( pointB - position ).normalized );
        var rayC = new Ray( position, ( pointC - position ).normalized );
        var rayD = new Ray( position, ( pointD - position ).normalized );

        Debug.DrawLine( rayA.origin, rayA.origin + rayA.direction * 200f );
        Debug.DrawLine( rayA.origin, rayB.origin + rayB.direction * 200f );
        Debug.DrawLine( rayA.origin, rayC.origin + rayC.direction * 200f );
        Debug.DrawLine( rayA.origin, rayD.origin + rayD.direction * 200f );



        rays[ 0 ] = rayA;
        rays[ 1 ] = rayB;
        rays[ 2 ] = rayC;
        rays[ 3 ] = rayD;


        for ( int i = 0; i < 4; i++ ) {
            var ray = rays[ i ];
            float hitDist;
            m_GroundPlane.Raycast( ray, out hitDist );
            points[ i ] = ray.GetPoint( hitDist );
        }

        Debug.DrawLine( points[ 0 ], points[ 1 ], Color.black );
        Debug.DrawLine( points[ 0 ], points[ 3 ], Color.blue );
        Debug.DrawLine( points[ 2 ], points[ 3 ], Color.gray );
        Debug.DrawLine( points[ 1 ], points[ 2 ], Color.green );

        float maxX = float.MinValue;
        for ( int i = 0; i < points.Length; i++ ) {
            if ( points[ i ].x > maxX ) {
                maxX = points[ i ].x;
            }
        }
        float maxY = float.MinValue;
        for ( int i = 0; i < points.Length; i++ ) {
            if ( points[ i ].z > maxY ) {
                maxY = points[ i ].z;
            }
        }

        float minX = float.MaxValue;
        for ( int i = 0; i < points.Length; i++ ) {
            if ( points[ i ].x < minX ) {
                minX = points[ i ].x;
            }
        }

        float minY = float.MaxValue;
        for ( int i = 0; i < points.Length; i++ ) {
            if ( points[ i ].z < minY ) {
                minY = points[ i ].z;
            }
        }
        return new Rect( minX, minY, maxX - minX, maxY - minY );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private void DrawCameraBounds ( Bounds bounds ) {
        /*    
C-------D
|       |
|       |
A-------B
*/
        var vertexA = bounds.center + new Vector3( bounds.extents.x * -1f, 0, bounds.extents.z * -1f );
        var vertexB = bounds.center + new Vector3( bounds.extents.x, 0, bounds.extents.z * -1f );
        var vertexC = bounds.center + new Vector3( bounds.extents.x * -1f, 0, bounds.extents.z );
        var vertexD = bounds.center + new Vector3( bounds.extents.x, 0, bounds.extents.z );

        Debug.DrawLine( vertexA, vertexB, Color.black );
        Debug.DrawLine( vertexB, vertexD, Color.black );
        Debug.DrawLine( vertexC, vertexD, Color.black );
        Debug.DrawLine( vertexA, vertexC, Color.black );


    }
    //---------------------------------------------------------------------------------------------------------------------
    private void DrawCameraRect ( Rect rect, Color color ) {
        /*    
C-------D
|       |
|       |
A-------B
*/

        var vertexA = new Vector3( rect.xMin, 0, rect.yMin );
        var vertexB = new Vector3( rect.xMax, 0, rect.yMin );
        var vertexC = new Vector3( rect.xMin, 0, rect.yMax );
        var vertexD = new Vector3( rect.xMax, 0, rect.yMax );

        Debug.DrawLine( vertexA, vertexB, color );
        Debug.DrawLine( vertexB, vertexD, color );
        Debug.DrawLine( vertexC, vertexD, color );
        Debug.DrawLine( vertexA, vertexC, color );



    }
    //---------------------------------------------------------------------------------------------------------------------
    Vector3 ScreenToGround ( System.Func<Vector3, Ray> map, Vector3 position ) {
        var cameraPos = Eye().transform.position;
        Eye().transform.position = stochasticEye.position;
        var viewRay = map( position );
        Eye().transform.position = cameraPos;
        float hitDist;
        m_GroundPlane.Raycast( viewRay, out hitDist );
        return viewRay.GetPoint( hitDist );    
    }
    //---------------------------------------------------------------------------------------------------------------------
    Vector3 ScreenToGroundFromScreenPoint ( Vector3 position ) {

        var cameraPos = Eye().transform.position;
        Eye().transform.position = stochasticEye.position;
        var viewRay = Eye().ScreenPointToRay( position );

        Eye().transform.position = cameraPos;
        float hitDist;
        m_GroundPlane.Raycast( viewRay, out hitDist );

        return viewRay.GetPoint( hitDist );    
    }
    //---------------------------------------------------------------------------------------------------------------------
    Vector3 ScreenToGroundFromViewportPoint ( Vector3 position ) {

        var cameraPos = Eye().transform.position;
        Eye().transform.position = stochasticEye.position;
        var viewRay = Eye().ViewportPointToRay( position );
        Eye().transform.position = cameraPos;
        float hitDist;

        m_GroundPlane.Raycast( viewRay, out hitDist );
        return viewRay.GetPoint( hitDist );    
    }
    //---------------------------------------------------------------------------------------------------------------------
    void ApplyInertia ( float decay, float dt ) {
        if ( m_fingerSpeed.magnitude < 0.2 ) {
            return;
        }
        //check if drag was too short.
        if ( dragTimer < 0.1f ) {
            m_fingerSpeed = Vector3.zero;
            return;
        }
        var deltaPos = m_fingerSpeed * dt;
        m_fingerSpeed -= m_fingerSpeed * decay;
        m_lastTouch -= deltaPos;

        var screenPoint = Eye().ViewportToScreenPoint( m_lastTouch );

        var groundPos = ScreenToGroundFromScreenPoint( screenPoint );

        var delta = m_lookAtAnchor - groundPos;
        stochasticEye.position += delta;
        m_lookAt = ScreenToGroundFromViewportPoint( m_viewCenter );

    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Update () {

        var dt = Time.deltaTime;

        HandleZoom();
        if ( m_dragging == 1 ) {
            onDrag( dt );
        } else {
            ApplyInertia( 0.2f, dt );
        }

        DrawCameraBounds( m_lookAtLimits );
        DrawCameraBounds( m_lookAtLimitsZoomIn );
        DrawCameraBounds( m_interpolatedBounds );

        var m_cameraViewArea = CalculateCameraViewArea( Eye(), m_GroundPlane, stochasticEye.position );

        DrawCameraRect( m_cameraViewArea, Color.red );


        var height = Eye().transform.position.y;
        var normalizedHeight = ( ( height - m_minHeight ) / ( m_maxHeight - m_minHeight ) );

        //Debug.Log (height + " "+m_minHeight+" "+ m_maxHeight);
        var interpolatedBoundsCenter = Vector3.Lerp( m_lookAtLimitsZoomIn.center, m_lookAtLimits.center, normalizedHeight );
        var interpolatedBoundsSize = Vector3.Lerp( m_lookAtLimitsZoomIn.size, m_lookAtLimits.size, normalizedHeight );
        m_interpolatedBounds.center = interpolatedBoundsCenter;
        m_interpolatedBounds.size = interpolatedBoundsSize;

        var clampedArea = ClampViewAreaToBounds( m_cameraViewArea, m_interpolatedBounds );
        DrawCameraRect( clampedArea, Color.blue );



        var clampedAreaCenter = new Vector3( clampedArea.center.x, 0, clampedArea.center.y );
        var areaCenter = new Vector3( m_cameraViewArea.center.x, 0, m_cameraViewArea.center.y );

        var boundsError = areaCenter - clampedAreaCenter;
        if ( m_dragging == 1 ) {

            m_lookAt -= boundsError * 0.8f;
        } else if ( m_zooming > 0 ) {
            m_lookAt -= boundsError * 0.8f;
        } else {
            m_lookAt -= boundsError * dt * lookAtLowPassFactor;
        }
        /*     stop intertia movement if hits a limit */
        if ( boundsError.magnitude > 0.2f ) {
            m_fingerSpeed = Vector3.zero;
        }

        var shake = m_shake.Offset();
        var screenToGround = ScreenToGroundFromViewportPoint( m_viewCenter );

        stochasticEye.position += m_lookAt - screenToGround + shake;



        var eyePosClamped = stochasticEye.position;
        eyePosClamped.y = Mathf.Clamp( eyePosClamped.y, m_minHeight, m_maxHeight );
        boundsError = stochasticEye.position - eyePosClamped;

        var projectedError = Vector3.Project( boundsError, Eye().transform.forward );
        stochasticEye.position -= projectedError * dt * eyeLowPassFactor;

        eyeCtrl.setpoint = stochasticEye.position;
        var eyeControl = eyeCtrl.Compute( Eye().transform.position, dt );
        Eye().transform.position += eyeControl * dt;

        Eye().transform.rotation = CalculateRotation( height, dt );

        Eye().fieldOfView = CalculateFoV( height, dt );

//    CameraModel model = Model as CameraModel;
//    model.SetPosition (Eye ().transform.position);
    }
    //---------------------------------------------------------------------------------------------------------------------
    float CalculateFoV ( float height, float dt ) {
        var normalizedHeight = ( ( height - m_minHeight ) / ( m_maxHeight - m_minHeight ) );
        var targetFov = Mathf.Lerp( m_lowerFov, m_upperFov, normalizedHeight );
        m_fov = Mathf.Lerp( m_fov, targetFov, dt * 20 );
        return targetFov;
    }
    //---------------------------------------------------------------------------------------------------------------------
    Quaternion CalculateRotation ( float height, float dt ) {
        var normalizedHeight = ( ( height - m_minHeight ) / ( m_maxHeight - m_minHeight ) );

        var targetRotation = Quaternion.Lerp( m_lowerRotation, m_upperRotation, normalizedHeight );
        return  Quaternion.Lerp( Eye().transform.rotation, targetRotation, dt * 15 );
    }
    //---------------------------------------------------------------------------------------------------------------------
    void HandleZoom () {
        if ( m_zooming == 0 && Input.touchCount == 2 ) {
            m_zooming = 1;
            m_dragging = 0;
            onDrag = (deltaTime ) => {
            };
            var t0 = Input.GetTouch( 1 ).position;
            var t1 = Input.GetTouch( 0 ).position;
            m_zoomCenter = ( t0 + t1 ) * 0.5f;
            m_pinchLength = Vector3.Distance( t0, t1 );
            m_lookAtAnchor = ScreenToGroundFromScreenPoint( m_zoomCenter );
            m_fingerSpeed = Vector3.zero;
        } else if ( m_zooming == 1 && Input.touchCount == 2 ) {
            var stride = Vector2.Distance( Input.GetTouch( 0 ).position, Input.GetTouch( 1 ).position );
            var delta = m_pinchLength - stride;
            Zoom( delta );
            m_pinchLength = stride;
            var groundPos = ScreenToGroundFromScreenPoint( m_zoomCenter );
            var groundDelta = m_lookAtAnchor - groundPos; 
            stochasticEye.position += groundDelta;
            m_lookAt = ScreenToGroundFromViewportPoint( m_viewCenter );
        } else if ( m_zooming > 0 ) {
            ++m_zooming;
        }
        if ( Input.touchCount == 0 ) {
            m_zooming = 0;
        }

    }
    //---------------------------------------------------------------------------------------------------------------------
    public void LookAt ( Vector3 pos ) {
        m_lookAt = pos;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnExplosion ( Vector3 origin ) {
        var position = ScreenToGroundFromScreenPoint( origin );
        m_shake.OnExplosion( position );
    }
    //---------------------------------------------------------------------------------------------------------------------
    System.Func<Vector3> touchPosition = () => Input.mousePosition;
    System.Action<float> onDrag = (dt ) => {
    };
    static Vector3 m_viewCenter = Vector3.one * 0.5f;
    static Plane m_GroundPlane = new Plane( Vector3.up, Vector3.zero );
    Vector3 m_lookAtAnchor;
    Vector3 m_zoomCenter;
    Vector3 m_lastTouch;
    int m_zooming = 0;
    int m_dragging = 0;
    float m_minHeight = 40;
    float m_maxHeight = 60;
    float m_lowerFov = 55;
    float m_upperFov = 60;
    Quaternion m_lowerRotation = Quaternion.identity;
    Quaternion m_upperRotation = Quaternion.identity;
    float m_fov = 0;
    float lookAtLowPassFactor = 3;
    float eyeLowPassFactor = 6;
    float zoomDeltaMultiplier = 1;
    float zoomDeltaLimit = 100f;
    Bounds m_lookAtLimits = new Bounds( new Vector3( 70, 0, 70 ), new Vector3( 240, 1, 240 ) );
    Bounds m_lookAtLimitsZoomIn = new Bounds( new Vector3( 50, 0, 35 ), new Vector3( 180, 1, 120 ) );
    Vector3 m_fingerSpeed = Vector3.zero;
    float dragTimer = 0f;
    Vector3 m_lastFingerPosition = Vector3.zero;
    Bounds m_interpolatedBounds = new Bounds();
    float m_pinchLength = 0;
    Vector3 m_lookAt;
    Transform stochasticEye;
    Filters.PIDVector eyeCtrl = new Filters.PIDVector();
    Shake m_shake;
    Ray[] rays = new Ray[4];
    Vector3[] points = new Vector3[4];
    public PerspectiveCameraConfiguration m_sceneConfiguration;
    public CameraInputHandler m_cameraInputHandler;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------
