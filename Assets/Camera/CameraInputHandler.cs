using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraInputHandler : MonoBehaviour {
    public List<ICameraInputListener> listeners = new List<ICameraInputListener>();
    //---------------------------------------------------------------------------------------------------------------------
    public void OnFingerBegin ( FingerManager.TouchHit touchHit ) {
        Debug.Log( "on finger begin-------" );
        m_isPotentialClick = true;

        for ( int i = 0; i < listeners.Count; i++ ) {
            listeners[ i ].OnPress( true, touchHit );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnFingerCancel ( FingerManager.TouchHit touchHit ) {
        for ( int i = 0; i < listeners.Count; i++ ) {
            listeners[ i ].OnPress( false, touchHit );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnFingerEnd ( FingerManager.TouchHit touchHit ) {
   
   
        for ( int i = 0; i < listeners.Count; i++ ) {
            listeners[ i ].OnPress( false, touchHit );
            if ( touchHit.TouchDownTime < 0.4f && m_isPotentialClick ) {
                listeners[ i ].OnClick( touchHit );
            }
        }


    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnFingerMove ( FingerManager.TouchHit touchHit ) {
        m_isPotentialClick = false;

        for ( int i = 0; i < listeners.Count; i++ ) {
            listeners[ i ].OnDrag( touchHit );
        }

    }
    //---------------------------------------------------------------------------------------------------------------------
    public void OnScroll ( float delta ) {
        for ( int i = 0; i < listeners.Count; i++ ) {
            listeners[ i ].OnScroll( delta );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    bool m_isPotentialClick = false;
}
