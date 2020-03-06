using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Runtime.CompilerServices;

public class Mandelbrot : MonoBehaviour
{
    static int Compute(float xc, float yc, int threshold)
    {
        int iter = 0;
        float r = 0;
        float i = 0;
        float r2 = 0;
        float i2 = 0;

        const float MAX_MAG_SQUARED = 10;

        while ((iter < threshold) && (r2 + i2 < MAX_MAG_SQUARED))
        {
            r2 = r * r;
            i2 = i * i;
            i = 2 * i * r + yc;
            r = r2 - i2 + xc;
            iter++;
        }

        return iter;
    }

    [BurstCompile]
    public struct Job : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Color32> colors;
        public int size;
        public float2 bounds;
        public float2 position;
        public int threshold;
        public float gain;

        public void Execute(int i)
        {
            int y = i / size;
            int x = i % size;

            float xc = position.x + bounds.x * x / size;
            float yc = position.y + bounds.y * y / size;

            int thres = threshold;
            int p = Compute(xc, yc, thres);

            //colors[i] = p < thres ?
            //new Color32(255, 255, 255, 255) :
            //new Color32(0, 0, 0, 255);

            float v = saturate(p * gain / threshold);

            float r = Band(0.33f, 0.33f, v) + Band(1, 0.33f, v);
            float g = Band(0.5f, 0.33f, v) + Band(1, 0.33f, v);
            float b = Band(0.66f, 0.33f, v) + Band(1, 0.33f, v);

            colors[i] = new Color32(
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255),
                255);
        }

        static float Band(float center, float width, float t)
        {
            return saturate(1 - abs((-center + t) / width));
        }
    }
}
