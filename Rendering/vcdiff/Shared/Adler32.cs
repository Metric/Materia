namespace VCDiff.Shared
{ 
    public class Adler32
    {
        /// <summary>
        /// Zlib implementation of the Adler32 Hash
        /// </summary>
        const uint BASE = 65521;
        const uint NMAX = 5552;

        public static uint Combine(uint adler1, uint adler2, uint len)
        {
            uint sum1 = 0;
            uint sum2 = 0;
            uint rem = 0;

            rem = (uint)(len % BASE);
            sum1 = adler1 & 0xffff;
            sum2 = rem * sum1;
            sum2 %= BASE;
            sum1 += (adler2 & 0xffff) + BASE - 1;
            sum2 += ((adler1 >> 16) & 0xffff) + ((adler2 >> 16) & 0xffff) + BASE - rem;
            if (sum1 >= BASE) sum1 -= BASE;
            if (sum1 >= BASE) sum1 -= BASE;
            if (sum2 >= (BASE << 1)) sum2 -= (BASE << 1);
            if (sum2 >= BASE) sum2 -= BASE;
            return sum1 | (sum2 << 16);
        }

        public static uint Hash(uint adler, byte[] buff)
        {
            if (buff == null || buff.Length == 0) return 1;

            uint sum2 = 0;
            uint n = 0;

            sum2 = (adler >> 16) & 0xffff;
            adler &= 0xffff;

            if(buff.Length == 1)
            {
                adler += buff[0];
                if(adler >= BASE)
                {
                    adler -= BASE;
                }
                sum2 += adler;
                if(sum2 >= BASE)
                {
                    sum2 -= BASE;
                }
                return adler | (sum2 << 16);
            }

            if(buff.Length < 16)
            {
                for(int i = 0; i < buff.Length; i++)
                {
                    adler += buff[i];
                    sum2 += adler;
                }
                if(adler >= BASE)
                {
                    adler -= BASE;
                }
                sum2 %= BASE;
                return adler | (sum2 << 16);
            }

            uint len = (uint)buff.Length;
            int dof = 0;
            while(len >= NMAX)
            {
                len -= NMAX;
                n = NMAX / 16;
                do
                {
                    DO16(adler, sum2, buff, dof, out adler, out sum2);
                    dof += 16;
                } while (--n > 0);
                adler %= BASE;
                sum2 %= BASE;
            }

            if(len > 0)
            {
                while(len >= 16)
                {
                    len -= 16;
                    DO16(adler, sum2, buff, dof, out adler, out sum2);
                    dof += 16;
                }
                while(len-- > 0)
                {
                    adler += buff[dof++];
                    sum2 += adler;
                }
                adler %= BASE;
                sum2 %= BASE;
            }

            return adler | (sum2 << 16);
        }

        static void DO1(uint adler, uint sum, byte[] buff, int i, out uint ald, out uint s)
        {
            adler += buff[i];
            sum += adler;
            ald = adler;
            s = sum;
        }
        static void DO2(uint adler, uint sum, byte[] buff, int i, out uint ald, out uint s)
        {
            DO1(adler, sum, buff, i, out ald, out s);
            DO1(ald, s, buff, i + 1, out ald, out s);
        }
        static void DO4(uint adler, uint sum, byte[] buff, int i, out uint ald, out uint s)
        {
            DO2(adler, sum, buff, i, out ald, out s);
            DO2(ald, s, buff, i+2, out ald, out s);
        }
        static void DO8(uint adler, uint sum, byte[] buff, int i, out uint ald, out uint s)
        {
            DO4(adler, sum, buff, i, out ald, out s);
            DO4(ald, s, buff, i + 4, out ald, out s);
        }
        static void DO16(uint adler, uint sum, byte[] buff, int i, out uint ald, out uint s)
        {
            DO8(adler, sum, buff, 0 + i, out ald, out s);
            DO8(ald, s, buff, 8 + i, out ald, out s);
        }
    }
}
