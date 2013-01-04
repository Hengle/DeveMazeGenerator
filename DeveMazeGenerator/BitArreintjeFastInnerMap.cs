﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DeveMazeGenerator
{
    public class BitArreintjeFastInnerMap : InnerMap
    {
        public override int Length
        {
            get { return innerData.Length; }
        }

        public BitArreintjeFastInnerMap(int width, int height)
        {
            innerData = new InnerMapArray[width];
            for (int i = 0; i < width; i++)
            {
                innerData[i] = new BooleanInnerMapArray(height);
            }
        }

        public override InnerMapArray this[int x]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return innerData[x];
            }
        }

    }
}
