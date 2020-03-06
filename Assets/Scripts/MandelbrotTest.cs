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

    public Material material;

    public bool updateOnGPU;

    void Start()
    {
        texture = new Texture2D(SIZE, SIZE,
            UnityEngine.Experimental.Rendering.DefaultFormat.LDR,
            UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

        texture.wrapMode = TextureWrapMode.Clamp;

        colors = texture.GetRawTextureData<Color32>();

        new PixelJobs.SetJob() { colors = colors }.Schedule(TSIZE, 512).Complete();

        texture.Apply(false);

        material.mainTexture = texture;
    }

    void OnDestroy()
    {
        Destroy(texture);
    }

    int zoomLevel = -4;
    float zoom;
    float smoothZoomLevel = -4;

    public double2 position;
    public double2 bounds;

    double2 lastMousePos;

    JobHandle schedule;

    bool lastUpdateOnGPU;
    bool lastPrecision;

    public Renderer gpuQuad;
    public Renderer cpuQuad;

    private void Update()
    {
        zoomLevel += (int)Input.mouseScrollDelta.y;
        smoothZoomLevel = Mathf.Lerp(smoothZoomLevel, zoomLevel, Time.deltaTime * 4);
        zoom = Mathf.Exp(-smoothZoomLevel) * 0.1f;

        bounds = zoom;

        float2 mouse = (Vector2)Input.mousePosition;
        if (Input.GetMouseButton(0) && !Nothke.ProtoGUI.GameWindow.IsMouseOverUI())
        {
            double2 diff = mouse - lastMousePos;
            position -= diff / SIZE * zoom;
        }
        lastMousePos = mouse;

        double2 outpos = position - bounds / 2;

        if (Input.GetKeyDown(KeyCode.Space))
            updateOnGPU = !updateOnGPU;

        if (updateOnGPU != lastUpdateOnGPU)
        {
            gpuQuad.enabled = updateOnGPU;
            cpuQuad.enabled = !updateOnGPU;
        }

        lastUpdateOnGPU = updateOnGPU;

        if (doublePrecision != lastPrecision)
        {
            if (doublePrecision)
                Shader.EnableKeyword("DOUBLE_PRECISION");
            else
                Shader.DisableKeyword("DOUBLE_PRECISION");
        }

        lastPrecision = doublePrecision;

        if (!updateOnGPU)
        {
            UpdateCPU(outpos, bounds);
        }
        else
        {
            Shader.SetGlobalVector("_PositionBounds", float4(float2(outpos), float2(bounds)));
        }
    }

    void UpdateCPU(double2 position, double2 bounds)
    {
        if (!finishJobInSameFrame)
            schedule.Complete();

        if (doublePrecision)
        {
            schedule = new Mandelbrot.JobDouble()
            {
                colors = colors,
                size = SIZE,
                bounds = bounds,
                position = position,
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
                position = (float2)position,
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
        return saturate(1 - abs((-start + t) / width));
    }
}
