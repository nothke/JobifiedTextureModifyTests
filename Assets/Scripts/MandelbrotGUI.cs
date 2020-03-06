using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nothke.ProtoGUI;

public class MandelbrotGUI : WindowGUI
{
    public override string WindowLabel => "";

    public MandelbrotTest m;

    string[] cpugpuText = { "CPU", "GPU" };
    string[] precisionText = { "Float", "Double" };

    private void Start()
    {
        draggable = false;
        windowRect.x = 10;
        windowRect.y = 10;
        labelWidth = 60;
        sliderNumberWidth = 30;
    }
    protected override void Window()
    {
        m.updateOnGPU = GUILayout.SelectionGrid(m.updateOnGPU ? 1 : 0, cpugpuText, 2) == 1;
        m.doublePrecision = GUILayout.SelectionGrid(m.doublePrecision ? 1 : 0, precisionText, 2) == 1;
        if (m.updateOnGPU) GUI.enabled = false;
        m.finishJobInSameFrame = !ToggleButton("Melt My CPU", !m.finishJobInSameFrame);
        if (m.updateOnGPU) GUI.enabled = true;
        Slider("Steps", ref m.threshold, 10, 400);
    }
}
