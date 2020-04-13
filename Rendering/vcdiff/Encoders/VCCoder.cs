using VCDiff.Includes;
using VCDiff.Shared;
using System.IO;

namespace VCDiff.Encoders
{
    public class VCCoder
    {
        IByteBuffer oldData;
        IByteBuffer newData;
        ByteStreamWriter sout;
        RollingHash hasher;
        int bufferSize;

        static byte[] MagicBytes = new byte[] { 0xD6, 0xC3, 0xC4, 0x00, 0x00 };
        static byte[] MagicBytesExtended = new byte[] { 0xD6, 0xC3, 0xC4, (byte)'S', 0x00 };

        /// <summary>
        /// The easy public structure for encoding into a vcdiff format
        /// Simply instantiate it with the proper streams and use the Encode() function.
        /// Does not check if data is equal already. You will need to do that.
        /// Returns VCDiffResult: should always return success, unless either the dict or the target streams have 0 bytes
        /// See the VCDecoder for decoding vcdiff format
        /// </summary>
        /// <param name="dict">The dictionary (previous data)</param>
        /// <param name="target">The new data</param>
        /// <param name="sout">The output stream</param>
        /// <param name="maxBufferSize">The maximum buffer size for window chunking. It is in Megabytes. 2 would mean 2 megabytes etc. Default is 1.</param>
        public VCCoder(Stream dict, Stream target, Stream sout, int maxBufferSize = 1)
        {
            if (maxBufferSize <= 0) maxBufferSize = 1;

            this.oldData = new ByteStreamReader(dict);
            this.newData = new ByteStreamReader(target);
            this.sout = new ByteStreamWriter(sout);
            hasher = new RollingHash(BlockHash.BlockSize);

            this.bufferSize = maxBufferSize * 1024 * 1024;
        }

        public VCCoder(IByteBuffer dict, IByteBuffer target, Stream sout, int maxBufferSize = 1)
        {
            this.oldData = dict;
            this.newData = target;
            this.sout = new ByteStreamWriter(sout);
            hasher = new RollingHash(BlockHash.BlockSize);

            this.bufferSize = maxBufferSize * 1024 * 1024;
        }

        /// <summary>
        /// Encodes the file
        /// </summary>
        /// <param name="interleaved">Set this to true to enable SDHC interleaved vcdiff google format</param>
        /// <param name="checksum">Set this to true to add checksum for encoded data windows</param>
        /// <returns></returns>
        public VCDiffResult Encode(bool interleaved = false, bool checksum = false)
        {
            if (newData.Length == 0 || oldData.Length == 0)
            {
                return VCDiffResult.ERRROR;
            }

            VCDiffResult result = VCDiffResult.SUCCESS;

            oldData.Position = 0;
            newData.Position = 0;

            //file header
            //write magic bytes
            if (!interleaved && !checksum)
            {
                sout.writeBytes(MagicBytes);
            }
            else
            {
                sout.writeBytes(MagicBytesExtended);
            }

            //buffer the whole olddata (dictionary)
            //otherwise it will be a slow process
            //even Google's version reads in the entire dictionary file to memory
            //it is just faster that way because of having to move the memory pointer around
            //to find all the hash comparisons and stuff.
            //It is much slower trying to random access read from file with FileStream class
            //however the newData is read in chunks and processed for memory efficiency and speed
            oldData.BufferAll();

            //read in all the dictionary it is the only thing that needs to be
            BlockHash dictionary = new BlockHash(oldData, 0, hasher);
            dictionary.AddAllBlocks();
            oldData.Position = 0;

            ChunkEncoder chunker = new ChunkEncoder(dictionary, oldData, hasher, interleaved, checksum);

            while (newData.CanRead)
            {
                using (ByteBuffer ntarget = new ByteBuffer(newData.ReadBytes(bufferSize)))
                {
                    chunker.EncodeChunk(ntarget, sout);
                }

                //just in case
                System.GC.Collect();
            }

            return result;
        }
    }
}
