using UnityEngine;
using System.Collections;

public class Cascade : MonoBehaviour {
    public int materialIndex = 0;
    public Vector2 uvAnimationRate = new Vector2( 1.0f, 0.0f );
    public string textureName = "_MainTex";
    Vector2 uvOffset = Vector2.zero;
    // Use this for initialization
    void Start () {
	
    }
    // Update is called once per frame
    void Update () {
		
        uvOffset += ( uvAnimationRate * Time.deltaTime );
        if ( renderer.enabled ) {
            renderer.materials[ materialIndex ].SetTextureOffset( textureName, uvOffset );
        }
	
    }
}
