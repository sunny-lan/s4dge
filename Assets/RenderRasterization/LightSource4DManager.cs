using RasterizationRenderer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using v2;

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

    void UpdateComputeBuffer(Matrix4x4 worldToCameraScaleAndRot, Vector4 worldToCameraTranslation)
    {
        var lightSourceArr = lightSources.Select(source => source.data).ToArray();

        if (_lightSourceBuffer != null)
        {
            _lightSourceBuffer.Release();
        }

        _lightSourceBuffer = RenderUtils.InitComputeBuffer(LightSource4D.Data.SizeBytes,
            lightSourceArr.Select(light => new LightSource4D.Data(worldToCameraScaleAndRot * light.position + worldToCameraTranslation)).ToArray());
    }

    public void UpdateTransform(TransformMatrixAffine4D worldToCameraTransform)
    {
        UpdateComputeBuffer(worldToCameraTransform.scaleAndRot, worldToCameraTransform.translation);
    }
}