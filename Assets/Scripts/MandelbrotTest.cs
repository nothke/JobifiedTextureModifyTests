using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using Unity.Collections;
using Unity.Jobs;

public class MandelbrotTest : MonoBehaviour
{
    public Texture2D texture;

    public Rect rect;
    public int threshold;
    public float gain;

    const int SIZE = 1024;
    const int TSIZE = SIZE * SIZE;

    NativeArray<Color32> colors;

    public float bandTest = 0;

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

    float x;
    float y;
    Vector2 lastMousePos;

    float zoom;
    float smoothZoomLevel;

    JobHandle schedule;

    private void Update()
    {
        zoomLevel += (int)Input.mouseScrollDelta.y;
        smoothZoomLevel = Mathf.Lerp(smoothZoomLevel, zoomLevel, Time.deltaTime * 4);
        zoom = Mathf.Exp(-smoothZoomLevel) * 0.1f;

        rect.size = Vector2.one * zoom;
        rect.position = new Vector2(x, y) - rect.size / 2;

        Vector2 mouse = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            Vector2 diff = mouse - lastMousePos;
            x -= diff.x / SIZE * zoom;
            y -= diff.y / SIZE * zoom;
        }
        lastMousePos = mouse;
        
        schedule.Complete();

        Profiler.BeginSample("Texture Apply");
        texture.Apply(false);
        Profiler.EndSample();

        schedule = new Mandelbrot.Job()
        {
            colors = colors,
            size = SIZE,
            bounds = rect.size,
            position = rect.position,
            threshold = threshold,
            gain = gain
        }.Schedule(TSIZE, 512);
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
