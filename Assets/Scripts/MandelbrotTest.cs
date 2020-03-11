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
    public bool updateOnGPU;

    public Material material;

    double[] positionBoundsArray;
    ComputeBuffer positionBoundsBuffer;

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

        positionBoundsBuffer = new ComputeBuffer(4, sizeof(double));
        positionBoundsArray = new double[4];
    }

    void OnDestroy()
    {
        Destroy(texture);
        positionBoundsBuffer.Dispose();
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

    bool lastInFrame;

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
            if (doublePrecision)
            {
                positionBoundsArray[0] = outpos.x;
                positionBoundsArray[1] = outpos.y;
                positionBoundsArray[2] = bounds.x;
                positionBoundsArray[3] = bounds.y;

                positionBoundsBuffer.SetData(positionBoundsArray);

                Shader.SetGlobalBuffer("_PositionBoundsDouble", positionBoundsBuffer);
            }
            else
            {
                Shader.SetGlobalVector("_PositionBounds", float4(float2(outpos), float2(bounds)));
            }

            Shader.SetGlobalInt("_FractalSteps", threshold);
        }
    }

    static string GetHex(double v)
    { return System.BitConverter.ToString(System.BitConverter.GetBytes(v)); }
    static string GetHex(int v)
    { return System.BitConverter.ToString(System.BitConverter.GetBytes(v)); }

    const string formatter = "{0,25:E16}{1,23:X16}";

    /*
    static uint4 double2_to_uint4(in double2 d)
    {
        return uint4(
            ulong_to_uint2(asulong(d.x)),
            ulong_to_uint2(asulong(d.y)));
    }*/

    static int4 double2_to_int4(in double2 d)
    {
        return int4(
            long_to_int2(tolong(d.x)),
            long_to_int2(tolong(d.y)));
    }

    static long tolong(double d)
    {
        return aslong(d);
        //return System.BitConverter.DoubleToInt64Bits(d);
    }

    static int2 long_to_int2(in long a)
    {
        return int2(
            (int)(a & uint.MaxValue),
            (int)(a >> 32));
    }

    /*
    static uint2 ulong_to_uint2(in ulong a)
    {
        return uint2(
            (uint)(a & uint.MaxValue),
            (uint)(a >> 32));
    }*/

    void UpdateCPU(double2 position, double2 bounds)
    {
        if (!finishJobInSameFrame || !lastInFrame)
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

        lastInFrame = finishJobInSameFrame;
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
