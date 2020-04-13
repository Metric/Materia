using VCDiff.Includes;
using VCDiff.Shared;
using System.IO;

namespace VCDiff.Decoders
{
    public class CustomCodeTableDecoder
    {
        byte nearSize;
        byte sameSize;
        CodeTable table;

        public byte NearSize
        {
            get
            {
                return nearSize;
            }
        }

        public byte SameSize
        {
            get
            {
                return sameSize;
            }
        }

        public CodeTable CustomTable
        {
            get
            {
                return table;
            }
        }

        public CustomCodeTableDecoder()
        {
 
        }

        public VCDiffResult Decode(IByteBuffer source)
        {
            VCDiffResult result = VCDiffResult.SUCCESS;

            //the custom codetable itself is a VCDiff file but it is required to be encoded with the standard table
            //the length should be the first thing after the hdr_indicator if not supporting compression
            //at least according to the RFC specs.
            int lengthOfCodeTable = VarIntBE.ParseInt32(source);

            if (lengthOfCodeTable == 0) return VCDiffResult.ERRROR;

            ByteBuffer codeTable = new ByteBuffer(source.ReadBytes(lengthOfCodeTable));

            //according to the RFC specifications the next two items will be the size of near and size of same
            //they are bytes in the RFC spec, but for some reason Google uses the varint to read which does
            //the same thing if it is a single byte
            //but I am going to just read in bytes because it is the RFC standard
            nearSize = codeTable.ReadByte();
            sameSize = codeTable.ReadByte();

            if(nearSize == 0 || sameSize == 0 || nearSize > byte.MaxValue || sameSize > byte.MaxValue)
            {
                return VCDiffResult.ERRROR;
            }

            table = new CodeTable();
            //get the original bytes of the default codetable to use as a dictionary
            IByteBuffer dictionary = table.GetBytes();

            //Decode the code table VCDiff file itself
            //stream the decoded output into a memory stream
            using(MemoryStream sout = new MemoryStream())
            {
                VCDecoder decoder = new VCDecoder(dictionary, codeTable, sout);
                result = decoder.Start();

                if(result != VCDiffResult.SUCCESS)
                {
                    return result;
                }

                long bytesWritten = 0;
                result = decoder.Decode(out bytesWritten);

                if(result != VCDiffResult.SUCCESS || bytesWritten == 0)
                {
                    return VCDiffResult.ERRROR;
                }

                //set the new table data that was decoded
                if(!table.SetBytes(sout.ToArray()))
                {
                    result = VCDiffResult.ERRROR;
                }
            }

            return result;
        }
    }
}
