using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public class PixelHDRTests : MonoBehaviour
{
    public Texture2D texture;

    const int SIZE = 512;
    const int TSIZE = SIZE * SIZE;

    NativeArray<half4> colors;

    void Start()
    {
        texture = new Texture2D(SIZE, SIZE,
            UnityEngine.Experimental.Rendering.DefaultFormat.HDR,
            UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

        colors = texture.GetRawTextureData<half4>();

        new PixelJobs.SetHDRJob() { colors = colors }.Schedule(TSIZE, 512).Complete();

        texture.Apply(false);
    }

    private void OnDestroy()
    {
        if (!enabled) return;
    }

    int testi = 0;

    private void Update()
    {
        new PixelJobs.SetHDRJob()
        {
            colors = colors,
            size = SIZE
        }.Schedule(TSIZE, 512).Complete();

        new PixelJobs.SetNoiseHDRJob()
        {
            colors = colors,
            size = SIZE,
            offset = Time.time * 20
        }.Schedule(TSIZE, 512).Complete();

        Profiler.BeginSample("Texture Apply");
        texture.Apply(false);
        Profiler.EndSample();
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, SIZE, SIZE), texture);
    }
}
