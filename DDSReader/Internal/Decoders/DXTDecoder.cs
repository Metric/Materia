#region Usings



#endregion

namespace DDSReader.Internal.Decoders
{
    public abstract class DXTDecoder : AbstractDecoder
    {
        protected DXTDecoder(DDSHeader header) : base(header)
        {
        }

        protected static int GetDataSize(uint width, uint height, int blocksize)
        {
            return (int) (((width + 3) / 4) * ((height + 3) / 4) * blocksize);
        }

        protected static RGBAColor GetDXTColor(ushort dxtColor)
        {
            var b = (byte) (dxtColor & 0x1f);
            var g = (byte) ((dxtColor & 0x7E0) >> 5);
            var r = (byte) ((dxtColor & 0xF800) >> 11);

            return new RGBAColor((byte) (r << 3 | r >> 2), (byte) (g << 2 | g >> 3), (byte) (b << 3 | r >> 2), 0xFF);
        }
    }
}
