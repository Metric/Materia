using System;
using System.Runtime.InteropServices;

namespace DDSReader.Internal
{
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct DDSPixelFormat
    {
        public readonly uint dwSize;

        public readonly DDPF dwFlags;

        public readonly FourCCValue dwFourCC;

        public readonly uint dwRGBBitCount;

        public readonly uint dwRBitMask;

        public readonly uint dwGBitMask;

        public readonly uint dwBBitMask;

        public readonly uint dwABitMask;
    }
}