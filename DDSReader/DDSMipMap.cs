#region Usings

using System.Collections.Generic;
using System.Runtime.CompilerServices;

#endregion

namespace DDSReader
{
    public class DDSMipMap
    {
        private readonly IList<byte[]> _mipmapData;

        public DDSMipMap(IList<byte[]> mipmapData, uint width, uint height)
        {
            _mipmapData = mipmapData;
            Width = width;
            Height = height;
        }

        public uint Width { get; private set; }

        public uint Height { get; private set; }

        public IList<byte[]> MipmapData
        {
            get { return new ReadOnlyCollectionBuilder<byte[]>(_mipmapData); }
        }
    }
}
