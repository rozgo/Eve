using UnityEngine;
using System.Collections;

public interface ICameraInputListener {
    //---------------------------------------------------------------------------------------------------------------------
    void OnPress ( bool pressDown, FingerManager.TouchHit touchHit );
    //---------------------------------------------------------------------------------------------------------------------
    void OnClick ( FingerManager.TouchHit touchHit );
    //---------------------------------------------------------------------------------------------------------------------
    void OnScroll ( float delta );
    //---------------------------------------------------------------------------------------------------------------------
    void OnDrag ( FingerManager.TouchHit touchHit );
}
