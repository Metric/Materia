#region Usings

using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace DDSReader
{
    public class DDSImage
    {
        private readonly uint _height;

        private readonly ICollection<DDSMessage> _messages = new Collection<DDSMessage>();

        private readonly uint _width;

        private readonly ICollection<DDSMipMap> _depthFrames = new Collection<DDSMipMap>();

        public DDSImage(uint width, uint height)
        {
            _width = width;
            _height = height;
        }

        public uint Width
        {
            get { return _width; }
        }

        public uint Height
        {
            get { return _height; }
        }

        public IEnumerable<DDSMessage> Messages
        {
            get { return _messages; }
        }

        public IEnumerable<DDSMipMap> Frames
        {
            get { return _depthFrames; }
        }

        internal void AddMessage(DDSMessage message)
        {
            if (message == null)
            {
                return;
            }

            _messages.Add(message);
        }

        internal void AddFrame(DDSMipMap mipMap)
        {
            if (mipMap == null)
            {
                return;
            }

            _depthFrames.Add(mipMap);
        }
    }
}
