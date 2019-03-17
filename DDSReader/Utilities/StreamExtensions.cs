#region Usings

using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

#endregion

namespace DDSReader.Utilities
{
    // Copied from http://stackoverflow.com/questions/4159184/c-read-structures-from-binary-file
    internal static class StreamExtensions
    {
        public static T ReadStruct<T>(this Stream stream) where T : struct
        {
            var sz = Marshal.SizeOf(typeof(T));
            var buffer = new byte[sz];
            stream.Read(buffer, 0, sz);

            using (var disposable = new GCHandleDisposable(GCHandle.Alloc(buffer, GCHandleType.Pinned)))
            {
                return (T) Marshal.PtrToStructure(disposable.Handle.AddrOfPinnedObject(), typeof(T));
            }
        }

        public static async Task<T> ReadStructAsync<T>(this Stream stream) where T : struct
        {
            var sz = Marshal.SizeOf(typeof(T));
            var buffer = new byte[sz];
            await stream.ReadAsync(buffer, 0, sz);

            using (var disposable = new GCHandleDisposable(GCHandle.Alloc(buffer, GCHandleType.Pinned)))
            {
                return (T) Marshal.PtrToStructure(disposable.Handle.AddrOfPinnedObject(), typeof(T));
            }
        }

        public static void WriteStruct<T>(this Stream stream, T val) where T : struct
        {
            var sz = Marshal.SizeOf(typeof(T));
            var buffer = new byte[sz];

            var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            using (var disposable = new GCHandleDisposable(GCHandle.Alloc(buffer, GCHandleType.Pinned)))
            {
                Marshal.StructureToPtr(val, pinnedBuffer.AddrOfPinnedObject(), false);

                stream.Write(buffer, 0, buffer.Length);
            }
        }

        public static async Task WriteStructAsync<T>(this Stream stream, T val) where T : struct
        {
            var sz = Marshal.SizeOf(typeof(T));
            var buffer = new byte[sz];

            var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            using (var disposable = new GCHandleDisposable(GCHandle.Alloc(buffer, GCHandleType.Pinned)))
            {
                Marshal.StructureToPtr(val, pinnedBuffer.AddrOfPinnedObject(), false);

                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}
