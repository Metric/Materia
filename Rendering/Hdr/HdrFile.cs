using Materia.Rendering.Textures;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VCDiff.Shared;

namespace Materia.Rendering.Hdr
{
    public class HdrFile : IDisposable
    {
        const int MINELEN = 8;
        const int MAXELEN = 0x7fff;

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public float[] Pixels { get; protected set; }

        public GLTexture2D Texture { get; protected set; }

        FileStream data;

        public HdrFile(string path)
        {
            data = new FileStream(path, FileMode.Open, FileAccess.Read);
            if (!Load())
            {
                throw new Exception("Invalid HDR file");
            }
        }

        public HdrFile(FileStream stream)
        {
            data = stream;
            if (!Load())
            {
                throw new Exception("Invalid HDR file");
            }
        }

        public GLTexture2D GetTexture()
        {
            if (Texture != null && Texture.Id != 0) return Texture;
            if (Pixels == null || Width == 0 || Height == 0) return null;

            Texture = new GLTexture2D(Interfaces.PixelInternalFormat.Rgb32f);
            Texture.Bind();
            Texture.SetData(Pixels, Interfaces.PixelFormat.Rgb, Width, Height);
            Texture.Linear();
            Texture.Repeat();
            GLTexture2D.Unbind();

            //release local memory
            Pixels = null;

            return Texture;
        }

        public bool Load()
        {
            if (data == null || !data.CanRead)
            {
                return false;
            }

            //going to load the entire file into memory
            //for faster access and parallel processing
            byte[] raw = new byte[data.Length];
            
            data.Read(raw, 0, raw.Length);
            data.Close();
            data = null;

            ByteBuffer byteBuffer = new ByteBuffer(raw);

            byte[] header = byteBuffer.ReadBytes(10);

            if (!Encoding.UTF8.GetString(header).Equals("#?RADIANCE"))
            {
                return false;
            }

            byteBuffer.Position++;

            int i = 0;
            byte c = 0, oldc = 0;
            byte[] cmd = new byte[200];

            while(byteBuffer.CanRead)
            {
                oldc = c;
                c = (byte)byteBuffer.ReadByte();
                if (c == 0xa && oldc == 0xa)
                {
                    break;
                }

                cmd[i++] = c;
            }

            byte[] reso = new byte[200];
            i = 0;
            while(byteBuffer.CanRead)
            {
                c = (byte)byteBuffer.ReadByte();
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

            long offset = byteBuffer.Position;

            Parallel.For(0, h, (y, state) =>
            {
                long cOffset = y * (w * 4) + offset;
                long rOffset = y * (w * 3);
                byte[,] scanline = new byte[w, 4];

                if (!Decrunch(scanline, w, raw, cOffset))
                {
                    return;
                }
                WorkOnRGBE(scanline, w, Pixels, rOffset);
            });
           
            return true;
        }

        static float ConvertComponent(int expo, int val)
        {
            float v = val / 256f;
            float d = MathF.Pow(2, expo);
            return v * d;
        }

        static void WorkOnRGBE(byte[,] scan, int len, float[] pixels, long offset = 0)
        {
            int scanOffset = 0;
            while (len > 0)
            {
                int expo = scan[scanOffset, 3] - 128;
                pixels[offset++] = ConvertComponent(expo, scan[scanOffset, 0]);
                pixels[offset++] = ConvertComponent(expo, scan[scanOffset, 1]);
                pixels[offset++] = ConvertComponent(expo, scan[scanOffset, 2]);
                ++scanOffset;
                --len;
            }
        }

        static bool ReadRaw(byte[,] scanline, int len, byte[] stream, int scanOffset = 0, long streamOffset = 0)
        {
            int rshift = 0;

            while(len > 0)
            {
                scanline[scanOffset, 0] = stream[streamOffset++];
                scanline[scanOffset, 1] = stream[streamOffset++];
                scanline[scanOffset, 2] = stream[streamOffset++];
                scanline[scanOffset, 3] = stream[streamOffset++];

                if (streamOffset >= stream.Length) return false;

                //RLE encoding on this part
                if (scanline[scanOffset, 0] == 1 && scanline[scanOffset, 1] == 1
                    && scanline[scanOffset, 2] == 1)
                {
                    for (int i = scanline[scanOffset, 3] << rshift; i > 0; --i)
                    {
                        scanline[scanOffset, 0] = scanline[scanOffset - 1, 0];
                        scanline[scanOffset, 1] = scanline[scanOffset - 1, 1];
                        scanline[scanOffset, 2] = scanline[scanOffset - 1, 2];
                        scanline[scanOffset, 3] = scanline[scanOffset - 1, 3];
                        ++scanOffset;
                        --len;
                    }

                    rshift += 8;
                    continue;
                }

                ++scanOffset;
                --len;
                rshift = 0;
            }

            return true;
        }

        static bool Decrunch(byte[,] scanline, int len, byte[] stream, long offset = 0)
        {
            int i, j;
            if (len < MINELEN || len > MAXELEN)
            {
                return ReadRaw(scanline, len, stream);
            }

            scanline[0, 0] = stream[offset++];
            scanline[0, 1] = stream[offset++];
            scanline[0, 2] = stream[offset++];
            scanline[0, 3] = stream[offset++];

            if (scanline[0,0] != 2 || scanline[0,1] != 2 || (scanline[0,2] & 128) != 0)
            {
                return ReadRaw(scanline, len - 1, stream, 1, offset);
            }

            for (i = 0; i < 4; ++i)
            {
                for (j = 0; j < len;)
                {
                    byte code = stream[offset++];
                    if (code > 128)
                    {
                        code -= 128;
                        byte value = stream[offset++];
                        while (code-- > 0)
                        {
                            scanline[j++, i] = value;
                        }
                    }
                    else
                    {
                        while(code-- > 0)
                        {
                            scanline[j++, i] = stream[offset++];
                        }
                    }
                }
            }

            return offset < stream.Length;
        }

        public void Dispose()
        {
            Texture?.Dispose();

            data?.Close();
            data = null;
        }
    }
}
