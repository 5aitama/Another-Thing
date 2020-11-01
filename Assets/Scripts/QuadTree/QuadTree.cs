using Unity.Mathematics;
using Unity.Collections;

using Saitama.Physics2D;

namespace Saitama.Procedural.QuadTree
{
    /// <summary>
    /// Simple quad tree implementation class.
    /// </summary>
    public class QuadTree : System.IDisposable
    {
        /// <summary>
        /// Center of the quad tree.
        /// </summary>
        public float2 Center { get; protected set; }

        /// <summary>
        /// The size of quad tree.
        /// </summary>
        public float2 Size { get; protected set; }

        /// <summary>
        /// Store all branches in the quad tree.
        /// </summary>
        public NativeList<Branch> branches { get; protected set; }

        /// <summary>
        /// Store distance for each level of details
        /// </summary>
        protected NativeArray<float> lodsDistance;

        /// <summary>
        /// Allocation type for all native array in QuadTree.
        /// </summary>
        protected Allocator allocator;

        /// <summary>
        /// Create new quad tree
        /// </summary>
        /// <param name="center">Center of quad tree</param>
        /// <param name="size">Size of quad tree</param>
        /// <param name="allocator">Allocation type for LODs distance array and branch array</param>
        public QuadTree(in float2 center, in float2 size, Allocator allocator)
        {
            Center = center;
            Size = size;

            branches = new NativeList<Branch>(allocator);

            var rootBranchBounds = new AABB2D(center, Size / 2f);
            branches.Add(new Branch(0, -1, -1, rootBranchBounds));

            this.allocator = allocator;
        }

        /// <summary>
        /// Update the level of details distances
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="maxDepth">The maximum subdivision level of quad tree</param>
        public void UpdateLODs(in float radius, in int maxDepth)
        {
            if(lodsDistance.IsCreated)
                lodsDistance.Dispose();
            
            lodsDistance = new NativeArray<float>(maxDepth, allocator, NativeArrayOptions.UninitializedMemory);

            for(var i = 0; i < maxDepth; i++)
                lodsDistance[i] = radius * math.pow(2f, maxDepth - i);
        }

        /// <summary>
        /// Update QuadTree geometry from multiple positions defined in <paramref name="targets"/>
        /// </summary>
        /// <param name="targets">Positions</param>
        public void UpdateWithTargets(in NativeArray<float2> targets)
        {
            branches.RemoveRange(1, branches.Length);
            Subdivide(targets);
        }

        
        private bool BranchNeedSubdivide(in NativeArray<float2> targets, in Branch branch, in int depth)
        {
            for(var i = 0; i < targets.Length; i++)
            {
                var lod = 0;

                var dist = math.distance(branch.Bounds.Center, targets[i]);

                for(var j = 0; j < lodsDistance.Length && dist < lodsDistance[j]; j++)
                    lod = lod > j ? lod : j;

                if(depth < lod)
                    return true;
            }

            return false;
        }

        private void Subdivide(in NativeArray<float2> targets, in int branchIndex = 0, int depth = 0)
        {
            var currentBranch = branches.ElementAt(branchIndex);

            if(!BranchNeedSubdivide(targets, currentBranch, depth))
                return;

            var childsBounds = SplitBounds(currentBranch.Bounds);
            var branchesLength = branches.Length;

            currentBranch.FirstChildIndex = branchesLength;
            currentBranch.Depth = depth++;

            var sw = new Branch(branchesLength    , branchIndex, -1, childsBounds[0], depth);
            var nw = new Branch(branchesLength + 1, branchIndex, -1, childsBounds[1], depth);
            var ne = new Branch(branchesLength + 2, branchIndex, -1, childsBounds[2], depth);
            var se = new Branch(branchesLength + 3, branchIndex, -1, childsBounds[3], depth);

            // Add new branches into our array
            branches.AddRange(new NativeArray<Branch>(new Branch[]
            {
                sw, 
                nw, 
                ne, 
                se,
            }, Allocator.Temp));

            // Update our current branch
            branches.ElementAt(branchIndex) = currentBranch;

            // Subdivide each of them...
            Subdivide(targets, sw.Index, depth);
            Subdivide(targets, nw.Index, depth);
            Subdivide(targets, ne.Index, depth);
            Subdivide(targets, se.Index, depth);
        }

        /// <summary>
        /// Split bounds into 4 little bounds.
        /// </summary>
        /// <param name="bounds">Bounds to split</param>
        private AABB2D[] SplitBounds(in AABB2D bounds)
        {
            var ext = bounds.Extents / 2f;

            return new AABB2D[]
            {
                new AABB2D(bounds.Center - new float2( ext.x,  ext.y), ext),
                new AABB2D(bounds.Center + new float2(-ext.x,  ext.y), ext),
                new AABB2D(bounds.Center + new float2( ext.x,  ext.y), ext),
                new AABB2D(bounds.Center + new float2( ext.x, -ext.y), ext),
            };
        }

        public byte NeighborConfigForBranch(in Branch branch)
        {
            var parent = branches[branch.ParentIndex];
            var generalConf = (byte)0;
            var branchConf = (byte)0;

            // Get parent configuration
            for(var i = 0; i < 4; i++)
            {
                branchConf |= (byte)((parent.FirstChildIndex + i == branch.Index ? 0x1 : 0x0) << i);
                generalConf |= (byte)((branches[parent.FirstChildIndex + i].IsLeaf ? 0x0 : 0x1) << i);
            }

            switch(branchConf)
            {
                case 0x1:
                    return (byte)((generalConf & 0x2) | (generalConf & 0x8) >> 1);
                case 0x2:
                    return (byte)((generalConf & 0x4) | (generalConf & 0x1) << 3);
                case 0x4:
                    return (byte)((generalConf & 0x2) >> 1 | (generalConf & 0x8));
                case 0x8:
                    return (byte)((generalConf & 0x1) | (generalConf & 0x4) >> 1);
                default:
                    throw new System.Exception($"Please implement a branch configuration for {branchConf} !");
            }
        }

        public void Dispose()
        {
            branches.Dispose();
        }
    }
}