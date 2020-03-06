using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using Unity.Collections;
using Unity.Jobs;

using Unity.Mathematics;
using static Unity.Mathematics.math;

public class MandelbrotTest : MonoBehaviour
{
    public Texture2D texture;

    public int threshold;
    public float gain;

    const int SIZE = 1024;
    const int TSIZE = SIZE * SIZE;

    NativeArray<Color32> colors;

    public float bandTest = 0;

    public bool finishJobInSameFrame = false;
    public bool doublePrecision = false;

    void Start()
    {
        texture = new Texture2D(SIZE, SIZE,
            UnityEngine.Experimental.Rendering.DefaultFormat.LDR,
            UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

        colors = texture.GetRawTextureData<Color32>();

        new PixelJobs.SetJob() { colors = colors }.Schedule(TSIZE, 512).Complete();

        texture.Apply(false);
    }

    int zoomLevel = 0;


    float zoom;
    float smoothZoomLevel;

    double2 position;
    double2 bounds;

    double2 lastMousePos;

    JobHandle schedule;

    private void Update()
    {
        zoomLevel += (int)Input.mouseScrollDelta.y;
        smoothZoomLevel = Mathf.Lerp(smoothZoomLevel, zoomLevel, Time.deltaTime * 4);
        zoom = Mathf.Exp(-smoothZoomLevel) * 0.1f;

        bounds = zoom;

        float2 mouse = (Vector2)Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            double2 diff = mouse - lastMousePos;
            position -= diff / SIZE * zoom;
        }
        lastMousePos = mouse;

        double2 outpos = position - bounds / 2;

        if (!finishJobInSameFrame)
            schedule.Complete();

        if (doublePrecision)
        {
            schedule = new Mandelbrot.JobDouble()
            {
                colors = colors,
                size = SIZE,
                bounds = bounds,
                position = outpos,
                threshold = threshold,
                gain = gain
            }.Schedule(TSIZE, 512);
        }
        else
        {
            schedule = new Mandelbrot.Job()
            {
                colors = colors,
                size = SIZE,
                bounds = (float2)bounds,
                position = (float2)outpos,
                threshold = threshold,
                gain = gain
            }.Schedule(TSIZE, 512);
        }

        if (finishJobInSameFrame)
        {
            schedule.Complete();
        }

        Profiler.BeginSample("Texture Apply");
        texture.Apply(false);
        Profiler.EndSample();
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, SIZE, SIZE), texture);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < 100; i++)
        {
            float x = (float)i / 100;
            float y = Band(bandTest, gain, x);
            Gizmos.DrawRay(new Vector3(x, 0, 0), Vector3.up * y);
        }
    }

    static float Band(float start, float width, float t)
    {
        return Unity.Mathematics.math.saturate(1 - Unity.Mathematics.math.abs((-start + t) / width));
    }
}
