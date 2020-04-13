using System;
using System.IO;
using System.Text;

namespace Materia.Rendering.Hdr
{
    public class HdrFile : IDisposable
    {
        const int MINELEN = 8;
        const int MAXELEN = 0x7fff;

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public float[] Pixels { get; protected set; }

        Stream data;

        public HdrFile(string path)
        {
            data = new FileStream(path, FileMode.Open, FileAccess.Read);
            if (!Load())
            {
                throw new Exception("Invalid HDR file");
            }
        }

        public HdrFile(Stream stream)
        {
            data = stream;
            if (!Load())
            {
                throw new Exception("Invalid HDR file");
            }
        }

        public bool Load()
        {
            if (data == null || !data.CanRead)
            {
                return false;
            }

            byte[] buffer = new byte[10];
            data.Read(buffer, 0, buffer.Length);

            if (!Encoding.UTF8.GetString(buffer).Equals("#?RADIANCE"))
            {
                return false;
            }

            data.Position++;

            int i = 0;
            byte c = 0, oldc = 0;
            byte[] cmd = new byte[200];

            while(data.CanRead)
            {
                oldc = c;
                c = (byte)data.ReadByte();
                if (c == 0xa && oldc == 0xa)
                {
                    break;
                }

                cmd[i++] = c;
            }

            byte[] reso = new byte[200];
            i = 0;
            while(data.CanRead)
            {
                c = (byte)data.ReadByte();
                reso[i++] = c;
                if (c == 0xa)
                {
                    break;
                }
            }

            int w = 0, h = 0;
            string[] resData = Encoding.UTF8.GetString(reso).Split(' ');

            if (resData.Length < 4)
            {
                return false;
            }

            Height = h = int.Parse(resData[1]);
            Width = w = int.Parse(resData[3]);
            Pixels = new float[w * h * 3];
            byte[,] scanline = new byte[w,4];

            int offset = 0;
            for(int y = h - 1; y >=0; --y)
            {
                if (!Decrunch(scanline, w, data))
                {
                    break;
                }
                WorkOnRGBE(scanline, w, Pixels, offset);
                offset += w * 3;
            }
            data.Close();
            return true;
        }

        static float ConvertComponent(int expo, int val)
        {
            float v = val / 256.0f;
            float d = MathF.Pow(2, expo);
            return v * d;
        }

        static void WorkOnRGBE(byte[,] scan, int len, float[] pixels, int offset = 0)
        {
            int scanOffset = 0;
            while (len-- > 0)
            {
                int expo = scan[scanOffset, 3] - 128;
                pixels[offset] = ConvertComponent(expo, scan[offset, 0]);
                pixels[offset+1] = ConvertComponent(expo, scan[offset, 1]);
                pixels[offset+2] = ConvertComponent(expo, scan[offset, 2]);
                offset += 3;
                ++scanOffset;
            }
        }

        static bool OldDecrunch(byte[,] scanline, int len, Stream stream, int offset = 0)
        {
            int i = 0;
            int rshift = 0;

            while(len > 0)
            {
                scanline[offset, 0] = (byte)stream.ReadByte();
                scanline[offset, 1] = (byte)stream.ReadByte();
                scanline[offset, 2] = (byte)stream.ReadByte();
                scanline[offset, 3] = (byte)stream.ReadByte();

                if (stream.Position >= stream.Length)
                {
                    return false;
                }

                if (scanline[offset, 0] == 1 && scanline[offset, 1] == 1 && scanline[offset, 2] == 1)
                {
                    for (i = scanline[offset,3] << rshift; i > 0; --i)
                    {
                        scanline[offset, 0] = scanline[offset - 1, 0];

                        ++offset;
                        --len;
                    }
                    rshift += 8;
                }
                else
                {
                    ++offset;
                    --len;
                    rshift = 0;
                }
            }

            return true;
        }

        static bool Decrunch(byte[,] scanline, int len, Stream stream)
        {
            int i, j;
            if (len < MINELEN || len > MAXELEN)
            {
                return OldDecrunch(scanline, len, stream);
            }

            i = stream.ReadByte();
            if (i != 2)
            {
                stream.Position--;
                return OldDecrunch(scanline, len, stream);
            }

            scanline[0, 1] = (byte)stream.ReadByte();
            scanline[0, 2] = (byte)stream.ReadByte();
            i = stream.ReadByte();

            if (scanline[0, 1] != 2 || (scanline[0, 2] & 128) != 0)
            {
                scanline[0, 0] = 2;
                scanline[0, 3] = (byte)i;
                return OldDecrunch(scanline, len - 1, stream, 1);
            }

            for (i = 0; i < 4; ++i)
            {
                for (j = 0; j < len;)
                {
                    byte code = (byte)stream.ReadByte();
                    if (code > 128)
                    {
                        code &= 127;
                        byte value = (byte)stream.ReadByte();
                        while (code-- > 0)
                        {
                            scanline[j++, i] = value;
                        }
                    }
                    else
                    {
                        while(code-- > 0)
                        {
                            scanline[j++, i] = (byte)stream.ReadByte();
                        }
                    }
                }
            }

            return stream.Position < stream.Length;
        }

        public void Dispose()
        {
            if (data != null)
            {
                data.Close();
                data = null;
            }
        }
    }
}
