#region Usings



#endregion

namespace DDSReader.Internal
{
    public struct RGBAColor
    {
        public byte a;

        public byte b;

        public byte g;

        public byte r;

        public RGBAColor(byte r, byte g, byte b, byte a)
        {
            this.b = b;
            this.g = g;
            this.r = r;
            this.a = a;
        }
    }
}
