using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Runtime.CompilerServices;

public static class Mandelbrot
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Compute(float2 c, int threshold)
    {
        int iter = 0;
        float r = 0, i = 0, rsqr = 0, isqr = 0;

        const float MAX_MAG_SQUARED = 10;

        while ((iter < threshold) && (rsqr + isqr < MAX_MAG_SQUARED))
        {
            rsqr = r * r;
            isqr = i * i;
            i = 2 * i * r + c.y;
            r = rsqr - isqr + c.x;
            iter++;
        }

        return iter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int ComputeDouble(double2 c, int threshold)
    {
        int iter = 0;
        double r = 0, i = 0, rsqr = 0, isqr = 0;

        const float MAX_MAG_SQUARED = 10;

        while ((iter < threshold) && (rsqr + isqr < MAX_MAG_SQUARED))
        {
            rsqr = r * r;
            isqr = i * i;
            i = 2 * i * r + c.y;
            r = rsqr - isqr + c.x;
            iter++;
        }

        return iter;
    }

    [BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
    public struct Job : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Color32> colors;
        public int size;
        public int threshold;
        public float gain;

        public float2 bounds;
        public float2 position;

        public void Execute(int i)
        {
            int2 texCoord = int2(i % size, i / size);

            float2 coord = position + bounds * texCoord / size;

            int p = Compute(coord, threshold);

            float v = saturate(p * gain / threshold);

            // Colors
            colors[i] = GetColor(v);
        }
    }

    [BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
    public struct JobDouble : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Color32> colors;
        public int size;
        public int threshold;
        public float gain;

        public double2 bounds;
        public double2 position;

        public void Execute(int i)
        {
            int2 texCoord = int2(i % size, i / size);

            double2 coord = position + bounds * texCoord / size;

            int p = ComputeDouble(coord, threshold);

            float v = saturate(p * gain / threshold);

            // Colors
            colors[i] = GetColor(v);
        }
    }

    static Color32 GetColor(float v)
    {
        float r = Band(0.33f, 0.33f, v) + Band(1, 0.33f, v);
        float g = Band(0.5f, 0.33f, v) + Band(1, 0.33f, v);
        float b = Band(0.66f, 0.33f, v) + Band(1, 0.33f, v);

        return new Color32(
            (byte)(r * 255),
            (byte)(g * 255),
            (byte)(b * 255),
            255);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float Band(float center, float width, float t)
    {
        return saturate(1 - abs((-center + t) / width));
    }
}
