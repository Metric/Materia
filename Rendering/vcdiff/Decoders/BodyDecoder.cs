using System;
using System.Collections.Generic;
using VCDiff.Shared;
using VCDiff.Includes;

namespace VCDiff.Decoders
{
    public class BodyDecoder : IDisposable
    {
        WindowDecoder window;
        ByteStreamWriter sout;
        IByteBuffer dict;
        IByteBuffer target;
        AddressCache addressCache;
        long decodedOnly = 0;
        long bytesWritten = 0;
        List<byte> targetData;
        CustomCodeTableDecoder customTable;

        //the total bytes decoded
        public long Decoded
        {
            get
            {
                return decodedOnly;
            }
        }

        /// <summary>
        /// The main decoder loop for the data
        /// </summary>
        /// <param name="w">the window decoder</param>
        /// <param name="dictionary">the dictionary data</param>
        /// <param name="target">the target data</param>
        /// <param name="sout">the out stream</param>
        /// <param name="customTable">custom table if any. Default is null.</param>
        public BodyDecoder(WindowDecoder w, IByteBuffer dictionary, IByteBuffer target, ByteStreamWriter sout, CustomCodeTableDecoder customTable = null)
        {
            if (customTable != null)
            {
                this.customTable = customTable;
                addressCache = new AddressCache((byte)customTable.NearSize, (byte)customTable.SameSize);
            }
            else 
            {
                addressCache = new AddressCache();
            }
            window = w;
            this.sout = sout;
            this.dict = dictionary;
            this.target = target;
            targetData = new List<byte>();
        }

        /// <summary>
        /// Decode if as expecting interleave
        /// </summary>
        /// <returns></returns>
        public VCDiffResult DecodeInterleave()
        { 
            VCDiffResult result = VCDiffResult.SUCCESS;
            //since interleave expected then the last point that was most likely decoded was the lengths section
            //so following is all data for the add run copy etc
            long interleaveLength = window.InstructionAndSizesLength;
            List<byte> previous = new List<byte>();
            bool didBreakBeforeComplete = false;
            int lastDecodedSize = 0;
            VCDiffInstructionType lastDecodedInstruction = VCDiffInstructionType.NOOP;

            while (interleaveLength > 0)
            {
                if (target.CanRead) {
                    //read in 
                    didBreakBeforeComplete = false;

                    //try to read in all interleaved bytes
                    //if not then it will buffer for next time
                    previous.AddRange(target.ReadBytes((int)interleaveLength));
                    ByteBuffer incoming = new ByteBuffer(previous.ToArray());
                    previous.Clear();
                    long initialLength = incoming.Length;

                    InstructionDecoder instrDecoder = new InstructionDecoder(incoming, this.customTable);

                    while(incoming.CanRead && decodedOnly < window.DecodedDeltaLength)
                    {
                        int decodedSize = 0;
                        byte mode = 0;
                        VCDiffInstructionType instruction = VCDiffInstructionType.NOOP;

                        if (lastDecodedSize > 0 && lastDecodedInstruction != VCDiffInstructionType.NOOP)
                        {
                            decodedSize = lastDecodedSize;
                            instruction = lastDecodedInstruction;                   
                        }
                        else {
                            instruction = instrDecoder.Next(out decodedSize, out mode);

                            switch (instruction)
                            {
                                case VCDiffInstructionType.EOD:
                                    didBreakBeforeComplete = true;
                                    break;
                                case VCDiffInstructionType.ERROR:
                                    targetData.Clear();
                                    return VCDiffResult.ERRROR;
                                default:
                                    break;
                            }
                        }

                        //if instruction is EOD then decodedSize will be 0 as well
                        //the last part of the buffer containing the instruction will be 
                        //buffered for the next loop
                        lastDecodedInstruction = instruction;
                        lastDecodedSize = decodedSize;

                        if (didBreakBeforeComplete)
                        {
                            //we don't have all the data so store this pointer into a temporary list to resolve next loop
                            didBreakBeforeComplete = true;
                            interleaveLength -= incoming.Position;

                            if (initialLength - incoming.Position > 0)
                            {
                                previous.AddRange(incoming.ReadBytes((int)(initialLength - incoming.Position)));
                            }

                            break;
                        }

                        switch (instruction)
                        {
                            case VCDiffInstructionType.ADD:
                                result = DecodeAdd(decodedSize, incoming);
                                break;
                            case VCDiffInstructionType.RUN:
                                result = DecodeRun(decodedSize, incoming);
                                break;
                            case VCDiffInstructionType.COPY:
                                result = DecodeCopy(decodedSize, mode, incoming);
                                break;
                            default:
                                targetData.Clear();
                                return VCDiffResult.ERRROR;
                        }

                        if (result == VCDiffResult.EOD)
                        {
                            //we don't have all the data so store this pointer into a temporary list to resolve next loop
                            didBreakBeforeComplete = true;
                            interleaveLength -= incoming.Position;

                            if (initialLength - incoming.Position > 0)
                            {
                                previous.AddRange(incoming.ReadBytes((int)(initialLength - incoming.Position)));
                            }

                            break;
                        }

                        //reset these as we have successfully used them
                        lastDecodedInstruction = VCDiffInstructionType.NOOP;
                        lastDecodedSize = 0;
                    }

                    if(!didBreakBeforeComplete)
                    {
                        interleaveLength -= initialLength;
                    }
                }
            }

            if (window.HasChecksum)
            {
                uint adler = Checksum.ComputeAdler32(targetData.ToArray());

                if (adler != window.Checksum)
                {
                    result = VCDiffResult.ERRROR;
                }
            }

            targetData.Clear();
            return result;
        }

