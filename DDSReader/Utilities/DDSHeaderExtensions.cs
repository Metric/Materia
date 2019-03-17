using System;
using DDSReader.Internal;

namespace DDSReader.Utilities
{
    public static class DDSHeaderExtensions
    {
        public static bool Is3DTexture(this DDSHeader header)
        {
            return header.dwFlags.HasFlag(DDSD.DEPTH);
        }

        public static bool HasMipmaps(this DDSHeader header)
        {
            return header.dwFlags.HasFlag(DDSD.MIPMAPCOUNT);
        }

        public static uint MipmapCount(this DDSHeader header)
        {
            return !header.HasMipmaps() ? 1 : header.dwMipMapCount;
        }

        public static uint TextureDepth(this DDSHeader header)
        {
            return !header.Is3DTexture() ? 1 : header.dwDepth;
        }

        public static uint Width(this DDSHeader header)
        {
            return header.dwWidth;
        }

        public static uint Height(this DDSHeader header)
        {
            return header.dwHeight;
        }

        public static PixelFormat GetPixelFormat(this DDSHeader header)
        {
            if (header.ddspf.dwFlags.HasFlag(DDPF.FOURCC))
            {
                switch (header.ddspf.dwFourCC)
                {
                    case FourCCValue.DXT1:
                        return PixelFormat.DXT1;
                    case FourCCValue.DXT2:
                        return PixelFormat.DXT2;
                    case FourCCValue.DXT3:
                        return PixelFormat.DXT3;
                    case FourCCValue.DXT4:
                        return PixelFormat.DXT4;
                    case FourCCValue.DXT5:
                        return PixelFormat.DXT5;
                    case FourCCValue.ATI1:
                        return PixelFormat.ATI1N;
                    case FourCCValue.ATI2:
                        return PixelFormat._3DC;
                    case FourCCValue.RXGB:
                        return PixelFormat.RXGB;
                    case FourCCValue.A16B16G16R16:
                        return PixelFormat.A16B16G16R16;
                    case FourCCValue.R16F:
                        return PixelFormat.R16F;
                    case FourCCValue.G16R16F:
                        return PixelFormat.G16R16F;
                    case FourCCValue.A16B16G16R16F:
                        return PixelFormat.A16B16G16R16F;
                    case FourCCValue.R32F:
                        return PixelFormat.R32F;
                    case FourCCValue.G32R32F:
                        return PixelFormat.G32R32F;
                    case FourCCValue.A32B32G32R32F:
                        return PixelFormat.A32B32G32R32F;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                if (header.ddspf.dwFlags.HasFlag(DDPF.LUMINANCE))
                {
                    if (header.ddspf.dwFlags.HasFlag(DDPF.ALPHAPIXELS))
                    {
                         return PixelFormat.LUMINANCE_ALPHA;
                    }
                    else
                    {
                        return PixelFormat.LUMINANCE;
                    }
                }
                else
                {

                    if (header.ddspf.dwFlags.HasFlag(DDPF.ALPHAPIXELS))
                    {
                        return PixelFormat.ARGB;
                    }
                    else
                    {
                        return PixelFormat.RGB;
                    }
                }
            }
        }
    }
}
