using System.IO;
using System.Threading.Tasks;

namespace DDSReader.Internal.Decoders
{
    public class DXT2Decoder : DXT3Decoder
    {
        public DXT2Decoder(DDSHeader header) : base(header)
        {
        }

        public async override Task<byte[]> DecodeFrame(Stream dataSource, uint width, uint height)
        {
            var dxt1Data = await base.DecodeFrame(dataSource, width, height);

            CorrectPreMult(dxt1Data);

            return dxt1Data;
        }

        public static void CorrectPreMult(byte[] dxt1Data)
        {
            for (var i = 0; i < dxt1Data.Length; i += BytesPerPixel)
            {
                if (dxt1Data[i + 3] != 0) // Cannot divide by 0.
                {
                    dxt1Data[i] = (byte)(((uint)dxt1Data[i] << 8) / dxt1Data[i + 3]);
                    dxt1Data[i + 1] = (byte)(((uint)dxt1Data[i + 1] << 8) / dxt1Data[i + 3]);
                    dxt1Data[i + 2] = (byte)(((uint)dxt1Data[i + 2] << 8) / dxt1Data[i + 3]);
                }
            }
        }
    }
}