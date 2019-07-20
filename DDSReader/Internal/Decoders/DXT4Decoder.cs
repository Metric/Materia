using System.IO;
using System.Threading.Tasks;

namespace DDSReader.Internal.Decoders
{
    public class DXT4Decoder : DXT5Decoder
    {
        public DXT4Decoder(DDSHeader header) : base(header)
        {
        }

        public override byte[] DecodeFrameSync(Stream dataSource, uint width, uint height)
        {
            var dxt5Data = base.DecodeFrameSync(dataSource, width, height);

            DXT2Decoder.CorrectPreMult(dxt5Data);

            return dxt5Data;
        }

        public async override Task<byte[]> DecodeFrame(Stream dataSource, uint width, uint height)
        {
            var dxt5Data = await base.DecodeFrame(dataSource, width, height);

            DXT2Decoder.CorrectPreMult(dxt5Data);

            return dxt5Data;
        }
    }
}