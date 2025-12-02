using System.Collections;
using UnityEngine;

public class WaterScript : MonoBehaviour
{

    public int width = 50;
    public int depth = 50;

    public Material WaterMaterial;



    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Vector3[] vertices;
    int[] triangles;


    void Awake()
    {
        mesh = new Mesh();
        // Mesh Filter hold a reference to the mesh data (contains vertices, uvs, tringles, normals)
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

        // Mesh Renderer renders the mesh on the screen (it controls materials, shaders, light)
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

    }


    void Start()
    {

        // Both of these work together to show the object in screen
        // MeshFilter          MeshRenderer
        //    │                    │
        //    │ (provides)         │ (applies)
        //    ▼                    ▼
        //  MESH    ────────►   MATERIAL   ────────►   SCREEN
        // (shape)             (appearance)           (final image)
        meshRenderer.sharedMaterial = WaterMaterial;

        StartCoroutine(GenerateWaterSurface(width, depth));

    }
    
    void Update(){
        UpdateMesh();
    }

    // width and depth will be for the number of quads 
    IEnumerator GenerateWaterSurface(int width, int depth)
    {

        vertices = new Vector3[(width+1) * (depth+1)];
        // this will store the vertices per triangle
        int vertexIndex = 0;
        for(int x = 0; x < width +1; x++)
        {
            for(int z = 0; z < depth +1; z++)
            {
                vertices[vertexIndex] = new Vector3(x, 0, z);
                vertexIndex++;
            }
        }
        Debug.Log($"Number of vertex is {vertexIndex}");


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
                yield return new WaitForSeconds(.1f);
            }
            vert++;
        }

        Debug.Log($"Number of triangleIndex is {triangleIndex}");
    }
    
    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

    }

    private void OnDrawGizmos(){
        if (vertices == null) return;


        Gizmos.color = Color.yellow;
        for(int i = 0; i < vertices.Length; i++){
            Gizmos.DrawSphere(vertices[i], .1f);
        }


    }
}

