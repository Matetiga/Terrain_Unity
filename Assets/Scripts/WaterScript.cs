using System.Collections;
using System.Security.Cryptography.X509Certificates;
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
    public int waterResolution = 2;


    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;


    void EnsureComponents()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
        }

        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
        }

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        if (WaterMaterial != null && meshRenderer != null)
        {
            meshRenderer.sharedMaterial = WaterMaterial;
        }
    }


    void Awake()
    {
        EnsureComponents();

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

    }


    void Start()
    {
        EnsureComponents();

        GenerateWaterSurface();
        UpdateMesh();

        if (meshRenderer == null || meshRenderer.sharedMaterial == null)
        {
            return;
        }

        meshRenderer.sharedMaterial.SetFloat("_WaveFrequency", waveFrequency);
        meshRenderer.sharedMaterial.SetFloat("_WaveSpeed", waveSpeed);
        meshRenderer.sharedMaterial.SetFloat("_WaveHeight", waveHeight);

    }

    void OnValidate()
    {
        EnsureComponents();

        if (meshRenderer == null || meshRenderer.sharedMaterial == null)
        {
            return;
        }

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

    void GenerateWaterSurface()
    {

        // width and depth will be for the number of quads 
        int widthResolution  = width * waterResolution; 
        int depthResolution = depth * waterResolution;

        vertices = new Vector3[(widthResolution +1 )* (depthResolution + 1)];
        uvs = new Vector2[vertices.Length];
        // this will store the vertices per triangle
        int vertexIndex = 0;
        for(int x = 0; x < widthResolution + 1; x++)
        {
            for(int z = 0; z < depthResolution + 1; z++)
            {
                vertices[vertexIndex] = new Vector3((float) x / waterResolution, 0,  (float) z/waterResolution);
                // uvs[vertexIndex] = new Vector2((float) x / width, (float) z/ depth);
                vertexIndex++;
            }
        }


        triangles = new int[width * depth * 6 * waterResolution * waterResolution];
        int triangleIndex = 0;
        int vert = 0;
        for(int z = 0; z < depthResolution; z++)
        {
            for(int x = 0; x < widthResolution; x++)
            {

                triangles[triangleIndex + 0] = vert;
                triangles[triangleIndex + 1] = vert + 1;
                triangles[triangleIndex + 2] = vert + widthResolution+1;

                triangles[triangleIndex + 3] = vert +1;
                triangles[triangleIndex + 4] = vert + widthResolution + 2;
                triangles[triangleIndex + 5] = vert + widthResolution + 1;

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

