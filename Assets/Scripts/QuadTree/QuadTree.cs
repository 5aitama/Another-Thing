using Unity.Mathematics;
using Unity.Collections;

using Saitama.Physics2D;

namespace Saitama.Procedural
{
    /// <summary>
    /// Simple quad tree implementation class.
    /// </summary>
    public class QuadTree : System.IDisposable
    {
        /// <summary>
        /// QuadTree boundary
        /// </summary>
        /// <value></value>
        public AABB2D Bound { get; protected set; }

        /// <summary>
        /// Store all branches in the quad tree.
        /// </summary>
        public NativeList<Branch> branches { get; protected set; }

        /// <summary>
        /// Allocation type for branches native array.
        /// </summary>
        public Allocator allocator { get; private set; }

        /// <summary>
        /// Create new quad tree
        /// </summary>
        /// <param name="center">Center of quad tree</param>
        /// <param name="size">Size of quad tree</param>
        /// <param name="allocator">Allocation type for LODs distance array and branch array</param>
        public QuadTree(in float2 center, in float2 size, Allocator allocator)
        {
            Bound = new AABB2D(center, size / 2f);

            branches = new NativeList<Branch>(allocator);
            branches.Add(new Branch(0, -1, -1, Bound));

            this.allocator = allocator;
        }

        /// <summary>
        /// Update QuadTree geometry from multiple positions defined in <paramref name="positions"/>
        /// </summary>
        /// <param name="positions">Position array</param>
        public void Construct(in NativeArray<float2> positions, in LODBuilder lodBuilder)
        {
            branches.RemoveRange(1, branches.Length);
            lodBuilder.Build(out NativeArray<float> lodsDistance, Allocator.Temp);

            ConstructRecursive(positions, lodsDistance);
        }

        /// <summary>
        /// Check if <paramref name="branch" /> can be splited into 4 little branch.
        /// </summary>
        /// <param name="positions">Position array</param>
        /// <param name="lodsDistance">LOD distance array</param>
        /// <param name="branch">The branch to check</param>
        /// <param name="depth">Current depth in the tree</param>
        private bool BranchNeedSubdivide(in NativeArray<float2> positions, in NativeArray<float> lodsDistance, in Branch branch, in int depth)
        {
            for(var i = 0; i < positions.Length; i++)
            {
                var lod = 0;

                var dist = math.distance(branch.Bounds.Center, positions[i]);

                for(var j = 0; j < lodsDistance.Length && dist < lodsDistance[j]; j++)
                    lod = lod > j ? lod : j;

                if(depth < lod)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Construct quad tree branch.
        /// </summary>
        /// <param name="positions">Position array</param>
        /// <param name="lodsDistance">LOD disance array</param>
        /// <param name="branchIndex">Current branch index</param>
        /// <param name="depth">Current tree depth</param>
        private void ConstructRecursive(in NativeArray<float2> positions, in NativeArray<float> lodsDistance, in int branchIndex = 0, int depth = 0)
        {
            var currentBranch = branches.ElementAt(branchIndex);

            if(!BranchNeedSubdivide(positions, lodsDistance, currentBranch, depth))
                return;

            var childsBounds = SplitBounds(currentBranch.Bounds);
            var branchesLength = branches.Length;

            currentBranch.FirstChildIndex = branchesLength;

            var sw = new Branch(branchesLength    , branchIndex, -1, childsBounds[0]);
            var nw = new Branch(branchesLength + 1, branchIndex, -1, childsBounds[1]);
            var ne = new Branch(branchesLength + 2, branchIndex, -1, childsBounds[2]);
            var se = new Branch(branchesLength + 3, branchIndex, -1, childsBounds[3]);

            // Add new branches into our array
            branches.AddRange(new NativeArray<Branch>(new Branch[]
            {
                sw, 
                nw, 
                ne, 
                se,
            }, Allocator.Temp));

            // Update current branch
            branches.ElementAt(branchIndex) = currentBranch;

            depth++;
            
            ConstructRecursive(positions, lodsDistance, sw.Index, depth);
            ConstructRecursive(positions, lodsDistance, nw.Index, depth);
            ConstructRecursive(positions, lodsDistance, ne.Index, depth);
            ConstructRecursive(positions, lodsDistance, se.Index, depth);
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

        /// <summary>
        /// Indicate which branches are neighboring <paramref name="branch" />.
        /// </summary>
        /// <returns>
        /// A byte where the 4 first bits correspond to 4 direction (in this order: West, North, East, South).
        /// When a bit is equal to 1 that indicate that have another branch that neighbouring <paramref name="branch" />.
        /// </returns>
        public byte NeighborConfigForBranch(in Branch branch)
        {
            if(branch.IsRoot)
                return 0;
            
            var parent = branches[branch.ParentIndex];
            var generalConf = 0;
            var branchConf = 0;

            for(var i = 0; i < 4; i++)
            {
                branchConf |= (parent.FirstChildIndex + i == branch.Index ? 0x1 : 0x0) << i;
                generalConf |= (branches[parent.FirstChildIndex + i].IsLeaf ? 0x0 : 0x1) << i;
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