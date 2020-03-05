using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

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

    const int SIZE = 1024;
    const int TSIZE = SIZE * SIZE;

    NativeArray<Color32> colors;

    void Start()
    {
        texture = new Texture2D(SIZE, SIZE);

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
        GUI.DrawTexture(new Rect(0, 0, 256, 256), texture);
    }
}
