using System;
using System.Collections.Generic;
using Materia.Rendering.Imaging;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Extensions;

namespace Materia.Nodes.Helpers
{
    public static class Utils
    {
        public static float CubicInterp(float v0, float v1, float v2, float v3, float t)
        {
            float a0, a1, a2, a3, mu2;
            mu2 = t * t;
            a0 = v3 - v2 - v0 + v1;
            a1 = v0 - v1 - a0;
            a2 = v2 - v0;
            a3 = v1;

            return (a0 * t * mu2 + a1 * mu2 + a2 * t + a3);
        }

        public static float CosineInterp(float v0, float v1, float t)
        {
            float mu2 = (1.0f - MathF.Cos(t * MathF.PI)) * 0.5f;
            return (v0 * (1.0f - mu2) + v1 * mu2);
        }


        public static float Rand(ref MVector vec2)
        {
            return MathF.Sin(MVector.Dot(vec2, new MVector(12.9898f, 78.233f)) * 43758.5453f).Fract() * 2.0f - 1.0f;
        }
    }
}
