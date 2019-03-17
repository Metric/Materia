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
            min = new float[3];
            max = new float[3];
            mid = new float[3];

            for(int i = 0; i < 3; i++)
            {
                min[i] = 0;
                mid[i] = 0.5f;
                max[i] = 1;
            }
        }

        public MultiRange(float[] min, float[] mid, float[] max)
        {
            this.min = min;
            this.mid = mid;
            this.max = max;
        }
    }
}
