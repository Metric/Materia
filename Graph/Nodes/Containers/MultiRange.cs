using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.Containers
{
    public class MultiRange
    {
        public float[] min;
        public float[] mid;
        public float[] max;

        public MultiRange()
        {
            min = new float[4];
            max = new float[4];
            mid = new float[4];

            for(int i = 0; i < 4; ++i)
            {
                min[i] = 0;
                mid[i] = 0.5f;
                max[i] = 1;
            }
        }

        public MultiRange(float[] min, float[] mid, float[] max)
        {
            this.min = new float[4];
            this.max = new float[4];
            this.mid = new float[4];

            for (int i = 0; i < 4; ++i)
            {
                if (i < min.Length)
                {
                    this.min[i] = min[i];
                }
                else
                {
                    this.min[i] = 0;
                }
                if(i < mid.Length)
                {
                    this.mid[i] = mid[i];
                }
                else
                {
                    this.mid[i] = 0.5f;
                }
                if(i < max.Length)
                {
                    this.max[i] = max[i];
                }
                else
                {
                    this.max[i] = 1;
                }
            }
        }
    }
}
