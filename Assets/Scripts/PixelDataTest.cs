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

    void Start()
    {
        texture = new Texture2D(SIZE, SIZE, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

        colors = texture.GetRawTextureData<Color32>();

        new PixelJobs.SetJob() { colors = colors }.Schedule(TSIZE, 512).Complete();

        texture.Apply(false);
    }

    private void Update()
    {
        new PixelJobs.SetJob() { colors = colors }.Schedule(TSIZE, 512).Complete();

        Profiler.BeginSample("Texture Apply");
        texture.Apply(false);
        Profiler.EndSample();
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, SIZE, SIZE), texture);
    }
}