        /// <summary>
        /// Decode normally
        /// </summary>
        /// <returns></returns>
        public VCDiffResult Decode()
        {
            ByteBuffer instructionBuffer = new ByteBuffer(window.InstructionsAndSizesData);
            ByteBuffer addressBuffer = new ByteBuffer(window.AddressesForCopyData);
            ByteBuffer addRunBuffer = new ByteBuffer(window.AddRunData);

            InstructionDecoder instrDecoder = new InstructionDecoder(instructionBuffer, this.customTable);

            VCDiffResult result = VCDiffResult.SUCCESS;

            while (decodedOnly < window.DecodedDeltaLength && instructionBuffer.CanRead)
            {
                int decodedSize = 0;
                byte mode = 0;

                VCDiffInstructionType instruction = instrDecoder.Next(out decodedSize, out mode);

                switch(instruction)
                {
                    case VCDiffInstructionType.EOD:
                        targetData.Clear();
                        return VCDiffResult.EOD;
                    case VCDiffInstructionType.ERROR:
                        targetData.Clear();
                        return VCDiffResult.ERRROR;
                    default:
                        break;
                }

                switch(instruction)
                {
                    case VCDiffInstructionType.ADD:
                        result = DecodeAdd(decodedSize, addRunBuffer);
                        break;
                    case VCDiffInstructionType.RUN:
                        result = DecodeRun(decodedSize, addRunBuffer);
                        break;
                    case VCDiffInstructionType.COPY:
                        result = DecodeCopy(decodedSize, mode, addressBuffer);
                        break;
                    default:
                        targetData.Clear();
                        return VCDiffResult.ERRROR;
                }
            }

            if(window.HasChecksum)
            {
                uint adler = Checksum.ComputeAdler32(targetData.ToArray());

                if(adler != window.Checksum)
                {
                    result = VCDiffResult.ERRROR;
                }
            }

            targetData.Clear();
            return result;
        }

        VCDiffResult DecodeCopy(int size, byte mode, ByteBuffer addresses)
        {
            long here = window.SourceLength + decodedOnly;
            long decoded = addressCache.DecodeAddress(here, mode, addresses);

            switch((VCDiffResult)decoded)
            {
                case VCDiffResult.ERRROR:
                    return VCDiffResult.ERRROR;
                case VCDiffResult.EOD:
                    return VCDiffResult.EOD;
                default:
                    if(decoded < 0 || decoded > here)
                    {
                        return VCDiffResult.ERRROR;
                    }
                    break;
            }

            if(decoded + size <= window.SourceLength)
            {
                dict.Position = decoded;
                byte[] rbytes = dict.ReadBytes(size);
                sout.writeBytes(rbytes);
                targetData.AddRange(rbytes);
                decodedOnly += size;
                return VCDiffResult.SUCCESS;
            }

            ///will come back to this once
            ///target data reading is implemented
           /*if(decoded < window.SourceLength)
           {
                long partial = window.SourceLength - decoded;
                dict.Position = decoded;
                sout.writeBytes(dict.ReadBytes((int)partial));
                bytesWritten += partial;
                size -= (int)partial;
           }

            decoded -= window.SourceLength;

            while(size > (bytesDecoded - decoded))
            {
                long partial = bytesDecoded - decoded;
                target.Position = decoded;
                sout.writeBytes(target.ReadBytes((int)partial));
                decoded += partial;
                size -= (int)partial;
                bytesWritten += partial;
            }

            target.Position = decoded;
            sout.writeBytes(target.ReadBytes(size));*/

            return VCDiffResult.ERRROR;
        }

        VCDiffResult DecodeRun(int size, ByteBuffer addRun)
        {
            if(addRun.Position + 1 > addRun.Length)
            {
                return VCDiffResult.EOD;
            }

            if(!addRun.CanRead)
            {
                return VCDiffResult.EOD;
            }

            byte b = addRun.ReadByte();

            for(int i = 0; i < size; i++)
            {
                sout.writeByte(b);
                targetData.Add(b);
            }

            decodedOnly += size;

            return VCDiffResult.SUCCESS;
        }

        VCDiffResult DecodeAdd(int size, ByteBuffer addRun)
        {
            if(addRun.Position + size > addRun.Length)
            {
                return VCDiffResult.EOD;
            }

            if(!addRun.CanRead)
            {
                return VCDiffResult.EOD;
            }

            byte[] rbytes = addRun.ReadBytes(size);
            sout.writeBytes(rbytes);
            targetData.AddRange(rbytes);
            decodedOnly += size;
            return VCDiffResult.SUCCESS;
        }

        public void Dispose()
        {
            targetData.Clear();
        }
    }
}
