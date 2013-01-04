﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DeveMazeGenerator
{
    public abstract class InnerMap
    {
        ////virtual can be overidden
        //public virtual void Print()
        //{
        //    StringBuilder build = new StringBuilder();
        //    for (int i = 0; i < this.Length / 8; i++)
        //    {
        //        for (int y = 0; y < 8; y++)
        //        {
        //            InnerMapArray b = this[i * 8 + y];
        //            if (b)
        //            {
        //                build.Append('1');
        //            }
        //            else
        //            {
        //                build.Append('0');
        //            }
        //        }
        //        build.Append(' ');
        //    }
        //    Console.WriteLine(build);
        //}

        public InnerMapArray[] innerData;

        //abstract must be overidden
        public abstract InnerMapArray this[int x] { get; }
        public abstract int Length { get; }
    }
}
