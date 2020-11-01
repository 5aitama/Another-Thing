using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

using Saitama.Procedural;

public class QuadTreeDebugger : MonoBehaviour
{
    public Transform[] targets;

    [Range(0f, 32f)]
    public float radius = 8f;

    [Range(1, 32)]
    public int maxLODs = 8;

    [Range(1, 32)]
    public int planeResolution = 8;

    private void OnDrawGizmos()
    {
        var pos = (float3)transform.position;
        var size = (float3)transform.localScale;

        var tree = new QuadTree(pos.xy, size.xy, Allocator.Temp);

        var positions = new NativeArray<float2>(targets.Length, Allocator.Temp);

        for(var i = 0; i < targets.Length; i++)
            positions[i] = ((float3)targets[i].position).xy;

        tree.Construct(positions, new LODBuilder(radius, maxLODs));
        
        var branches = tree.branches;

        var c = Gizmos.color;

        for(var i = 0; i < branches.Length; i++)
        {
            var b = branches[i].Bounds;

            var sw = new float3(b.SWCorner, 0);
            var nw = new float3(b.NWCorner, 0);
            var ne = new float3(b.NECorner, 0);
            var se = new float3(b.SECorner, 0);

            if(branches[i].IsLeaf)
            {
                Gizmos.color = Color.red;
                DebugBranch(tree, branches[i], branches);
                Gizmos.color = c;
            }

            Gizmos.DrawLine(sw, nw);
            Gizmos.DrawLine(nw, ne);
            Gizmos.DrawLine(ne, se);
            Gizmos.DrawLine(se, sw);

        }
    }

    private void DebugBranch(in QuadTree tree, in Branch branch, in NativeArray<Branch> branches)
    {
        var conf = tree.NeighborConfigForBranch(branch);
        var lodPlane = new LODPlane(new float3(branch.Bounds.SWCorner, 0), branch.Bounds.Extents * 2f, planeResolution);
        
        NativeArray<float3> v = new NativeArray<float3>();
        NativeList<int> t = new NativeList<int>();

        lodPlane.SetNeighbors((Directions)conf);
        lodPlane.ConstructPlane(quaternion.identity, ref v, ref t, Allocator.Temp);

        for(var i = 0; i < t.Length; i += 3)
        {
            Gizmos.DrawLine(v[t[i    ]], v[t[i + 1]]);
            Gizmos.DrawLine(v[t[i + 1]], v[t[i + 2]]);
            Gizmos.DrawLine(v[t[i + 2]], v[t[i   ]]);
        }

        v.Dispose();
        t.Dispose();
    }
}
