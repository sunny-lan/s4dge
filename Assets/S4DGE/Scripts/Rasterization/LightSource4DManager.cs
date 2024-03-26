using RasterizationRenderer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightSource4DManager
{
    List<LightSource4D> _lightSources;
    public List<LightSource4D> LightSources
    {
        get { return _lightSources; }
        internal set { _lightSources = value; }
    }
    ComputeBuffer _lightSourceBuffer;
    public ComputeBuffer LightSourceBuffer { get => _lightSourceBuffer; }

    public int Count
    {
        get => _lightSources.Count;
    }

    public LightSource4DManager(List<LightSource4D> lightSources)
    {
        this._lightSources = lightSources;
    }

    public void Add(LightSource4D lightSource)
    {
        _lightSources.Add(lightSource);
    }

    public void UpdateComputeBuffer()
    {
        var lightSourceArr = _lightSources.Select(source => source.Data).ToArray();

        if (_lightSourceBuffer != null)
        {
            _lightSourceBuffer.Release();
        }

        _lightSourceBuffer = RenderUtils.InitComputeBuffer(LightSource4D.ShaderData.SizeBytes,
            lightSourceArr);
    }
}