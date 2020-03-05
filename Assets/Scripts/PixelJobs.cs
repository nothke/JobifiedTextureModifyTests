using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

using Unity.Mathematics;
using static Unity.Mathematics.math;

public static class PixelJobs
{
    [BurstCompile]
    public struct SetJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Color32> colors;
        public int size;

        public void Execute(int i)
        {
            colors[i] = new Color32(255, 255, 255, 255);
        }
    }

    [BurstCompile]
    public struct SetNoiseJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Color32> colors;
        public int size;

        public void Execute(int i)
        {
            int y = i / size;
            int x = i % size;

            float2 pos = float2(x, y) * 0.1234f;
            colors[i] = new Color32((byte)(noise.cnoise(pos) * 256), 0, 0, 255);
        }
    }
}
