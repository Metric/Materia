using VCDiff.Shared;
using VCDiff.Includes;

namespace VCDiff.Decoders
{
    public class WindowDecoder
    {
        IByteBuffer buffer;
        int returnCode;
        long deltaEncodingLength;
        long deltaEncodingStart;
        ParseableChunk chunk;
        byte deltaIndicator;
        long dictionarySize;
        byte winIndicator;
        long sourceLength;
        long sourcePosition;
        long targetLength;
        long addRunLength;
        long instructionAndSizesLength;
        long addressForCopyLength;
        uint checksum;
        bool hasChecksum;

        byte[] addRun;
        byte[] instructionAndSizes;
        byte[] addressesForCopy;

        public byte[] AddRunData
        {
            get
            {
                return addRun;
            }
        }

        public byte[] InstructionsAndSizesData
        {
            get
            {
                return instructionAndSizes;
            }
        }

        public byte[] AddressesForCopyData
        {
            get
            {
                return addressesForCopy;
            }
        }

        public long AddRunLength
        {
            get
            {
                return addRunLength;
            }
        }

        public long InstructionAndSizesLength
        {
            get
            {
                return instructionAndSizesLength;
            }
        }

        public long AddressesForCopyLength
        {
            get
            {
                return addressForCopyLength;
            }
        }

        public byte WinIndicator
        {
            get
            {
                return winIndicator;
            }
        }

        public long SourcePosition
        {
            get
            {
                return sourcePosition;
            }
        }

        public long SourceLength
        {
            get
            {
                return sourceLength;
            }
        }

        public long DecodedDeltaLength
        {
            get
            {
                return targetLength;
            }
        }

        public long DeltaStart
        {
            get
            {
                return deltaEncodingStart;
            }
        }

        public long DeltaLength
        {
            get
            {
                return deltaEncodingStart + deltaEncodingLength;
            }
        }

        public byte DeltaIndicator
        {
            get
            {
                return deltaIndicator;
            }
        }

        public uint Checksum
        {
            get
            {
                return checksum;
            }
        }

        public bool HasChecksum
        {
            get
            {
                return hasChecksum;
            }
        }

        public int Result
        {
            get
            {
                return returnCode;
            }
        }

        /// <summary>
        /// Parses the window from the data
        /// </summary>
        /// <param name="dictionarySize">the dictionary size</param>
        /// <param name="buffer">the buffer containing the incoming data</param>
        public WindowDecoder(long dictionarySize, IByteBuffer buffer)
        {
            this.dictionarySize = dictionarySize;
            this.buffer = buffer;
            chunk = new ParseableChunk(buffer.Position, buffer.Length);
            returnCode = (int)VCDiffResult.SUCCESS;
        }

        /// <summary>
        /// Decodes the window header - Parses it basically
        /// </summary>
        /// <param name="googleVersion">if true will check for checksum and if interleaved</param>
        /// <returns></returns>
        public bool Decode(bool googleVersion)
        {
            bool success = false;

            success = ParseWindowIndicatorAndSegment(dictionarySize, 0, false, out winIndicator, out sourceLength, out sourcePosition);

            if(!success)
            {
                return false;
            }

            success = ParseWindowLengths(out targetLength);

            if(!success)
            {
                return false;
            }

            success = ParseDeltaIndicator();

            if(!success)
            {
                return false;
            }

            hasChecksum = false;
            if((winIndicator & (int)VCDiffWindowFlags.VCDCHECKSUM) != 0 && googleVersion)
            {
                hasChecksum = true;
            }

            success = ParseSectionLengths(hasChecksum, out addRunLength, out instructionAndSizesLength, out addressForCopyLength, out checksum);

            if(!success)
            {
                return false;
            }

            if(googleVersion && addRunLength == 0 && addressForCopyLength == 0 && instructionAndSizesLength > 0)
            {
                //interleave format
                return true;
            }       

            if (buffer.CanRead)
            {
                addRun = buffer.ReadBytes((int)addRunLength);
            }
            if(buffer.CanRead)
            {
                instructionAndSizes = buffer.ReadBytes((int)instructionAndSizesLength);
            }
            if(buffer.CanRead)
            {
                addressesForCopy = buffer.ReadBytes((int)addressForCopyLength);
            }

            return true;
        }

