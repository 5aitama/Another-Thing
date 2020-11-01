using Unity.Mathematics;
using Unity.Collections;

namespace Saitama.Physics2D
{
    /// <summary>
    /// Simple 2D Axis-Aligned Bounding Box
    /// </summary>
    public struct AABB2D
    {
        /// <summary>
        /// Center of this bounding box.
        /// </summary>
        public float2 Center { get; private set; }

        /// <summary>
        /// Half size of this bounding box.
        /// </summary>
        public float2 Extents { get; private set; }
        
        /// <summary>
        /// The south-west corner position of this bounding box.
        /// </summary>
        public float2 SWCorner => Center - Extents;

        /// <summary>
        /// The North-East corner position of this bounding box.
        /// </summary>
        public float2 NECorner => Center + Extents;

        /// <summary>
        /// The North-West corner position of this bounding box.
        /// </summary>
        public float2 NWCorner => Center + new float2(-Extents.x, Extents.y);

        /// <summary>
        /// The South-Est corner position of this bounding box.
        /// </summary>
        public float2 SECorner => Center + new float2(Extents.x, -Extents.y);

        /// <summary>
        /// The size of bounding box.
        /// </summary>
        public float2 Size => Extents * 2f;

        /// <summary>
        /// Create new 2D AABB.
        /// </summary>
        /// <param name="center">Center of the bounding box</param>
        /// <param name="extents">Half size of the bounding box</param>
        public AABB2D(in float2 center, in float2 extents)
        {
            Center = center;
            Extents = extents;
        }

        /// <summary>
        /// Check if <paramref name="pos"/> is inside this bounding box.
        /// </summary>
        /// <param name="pos">Position to be checked</param>
        public bool Contain(in float2 pos) =>
            (pos.x >= Center.x - Extents.x && pos.y >= Center.y - Extents.y &&
             pos.x <= Center.x + Extents.x && pos.y <= Center.y + Extents.y);

        /// <summary>
        /// Check if all positions in <paramref name="positions"/> is inside this bounding box.
        /// </summary>
        /// <param name="positions">Position array to be checked</param>
        public bool Contains(in NativeArray<float2> positions)
        {
            for(var i = 0; i < positions.Length; i++)
                if(Contain(positions[i]))
                    return true;

            return false;
        }
    }
}