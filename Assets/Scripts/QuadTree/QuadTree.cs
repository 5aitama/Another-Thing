using Unity.Mathematics;
using Unity.Collections;

namespace Saitama
{
    public struct Branch
    {
        public int parentIndex;
        public int childIndex;
        public bool isRoot;
        public bool isLeaf;

        public void SetChildIndex(in int childIndex)
        {
            this.childIndex = childIndex;
        }
    }

    public class QuadTree
    {
        public int2 Size { get; private set; }
        public NativeList<Branch> branches;

        public QuadTree(in int2 size, Allocator allocator)
        {
            Size = size;
            branches = new NativeList<Branch>(allocator);
        }

        public void Build()
        {
            InternalBuild();
        }

        private void InternalBuild(in int branchIndex = 0)
        {
            var currentBranch = branches[branchIndex];
            
            // Subdivide branch into 4 branches

            var sw = new Branch { parentIndex = branchIndex, childIndex = -1, isRoot = false, isLeaf = true };
            var nw = new Branch { parentIndex = branchIndex, childIndex = -1, isRoot = false, isLeaf = true };
            var ne = new Branch { parentIndex = branchIndex, childIndex = -1, isRoot = false, isLeaf = true };
            var se = new Branch { parentIndex = branchIndex, childIndex = -1, isRoot = false, isLeaf = true };

            branches[branchIndex].SetChildIndex(branchIndex + 1);
        }
    }
}