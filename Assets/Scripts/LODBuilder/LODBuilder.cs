using Unity.Collections;
using Unity.Mathematics;

namespace Saitama.Procedural
{
    /// <summary>
    /// Level of detail builder.
    /// </summary>
    public struct LODBuilder
    {
        /// <summary>
        /// Minimum lod distance.
        /// </summary>
        public float MinDistance { get; private set; }
        
        /// <summary>
        /// Amout of level of detail.
        /// </summary>
        public int LODAmount { get; private set; }

        /// <summary>
        /// Create new LODBuilder.
        /// </summary>
        /// <param name="minDistance">The minimum lod distance</param>
        /// <param name="lodAmount">The amount of level of detail</param>
        public LODBuilder(in float minDistance, in int lodAmount)
        {
            MinDistance = minDistance;
            LODAmount = lodAmount;
        }

        /// <summary>
        /// Calculate distance for each level of detail.
        /// </summary>
        /// <remarks>
        /// Distances are stored in desc order.
        /// </remarks>
        /// <param name="distances">NativeArray that store distances calculated</param>
        /// <param name="allocator">Allocator for <paramref name="distances"/> native array</param>
        public void Build(out NativeArray<float> distances, Allocator allocator)
        {
            distances = new NativeArray<float>(LODAmount, allocator, NativeArrayOptions.UninitializedMemory);

            for(var i = 0; i < LODAmount; i++)
                distances[i] = MinDistance * math.pow(2f, LODAmount - i);
        }
    }
}