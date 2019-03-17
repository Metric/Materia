#region Usings

using System.IO;
using System.Threading.Tasks;

#endregion

namespace DDSReader.Internal.Decoders
{
    public abstract class AbstractDecoder : IDataDecoder
    {
        public const int BytesPerPixel = 4;

        protected AbstractDecoder(DDSHeader header)
        {
            Header = header;
        }

        protected DDSHeader Header { get; private set; }

        #region IDataDecoder Members

        public abstract Task<byte[]> DecodeFrame(Stream dataSource, uint width, uint height);

        #endregion
    }
}
