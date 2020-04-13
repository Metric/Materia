using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCDiff.Shared
{
    public abstract class IByteBuffer : IDisposable
    {
        public abstract long Length
        {
            get;
        }

        public abstract long Position
        {
            get; set;
        }

        public abstract bool CanRead
        {
            get;
        }
        public abstract byte[] ReadBytes(int len);
        public abstract byte ReadByte();
        public abstract byte[] PeekBytes(int len);
        public abstract byte PeekByte();
        public abstract void Skip(int len);
        public abstract void Next();
        public abstract void BufferAll();

        public abstract void Dispose();
    }
}
