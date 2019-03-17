#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace DDSReader.Internal
{
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct DDSHeader
    {
        public readonly uint dwMagic;

        public readonly uint dwSize;

        public readonly DDSD dwFlags;

        public readonly uint dwHeight;

        public readonly uint dwWidth;

        public readonly uint dwPitchOrLinearSize;

        public readonly uint dwDepth;

        public readonly uint dwMipMapCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly uint[] dwReserved1;

        public readonly DDSPixelFormat ddspf;

        public readonly uint dwCaps;

        public readonly uint dwCaps2;

        public readonly uint dwCaps3;

        public readonly uint dwCaps4;

        public readonly uint dwReserved2;
    }
}
