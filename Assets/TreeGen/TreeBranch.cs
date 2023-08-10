using RasterizationRenderer;
using System;
using System.Collections.Generic;
using UnityEngine;
using v2;

namespace TreeGen
{
    [Serializable]
    public class TreeBranch
    {
        public readonly List<TreeBranch> Children = new();
        public TransformMatrixAffine4D BaseTransform;

        public struct Segment
        {
            public TransformMatrixAffine4D Frame;
            public float Length;
            public float Offset;
            public float Rad0, Rad1;
        }

        public readonly List<Segment> Segments = new();
        public int Depth;

        public Func<float, float> RadiusFunction;
    }

    public static class TreeGenerator
    {
        public static void GrowSingleBranch(TreeBranch root, TreeGenParameters p, System.Random rng)
        {
            float splitError = 0.0f;
            float length = p.Scale.Randomize(p.ScaleV, rng) * p.Length.Randomize(p.LengthV, rng);
            float radius = length * p.Ratio * p.Scale.Randomize(p.ScaleV, rng);

            float radFunction(float s)
            {
                if (p.Taper is >= 0 and <= 1)
                {
                    float t = s / length;
                    float b = radius * (1 - p.Taper);
                    return b * t + radius * (1 - t);
                }
                throw new NotImplementedException();
            }

            root.RadiusFunction = radFunction;

            void growBranch(int level, TransformMatrixAffine4D curSegFrame, float currentOffset)
            {
                if (level >= p.CurveRes) return;

                var segmentLen = length / p.CurveRes;

                root.Segments.Add(new TreeBranch.Segment()
                {
                    Length = segmentLen,
                    Frame = curSegFrame,
                    Offset = currentOffset,
                    Rad0 = radFunction(currentOffset),
                    Rad1 = radFunction(currentOffset + segmentLen)
                });

                Vector4 currentDirection = curSegFrame.scaleAndRot.GetColumn(2).normalized;

                float declination = Mathf.Acos(currentDirection.z);
                

                int segSplitsEffective;
                if (level == 0)
                {
                    segSplitsEffective = p.BaseSplits;
                }
                else
                {
                    segSplitsEffective = Mathf.RoundToInt(splitError + p.SegSplits);
                    splitError -= segSplitsEffective - p.SegSplits;
                }

                if (segSplitsEffective == 0)
                {
                    // Straight line

                    float curveRot;
                    if (p.CurveBack == 0)
                    {
                        curveRot = p.Curve / p.CurveRes;
                    }
                    else
                    {
                        if (level < p.CurveRes / 2.0)
                        {
                            curveRot = p.Curve / (p.CurveRes / 2f);
                        }
                        else
                        {
                            curveRot = p.CurveBack / (p.CurveRes / 2f);
                        }
                    }

                    curveRot = curveRot.Randomize(p.CurveV, rng);
                    curveRot *= Mathf.Deg2Rad;

                    TransformMatrixAffine4D subFrame = new()
                    {
                        scaleAndRot = TransformMatrixAffine4D.RotationMatrix(0, 2, curveRot),
                        translation = new(0, 0, segmentLen, 0),
                    };

                    growBranch(level + 1, curSegFrame * subFrame, currentOffset + segmentLen);
                }
                else
                {
                    // Split branch
                    float splitAngle = p.SplitAngle.Randomize(p.SplitAngleV, rng);
                    splitAngle *= Mathf.Deg2Rad;

                    Matrix4x4 curRot = TransformMatrixAffine4D.RotationMatrix(0, 2, splitAngle);

                    float sidewaysAngle =
                        40 + 0.75f * (30 + Mathf.Abs(declination)) * Mathf.Pow((float)rng.NextDouble(), 2);
                    sidewaysAngle *= Mathf.Deg2Rad;

                    Matrix4x4 sidewaysRot = TransformMatrixAffine4D.RotationMatrix(0, 1, sidewaysAngle);

                    for (int cloneNum = 0; cloneNum <= segSplitsEffective; cloneNum++)
                    {
                        curRot = sidewaysRot * curRot;
                        TransformMatrixAffine4D subFrame = new()
                        {
                            scaleAndRot = curRot,
                            translation = new(0, 0, segmentLen, 0),
                        };

                        growBranch(level + 1, curSegFrame * subFrame, currentOffset + segmentLen);


                    }
                }


            }

            growBranch(0, root.BaseTransform, 0);

        }

        public static void Grow(TreeBranch root, Func<int, TreeGenParameters> parms, System.Random rng)
        {

            TreeGenParameters p = parms(root.Depth);

            float splitError = 0.0f;
            float length = p.Scale.Randomize(p.ScaleV, rng) * p.Length.Randomize(p.LengthV, rng);



        }

        public static void Render(TreeBranch root, TetMesh_raw mesh)
        {

            foreach (var segment in root.Segments)
            {
                var a = segment.Frame.translation;
                var b = a + segment.Length * segment.Frame.scaleAndRot.GetColumn(2);
                ParametricShape1D segmentParametric = new ParametricShape1D()
                {
                    Divisions = 1,
                    Start =0,
                    End = 1,
                    Path = s =>
                    {
                        return s * b + (1 - s) * a;
                    }
                };

                Matrix4x4 frame = segment.Frame.scaleAndRot;
                var tmp = frame.GetColumn(2);
                frame.SetColumn(2, frame.GetColumn(0));
                frame.SetColumn(0, tmp);

                ParametricShape3D manifold3 = ManifoldConverter.HyperCylinderify(
                    segmentParametric,
                    s => s*segment.Rad1 + (1-s)*segment.Rad0,
                    s => frame
                );

                TetMesh4D cylinder = MeshGenerator4D.GenerateTetMesh(manifold3);
                mesh.Append(cylinder);
            }

            foreach (var child in root.Children)
            {
                Render(child, mesh);
            }
        }
    }
}