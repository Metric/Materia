using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDSReader.Internal.Decoders
{
    public class RGBDecoder : AbstractDecoder
    {
        public RGBDecoder(DDSHeader header) : base(header) { }

        public override byte[] DecodeFrameSync(Stream dataSource, uint width, uint height)
        {
            byte[] data = new byte[width * height * 3];
            byte[] buffer = new byte[3];

            try
            {
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        int idx = (x + y * (int)width) * 3;
                        int len = dataSource.Read(buffer, 0, buffer.Length);
                        if (len > 0)
                        {
                            data[idx] = buffer[2];
                            data[idx + 1] = buffer[1];
                            data[idx + 2] = buffer[0];
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return data;
        }

        public override Task<byte[]> DecodeFrame(Stream dataSource, uint width, uint height)
        {
            return Task.Run<byte[]>(() =>
            {
                byte[] data = new byte[width * height * 3];
                byte[] buffer = new byte[3];

                try
                {
                    for (int y = 0; y < height; ++y)
                    {
                        for (int x = 0; x < width; ++x)
                        {
                            int idx = (x + y * (int)width) * 3;
                            int len = dataSource.Read(buffer, 0, buffer.Length);
                            if (len > 0)
                            {
                                data[idx] = buffer[2];
                                data[idx + 1] = buffer[1];
                                data[idx + 2] = buffer[0];
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }

                return data;
            });
        }
    }
}
