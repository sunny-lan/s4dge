using System;

namespace TreeGen
{
    [Serializable]
    public class TreeGenParameters
    {
        public int CurveRes;
        public float Curve, CurveBack;
        public float CurveV;

        public float SegSplits;
        public int BaseSplits;

        public float Length, LengthV;
        public float Scale, ScaleV;

        public float SplitAngle, SplitAngleV;

        public float Ratio;
        public float Taper;
        
    }
}