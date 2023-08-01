using RasterizationRenderer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightSource4DManager
{
    List<LightSource4D> lightSources;
    ComputeBuffer _lightSourceBuffer;
    public ComputeBuffer LightSourceBuffer { get => _lightSourceBuffer; }

    public int Count
    {
        get => lightSources.Count;
    }

    public LightSource4DManager(List<LightSource4D> lightSources)
    {
        this.lightSources = lightSources;
    }

    public void Add(LightSource4D lightSource)
    {
        lightSources.Add(lightSource);
    }

    public void UpdateComputeBuffer()
    {
        var lightSourceArr = lightSources.Select(source => source.data).ToArray();

        if (_lightSourceBuffer != null)
        {
            _lightSourceBuffer.Release();
        }

        _lightSourceBuffer = RenderUtils.InitComputeBuffer(LightSource4D.Data.SizeBytes,
            lightSourceArr);
    }
}