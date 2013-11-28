//---------------------------------------------------------------------------------------------------------------------   
using System;
using UnityEngine;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class PerspectiveCameraInputListener : ICameraInputListener {
    //---------------------------------------------------------------------------------------------------------------------
    public PerspectiveCameraInputListener ( PerspectiveCamera cameraController ) {
        m_cameraController = cameraController;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnPress ( bool isDown, FingerManager.TouchHit touchHit ) {
        Debug.Log( "on press " + isDown );
        if ( Input.touchCount > 1 ) {
            return;
        }

        if ( isDown ) {
            m_lastTouchData = touchHit.TouchData;
            m_cameraController.DoSelection();
        } else {
            // just released.
            // Let the pan continue briefly 
            m_cameraController.OnPressFinished();
        }

    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnClick ( FingerManager.TouchHit touchHit ) {
        return;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnDrag ( FingerManager.TouchHit touchHit ) {
        if ( Input.touchCount > 1 ) {
            return;
        }

        // TODO this is too sensitive on iOS, delta is often large when just tapping
        // move over to http://fingergestures.fatalfrog.com/

        m_cameraController.DragSelection( touchHit.TouchData.position - m_lastTouchData.position, true );

        m_lastTouchData = touchHit.TouchData;

    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnScroll ( float delta ) {
        m_cameraController.Zoom( -delta );
    }
    //---------------------------------------------------------------------------------------------------------------------
    PerspectiveCamera m_cameraController;
    TouchData m_lastTouchData;
}
//---------------------------------------------------------------------------------------------------------------------


