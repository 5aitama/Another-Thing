using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public class LODPlaneDebug : MonoBehaviour
{
    public float2 size;
    public int2 resolutions;

    [SerializeField]
    public Procedural.Directions neighboringPlanes;

    private void OnDrawGizmos()
    {
        var LODPlane = new Procedural.LODPlane(size, resolutions);

        LODPlane.SetNeighbors(neighboringPlanes);
        LODPlane.ConstructPlane(transform.position, out NativeArray<float3> vertices, out NativeList<int3> triangles, Allocator.Temp);

        Debug.Log($"LODPlane vertex amount: {vertices.Length}, triangle amount: {triangles.Length} ({triangles.Length * 3} indices)");

        for(var t = 0; t < triangles.Length; t++)
        {
            var triangle = triangles[t];

            Gizmos.DrawLine(vertices[triangle.x], vertices[triangle.y]);
            Gizmos.DrawLine(vertices[triangle.y], vertices[triangle.z]);
            Gizmos.DrawLine(vertices[triangle.z], vertices[triangle.x]);
        }
    }
}
