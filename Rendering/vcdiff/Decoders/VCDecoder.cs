using VCDiff.Shared;
using VCDiff.Includes;
using System.IO;

namespace VCDiff.Decoders
{
    public class VCDecoder
    {
        ByteStreamWriter sout;
        IByteBuffer delta;
        IByteBuffer dict;
        CustomCodeTableDecoder customTable;
        bool googleVersion;
        bool isStarted;

        static byte[] MagicBytes = new byte[] { 0xD6, 0xC3, 0xC4, 0x00, 0x00 };

        public bool IsSDHCFormat
        {
            get
            {
                return googleVersion;
            }
        }

        public bool IsStarted
        {
            get
            {
                return isStarted;
            }
        }

        /// <summary>
        /// Dict is the dictionary file
        /// Delta is the diff file
        /// Sout is the stream for output
        /// </summary>
        /// <param name="dict">Dictionary</param>
        /// <param name="delta">Target file / Diff / Delta file</param>
        /// <param name="sout">Output Stream</param>
        public VCDecoder(Stream dict, Stream delta, Stream sout)
        {
            this.delta = new ByteStreamReader(delta);
            this.dict = new ByteStreamReader(dict);
            this.sout = new ByteStreamWriter(sout);
            isStarted = false;
        }

        public VCDecoder(IByteBuffer dict, IByteBuffer delta, Stream sout)
        {
            this.delta = delta;
            this.dict = dict;
            this.sout = new ByteStreamWriter(sout);
            isStarted = false;
        }

        /// <summary>
        /// Call this before calling decode
        /// This expects at least the header part of the delta file
        /// is available in the stream
        /// </summary>
        /// <returns></returns>
        public VCDiffResult Start()
        {
            if (!delta.CanRead) return VCDiffResult.EOD;

            byte V = delta.ReadByte();

            if (!delta.CanRead) return VCDiffResult.EOD;

            byte C = delta.ReadByte();

            if (!delta.CanRead) return VCDiffResult.EOD;

            byte D = delta.ReadByte();

            if (!delta.CanRead) return VCDiffResult.EOD;

            byte version = delta.ReadByte();

            if (!delta.CanRead) return VCDiffResult.EOD;

            byte hdr = delta.ReadByte();

            if (V != MagicBytes[0])
            {
                return VCDiffResult.ERRROR;
            }

            if (C != MagicBytes[1])
            {
                return VCDiffResult.ERRROR;
            }

            if (D != MagicBytes[2])
            {
                return VCDiffResult.ERRROR;
            }

            if (version != 0x00 && version != 'S')
            {
                return VCDiffResult.ERRROR;
            }

            //compression not supported
            if ((hdr & (int)VCDiffCodeFlags.VCDDECOMPRESS) != 0)
            {
                return VCDiffResult.ERRROR;
            }

            //custom code table!
            if((hdr & (int)VCDiffCodeFlags.VCDCODETABLE) != 0)
            {
                if (!delta.CanRead) return VCDiffResult.EOD;

                //try decoding the custom code table
                //since we don't support the compress the next line should be the length of the code table
                customTable = new CustomCodeTableDecoder();
                VCDiffResult result = customTable.Decode(delta);

                if(result != VCDiffResult.SUCCESS)
                {
                    return result;
                }
            }

            googleVersion = version == 'S';

            isStarted = true;

            //buffer all the dictionary up front
            dict.BufferAll();

            return VCDiffResult.SUCCESS;
        }

        /// <summary>
        /// Use this after calling Start
        /// Each time the decode is called it is expected
        /// that at least 1 Window header is available in the stream
        /// </summary>
        /// <param name="bytesWritten">bytes decoded for all available windows</param>
        /// <returns></returns>
        public VCDiffResult Decode(out long bytesWritten)
        {
            if(!isStarted)
            {
                bytesWritten = 0;
                return VCDiffResult.ERRROR;
            }

            VCDiffResult result = VCDiffResult.SUCCESS;
            bytesWritten = 0;

            if (!delta.CanRead) return VCDiffResult.EOD;

            while (delta.CanRead)
            {
                //delta is streamed in order aka not random access
                WindowDecoder w = new WindowDecoder(dict.Length, delta);

                if (w.Decode(googleVersion))
                {
                    using (BodyDecoder body = new BodyDecoder(w, dict, delta, sout))
                    {

                        if (googleVersion && w.AddRunLength == 0 && w.AddressesForCopyLength == 0 && w.InstructionAndSizesLength > 0)
                        {
                            //interleaved
                            //decodedinterleave actually has an internal loop for waiting and streaming the incoming rest of the interleaved window
                            result = body.DecodeInterleave();

                            if (result != VCDiffResult.SUCCESS && result != VCDiffResult.EOD)
                            {
                                return result;
                            }

                            bytesWritten += body.Decoded;
                        }
                        //technically add could be 0 if it is all copy instructions
                        //so do an or check on those two
                        else if (googleVersion && (w.AddRunLength > 0 || w.AddressesForCopyLength > 0) && w.InstructionAndSizesLength > 0)
                        {
                            //not interleaved
                            //expects the full window to be available
                            //in the stream

                            result = body.Decode();

                            if (result != VCDiffResult.SUCCESS)
                            {
                                return result;
                            }

                            bytesWritten += body.Decoded;
                        }
                        else if (!googleVersion)
                        {
                            //not interleaved
                            //expects the full window to be available 
                            //in the stream
                            result = body.Decode();

                            if (result != VCDiffResult.SUCCESS)
                            {
                                return result;
                            }

                            bytesWritten += body.Decoded;
                        }
                        else
                        {
                            //invalid file
                            return VCDiffResult.ERRROR;
                        }
                    }
                }
                else
                {
                    return (VCDiffResult)w.Result;
                }
            }

            return result;
        }
    }
}
