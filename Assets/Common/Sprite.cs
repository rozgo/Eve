using UnityEngine;
using System.Collections;

public class Sprite : MonoBehaviour {
    public MeshFilter meshFilter;
    public int width = 8;
    public int height = 7;
    public float offset = 0f;
    public float speed = 1.0f;
    public bool loop = true;
    //---------------------------------------------------------------------------------------------->
    public void Start () {
        m_stepX = 1.0f / width;
        m_stepY = 1.0f / height;
        meshFilter.mesh.uv = m_uv;
        StartCoroutine( Loop() );
    }
    //---------------------------------------------------------------------------------------------->
    public IEnumerator Loop () {
        yield return new WaitForSeconds( offset );
        do {
            for ( int i = 0; i < height; i++ ) {
                for ( int j = 0; j < width; j++ ) {
                    m_uv[ 0 ] = new Vector2( j * m_stepX, ( 1 - m_stepY ) - i * m_stepY );
                    m_uv[ 1 ] = new Vector2( m_stepX + j * m_stepX, 1f - i * m_stepY );
                    m_uv[ 2 ] = new Vector2( m_stepX + j * m_stepX, ( 1 - m_stepY ) - i * ( m_stepY ) );
                    m_uv[ 3 ] = new Vector2( j * m_stepX, 1f - i * m_stepY );
                    meshFilter.mesh.uv = m_uv;
                    yield return new WaitForSeconds( speed );
                } 
            }
        } while( loop );
    }
    //---------------------------------------------------------------------------------------------->
    float m_stepX = 1;
    float m_stepY = 1;
    Vector2[] m_uv = new Vector2[4];
}
