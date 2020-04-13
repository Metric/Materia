using System.Collections.Generic;
using VCDiff.Includes;

namespace VCDiff.Shared
{
    public class CodeTable
    {
        /// <summary>
        /// Default CodeTable as described in the RFC doc
        /// </summary>
        public const int kNoOpcode = 0x100;
        public const int kCodeTableSize = 256;

        public byte[] inst1;
        public byte[] inst2;
        public byte[] size1;
        public byte[] size2;
        public byte[] mode1;
        public byte[] mode2;

        public const byte N = (byte)VCDiffInstructionType.NOOP;
        public const byte A = (byte)VCDiffInstructionType.ADD;
        public const byte R = (byte)VCDiffInstructionType.RUN;
        public const byte C = (byte)VCDiffInstructionType.COPY;
        public const byte ERR = (byte)VCDiffInstructionType.ERROR;
        public const byte EOD = (byte)VCDiffInstructionType.EOD;

        public static CodeTable DefaultTable
        {
            get
            {
                return new CodeTable();
            }
        }

        public CodeTable()
        {
            inst1 = new byte[]
            {
                R,  // opcode 0
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,  // opcodes 1-18
                C, C, C, C, C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 19-34
                C, C, C, C, C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 35-50
                C, C, C, C, C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 51-66
                C, C, C, C, C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 67-82
                C, C, C, C, C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 83-98
                C, C, C, C, C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 99-114
                C, C, C, C, C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 115-130
                C, C, C, C, C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 131-146
                C, C, C, C, C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 147-162
                A, A, A, A, A, A, A, A, A, A, A, A,  // opcodes 163-174
                A, A, A, A, A, A, A, A, A, A, A, A,  // opcodes 175-186
                A, A, A, A, A, A, A, A, A, A, A, A,  // opcodes 187-198
                A, A, A, A, A, A, A, A, A, A, A, A,  // opcodes 199-210
                A, A, A, A, A, A, A, A, A, A, A, A,  // opcodes 211-222
                A, A, A, A, A, A, A, A, A, A, A, A,  // opcodes 223-234
                A, A, A, A,  // opcodes 235-238
                A, A, A, A,  // opcodes 239-242
                A, A, A, A,  // opcodes 243-246
                C, C, C, C, C, C, C, C, C  // opcodes 247-255
            };
            inst2 = new byte[]
            {
                N,  // opcode 0
                N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N,  // opcodes 1-18
                N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N,  // opcodes 19-34
                N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N,  // opcodes 35-50
                N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N,  // opcodes 51-66
                N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N,  // opcodes 67-82
                N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N,  // opcodes 83-98
                N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N,  // opcodes 99-114
                N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N,  // opcodes 115-130
                N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N,  // opcodes 131-146
                N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N,  // opcodes 147-162
                C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 163-174
                C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 175-186
                C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 187-198
                C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 199-210
                C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 211-222
                C, C, C, C, C, C, C, C, C, C, C, C,  // opcodes 223-234
                C, C, C, C,  // opcodes 235-238
                C, C, C, C,  // opcodes 239-242
                C, C, C, C,  // opcodes 243-246
                A, A, A, A, A, A, A, A, A  // opcodes 247-255
            };
            size1 = new byte[]
            {
                0,  // opcode 0
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17,  // 1-18
                0, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,  // 19-34
                0, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,  // 35-50
                0, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,  // 51-66
                0, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,  // 67-82
                0, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,  // 83-98
                0, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,  // 99-114
                0, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,  // 115-130
                0, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,  // 131-146
                0, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,  // 147-162
                1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4,  // opcodes 163-174
                1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4,  // opcodes 175-186
                1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4,  // opcodes 187-198
                1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4,  // opcodes 199-210
                1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4,  // opcodes 211-222
                1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4,  // opcodes 223-234
                1, 2, 3, 4,  // opcodes 235-238
                1, 2, 3, 4,  // opcodes 239-242
                1, 2, 3, 4,  // opcodes 243-246
                4, 4, 4, 4, 4, 4, 4, 4, 4  // opcodes 247-255
            };
            size2 = new byte[]
            {
                0,  // opcode 0
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 1-18
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 19-34
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 35-50
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 51-66
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 67-82
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 83-98
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 99-114
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 115-130
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 131-146
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 147-162
                4, 5, 6, 4, 5, 6, 4, 5, 6, 4, 5, 6,  // opcodes 163-174
                4, 5, 6, 4, 5, 6, 4, 5, 6, 4, 5, 6,  // opcodes 175-186
                4, 5, 6, 4, 5, 6, 4, 5, 6, 4, 5, 6,  // opcodes 187-198
                4, 5, 6, 4, 5, 6, 4, 5, 6, 4, 5, 6,  // opcodes 199-210
                4, 5, 6, 4, 5, 6, 4, 5, 6, 4, 5, 6,  // opcodes 211-222
                4, 5, 6, 4, 5, 6, 4, 5, 6, 4, 5, 6,  // opcodes 223-234
                4, 4, 4, 4,  // opcodes 235-238
                4, 4, 4, 4,  // opcodes 239-242
                4, 4, 4, 4,  // opcodes 243-246
                1, 1, 1, 1, 1, 1, 1, 1, 1  // opcodes 247-255
            };
            mode1 = new byte[]
            {
                0,  // opcode 0
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 1-18
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 19-34
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,  // opcodes 35-50
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,  // opcodes 51-66
                3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,  // opcodes 67-82
                4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,  // opcodes 83-98
                5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,  // opcodes 99-114
                6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,  // opcodes 115-130
                7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,  // opcodes 131-146
                8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,  // opcodes 147-162
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 163-174
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 175-186
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 187-198
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 199-210
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 211-222
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 223-234
                0, 0, 0, 0,  // opcodes 235-238
                0, 0, 0, 0,  // opcodes 239-242
                0, 0, 0, 0,  // opcodes 243-246
                0, 1, 2, 3, 4, 5, 6, 7, 8  // opcodes 247-255
            };
            mode2 = new byte[]
            {
                0,  // opcode 0
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 1-18
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 19-34
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 35-50
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 51-66
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 67-82
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 83-98
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 99-114
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 115-130
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 131-146
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 147-162
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // opcodes 163-174
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,  // opcodes 175-186
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,  // opcodes 187-198
                3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,  // opcodes 199-210
                4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,  // opcodes 211-222
                5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,  // opcodes 223-234
                6, 6, 6, 6,  // opcodes 235-238
                7, 7, 7, 7,  // opcodes 239-242
                8, 8, 8, 8,  // opcodes 243-246
                0, 0, 0, 0, 0, 0, 0, 0, 0  // opcodes 247-255
            };
        }

