using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using Unity.Collections;
using Unity.Jobs;

public class PixelDataTest : MonoBehaviour
{
    public Texture2D texture;

    const int SIZE = 512;
    const int TSIZE = SIZE * SIZE;

    NativeArray<Color32> colors;
    NativeArray<float> heights;

    //NativeArray<LargeStruct> largeStructs;

    //NativeArray<HalfStruct> halfStructs1;
    //NativeArray<HalfStruct> halfStructs2;

    void Start()
    {
        texture = new Texture2D(SIZE, SIZE,
            UnityEngine.Experimental.Rendering.DefaultFormat.LDR,
            UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

        colors = texture.GetRawTextureData<Color32>();

        new PixelJobs.SetJob() { colors = colors }.Schedule(TSIZE, 512).Complete();

        texture.Apply(false);

        heights = new NativeArray<float>(TSIZE, Allocator.Persistent);

        new PixelJobs.SetNoiseHeightsJob()
        {
            heights = heights,
            size = SIZE,
            offset = Time.time * 20
        }.Schedule(TSIZE, 512).Complete();

        /*
        largeStructs = new NativeArray<LargeStruct>(TSIZE, Allocator.Persistent);

        halfStructs1 = new NativeArray<HalfStruct>(TSIZE, Allocator.Persistent);
        halfStructs2 = new NativeArray<HalfStruct>(TSIZE, Allocator.Persistent);
        */
    }

    private void OnDestroy()
    {
        if (!enabled) return;

        heights.Dispose();
        //largeStructs.Dispose();
        //halfStruct1.Dispose();
        //halfStruct2.Dispose();
    }

    int testi = 0;

    private void Update()
    {
        //new PixelJobs.SetJob() { colors = colors }.Schedule(TSIZE, 512).Complete();


        Profiler.BeginSample("Set single pixel");
        heights[testi] = 1;
        testi++;
        Profiler.EndSample();

        /*
        new LargeStructJobs.MultiValueRWSet()
        {
            largeStructs = largeStructs
        }.Schedule(TSIZE, 512).Complete();

        new LargeStructJobs.CopyHalfToHalf()
        {
            readStructs = halfStructs1,
            writeStructs = halfStructs2
        }.Schedule(TSIZE, 512).Complete();

        new LargeStructJobs.CopyToInJob()
        {
            readStructs = halfStructs1,
            writeStructs = halfStructs2
        }.Schedule().Complete();

        Profiler.BeginSample("Copy to outside of job");
        halfStructs1.CopyTo(halfStructs2);
        Profiler.EndSample();
        */

        new PixelJobs.SetNoiseHeightsJob()
        {
            heights = heights,
            size = SIZE,
            offset = Time.time * 20
        }.Schedule(TSIZE, 512).Complete();

        /*
        new PixelJobs.CopyHeightsToPixelsJob()
        {
            heights = heights,
            colors = colors,
            size = SIZE
        }.Schedule(TSIZE, 512).Complete();
        */

        Profiler.BeginSample("Texture Apply");
        texture.Apply(false);
        Profiler.EndSample();

    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, SIZE, SIZE), texture);
    }
}
