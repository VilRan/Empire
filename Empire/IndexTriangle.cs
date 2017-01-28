using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire
{
    public struct IndexTriangle
    {
        public readonly int AIndex, BIndex, CIndex;

        public IndexTriangle(int aIndex, int bIndex, int cIndex)
        {
            AIndex = aIndex;
            BIndex = bIndex;
            CIndex = cIndex;
        }
    }
}
