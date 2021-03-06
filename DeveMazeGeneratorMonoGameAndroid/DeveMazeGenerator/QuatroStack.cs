﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DeveMazeGenerator
{
    public class QuatroStack
    {
        private int cur = -1;
        public QuatroList innerList;

        public QuatroStack()
        {
            innerList = new QuatroList();
        }

        public void Push(int input)
        {
            cur++;
            innerList[cur] = input;
        }

        public int Peek()
        {
            if (cur == -1)
                throw new ArgumentException("Stack is empty");
            return innerList[cur];
        }

        public int Pop()
        {
            if (cur == -1)
                throw new ArgumentException("Stack is empty");
            cur--;
            return innerList[cur + 1];
        }

        public int Count
        {
            get { return cur + 1; }
        }
    }
}
