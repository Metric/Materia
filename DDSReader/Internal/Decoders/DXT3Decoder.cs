using System;
using System.IO;
using System.Threading.Tasks;
using DDSReader.Utilities;

namespace DDSReader.Internal.Decoders
{
    public class DXT3Decoder : DXTDecoder
    {
        private const int BlockSize = 16;


        public DXT3Decoder(DDSHeader header) : base(header)
        {
        }

        public override async Task<byte[]> DecodeFrame(Stream dataSource, uint width, uint height)
        {
            var compressedData = new byte[GetDataSize(width, height, BlockSize)];

            var readCount = await dataSource.ReadAsync(compressedData, 0, compressedData.Length);

            if (readCount != compressedData.Length)
            {
                throw new IOException("Not enough data!");
            }

            var scanlineSize = width * BytesPerPixel;

            var frameData = new byte[scanlineSize * height];

            Parallel.ForEach(RangeEnumerable.Range(0, (int)width, 4), new ParallelOptions(), y =>
            {
                var colors = new RGBAColor[4];

                var offset = (int)((y / 4) * (width / 4) * BlockSize);

                for (var x = 0; x < width; x += 4, offset+=BlockSize)
                {
                    var color0 = BitConverter.ToUInt16(compressedData, offset + 8);
                    var color1 = BitConverter.ToUInt16(compressedData, offset + 10);

                    var bitmask = BitConverter.ToUInt32(compressedData, offset + 12);

                    colors[0] = GetDXTColor(color0);
                    colors[1] = GetDXTColor(color1);

                    colors[2].b = (byte) ((2 * colors[0].b + colors[1].b + 1) / 3);
                    colors[2].g = (byte) ((2 * colors[0].g + colors[1].g + 1) / 3);
                    colors[2].r = (byte) ((2 * colors[0].r + colors[1].r + 1) / 3);

                    colors[3].b = (byte) ((colors[0].b + 2 * colors[1].b + 1) / 3);
                    colors[3].g = (byte) ((colors[0].g + 2 * colors[1].g + 1) / 3);
                    colors[3].r = (byte)((colors[0].r + 2 * colors[1].r + 1) / 3);

                    var k = 0;
                    for (var j = 0; j < 4; j++)
                    {
                        for (var i = 0; i < 4; i++, k++)
                        {
                            var select = (bitmask & (0x03 << k * 2)) >> k * 2;
                            var current = colors[select];

                            if (((x + i) >= width) || ((y + j) >= height))
                            {
                                continue;
                            }
                            
                            var dataOffset = (y + j) * scanlineSize + (x + i) * BytesPerPixel;
                            frameData[dataOffset + 0] = current.b;
                            frameData[dataOffset + 1] = current.g;
                            frameData[dataOffset + 2] = current.r;
                        }
                    }

                    for (var j = 0; j < 4; j++)
                    {
                        var word = compressedData[offset + 2 * j] + 256 * compressedData[offset + 2 * j + 1];
                        for (var i = 0; i < 4; i++)
                        {
                            if (((x + i) < width) && ((y + j) <height))
                            {
                                var dataOffset = (y + j) * scanlineSize + (x + i) * BytesPerPixel + 3;
                                frameData[dataOffset] = (byte) (word & 0x0F);
                                frameData[dataOffset] = (byte) (frameData[dataOffset] | (frameData[dataOffset] << 4));
                            }

                            word >>= 4;
                        }
                    }
                }
            });

            return frameData;
        }
    }
}