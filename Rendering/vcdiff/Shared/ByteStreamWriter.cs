using System;
using System.IO;

namespace VCDiff.Shared
{
    public class ByteStreamWriter : IDisposable
    {
        Stream buffer;

        bool isLittle;

        /// <summary>
        /// Wrapper class for writing to streams
        /// with a little bit easier functionality
        /// also detects whether it is little endian
        /// to encode into BE properly
        /// </summary>
        /// <param name="s"></param>
        public ByteStreamWriter(Stream s)
        {
            buffer = s;
            isLittle = BitConverter.IsLittleEndian;
        }

        public byte[] ToArray()
        {
            if(buffer.GetType().Equals(typeof(MemoryStream)))
            {
                MemoryStream buff = (MemoryStream)buffer;
                return buff.ToArray();
            }

            return new byte[0];
        }

        public long Position
        {
            get
            {
                return buffer.Position;
            }
        }

        public void writeByte(byte b)
        {
            this.buffer.WriteByte(b);
        }

        public void writeBytes(byte[] b)
        {
            this.buffer.Write(b, 0, b.Length);
        }

        public void writeUInt16(ushort s)
        {
            byte[] bytes = BitConverter.GetBytes(s);

            if (isLittle)
            {
                Array.Reverse(bytes);
            }

            this.writeBytes(bytes);
        }

        public void writeUInt32(uint i)
        {
            byte[] bytes = BitConverter.GetBytes(i);

            if (isLittle)
            {
                Array.Reverse(bytes);
            }

            this.writeBytes(bytes);
        }

        public void writeFloat(float f)
        {
            byte[] bytes = BitConverter.GetBytes(f);

            if (isLittle)
            {
                Array.Reverse(bytes);
            }

            this.writeBytes(bytes);
        }

        public void writeDouble(double d)
        {
            byte[] bytes = BitConverter.GetBytes(d);

            if (isLittle)
            {
                Array.Reverse(bytes);
            }

            this.writeBytes(bytes);
        }

        public void Dispose()
        {
            this.buffer.Dispose();
        }
    }
}
