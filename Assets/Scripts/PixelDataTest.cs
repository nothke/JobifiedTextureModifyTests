using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

using Unity.Mathematics;
using static Unity.Mathematics.math;

public class PixelDataTest : MonoBehaviour
{
    public Texture2D texture;

    [BurstCompile]
    struct SetPixelsJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Color32> colors;

        public void Execute(int i)
        {
            colors[i] = new Color32(255, 255, 255, 255);
        }
    }

    [BurstCompile]
    struct SetNoisePixelsJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Color32> colors;

        public void Execute(int i)
        {
            int y = i / SIZE;
            int x = i % SIZE;

            float2 pos = float2(x, y) * 0.1234f;
            colors[i] = new Color32((byte)(noise.cnoise(pos) * 256), 0, 0, 255);
        }
    }

    const int SIZE = 512;
    const int TSIZE = SIZE * SIZE;

    NativeArray<Color32> colors;

    void Start()
    {
        texture = new Texture2D(SIZE, SIZE, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

        colors = texture.GetRawTextureData<Color32>();

        new SetPixelsJob() { colors = colors }.Schedule(TSIZE, 512).Complete();

        texture.Apply(false);
    }

    private void Update()
    {
        new SetPixelsJob() { colors = colors }.Schedule(TSIZE, 512).Complete();

        Profiler.BeginSample("Texture Apply");
        texture.Apply(false);
        Profiler.EndSample();
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, SIZE, SIZE), texture);
    }
}
