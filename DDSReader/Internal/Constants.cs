#region Usings

using System;

#endregion

namespace DDSReader.Internal
{
    // ReSharper disable InconsistentNaming
    [Flags]
    public enum DDSD : uint
    {
        CAPS = 0x1,

        HEIGHT = 0x2,

        WIDTH = 0x4,

        PITCH = 0x8,

        PIXELFORMAT = 0x1000,

        MIPMAPCOUNT = 0x20000,

        LINEARSIZE = 0x80000,

        DEPTH = 0x800000,
    }

    [Flags]
    public enum DDPF : uint
    {
        ALPHAPIXELS = 0x1,

        ALPHA = 0x2,

        FOURCC = 0x4,

        RGB = 0x40,

        YUV = 0x200,

        LUMINANCE = 0x20000,
    }

    public enum FourCCValue : uint
    {
        // This list was taken von DevIL sources

        DXT1 = 0x31545844,

        DXT2 = 0x32545844,

        DXT3 = 0x33545844,

        DXT4 = 0x34545844,

        DXT5 = 0x35545844,

        ATI1 = 0x31495441,

        ATI2 = 0x32495441,

        RXGB = 0x42475852,

        A16B16G16R16 = 0x24,

        R16F = 0x6F,

        G16R16F = 0x70,

        A16B16G16R16F = 0x71,

        R32F = 0x72,

        G32R32F = 0x73,

        A32B32G32R32F = 0x74,

        DX10 = 0x44583130,
    }

    // ReSharper restore InconsistentNaming

    public static class Constants
    {
        public const uint DDSMagic = 0x20534444;

        public const uint HeaderSize = 124;

        public const uint PixelformatSize = 32;
    }
}
