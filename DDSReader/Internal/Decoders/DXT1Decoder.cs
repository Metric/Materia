#region Usings

using System;
using System.IO;
using System.Threading.Tasks;
using DDSReader.Utilities;

#endregion

namespace DDSReader.Internal.Decoders
{
    public class DXT1Decoder : DXTDecoder
    {
        private const int BlockSize = 8;

        public DXT1Decoder(DDSHeader header) : base(header)
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

            Parallel.ForEach(RangeEnumerable.Range(0, (int) height, 4), new ParallelOptions(),y =>
            {
                var colors = new RGBAColor[4];
                colors[0].a = 0xFF;
                colors[1].a = 0xFF;
                colors[2].a = 0xFF;

                var offset = (int) ((y / 4) * (width / 4) * BlockSize);

                for (var x = 0; x < width; x += 4, offset+=BlockSize)
                {
                    var color0 = BitConverter.ToUInt16(compressedData, offset);
                    var color1 = BitConverter.ToUInt16(compressedData, offset + 2);

                    var bitmask = BitConverter.ToUInt32(compressedData, offset + 4);

                    colors[0] = GetDXTColor(color0);
                    colors[1] = GetDXTColor(color1);

                    if (color0 > color1)
                    {
                        // Four-color block: derive the other two colors.
                        // 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
                        // These 2-bit codes correspond to the 2-bit fields 
                        // stored in the 64-bit block.
                        colors[2].b = (byte) ((2 * colors[0].b + colors[1].b + 1) / 3);
                        colors[2].g = (byte) ((2 * colors[0].g + colors[1].g + 1) / 3);
                        colors[2].r = (byte) ((2 * colors[0].r + colors[1].r + 1) / 3);
                        //colours[2].a = 0xFF;

                        colors[3].b = (byte) ((colors[0].b + 2 * colors[1].b + 1) / 3);
                        colors[3].g = (byte) ((colors[0].g + 2 * colors[1].g + 1) / 3);
                        colors[3].r = (byte) ((colors[0].r + 2 * colors[1].r + 1) / 3);
                        colors[3].a = 0xFF;
                    }
                    else
                    {
                        // Three-color block: derive the other color.
                        // 00 = color_0,  01 = color_1,  10 = color_2,
                        // 11 = transparent.
                        // These 2-bit codes correspond to the 2-bit fields 
                        // stored in the 64-bit block. 
                        colors[2].b = (byte) ((colors[0].b + colors[1].b) / 2);
                        colors[2].g = (byte) ((colors[0].g + colors[1].g) / 2);
                        colors[2].r = (byte) ((colors[0].r + colors[1].r) / 2);
                        //colours[2].a = 0xFF;

                        colors[3].b = (byte) ((colors[0].b + 2 * colors[1].b + 1) / 3);
                        colors[3].g = (byte) ((colors[0].g + 2 * colors[1].g + 1) / 3);
                        colors[3].r = (byte) ((colors[0].r + 2 * colors[1].r + 1) / 3);
                        colors[3].a = 0x00;
                    }

                    var k = 0;
                    for (var j = 0; j < 4; j++)
                    {
                        for (var i = 0; i < 4; i++, k++)
                        {
                            // Complicated way of saying get the two bits at index k * 2 in bitmask
                            var select = (uint) ((bitmask & (0x03 << k * 2)) >> k * 2);
                            var col = colors[select];

                            if (((x + i) >= width) || ((y + j) >= height))
                            {
                                continue;
                            }

                            var dataOffset = (y + j) * scanlineSize + (x + i) * BytesPerPixel;
                            frameData[dataOffset + 0] = col.b;
                            frameData[dataOffset + 1] = col.g;
                            frameData[dataOffset + 2] = col.r;
                            frameData[dataOffset + 3] = col.a;
                        }
                    }
                }
            });

            return frameData;
        }
    }
}
