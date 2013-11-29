//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class BaseOutlineView {
    //---------------------------------------------------------------------------------------------------------------------
    public BaseOutlineView ( int width, int length, int[,] baseOutline ) {
        m_width = width;
        m_length = length;
        m_outline = baseOutline;

        // Create the game object containing the renderer
        m_gameObject = new GameObject( "BaseOutline" );

        MeshFilter meshfilter = ( MeshFilter )m_gameObject.AddComponent( "MeshFilter" );
        m_renderer = ( MeshRenderer )m_gameObject.AddComponent( "MeshRenderer" );

        m_defaultColor = new Color( 1.0f, 1.0f, 0.0f, 0.5f );

        m_renderer.material.color = m_defaultColor;
        m_renderer.material.shader = Shader.Find( "Transparent/Diffuse" );

        // Retrieve a mesh instance
        m_mesh = meshfilter.mesh;
        int y = 0;
        int x = 0;

        // Build vertices and UVs
        m_vertices = new Vector3[length * width];
        m_triangles = new BetterList<int>();

        Vector3 sizeScale = new Vector3( SpaceConversion.MapTileSize, 1, SpaceConversion.MapTileSize );

        for ( x = 0; x < width; x++ ) {
            for ( y = 0; y < length; y++ ) {
                Vector3 vertex = new Vector3( x, 0.05f, y );
                m_vertices[ y * width + x ] = Vector3.Scale( sizeScale, vertex );
            }
        }
  
        // Assign them to the mesh
        m_mesh.vertices = m_vertices;

        m_gameObject.SetActive( false );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Recalculate () {
        m_triangles.Clear();

        // Build triangle indices: 3 indices into vertex array for each triangle
        for ( int x = 0; x < m_width; x++ ) {
            for ( int y = 0; y < m_length; y++ ) {
                if ( m_outline[ x, y ] != 0 ) {
                    // For each grid cell output two triangles
                    m_triangles.Add( ( y * m_width ) + x );
                    m_triangles.Add( ( ( y + 1 ) * m_width ) + x );
                    m_triangles.Add( ( y * m_width ) + x + 1 );

                    m_triangles.Add( ( ( y + 1 ) * m_width ) + x );
                    m_triangles.Add( ( ( y + 1 ) * m_width ) + x + 1 );
                    m_triangles.Add( ( y * m_width ) + x + 1 );
                }
            }
        }

        m_gameObject.SetActive( true );

        // And assign them to the mesh
        m_mesh.triangles = m_triangles.ToArray();
    
        // Auto-calculate vertex normals from the mesh
        m_mesh.RecalculateNormals();

        m_secondsSinceLastReCalculate = 0;

        m_renderer.material.color = m_defaultColor;
    }

    public void ResetTimer () {
        m_secondsSinceLastReCalculate = 0;
        m_gameObject.SetActive( true );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Update ( float deltaTime ) {
        float totalFadeTime = 1.5f;
        if ( m_secondsSinceLastReCalculate < totalFadeTime ) {
            float normalized = ( totalFadeTime - m_secondsSinceLastReCalculate ) / totalFadeTime;
            normalized *= 0.5f;

            float alpha = Mathf.Max( normalized, 0.0f );

            m_renderer.material.color = new Color( m_defaultColor.r, m_defaultColor.g, m_defaultColor.b, alpha );

            m_secondsSinceLastReCalculate += deltaTime;
        } else {
            m_gameObject.SetActive( false );
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void Destroy () {
        GameObject.Destroy( m_gameObject );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private int m_width;
    private int m_length;
    private Mesh m_mesh;
    private Vector3[] m_vertices;
    private BetterList<int> m_triangles;
    private readonly int[,] m_outline;
    private readonly Color m_defaultColor;
    float m_secondsSinceLastReCalculate;
    MeshRenderer m_renderer;
    GameObject m_gameObject;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------

