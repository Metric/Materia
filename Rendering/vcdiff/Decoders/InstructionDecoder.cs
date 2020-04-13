using VCDiff.Shared;
using VCDiff.Includes;

namespace VCDiff.Decoders
{
    public class InstructionDecoder
    {
        CodeTable table;
        ByteBuffer source;
        int pendingSecond;

        /// <summary>
        /// Decodes the incoming instruction from the buffer
        /// </summary>
        /// <param name="sin">the instruction buffer</param>
        /// <param name="customTable">custom code table if any. Default is null.</param>
        public InstructionDecoder(ByteBuffer sin, CustomCodeTableDecoder customTable = null)
        {
            if (customTable != null)
            {
                table = customTable.CustomTable;
            }
            else
            {
                table = CodeTable.DefaultTable;
            }
            source = sin;
            pendingSecond = CodeTable.kNoOpcode;
        }

        /// <summary>
        /// Gets the next instruction from the buffer
        /// </summary>
        /// <param name="size">the size</param>
        /// <param name="mode">the mode</param>
        /// <returns></returns>
        public VCDiffInstructionType Next(out int size, out byte mode)
        {
            byte opcode = 0;
            byte instructionType = CodeTable.N;
            int instructionSize = 0;
            byte instructionMode = 0;
            int start = (int)source.Position;
            do
            {
                if (pendingSecond != CodeTable.kNoOpcode)
                {
                    opcode = (byte)pendingSecond;
                    pendingSecond = CodeTable.kNoOpcode;
                    instructionType = table.inst2[opcode];
                    instructionSize = table.size2[opcode];
                    instructionMode = table.mode2[opcode];
                    break;
                }

                if (!source.CanRead)
                {
                    size = 0;
                    mode = 0;
                    return VCDiffInstructionType.EOD;
                }

                opcode = source.PeekByte();
                if (table.inst2[opcode] != CodeTable.N)
                {
                    pendingSecond = source.PeekByte();
                }
                source.Next();
                instructionType = table.inst1[opcode];
                instructionSize = table.size1[opcode];
                instructionMode = table.mode1[opcode];
            } while (instructionType == CodeTable.N);

            if (instructionSize == 0)
            {
                switch (size = VarIntBE.ParseInt32(source))
                {
                    case (int)VCDiffResult.ERRROR:
                        mode = 0;
                        size = 0;
                        return VCDiffInstructionType.ERROR;
                    case (int)VCDiffResult.EOD:
                        mode = 0;
                        size = 0;
                        //reset it back before we read the instruction
                        //otherwise when parsing interleave we will miss data
                        source.Position = start;
                        return VCDiffInstructionType.EOD;
                    default:
                        break;
                }
            }
            else { 
                size = instructionSize;
            }
            mode = instructionMode;
            return (VCDiffInstructionType)instructionType;
        }
    }
}
