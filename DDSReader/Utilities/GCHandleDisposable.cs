using System;
using System.Runtime.InteropServices;

namespace DDSReader.Utilities
{
    public sealed class GCHandleDisposable : IDisposable
    {
        public GCHandleDisposable(GCHandle handle)
        {
            Handle = handle;
        }

        public GCHandle Handle { get; private set; }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            Handle.Free();

            _disposed = true;
        }

        ~GCHandleDisposable()
        {
            Dispose();
        }
    }
}