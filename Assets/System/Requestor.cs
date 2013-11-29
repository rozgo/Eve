using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Requestor : MonoBehaviour {

    static bool Error ( WWW www ) {
        if ( !string.IsNullOrEmpty( www.error ) ) {
            return true;
        }
        return false;
    }

    public static IEnumerator ForJSON ( string url, System.Action<string> action ) {
        var www = new WWW( url );
        yield return www;
        if ( string.IsNullOrEmpty( www.error ) ) {
            action( www.text );
        }
        else {
            Debug.Log( url );
            Debug.Log( www.error );
        }
    }

}
