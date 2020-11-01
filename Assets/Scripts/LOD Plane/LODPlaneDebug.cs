using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public class LODPlaneDebug : MonoBehaviour
{
    public float2 size;
    public int2 resolutions;

    [SerializeField]
    public Procedural.Directions neighboringPlanes;

    public Material material;

    private void Start()
    {
        var planeGameObject = new GameObject("Plane");

        var m = new Mesh();
        var mf = planeGameObject.AddComponent<MeshFilter>();
        var mc = planeGameObject.AddComponent<MeshCollider>();
        
        planeGameObject.AddComponent<MeshRenderer>().material = material;

        NativeArray<float3> vertices = new NativeArray<float3>();
        NativeList<int> triangles = new NativeList<int>();

        var LODPlane = new Procedural.LODPlane(transform.position, size, resolutions);
        LODPlane.SetNeighbors(neighboringPlanes);
        LODPlane.ConstructPlane(quaternion.Euler(math.radians(transform.rotation.eulerAngles)), ref vertices, ref triangles, Allocator.Temp);

        m.SetVertices(vertices);
        m.SetIndices<int>(triangles, MeshTopology.Triangles, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();

        mf.mesh = m;
        mc.sharedMesh = m;
    }

    private void OnDrawGizmos()
    {
        NativeArray<float3> vertices = new NativeArray<float3>();
        NativeList<int> triangles = new NativeList<int>();

        var LODPlane = new Procedural.LODPlane(transform.position, size, resolutions);

        LODPlane.SetNeighbors(neighboringPlanes);
        LODPlane.ConstructPlane(quaternion.Euler(math.radians(transform.rotation.eulerAngles)), ref vertices, ref triangles, Allocator.Temp);

        // Debug.Log($"LODPlane vertex amount: {vertices.Length}, triangle amount: {triangles.Length} ({triangles.Length * 3} indices)");

        for(var t = 0; t < triangles.Length; t += 3)
        {
            Gizmos.DrawLine(vertices[triangles[t    ]], vertices[triangles[t + 1]]);
            Gizmos.DrawLine(vertices[triangles[t + 1]], vertices[triangles[t + 2]]);
            Gizmos.DrawLine(vertices[triangles[t + 2]], vertices[triangles[t    ]]);
        }
    }
}