        bool ParseByte(out byte value)
        {
            if((int)VCDiffResult.SUCCESS != returnCode)
            {
                value = 0;
                return false;
            }
            if(chunk.IsEmpty)
            {
                value = 0;
                returnCode = (int)VCDiffResult.EOD;
                return false;
            }
            value = buffer.ReadByte();
            chunk.Position = buffer.Position;
            return true;
        }

        bool ParseInt32(out int value)
        {
            if ((int)VCDiffResult.SUCCESS != returnCode)
            {
                value = 0;
                return false;
            }
            if (chunk.IsEmpty)
            {
                value = 0;
                returnCode = (int)VCDiffResult.EOD;
                return false;
            }

            int parsed = VarIntBE.ParseInt32(buffer);
            switch(parsed)
            {
                case (int)VCDiffResult.ERRROR:
                    value = 0;
                    return false;
                case (int)VCDiffResult.EOD:
                    value = 0;
                    return false;
                default:
                    break;
            }
            chunk.Position = buffer.Position;
            value = parsed;
            return true;
        }

        bool ParseUInt32(out uint value)
        {
            if ((int)VCDiffResult.SUCCESS != returnCode)
            {
                value = 0;
                return false;
            }
            if (chunk.IsEmpty)
            {
                value = 0;
                returnCode = (int)VCDiffResult.EOD;
                return false;
            }

            long parsed = VarIntBE.ParseInt64(buffer);
            switch (parsed)
            {
                case (int)VCDiffResult.ERRROR:
                    value = 0;
                    return false;
                case (int)VCDiffResult.EOD:
                    value = 0;
                    return false;
                default:
                    break;
            }
            if(parsed > 0xFFFFFFFF)
            {
                returnCode = (int)VCDiffResult.ERRROR;
                value = 0;
                return false;
            }
            chunk.Position = buffer.Position;
            value = (uint)parsed;
            return true;
        }

        bool ParseSourceSegmentLengthAndPosition(long from, out long sourceLength, out long sourcePosition)
        {
            int outLength;
            if(!ParseInt32(out outLength))
            {
                sourceLength = 0;
                sourcePosition = 0;
                return false;
            }
            sourceLength = outLength;
            if(sourceLength > from)
            {
                returnCode = (int)VCDiffResult.ERRROR;
                sourceLength = 0;
                sourcePosition = 0;
                return false;
            }
            int outPos;
            if(!ParseInt32(out outPos))
            {
                sourcePosition = 0;
                sourceLength = 0;
                return false;
            }
            sourcePosition = outPos;
            if(sourcePosition > from)
            {
                returnCode = (int)VCDiffResult.ERRROR;
                sourceLength = 0;
                sourcePosition = 0;
                return false;
            }

            long segmentEnd = sourcePosition + sourceLength;
            if(segmentEnd > from)
            {
                returnCode = (int)VCDiffResult.ERRROR;
                sourceLength = 0;
                sourcePosition = 0;
                return false;
            }

            return true;
        }

