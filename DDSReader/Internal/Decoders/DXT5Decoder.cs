#region Usings

using System;
using System.IO;
using System.Threading.Tasks;
using DDSReader.Utilities;

#endregion

namespace DDSReader.Internal.Decoders
{
    public class DXT5Decoder : DXTDecoder
    {
        private const int BlockSize = 16;

        public DXT5Decoder(DDSHeader header) : base(header)
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

            Parallel.ForEach(RangeEnumerable.Range(0, (int) width, 4), new ParallelOptions(), y =>
            {
                var colors = new RGBAColor[4];
                var alphas = new byte[8];

                var offset = (int) ((y / 4) * (width / 4) * BlockSize);

                for (var x = 0; x < width; x += 4, offset += BlockSize)
                {
                    alphas[0] = compressedData[offset];
                    alphas[1] = compressedData[offset + 1];
                    var alphamaskIndex = offset + 2;

                    colors[0] = GetDXTColor(BitConverter.ToUInt16(compressedData, offset + 8));
                    colors[1] = GetDXTColor(BitConverter.ToUInt16(compressedData, offset + 10));

                    var bitmask = BitConverter.ToUInt32(compressedData, offset + 12);

                    colors[2].b = (byte) ((2 * colors[0].b + colors[1].b + 1) / 3);
                    colors[2].g = (byte) ((2 * colors[0].g + colors[1].g + 1) / 3);
                    colors[2].r = (byte) ((2 * colors[0].r + colors[1].r + 1) / 3);

                    colors[3].b = (byte) ((colors[0].b + 2 * colors[1].b + 1) / 3);
                    colors[3].g = (byte) ((colors[0].g + 2 * colors[1].g + 1) / 3);
                    colors[3].r = (byte) ((colors[0].r + 2 * colors[1].r + 1) / 3);

                    var k = 0;
                    for (var j = 0; j < 4; j++)
                    {
                        for (var i = 0; i < 4; i++, k++)
                        {
                            var select = (bitmask & (0x03 << k * 2)) >> k * 2;
                            var col = colors[select];

                            // only put pixels out < width or height
                            if (((x + i) >= width) || ((y + j) >= height))
                            {
                                continue;
                            }

                            var dataOffset = (y + j) * scanlineSize + (x + i) * BytesPerPixel;
                            frameData[dataOffset + 0] = col.b;
                            frameData[dataOffset + 1] = col.g;
                            frameData[dataOffset + 2] = col.r;
                        }
                    }

                    // 8-alpha or 6-alpha block?    
                    if (alphas[0] > alphas[1])
                    {
                        // 8-alpha block:  derive the other six alphas.    
                        // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                        alphas[2] = (byte) ((6 * alphas[0] + 1 * alphas[1] + 3) / 7); // bit code 010
                        alphas[3] = (byte) ((5 * alphas[0] + 2 * alphas[1] + 3) / 7); // bit code 011
                        alphas[4] = (byte) ((4 * alphas[0] + 3 * alphas[1] + 3) / 7); // bit code 100
                        alphas[5] = (byte) ((3 * alphas[0] + 4 * alphas[1] + 3) / 7); // bit code 101
                        alphas[6] = (byte) ((2 * alphas[0] + 5 * alphas[1] + 3) / 7); // bit code 110
                        alphas[7] = (byte) ((1 * alphas[0] + 6 * alphas[1] + 3) / 7); // bit code 111
                    }
                    else
                    {
                        // 6-alpha block.
                        // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                        alphas[2] = (byte) ((4 * alphas[0] + 1 * alphas[1] + 2) / 5); // Bit code 010
                        alphas[3] = (byte) ((3 * alphas[0] + 2 * alphas[1] + 2) / 5); // Bit code 011
                        alphas[4] = (byte) ((2 * alphas[0] + 3 * alphas[1] + 2) / 5); // Bit code 100
                        alphas[5] = (byte) ((1 * alphas[0] + 4 * alphas[1] + 2) / 5); // Bit code 101
                        alphas[6] = 0x00; // Bit code 110
                        alphas[7] = 0xFF; // Bit code 111
                    }

                    // Note: Have to separate the next two loops,
                    //	it operates on a 6-byte system.

                    // First three bytes
                    //bits = *((ILint*)alphamask);
                    var bits = (compressedData[alphamaskIndex]) | (compressedData[alphamaskIndex + 1] << 8) |
                               (compressedData[alphamaskIndex + 2] << 16);
                    for (var j = 0; j < 2; j++)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            // only put pixels out < width or height
                            if (((x + i) < width) && ((y + j) < height))
                            {
                                var dataOffset = (y + j) * scanlineSize + (x + i) * BytesPerPixel + 3;
                                frameData[dataOffset] = alphas[bits & 0x07];
                            }

                            bits >>= 3;
                        }
                    }

                    // Last three bytes
                    //bits = *((ILint*)&alphamask[3]);
                    bits = (compressedData[alphamaskIndex + 3]) | (compressedData[alphamaskIndex + 4] << 8) |
                           (compressedData[alphamaskIndex + 5] << 16);
                    for (var j = 2; j < 4; j++)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            // only put pixels out < width or height
                            if (((x + i) < width) && ((y + j) < height))
                            {
                                var dataOffset = (y + j) * scanlineSize + (x + i) * BytesPerPixel + 3;
                                frameData[dataOffset] = alphas[bits & 0x07];
                            }
                            bits >>= 3;
                        }
                    }
                }
            });

            return frameData;
        }
    }
}
