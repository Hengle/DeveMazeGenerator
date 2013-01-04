﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DeveMazeGenerator.Generators
{
    public class AlgorithmBacktrackNoInlining : Algorithm
    {
        public Maze Generate(int width, int height, InnerMapType innerMapType)
        {
            Maze maze = new Maze(width, height, innerMapType);
            GoGenerate(maze.InnerMap, maze, new Random());
            return maze;
        }

        public Maze Generate(int width, int height, InnerMapType innerMapType, int seed)
        {
            Maze maze = new Maze(width, height, innerMapType);
            GoGenerate(maze.InnerMap, maze, new Random(seed));
            return maze;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool isValid(int x, int y, InnerMap map, Maze maze)
        {
            //Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            if (x > 0 && x < maze.Width - 1 && y > 0 && y < maze.Height - 1)
            {
                return !map[x][y];
            }
            return false;
        }

        private void GoGenerate(InnerMap map, Maze maze, Random r)
        {

            int x = 1;
            int y = 1;

            Stack<MazePoint> stackje = new Stack<MazePoint>();
            stackje.Push(new MazePoint(x, y));
            map[x][y] = true;
            //form.drawPixel(x, y, brushThisUses);
            while (stackje.Count != 0)
            {

                MazePoint cur = stackje.Peek();
                x = cur.X;
                y = cur.Y;

                List<MazePoint> targets = new List<MazePoint>();
                if (isValid(x - 2, y, map, maze))
                {
                    targets.Add(new MazePoint(x - 2, y));
                }
                if (isValid(x + 2, y, map, maze))
                {
                    targets.Add(new MazePoint(x + 2, y));
                }
                if (isValid(x, y - 2, map, maze))
                {
                    targets.Add(new MazePoint(x, y - 2));
                }
                if (isValid(x, y + 2, map, maze))
                {
                    targets.Add(new MazePoint(x, y + 2));
                }

                //Thread.Sleep(1000);

                if (targets.Count > 0)
                {
                    var target = targets[r.Next(targets.Count)];
                    stackje.Push(target);
                    map[target.X][target.Y] = true;



                    if (target.X < x)
                    {
                        map[x - 1][y] = true;
                        //form.drawPixel(x - 1, y, brushThisUses);
                    }
                    else if (target.X > x)
                    {
                        map[x + 1][y] = true;
                        //form.drawPixel(x + 1, y, brushThisUses);
                    }
                    else if (target.Y < y)
                    {
                        map[x][y - 1] = true;
                        //form.drawPixel(x, y - 1, brushThisUses);
                    }
                    else if (target.Y > y)
                    {
                        map[x][y + 1] = true;
                        //form.drawPixel(x, y + 1, brushThisUses);
                    }

                    //form.drawPixel(target.X, target.Y, brushThisUses);
                }
                else
                {
                    stackje.Pop();
                }


            }
        }
    }
}
