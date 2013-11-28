using UnityEngine;
using System.Collections;
using System.Linq;

public class DynamicAmbient : MonoBehaviour {
    public Gradient[] ambientGradient;
    public Texture2D ambientRamp;
    [Range( 0.0f, 1.0f )]
    public float time;

    void Start () {
        var gradients = transform.GetComponentsInChildren<GradientHolder>();
        ambientGradient = gradients.OrderBy( h => h.index ).Select( h => h.ambientGradient ).ToArray();
    }

    void Update () {

        RenderSettings.ambientLight = Color.white;

        var firstIndex = Mathf.FloorToInt( ( time % 1 ) * ( ambientGradient.Length ) );

        var secondIndex = ( firstIndex + 1 ) % ( ambientGradient.Length );

        int y = 0;
        while ( y < ambientRamp.height ) {
            int x = 0;
            while ( x < ambientRamp.width ) {

                if ( firstIndex < secondIndex || true ) {

                    Color colorA = ambientGradient[ firstIndex ].Evaluate( x / ( float )ambientRamp.width );
                    Color colorB = ambientGradient[ secondIndex ].Evaluate( x / ( float )ambientRamp.width );
                    Color result = Color.Lerp( colorA, colorB, ( time * ambientGradient.Length ) % 1 );

                    ambientRamp.SetPixel( x, y, result );
                } 

                ++x;
            }
            ++y;
        }
        ambientRamp.Apply();
        Shader.SetGlobalTexture( "_AmbientRamp", ambientRamp );
    }
}

