#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DDSReader.Internal;
using DDSReader.Internal.Decoders;
using DDSReader.Utilities;

#endregion

namespace DDSReader
{
    public class DDSReader
    {
        public static async Task<DDSImage> ReadImageAsync(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return await ReadImageAsync(stream);
            }
        }

        public static async Task<DDSHeader> ReadHeaderAsync(Stream stream)
        {
            var header = await stream.ReadStructAsync<DDSHeader>();

            if (header.dwMagic != Constants.DDSMagic)
            {
                throw new NotSupportedException("Provided stream is no DDS file!");
            }

            if (header.dwSize != Constants.HeaderSize)
            {
                throw new NotSupportedException("Invalid header size value!");
            }

            if (header.ddspf.dwSize != Constants.PixelformatSize)
            {
                throw new NotSupportedException("Invalid pixel format size value!");
            }

            return header;
        }

        public static async Task<DDSImage> ReadImageAsync(Stream stream)
        {
            var header = await ReadHeaderAsync(stream);

            var workImage = new DDSImage(header.Width(), header.Height());

            CheckHeaderFlags(workImage, header.dwFlags);

            var decoder = ChooseDecoder(header);

            var depth = header.TextureDepth();
            var width = header.Width();
            var height = header.Height();

            for (var mipmap = 0; mipmap < header.MipmapCount(); mipmap++)
            {
                var frameList = new List<byte[]>((int) depth);

                for (var depthNum = 0; depthNum < depth; depthNum++)
                {
                    frameList.Add(await decoder.DecodeFrame(stream, width, height));
                }

                workImage.AddFrame(new DDSMipMap(frameList, width, height));

                depth = Math.Max(1, depth / 2);
                width = Math.Max(1, width / 2);
                height = Math.Max(1, height / 2);
            }

            if (decoder == null)
            {
                throw new NotSupportedException("Unsupported texture format!");
            }

            return workImage;
        }

        private static void CheckHeaderFlags(DDSImage workImage, DDSD flags)
        {
            if (!flags.HasFlag(DDSD.CAPS))
            {
                workImage.AddMessage(new DDSMessage(DDSMessageType.Warning, "Header did not contain the CAPS flag!"));
            }
            if (!flags.HasFlag(DDSD.HEIGHT))
            {
                workImage.AddMessage(new DDSMessage(DDSMessageType.Warning, "Header did not contain the HEIGHT flag!"));
            }
            if (!flags.HasFlag(DDSD.WIDTH))
            {
                workImage.AddMessage(new DDSMessage(DDSMessageType.Warning, "Header did not contain the WIDTH flag!"));
            }
            if (!flags.HasFlag(DDSD.PIXELFORMAT))
            {
                workImage.AddMessage(new DDSMessage(DDSMessageType.Warning, "Header did not contain the PIXELFORMAT flag!"));
            }
        }

        private static IDataDecoder ChooseDecoder(DDSHeader header)
        {
            switch (header.GetPixelFormat())
            {
                case PixelFormat.ARGB:
                    break;
                case PixelFormat.RGB:
                    return new RGBDecoder(header);
                case PixelFormat.DXT1:
                    return new DXT1Decoder(header);
                case PixelFormat.DXT2:
                    return new DXT2Decoder(header);
                case PixelFormat.DXT3:
                    return new DXT3Decoder(header);
                case PixelFormat.DXT4:
                    return new DXT4Decoder(header);
                case PixelFormat.DXT5:
                    return new DXT5Decoder(header);
                case PixelFormat._3DC:
                    break;
                case PixelFormat.ATI1N:
                    break;
                case PixelFormat.LUMINANCE:
                    break;
                case PixelFormat.LUMINANCE_ALPHA:
                    break;
                case PixelFormat.RXGB:
                    break;
                case PixelFormat.A16B16G16R16:
                    break;
                case PixelFormat.R16F:
                    break;
                case PixelFormat.G16R16F:
                    break;
                case PixelFormat.A16B16G16R16F:
                    break;
                case PixelFormat.R32F:
                    break;
                case PixelFormat.G32R32F:
                    break;
                case PixelFormat.A32B32G32R32F:
                    break;
                case PixelFormat.UNKNOWN:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }
    }
}
