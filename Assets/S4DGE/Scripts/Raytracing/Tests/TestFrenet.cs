using NUnit.Framework;
using UnityEngine;
using S4DGE;

namespace RaytraceRenderer
{
    public class TestFrenet
    {
        [Test]
        public void TestBasicLine()
        {
            var line = new ParametricShape1D()
            {
                Divisions = 2,
                End = 1,
                Start = 0,
                Path = s =>
                {
                    Vector4 st = new(0, 0, 0, 0), ed = new(1, 0, 0, 0);
                    return s * ed + (1 - s) * st;
                }
            };


            Debug.Log(line.Path.FrenetFrame()(0.5f));
        }
    }
}