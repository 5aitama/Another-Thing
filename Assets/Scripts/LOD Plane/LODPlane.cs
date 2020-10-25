using Unity.Mathematics;
using Unity.Collections;

namespace Procedural
{
    public class LODPlane
    {
        public float2 Size      { get; private set; }
        public int2 Resolution  { get; private set; }
        public byte Neighbors   { get; private set; }

        public static int3[] GetSquareTrianglesFor(
            in Directions planeConfig, 
            in Directions squareConfig, 
            in int2 squarePosition, 
            in int squareIndex, 
            in int2 planeResolution, 
            in int egdesVertexAmount)
        {
            var amount = planeResolution.Amount();
            var lastIndex = amount;

            var edges = new int4(0);
            
            for(int i = 0, j = 1; i < 4; i++, j += j)
            {
                if(((byte)planeConfig & j) == j)
                {
                    edges[i] = lastIndex;
                    lastIndex += egdesVertexAmount - 1;
                }
            }
            
            var edgeIndices = new int[]
            {
                squareIndex - planeResolution.x - 1, // • Bottom left
                edges[0] + squarePosition.y - 1,     // • Middle left
                squareIndex - 1,                     // • Top left
                edges[1] + squarePosition.x - 1,     // • Middle top
                squareIndex,                         // • Top right
                edges[2] + squarePosition.y - 1,     // • Middle right
                squareIndex - planeResolution.x,     // • Bottom right
                edges[3] + squarePosition.x - 1,     // • Middle bottom
            };

            switch((byte)squareConfig)
            {
                case 0x0: return new int3[] 
                { 
                    new int3(edgeIndices[0], edgeIndices[2], edgeIndices[4]), 
                    new int3(edgeIndices[0], edgeIndices[4], edgeIndices[6]),
                };

                case 0x1: return new int3[] 
                {
                    new int3(edgeIndices[1], edgeIndices[2], edgeIndices[4]),
                    new int3(edgeIndices[1], edgeIndices[4], edgeIndices[6]),
                    new int3(edgeIndices[1], edgeIndices[6], edgeIndices[0]),
                };

                case 0x2: return new int3[]
                {
                    new int3(edgeIndices[3], edgeIndices[4], edgeIndices[6]),
                    new int3(edgeIndices[3], edgeIndices[6], edgeIndices[0]),
                    new int3(edgeIndices[3], edgeIndices[0], edgeIndices[2]),
                };

                case 0x4: return new int3[]
                {
                    new int3(edgeIndices[5], edgeIndices[6], edgeIndices[0]),
                    new int3(edgeIndices[5], edgeIndices[0], edgeIndices[2]),
                    new int3(edgeIndices[5], edgeIndices[2], edgeIndices[4]),
                };

                case 0x8: return new int3[]
                {
                    new int3(edgeIndices[7], edgeIndices[0], edgeIndices[2]),
                    new int3(edgeIndices[7], edgeIndices[2], edgeIndices[4]),
                    new int3(edgeIndices[7], edgeIndices[4], edgeIndices[6]),
                };

                case 0x3: return new int3[]
                {
                    new int3(edgeIndices[6], edgeIndices[0], edgeIndices[1]),
                    new int3(edgeIndices[6], edgeIndices[1], edgeIndices[2]),
                    new int3(edgeIndices[6], edgeIndices[2], edgeIndices[3]),
                    new int3(edgeIndices[6], edgeIndices[3], edgeIndices[4]),
                };

                case 0x6: return new int3[]
                {
                    new int3(edgeIndices[0], edgeIndices[2], edgeIndices[3]),
                    new int3(edgeIndices[0], edgeIndices[3], edgeIndices[4]),
                    new int3(edgeIndices[0], edgeIndices[4], edgeIndices[5]),
                    new int3(edgeIndices[0], edgeIndices[5], edgeIndices[6]),
                };

                case 0xC: return new int3[]
                {
                    new int3(edgeIndices[2], edgeIndices[4], edgeIndices[5]),
                    new int3(edgeIndices[2], edgeIndices[5], edgeIndices[6]),
                    new int3(edgeIndices[2], edgeIndices[6], edgeIndices[7]),
                    new int3(edgeIndices[2], edgeIndices[7], edgeIndices[0]),
                };

                case 0x9: return new int3[]
                {
                    new int3(edgeIndices[4], edgeIndices[6], edgeIndices[7]),
                    new int3(edgeIndices[4], edgeIndices[7], edgeIndices[0]),
                    new int3(edgeIndices[4], edgeIndices[0], edgeIndices[1]),
                    new int3(edgeIndices[4], edgeIndices[1], edgeIndices[2]),
                };

                default: throw new System.IndexOutOfRangeException($"This configuration is not supported : {((byte)squareConfig).ToString("X2")}");
            }
        }

