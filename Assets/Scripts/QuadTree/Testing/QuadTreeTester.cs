using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

using Saitama.Procedural.QuadTree;

public class QuadTreeTester : MonoBehaviour
{
    public float2 position = 0f;
    public Transform[] targets;
    public float2 size = 16f;
    public float radius = 1f;
    public int maxLODs = 4;

    [Space]

    public int leafIndex = 0;

    private void OnDrawGizmos()
    {
        var tree = new QuadTree(position, size, Allocator.Temp);
        tree.UpdateLODs(radius, maxLODs);

        var arr = new NativeArray<float2>(targets.Length, Allocator.Temp);

        for(var i = 0; i < targets.Length; i++)
            arr[i] = ((float3)targets[i].position).xy;

        tree.UpdateWithTargets(arr);
        
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

        tree.Dispose();
    }

    private void DebugBranch(in QuadTree tree, in Branch branch, in NativeArray<Branch> branches)
    {
        var conf = tree.NeighborConfigForBranch(branch);
        var lodPlane = new Procedural.LODPlane(new float3(branch.Bounds.SWCorner, 0), branch.Bounds.Extents * 2f, 8);
        
        NativeArray<float3> v = new NativeArray<float3>();
        NativeList<int> t = new NativeList<int>();

        lodPlane.SetNeighbors((Procedural.Directions)conf);
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