        public bool SetBytes(byte[] items)
        {
            if(items.Length != 256 * 6)
            {
                return false;
            }

            int offset = 0;
            for(int i = 0; i < 256; i++)
            {
                inst1[i] = items[offset + i];
            }
            offset += 256;
            for(int i = 0; i < 256; i++)
            {
                inst2[i] = items[offset + i];
            }
            offset += 256;
            for(int i = 0; i < 256; i++)
            {
                size1[i] = items[offset + i];
            }
            offset += 256;
            for(int i = 0; i < 256; i++)
            {
                size2[i] = items[offset + i];
            }
            offset += 256;
            for(int i = 0; i < 256; i++)
            {
                mode1[i] = items[offset + i];
            }
            offset += 256;
            for(int i = 0; i < 256; i++)
            {
                mode2[i] = items[offset + i];
            }

            return true;
        }

        public IByteBuffer GetBytes()
        {
            List<byte> bytes = new List<byte>();
 
            for(int i = 0; i < inst1.Length; i++)
            {
                bytes.Add(inst1[i]);
            }
            for(int i = 0; i < inst2.Length; i++)
            {
                bytes.Add(inst2[i]);
            }
            for(int i = 0; i < size1.Length; i++)
            {
                bytes.Add(size1[i]);
            }
            for(int i = 0; i < size2.Length; i++)
            {
                bytes.Add(size2[i]);
            }
            for (int i = 0; i < mode1.Length; i++)
            {
                bytes.Add(mode1[i]);
            }
            for (int i = 0; i < mode2.Length; i++)
            {
                bytes.Add(mode2[i]);
            }

            return new ByteBuffer(bytes.ToArray());
        }

        static bool Validate(byte maxMode)
        {
            return true;
        }

        static bool ValidateOpCode(int opcode, byte inst, byte size, byte mode, byte maxMode)
        {
            return true;
        }
    }
}