        public LODPlane(in float2 size, in int2 resolution)
        {
            Size = size;
            Resolution = resolution;
        }

        public void SetNeighbors(Directions neighbors)
        {
            Neighbors = (byte)neighbors;
        }

        public bool HavePlaneAt(in Directions directions)
            => ((byte)directions & Neighbors) == (byte)directions;

        public void ConstructPlane(in float3 position, out NativeArray<float3> vertices, out NativeList<int3> triangles, Allocator allocator)
        {
            
            var pointAmount = Resolution.Amount();
            var offset = Size / (float2)(Resolution - 1);

            var edgeVertexAmount = math.max(Resolution.x, Resolution.y);

            var _vertices = new NativeArray<float3>(pointAmount, allocator, NativeArrayOptions.UninitializedMemory);
            var sides = new NativeArray<float3>(edgeVertexAmount * 4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            var sideArraysCounter = new int4(0);

            triangles = new NativeList<int3>(Allocator.Temp);

            for(var i = 0; i < pointAmount; i++)
            {
                var localPos = i.To2D(Resolution);
                var worldPos = localPos * offset + position.xy;
                
                _vertices[i] = new float3(worldPos.xy, 0);

                var minEdges = localPos == 0;
                var maxEdges = localPos == Resolution - 1;

                #region Edges vertices calculation

                if(minEdges.x && !minEdges.y && HavePlaneAt(Directions.West))
                {
                    var p = worldPos - new float2(0, offset.y) / 2f;
                    sides[sideArraysCounter.x++] = new float3(p.xy, 0);
                }

                if(maxEdges.y && !minEdges.x && HavePlaneAt(Directions.North))
                {
                    var p = worldPos - new float2(offset.x, 0) / 2f;
                    sides[edgeVertexAmount + sideArraysCounter.y++] = new float3(p.xy, 0);
                }

                if(maxEdges.x && !minEdges.y && HavePlaneAt(Directions.East))
                {
                    var p = worldPos - new float2(0, offset.y) / 2f;
                    sides[edgeVertexAmount * 2 + sideArraysCounter.z++] = new float3(p.xy, 0);
                }

                if(!minEdges.x && minEdges.y && HavePlaneAt(Directions.South))
                {
                    var p = worldPos - new float2(offset.x, 0) / 2f;
                    sides[edgeVertexAmount * 3 + sideArraysCounter.w++] = new float3(p.xy, 0);
                }

                #endregion
                
                if(minEdges.x || minEdges.y)
                    continue;
                
                var minEdges2 = localPos == 1;
                var squareConfig = Directions.None;

                squareConfig |= minEdges2.x && HavePlaneAt(Directions.West)  ? Directions.West  : Directions.None;
                squareConfig |= maxEdges.y  && HavePlaneAt(Directions.North) ? Directions.North : Directions.None;
                squareConfig |= maxEdges.x  && HavePlaneAt(Directions.East)  ? Directions.East  : Directions.None;
                squareConfig |= minEdges2.y && HavePlaneAt(Directions.South) ? Directions.South : Directions.None;

                var indices = GetSquareTrianglesFor((Directions)Neighbors, squareConfig, localPos, i, Resolution, edgeVertexAmount);
                triangles.AddRange(new NativeArray<int3>(indices, allocator));
            }

            var finalVertexArray = new NativeArray<float3>(_vertices.Length + sideArraysCounter.Sum(), allocator, NativeArrayOptions.UninitializedMemory);
            finalVertexArray.Slice(0, _vertices.Length).CopyFrom(_vertices);

            var lastStartIndex = _vertices.Length;

            for(var i = 0; i < 4; i++)
            {
                if(sideArraysCounter[i] == 0)
                    continue;

                finalVertexArray.Slice(lastStartIndex, sideArraysCounter[i]).CopyFrom(sides.Slice(edgeVertexAmount * i, sideArraysCounter[i]));
                lastStartIndex += sideArraysCounter[i];
            }

            vertices = finalVertexArray;
        }
    }
}