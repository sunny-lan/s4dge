using RasterizationRenderer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightSource4DManager
{
    List<LightSource4D> lightSources;
    ComputeBuffer _lightSourceBuffer;
    public ComputeBuffer LightSourceBuffer
    {
        get
        {
            UpdateComputeBuffer();
            return _lightSourceBuffer;
        }
        private set => _lightSourceBuffer = value;
    }

    public int Count
    {
        get => lightSources.Count;
    }

    bool dirty;

    public LightSource4DManager(List<LightSource4D> lightSources)
    {
        this.lightSources = lightSources;
        dirty = false;
    }

    public void Add(LightSource4D lightSource)
    {
        lightSources.Add(lightSource);
        dirty = true;
    }

    void UpdateComputeBuffer()
    {
        if (dirty)
        {
            if (_lightSourceBuffer != null)
            {
                _lightSourceBuffer.Release();
            }

            _lightSourceBuffer = RenderUtils.InitComputeBuffer(LightSource4D.Data.SizeBytes, lightSources.Select(source => source.data).ToArray());

            dirty = false;
        }
    }
}