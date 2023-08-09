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
        }

        public readonly List<Segment> Segments = new();
        public int Depth;
    }

    public static class TreeGenerator
    {
        public static void GrowSingleBranch(TreeBranch root, TreeGenParameters p, System.Random rng)
        {
            float splitError = 0.0f;
            float length = p.Scale.Randomize(p.ScaleV, rng) * p.Length.Randomize(p.LengthV, rng);

            void growBranch(int level, TransformMatrixAffine4D curSegFrame, float currentOffset)
            {
                if (level >= p.CurveRes) return;

                root.Segments.Add(new TreeBranch.Segment()
                {
                    Length = length,
                    Frame = curSegFrame,
                    Offset = currentOffset,
                });

                Vector4 currentDirection = curSegFrame.scaleAndRot.GetColumn(3).normalized;

                float declination = Mathf.Acos(currentDirection.z);

                Vector4 endOfSegment = curSegFrame.translation + currentDirection * length;

                int segSplitsEffective = Mathf.RoundToInt(splitError + p.SegSplits);
                splitError -= segSplitsEffective - p.SegSplits;

                if (segSplitsEffective == 1)
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

                    TransformMatrixAffine4D subFrame = new()
                    {
                        scaleAndRot = TransformMatrixAffine4D.RotationMatrix(0, 3, curveRot),
                        translation = new(0, 0, 0, length),
                    };

                    growBranch(level + 1, curSegFrame * subFrame, currentOffset+length);
                }
                else
                {
                    // Split branch
                    float splitAngle = p.SplitAngle + p.SplitAngleV * (float)(rng.NextDouble() * 2 - 1);

                    Matrix4x4 curRot = TransformMatrixAffine4D.RotationMatrix(0, 3, splitAngle);

                    float sidewaysAngle = Mathf.Deg2Rad *
                        (20 + 0.75f * (30 + Mathf.Abs(declination)) * Mathf.Pow((float)rng.NextDouble(), 2));

                    Matrix4x4 sidewaysRot = TransformMatrixAffine4D.RotationMatrix(2, 3, sidewaysAngle);

                    for (int cloneNum = 0; cloneNum < segSplitsEffective; cloneNum++)
                    {
                        TransformMatrixAffine4D subFrame = new()
                        {
                            scaleAndRot = curRot,
                            translation = new(0, 0, 0, length),
                        };

                        growBranch(level + 1, curSegFrame * subFrame, currentOffset + length);

                        curRot = sidewaysRot * curRot;

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

            foreach(var segment in root.Segments)
            {
                var a = segment.Frame.translation;
                var b = a + segment.Length * segment.Frame.scaleAndRot.GetColumn(3);
                ParametricShape1D segmentParametric = new ParametricShape1D()
                {
                    Divisions=2,
                    Start=0,
                    End=1,
                    Path = s =>
                    {
                        return s * b + (1 - s) * a;
                    }
                };

                Matrix4x4 frame = segment.Frame.scaleAndRot;
                var tmp = frame.GetColumn(3);
                frame.SetColumn(3, frame.GetColumn(0));
                frame.SetColumn(0, tmp);

                ParametricShape3D manifold3 = ManifoldConverter.HyperCylinderify(
                    segmentParametric,
                    s => 0.3f,
                    s => frame
                );

                TetMesh4D cylinder = MeshGenerator4D.GenerateTetMesh(manifold3);
                mesh.Append(cylinder);
            }

            foreach(var child in root.Children)
            {
                Render(child, mesh);
            }
        }
    }
}