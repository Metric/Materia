using System;
using System.Collections.Generic;
using System.IO;

namespace VCDiff.Shared
{
    //Wrapper Class for any stream that supports Position
    //and Length to make reading bytes easier
    //also has a helper function for reading all the bytes in at once
    public class ByteStreamReader : IByteBuffer, IDisposable
    {
        Stream buffer;
        int lastLenRead;
        bool readAll;
        List<byte> internalBuffer;
        long offset;

        public ByteStreamReader(Stream stream)
        {
            buffer = stream;
        }

        public override long Position
        {
            get
            {
                if(readAll)
                {
                    return offset;
                }
                return buffer.Position;
            }
            set
            {
                if(readAll)
                {
                    if(value >= 0)
                        offset = value;
                }
                if(buffer.CanRead && value >= 0)
                    buffer.Position = value;
            }
        }

        public override long Length
        {
            get
            {
                if(readAll)
                {
                    return internalBuffer.Count;
                }

                if (buffer.CanRead)
                    return buffer.Length;

                return 0;
            }
        }

        public override bool CanRead
        {
            get
            {
                if (readAll)
                {
                    return offset < internalBuffer.Count;
                }

                return buffer.CanRead && buffer.Position < buffer.Length;
            }
        }

        public override void BufferAll()
        {
            if (!readAll)
            {
                offset = 0;
                internalBuffer = new List<byte>();
                readAll = true;

                byte[] buff = new byte[1024 * 8];

                lastLenRead = buffer.Read(buff, 0, buff.Length);

                while (lastLenRead > 0 && buffer.CanRead)
                {
                    for (int i = 0; i < lastLenRead; i++)
                    {
                        internalBuffer.Add(buff[i]);
                    }

                    lastLenRead = buffer.Read(buff, 0, buff.Length);
                }
            }
        }

        public override byte[] PeekBytes(int len)
        {
            if(readAll)
            {
                
                int end = (int)offset + len > internalBuffer.Count ? internalBuffer.Count : (int)offset + len;
                int realLen = (int)offset + len > internalBuffer.Count ? (int)internalBuffer.Count - (int)offset : len;

                byte[] rbuff = new byte[realLen];
                int rcc = 0;
                for(int i = (int)offset; i < end; i++)
                {
                    rbuff[rcc] = internalBuffer[i];
                    rcc++;
                }
                return rbuff;
            }

            long oldPos = buffer.Position;
            byte[] buf = new byte[len];

            int actualRead = buffer.Read(buf, 0, len);
            lastLenRead = actualRead;
            if (actualRead > 0)
            {
                if (actualRead == len)
                {
                    buffer.Position = oldPos;
                    return buf;
                }

                byte[] actualData = new byte[actualRead];
                for (int i = 0; i < actualRead; i++)
                {
                    actualData[i] = buf[i];
                }

                buffer.Position = oldPos;
                return actualData;
            }

            buffer.Position = oldPos;
            return new byte[0];
        }

        public override byte ReadByte()
        {
            if (!CanRead) throw new Exception("Trying to read past end of buffer");
            if(readAll)
            {
                return internalBuffer[(int)offset++];
            }
            lastLenRead = buffer.ReadByte();
            if (lastLenRead > -1)
                return (byte)lastLenRead;
            return 0;
        }

        public override byte[] ReadBytes(int len)
        {
            if (readAll)
            {
                int end = (int)offset + len > internalBuffer.Count ? internalBuffer.Count : (int)offset + len;
                int realLen = (int)offset + len > internalBuffer.Count ? (int)internalBuffer.Count - (int)offset : len;

                byte[] rbuff = new byte[realLen];
                int rcc = 0;
                for (int i = (int)offset; i < end; i++)
                {
                    rbuff[rcc] = internalBuffer[i];
                    rcc++;
                }
                offset += len;
                return rbuff;
            }

            byte[] buf = new byte[len];

            int actualRead = buffer.Read(buf, 0, len);
            lastLenRead = actualRead;
            if(actualRead > 0)
            {
                if(actualRead == len)
                {
                    return buf;
                }

                byte[] actualData = new byte[actualRead];
                for(int i = 0; i < actualRead; i++)
                {
                    actualData[i] = buf[i];
                }

                return actualData;
            }

            return new byte[0];
        }

        public override byte PeekByte()
        {
            if (!CanRead) throw new Exception("Trying to read past end of buffer");
            if(readAll)
            {
                return internalBuffer[(int)offset];
            }
            long lastPos = buffer.Position;
            byte b = ReadByte();
            buffer.Position = lastPos;
            return b;
        }

        //increases the offset by 1
        public override void Next()
        {
            buffer.Position++;
        }

        public override void Skip(int len)
        {
            buffer.Position += len;
        }

        public override void Dispose()
        {
            buffer.Dispose();
        }
    }
}
