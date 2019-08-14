using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Math3D
{
    public static class MatrixExtensions
    {
        public static void FromArray(this Matrix2 m, float[] arr)
        {
            if (arr.Length != 4) return;

            m.M11 = arr[0];
            m.M12 = arr[1];
            m.M21 = arr[2];
            m.M22 = arr[3];
        }

        public static void FromArray(this Matrix3 m, float[] arr)
        {
            if (arr.Length != 9) return;

            m.M11 = arr[0];
            m.M12 = arr[1];
            m.M13 = arr[2];

            m.M21 = arr[3];
            m.M22 = arr[4];
            m.M23 = arr[5];

            m.M31 = arr[6];
            m.M32 = arr[7];
            m.M33 = arr[8];
        }

        public static void FromArray(this Matrix4 m, float[] arr)
        {
            if (arr.Length != 16) return;
            m.M11 = arr[0];
            m.M12 = arr[1];
            m.M13 = arr[2];
            m.M14 = arr[3];

            m.M21 = arr[4];
            m.M22 = arr[5];
            m.M23 = arr[6];
            m.M24 = arr[7];

            m.M31 = arr[8];
            m.M32 = arr[9];
            m.M33 = arr[10];
            m.M34 = arr[11];

            m.M41 = arr[12];
            m.M42 = arr[13];
            m.M43 = arr[14];
            m.M44 = arr[15];
        }

        public static float[] ToArray(this Matrix2 m)
        {
            float[] f = new float[4];

            f[0] = m.M11;
            f[1] = m.M12;
            f[2] = m.M21;
            f[3] = m.M22;

            return f;
        }

        public static float[] ToArray(this Matrix3 m)
        {
            float[] f = new float[9];

            f[0] = m.M11;
            f[1] = m.M12;
            f[2] = m.M13;
            f[3] = m.M21;
            f[4] = m.M22;
            f[5] = m.M23;
            f[6] = m.M31;
            f[7] = m.M32;
            f[8] = m.M33;

            return f;
        }

        public static float[] ToArray(this Matrix4 m)
        {
            float[] f = new float[16];

            f[0] = m.M11;
            f[1] = m.M12;
            f[2] = m.M13;
            f[3] = m.M14;
            f[4] = m.M21;
            f[5] = m.M22;
            f[6] = m.M23;
            f[7] = m.M24;
            f[8] = m.M31;
            f[9] = m.M32;
            f[10] = m.M33;
            f[11] = m.M34;
            f[12] = m.M41;
            f[13] = m.M42;
            f[14] = m.M43;
            f[15] = m.M44;

            return f;
        }
    }
}
