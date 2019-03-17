using System.IO;
using System.Threading.Tasks;

namespace DDSReader.Internal
{
    public interface IDataDecoder
    {
        Task<byte[]> DecodeFrame(Stream dataSource, uint width, uint height);
    }
}