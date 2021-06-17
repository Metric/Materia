using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Materia.Graph.IO
{
    internal static class Utils
    {
        public static unsafe float ToSingle(this ArraySegment<byte> buffer, int offset = 0)
        {
            return buffer.Array.ToSingle(buffer.Offset + offset);
        }

        public static unsafe float ToSingle(this byte[] arr, int offset = 0)
        {
            fixed(byte* ptr = &arr[offset])
            {
                return *((float*)(int*)ptr);
            }
        }

        public static unsafe byte[] GetBytes(this float value)
        {
            byte[] bytes = new byte[sizeof(float)];
            fixed (byte* b = bytes) 
            {
                *((int*)b) = *(int*)&value;
            }
            return bytes;
        }
    }
}