        bool ParseWindowIndicatorAndSegment(long dictionarySize, long decodedTargetSize, bool allowVCDTarget, out byte winIndicator, out long sourceSegmentLength, out long sourceSegmentPosition)
        {
            if(!ParseByte(out winIndicator))
            {
                winIndicator = 0;
                sourceSegmentLength = 0;
                sourceSegmentPosition = 0;
                return false;
            }

            int sourceFlags = winIndicator & ((int)VCDiffWindowFlags.VCDSOURCE | (int)VCDiffWindowFlags.VCDTARGET);

            switch(sourceFlags)
            {
                case (int)VCDiffWindowFlags.VCDSOURCE:
                    return ParseSourceSegmentLengthAndPosition(dictionarySize, out sourceSegmentLength, out sourceSegmentPosition);
                case (int)VCDiffWindowFlags.VCDTARGET:
                    if(!allowVCDTarget)
                    {
                        winIndicator = 0;
                        sourceSegmentLength = 0;
                        sourceSegmentPosition = 0;
                        returnCode = (int)VCDiffResult.ERRROR;
                        return false;
                    }
                    return ParseSourceSegmentLengthAndPosition(decodedTargetSize, out sourceSegmentLength, out sourceSegmentPosition);
                case (int)VCDiffWindowFlags.VCDSOURCE | (int)VCDiffWindowFlags.VCDTARGET:
                    winIndicator = 0;
                    sourceSegmentPosition = 0;
                    sourceSegmentLength = 0;
                    return false;
            }

            winIndicator = 0;
            sourceSegmentPosition = 0;
            sourceSegmentLength = 0;
            return false;
        }

        bool ParseWindowLengths(out long targetWindowLength)
        {
            int deltaLength;
            if(!ParseInt32(out deltaLength))
            {
                targetWindowLength = 0;
                return false;
            }
            deltaEncodingLength = deltaLength;

            deltaEncodingStart = chunk.ParsedSize;
            int outTargetLength;
            if(!ParseInt32(out outTargetLength))
            {
                targetWindowLength = 0;
                return false;
            }
            targetWindowLength = outTargetLength;
            return true;
        }

        bool ParseDeltaIndicator()
        {
            if(!ParseByte(out deltaIndicator))
            {
                returnCode = (int)VCDiffResult.ERRROR;
                return false;
            }
            if((deltaIndicator & ((int)VCDiffCompressFlags.VCDDATACOMP | (int)VCDiffCompressFlags.VCDINSTCOMP | (int)VCDiffCompressFlags.VCDADDRCOMP)) > 0)
            {
                returnCode = (int)VCDiffResult.ERRROR;
                return false;
            }
            return true; 
        }


        public bool ParseSectionLengths(bool hasChecksum, out long addRunLength, out long instructionsLength, out long addressLength, out uint checksum)
        {
            int outAdd;
            int outInstruct;
            int outAddress;
            ParseInt32(out outAdd);
            ParseInt32(out outInstruct);
            ParseInt32(out outAddress);
            checksum = 0;

            if(hasChecksum)
            {
                ParseUInt32(out checksum);
            }

            addRunLength = outAdd;
            addressLength = outAddress;
            instructionsLength = outInstruct;

            if(returnCode != (int)VCDiffResult.SUCCESS)
            {
                return false;
            }

            long deltaHeaderLength = chunk.ParsedSize - deltaEncodingStart;
            long totalLen = deltaHeaderLength + addRunLength + instructionsLength + addressLength;

            if(deltaEncodingLength != totalLen)
            {
                returnCode = (int)VCDiffResult.ERRROR;
                return false;
            }

            return true;
        }

        public class ParseableChunk
        {
            long end;
            long position;
            long start;

            public long UnparsedSize
            {
                get
                {
                    return end - position;
                }
            }

            public long End
            {
                get
                {
                    return end;
                }
            }

            public bool IsEmpty
            {
                get
                {
                    return 0 == UnparsedSize;
                }
            }

            public long Start
            {
                get
                {
                    return start;
                }
            }

            public long ParsedSize
            {
                get
                {
                    return position - start;
                }
            }

            public long Position
            {
                get
                {
                    return position;
                }
                set
                {
                    if(position < start)
                    {
                        return;
                    }
                    if(position > end)
                    {
                        return;
                    }
                    position = value;
                }
            }

            public ParseableChunk(long s, long len)
            {
                this.start = s;
                this.end = s + len;
                this.position = s;
            }
        }
    }
}
