using System.Collections;
using UnityEngine;

public class WaterScript : MonoBehaviour
{

    public int width = 50;
    public int depth = 50;

    public Material WaterMaterial;
    public Transform lightSource;
    public float waveFrequency;
    public float waveSpeed;
    public float waveHeight;



    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;


    void Awake()
    {
        mesh = new Mesh();

        // Both of these work together to show the object in screen
        // MeshFilter          MeshRenderer
        //    │                    │
        //    │ (provides)         │ (applies)
        //    ▼                    ▼
        //  MESH    ────────►   MATERIAL   ────────►   SCREEN
        // (shape)             (appearance)           (final image)

        // Mesh Filter hold a reference to the mesh data (contains vertices, uvs, tringles, normals)
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Mesh Renderer renders the mesh on the screen (it controls materials, shaders, light)
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

        if(WaterMaterial != null) meshRenderer.sharedMaterial = WaterMaterial;
    }


    void Start()
    {

        GenerateWaterSurface();
        UpdateMesh();

        meshRenderer.sharedMaterial.SetFloat("_WaveFrequency", waveFrequency);
        meshRenderer.sharedMaterial.SetFloat("_WaveSpeed", waveSpeed);
        meshRenderer.sharedMaterial.SetFloat("_WaveHeight", waveHeight);

    }

    void OnValidate()
    {

        meshRenderer.sharedMaterial.SetFloat("_WaveFrequency", waveFrequency);
        meshRenderer.sharedMaterial.SetFloat("_WaveSpeed", waveSpeed);
        meshRenderer.sharedMaterial.SetFloat("_WaveHeight", waveHeight);
    }
    
    void Update(){
        GenerateWaterSurface();
        UpdateMesh();
        
        // Update light direction in shader
        if(lightSource != null && WaterMaterial != null)
        {
            Vector3 lightDir = lightSource.position.normalized;
            WaterMaterial.SetVector("_LightDirection", lightDir);
        }
    }

    // width and depth will be for the number of quads 
    void GenerateWaterSurface()
    {

        vertices = new Vector3[(width+1) * (depth+1)];
        uvs = new Vector2[vertices.Length];
        // this will store the vertices per triangle
        int vertexIndex = 0;
        for(int x = 0; x < width +1; x++)
        {
            for(int z = 0; z < depth +1; z++)
            {
                vertices[vertexIndex] = new Vector3(x, 0, z);
                uvs[vertexIndex] = new Vector2((float) x / width, (float) z/ depth);
                vertexIndex++;
            }
        }


        triangles = new int[width * depth * 6];
        int triangleIndex = 0;
        int vert = 0;
        for(int z = 0; z < depth; z++)
        {
            for(int x = 0; x < width; x++)
            {

                triangles[triangleIndex + 0] = vert;
                triangles[triangleIndex + 1] = vert + 1;
                triangles[triangleIndex + 2] = vert + width +1;

                triangles[triangleIndex + 3] = vert +1;
                triangles[triangleIndex + 4] = vert + width+ 2;
                triangles[triangleIndex + 5] = vert + width+ 1;

                vert++;
                triangleIndex += 6;
            }
            vert++;
        }

    }
    
    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // mesh.RecalculateNormals();

    }

    // private void OnDrawGizmos(){
    //     if (vertices == null) return;


    //     Gizmos.color = Color.yellow;
    //     for(int i = 0; i < vertices.Length; i++){
    //         Gizmos.DrawSphere(vertices[i], .1f);
    //     }


    // }
}

