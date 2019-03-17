#region Usings

using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace DDSReader.Utilities
{
    public static class RangeEnumerable
    {
        public static IEnumerable<int> Range(int begin, int endExcl, int stepsize = 1)
        {
            Debug.Assert(stepsize != 0);

            if (stepsize < 0)
            {
                Debug.Assert(begin >= endExcl);
            }
            else
            {
                Debug.Assert(begin <= endExcl);
            }

            for (var i = begin; i < endExcl; i += stepsize)
            {
                yield return i;
            }
        }
    }
}
