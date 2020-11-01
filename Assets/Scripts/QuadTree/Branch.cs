using Saitama.Physics2D;

namespace Saitama.Procedural.QuadTree
{
    public struct Branch
    {
        /// <summary>
        /// Index of parent in branches array.
        /// </summary>
        public int ParentIndex { get; private set; }

        /// <summary>
        /// First child index of this branch.
        /// </summary>
        /// <remarks>
        /// Can return -1 if the current branch have no child, so
        /// it would be better to check if the current branch have
        /// child before use this property.
        /// </remarks>
        public int FirstChildIndex  { get; set; }

        public AABB2D Bounds        { get; set; }
        public int Depth            { get; set; }
        public int Index            { get; private set; }

        public bool HaveChild => (FirstChildIndex != -1);
        public bool IsRoot => (ParentIndex == -1);
        public bool IsLeaf => !HaveChild;
        
        public Branch(in int index, in int parentIndex, in int firstChildIndex = -1, in AABB2D bounds = default, in int depth = 0)
        {
            Index = index;
            ParentIndex = parentIndex;
            FirstChildIndex = firstChildIndex;
            Bounds = bounds;
            Depth = depth;
        }

    }
}