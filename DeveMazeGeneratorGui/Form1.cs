﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeveMazeGenerator;
using DeveMazeGenerator.Generators;
using System.IO;
using System.Threading;
using System.Drawing.Imaging;
using DeveMazeGenerator.Generators.Tests;
using DeveMazeGenerator.PathFinders;

namespace DeveMazeGeneratorGui
{
    public partial class Form1 : Form
    {
        private Maze lastMegaTerrorMaze = null;
        private List<MazePoint> lastMegaTerrorMazePath = null;
        private List<MazePointPos> lastMegaTerrorMazePathPos = null;
        private QuatroStack lastMegaTerrorMazeQuatroDirections = null;

        private Boolean forceesStoppenEnzo = false;
        private Random r = new Random();

        private int curDelay;

        private long totalStepsToCalcPercentage = 100;
        private long currentStepsToCalcPercentage = 0;

        private long lastSteps = 0;
        private Stopwatch stopwatchTimeSinceLastTimerTick = Stopwatch.StartNew();

        private int curXInMaze = 0;
        private int curYInMaze = 0;

        private int mazeLinesToSave = 0;
        private int curMazeLineSaving = 0;

        private StreamWriter debugMsgWriter;

        public Form1()
        {
            debugMsgWriter = new StreamWriter(new FileStream("debugoutput.txt", FileMode.Create, FileAccess.Write, FileShare.Read)); //Autoflush is false because then it writes after every char. We do it every line.

            InitializeComponent();
            comboBox1.SelectedIndex = 1;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 1;
            curDelay = int.Parse(comboBox2.SelectedItem.ToString());

            for (int i = 10; i < 30; i++)
            {
                comboBox4.Items.Add(ConvertNumberToNiceString((long)Math.Pow(2, i)));
            }
            comboBox4.SelectedIndex = 9;

            foreach (var saveType in Enum.GetValues(typeof(MazeSaveFileType)))
            {
                comboBoxImageSaveType.Items.Add(saveType);
            }
            comboBoxImageSaveType.SelectedIndex = 1;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Algorithm curalg = new AlgorithmBacktrack();
                Stopwatch w = new Stopwatch();
                w.Start();
                int size = (int)Math.Pow(2.0, 10.0);
                DebugMSG("Generating maze of size: " + size);
                DebugMSG("Saved size it should be: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 8.0 + " mb");
                Maze maze = curalg.Generate(size, size, InnerMapType.BitArreintjeFast, 1337, null);
                w.Stop();
                DebugMSG("Generating time: " + w.Elapsed.TotalSeconds);
                DebugMSG("Finding path...");
                w.Reset();
                w.Start();
                var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(size - 3, size - 3), maze.InnerMap, null);
                w.Stop();
                DebugMSG("Time it took to find path: " + w.Elapsed.TotalSeconds);

                DebugMSG("Saving...");
                w.Reset();
                w.Start();

                maze.SaveMazeAsImage("zzzzzzzzzzz2.bmp", ImageFormat.Bmp, path, MazeSaveType.ColorDepth4Bits);
                DebugMSG("Done saving, saving time: " + w.Elapsed.TotalSeconds);
                DebugMSG("Location: " + System.IO.Directory.GetCurrentDirectory());
            });

        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Boolean test(int x, int y)
        {
            //DebugMSG(System.Reflection.MethodBase.GetCurrentMethod().Name);
            return x == y;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Boolean test2(int x, int y)
        {
            //DebugMSG(System.Reflection.MethodBase.GetCurrentMethod().Name);
            return x == y;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
                {
                    Directory.CreateDirectory("bigmazes");

                    Stopwatch w = new Stopwatch();
                    w.Start();

                    Algorithm alg = new AlgorithmBacktrack();
                    ParallelOptions options = new ParallelOptions();
                    //options.MaxDegreeOfParallelism = 8;
                    Parallel.For(0, 1000, options, new Action<int>((i) =>
                    {
                        if (i % 50 == 0)
                        {
                            DebugMSG(i / 10 + "%");
                        }
                        int size = 1024;
                        Maze maze = alg.Generate(size, size, InnerMapType.BitArreintjeFast, i, null);
                        var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(size - 3, size - 3), maze.InnerMap, null);
                        maze.SaveMazeAsImage("bigmazes\\" + i + ".bmp", ImageFormat.Bmp, path, MazeSaveType.ColorDepth4Bits);
                    }));

                    w.Stop();
                    DebugMSG("Done: " + w.Elapsed.TotalSeconds);
                });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Directory.CreateDirectory("mazes");

                Stopwatch w = new Stopwatch();
                w.Start();

                Algorithm alg = new AlgorithmBacktrack();
                ParallelOptions options = new ParallelOptions();
                //options.MaxDegreeOfParallelism = 8;
                for (int i = 0; i < 1000; i++)
                {
                    if (i % 50 == 0)
                    {
                        DebugMSG(i / 10 + "%");
                    }
                    int size = 256;
                    Maze maze = alg.Generate(size, size, InnerMapType.BitArreintjeFast, i, null);
                    var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(size - 3, size - 3), maze.InnerMap, null);
                    maze.SaveMazeAsImage("mazes\\" + i + ".bmp", ImageFormat.Bmp, path, MazeSaveType.ColorDepth4Bits);
                };

                w.Stop();
                DebugMSG("Done: " + w.Elapsed.TotalSeconds);
            });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Task.Run(new Action(() =>
            {
                Algorithm curalg = new AlgorithmBacktrack();
                Stopwatch w = new Stopwatch();
                int size = (int)Math.Pow(2.0, 15.0);
                DebugMSG("Generating maze of size: " + size);
                DebugMSG("Saved size it should be: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 8.0 + " mb");
                w.Start();
                Maze maze = curalg.Generate(size, size, InnerMapType.BitArreintjeFast, 1337, null);
                w.Stop();
                DebugMSG("Generating time: " + w.Elapsed.TotalSeconds);
                DebugMSG("Finding path...");
                w.Reset();
                w.Start();
                var path = PathFinderDepthFirstSmart.GoFind(maze.InnerMap, null);
                w.Stop();
                DebugMSG("Time it took to find path: " + w.Elapsed.TotalSeconds);
                DebugMSG("Path length: " + path.Count);

                GC.Collect();

                DebugMSG("Saving...");
                w.Reset();
                w.Start();

                maze.SaveMazeAsImage("bigmazeetc.png", ImageFormat.Png, path, MazeSaveType.ColorDepth4Bits);
                maze.SaveMazeAsImage("bigmazeetc.bmp", ImageFormat.Bmp, path, MazeSaveType.ColorDepth4Bits);
                DebugMSG("Done saving, saving time: " + w.Elapsed.TotalSeconds);
                DebugMSG("Location: " + System.IO.Directory.GetCurrentDirectory());
                maze = null;
                path = null;
                GC.Collect();
            }));
        }

        //private void button6_Click(object sender, EventArgs e)
        //{
        //    Task.Run(() =>
        //    {
        //        Stopwatch w = new Stopwatch();

        //        ParallelOptions options = new ParallelOptions();
        //        //options.MaxDegreeOfParallelism = 8;
        //        Algorithm alg;

        //        w.Start();
        //        alg = new AlgorithmBacktrack();
        //        Parallel.For(0, 1000, options, new Action<int>((i) =>
        //        {
        //            //if (i % 50 == 0)
        //            //{
        //            //    DebugMSG(i / 10 + "%");
        //            //}
        //            int size = 256;
        //            Maze maze = alg.Generate(size, size, InnerMapType.BitArreintjeFast, i, null);
        //            //var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(size - 3, size - 3), maze.InnerMap);
        //            //maze.SaveMazeAsBmpWithPath2("mazes\\" + i + ".bmp", path);
        //        }));
        //        w.Stop();
        //        DebugMSG("Time aggressive: " + w.Elapsed.TotalSeconds);
        //        w.Reset();

        //        w.Start();
        //        alg = new AlgorithmBacktrackNoInlining();
        //        Parallel.For(0, 1000, options, new Action<int>((i) =>
        //        {
        //            //if (i % 50 == 0)
        //            //{
        //            //    DebugMSG(i / 10 + "%");
        //            //}
        //            int size = 256;
        //            Maze maze = alg.Generate(size, size, InnerMapType.BitArreintjeFast, i);
        //            //var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(size - 3, size - 3), maze.InnerMap);
        //            //maze.SaveMazeAsBmpWithPath2("mazes\\" + i + ".bmp", path);
        //        }));
        //        w.Stop();
        //        DebugMSG("Time noinlining: " + w.Elapsed.TotalSeconds);
        //    });
        //}

        //private void button7_Click(object sender, EventArgs e)
        //{
        //    Task.Run(() =>
        //    {
        //        Stopwatch w = new Stopwatch();

        //        ParallelOptions options = new ParallelOptions();
        //        //options.MaxDegreeOfParallelism = 8;
        //        Algorithm alg;

        //        for (int y = 0; y < 100; y++)
        //        {
        //            DebugMSG("----------------------");
        //            w.Start();
        //            alg = new AlgorithmBacktrack();
        //            for (int i = 0; i < 1000; i++)
        //            {
        //                //if (i % 50 == 0)
        //                //{
        //                //    DebugMSG(i / 10 + "%");
        //                //}
        //                int size = 256;
        //                Maze maze = alg.Generate(size, size, InnerMapType.BitArreintjeFast, i);
        //                //var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(size - 3, size - 3), maze.InnerMap);
        //                //maze.SaveMazeAsBmpWithPath2("mazes\\" + i + ".bmp", path);
        //            }
        //            w.Stop();
        //            DebugMSG("Time aggressive: " + w.Elapsed.TotalSeconds);
        //            w.Reset();

        //            w.Start();
        //            alg = new AlgorithmBacktrackNoInlining();
        //            for (int i = 0; i < 1000; i++)
        //            {
        //                //if (i % 50 == 0)
        //                //{
        //                //    DebugMSG(i / 10 + "%");
        //                //}
        //                int size = 256;
        //                Maze maze = alg.Generate(size, size, InnerMapType.BitArreintjeFast, i);
        //                //var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(size - 3, size - 3), maze.InnerMap);
        //                //maze.SaveMazeAsBmpWithPath2("mazes\\" + i + ".bmp", path);
        //            }
        //            w.Stop();

        //            DebugMSG("Time noinlining: " + w.Elapsed.TotalSeconds);
        //            w.Reset();
        //        }
        //    });
        //}

        //private void button8_Click(object sender, EventArgs e)
        //{
        //    Task.Run(() =>
        //    {
        //        Stopwatch w = new Stopwatch();
        //        int loops = 1000;

        //        for (int y = 0; y < 1; y++)
        //        {
        //            DebugMSG("----------------------");
        //            AlgorithmBacktrack alg = new AlgorithmBacktrack();
        //            w.Start();
        //            for (int i = 0; i < loops; i++)
        //            {
        //                //if (i % 50 == 0)
        //                //{
        //                //    DebugMSG(i / 10 + "%");
        //                //}
        //                int size = 256;
        //                Maze maze = alg.Generate(size, size, InnerMapType.BitArreintjeFast, i);
        //                //var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(size - 3, size - 3), maze.InnerMap);
        //                //maze.SaveMazeAsBmpWithPath2("mazes\\" + i + ".bmp", path);
        //            }
        //            w.Stop();
        //            DebugMSG("Time AlgorithmBacktrack: " + w.Elapsed.TotalSeconds);
        //            w.Reset();

        //            AlgorithmBacktrackWithCallback alg2 = new AlgorithmBacktrackWithCallback();
        //            w.Start();
        //            for (int i = 0; i < loops; i++)
        //            {
        //                //if (i % 50 == 0)
        //                //{
        //                //    DebugMSG(i / 10 + "%");
        //                //}
        //                int size = 256;
        //                Maze maze = alg2.SpecialGenerate(size, size, InnerMapType.BitArreintjeFast, i, (xx, yy) => { ChangeEenPixel(xx, yy); });
        //                //var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(size - 3, size - 3), maze.InnerMap);
        //                //maze.SaveMazeAsBmpWithPath2("mazes\\" + i + ".bmp", path);
        //            }
        //            w.Stop();

        //            DebugMSG("Time AlgorithmBacktrackWithCallback: " + w.Elapsed.TotalSeconds);
        //            w.Reset();

        //            AlgorithmBacktrackWithCallback alg3 = new AlgorithmBacktrackWithCallback();
        //            w.Start();
        //            for (int i = 0; i < loops; i++)
        //            {
        //                //if (i % 50 == 0)
        //                //{
        //                //    DebugMSG(i / 10 + "%");
        //                //}
        //                int size = 256;
        //                Maze maze = alg3.SpecialGenerate(size, size, InnerMapType.BitArreintjeFast, i, null);
        //                //var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(size - 3, size - 3), maze.InnerMap);
        //                //maze.SaveMazeAsBmpWithPath2("mazes\\" + i + ".bmp", path);
        //            }
        //            w.Stop();

        //            DebugMSG("Time AlgorithmBacktrackWithCallback met nullcallback: " + w.Elapsed.TotalSeconds);
        //            w.Reset();
        //        }
        //    });
        //}

        public void ChangeEenPixel(int x, int y)
        {
            int b = x + y;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Stopwatch w = new Stopwatch();

                for (int y = 0; y < 10; y++)
                {
                    DebugMSG("----------------------");
                    AlgorithmBacktrack alg = new AlgorithmBacktrack();
                    w.Start();
                    for (int i = 0; i < 800; i++)
                    {
                        //if (i % 50 == 0)
                        //{
                        //    DebugMSG(i / 10 + "%");
                        //}
                        int size = 256;
                        Maze maze = alg.Generate(size, size, InnerMapType.BitArreintjeFast, i, null);
                        //var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(size - 3, size - 3), maze.InnerMap);
                        //maze.SaveMazeAsBmpWithPath2("mazes\\" + i + ".bmp", path);
                    }
                    w.Stop();
                    DebugMSG("Time AlgorithmBacktrack1: " + w.Elapsed.TotalSeconds);
                    w.Reset();

                    AlgorithmBacktrack alg2 = new AlgorithmBacktrack();
                    w.Start();
                    for (int i = 0; i < 800; i++)
                    {
                        //if (i % 50 == 0)
                        //{
                        //    DebugMSG(i / 10 + "%");
                        //}
                        int size = 256;
                        Maze maze = alg2.Generate(size, size, InnerMapType.BitArreintjeFast, i, null);
                        //var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(size - 3, size - 3), maze.InnerMap);
                        //maze.SaveMazeAsBmpWithPath2("mazes\\" + i + ".bmp", path);
                    }
                    w.Stop();

                    DebugMSG("Time AlgorithmBacktrack2: " + w.Elapsed.TotalSeconds);
                    w.Reset();
                }
            });
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                DebugMSG("Generating...");
                Stopwatch w = new Stopwatch();
                AlgorithmBacktrack algje = new AlgorithmBacktrack();
                w.Start();
                algje.Generate(20000, 20000, InnerMapType.BitArreintjeFast, null);
                w.Stop();
                DebugMSG("Seconds it took: " + w.Elapsed.TotalSeconds);
            });
        }


        public void DebugMSG(String str)
        {
            str = DateTime.Now.ToLongTimeString() + ":    " + str;

            lock (debugMsgWriter)
            {
                debugMsgWriter.WriteLine(str);
                debugMsgWriter.Flush();
            }
            listBox1.Invoke(new Action(() =>
            {
                listBox1.Items.Insert(0, str);
            }));
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                AlgorithmBacktrack back = new AlgorithmBacktrack();

                int width = 16;
                int height = 16;
                Maze maze = back.Generate(width, height, InnerMapType.BitArreintjeFast, null);
                var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(width - 3, height - 3), maze.InnerMap, null);
                maze.SaveMazeAsImage("mazePath.bmp", ImageFormat.Bmp, path, MazeSaveType.ColorDepth4Bits);
                maze.SaveMazeAsImage("maze1.bmp", ImageFormat.Bmp);

                //foreach (var v in maze.InnerMap)
                //{
                //    v.Print();
                //}

                var walls = maze.GenerateListOfMazeWalls();

                foreach (var wall in walls)
                {
                    DebugMSG("New wall found: " + wall.xstart + ":" + wall.ystart + "  " + wall.xend + ":" + wall.yend);
                }

                Maze loadedfromwall = Maze.LoadMazeFromWalls(walls, width, height);
                loadedfromwall.SaveMazeAsImage("maze2.bmp", ImageFormat.Bmp);

                DebugMSG("Ok done");
            });
        }



        private void GenerateMazeWithThisTypeAndShowTime(InnerMapType type, int size)
        {
            Stopwatch w = new Stopwatch();
            w.Start();

            AlgorithmBacktrack back = new AlgorithmBacktrack();
            DebugMSG("Generating maze of type: " + type + " of size: " + size);
            Maze maze = back.Generate(size, size, type, (x, y, cur, tot) =>
            {
                curXInMaze = x;
                curYInMaze = y;
                currentStepsToCalcPercentage = cur;
                totalStepsToCalcPercentage = tot;
            });
            //var path = PathFinderDepthFirstSmart.GoFind(new MazePoint(1, 1), new MazePoint(size - 3, size - 3), maze.InnerMap);
            //maze.SaveMazeAsBmpWithPath4bpp("maze.bmp", path);
            DebugMSG("Ok done, time: " + w.Elapsed.TotalSeconds);
            DebugMSG("");

        }

        private void button12_Click(object sender, EventArgs e)
        {
            Task t = Task.Run(new Action(() =>
            {
                try
                {
                    DebugMSG("---------------");
                    int size = 2048;
                    GenerateMazeWithThisTypeAndShowTime(InnerMapType.BitArrayMappedOnHardDisk, size);
                    GenerateMazeWithThisTypeAndShowTime(InnerMapType.BitArreintjeFast, size);
                    GenerateMazeWithThisTypeAndShowTime(InnerMapType.BooleanArray, size);
                    GenerateMazeWithThisTypeAndShowTime(InnerMapType.DotNetBitArray, size);
                    GenerateMazeWithThisTypeAndShowTime(InnerMapType.Hybrid, size);
                }
                catch (Exception eee)
                {
                    DebugMSG(eee.ToString());
                }
            }));
        }

        //Vind langste path
        private void button13_Click(object sender, EventArgs e)
        {
            forceesStoppenEnzo = false;
            int hoeveelHijErDoetPerKeer = 10000000;
            int size = 1024 * 8;

            Task.Run(() =>
            {
                object lockertje = new object();
                int curLongest = 0;
                Maze curLongestMaze = null;
                List<MazePoint> pathlongest = null;

                Directory.CreateDirectory("longestpath");

                foreach (String str in Directory.GetFiles("longestpath"))
                {
                    String filename = Path.GetFileNameWithoutExtension(str);
                    int length = int.Parse(filename);
                    if (length > curLongest)
                    {
                        curLongest = length;
                    }
                }

                DebugMSG("Current longest maze: " + curLongest);

                Random r = new Random();
                int starthere = r.Next(int.MaxValue - hoeveelHijErDoetPerKeer);

                Algorithm alg = new AlgorithmBacktrack();

                Parallel.For(starthere, starthere + hoeveelHijErDoetPerKeer, new Action<int>((i) =>
                {
                    if (forceesStoppenEnzo)
                    {

                    }
                    else
                    {
                        Maze m = alg.Generate(size, size, InnerMapType.BitArreintjeFast, i, null);
                        var path = PathFinderDepthFirstSmart.GoFind(m.InnerMap, null);

                        DebugMSG("Weer een maze done, path length: " + path.Count);

                        if (path.Count > curLongest)
                        {
                            lock (lockertje)
                            {
                                if (path.Count > curLongest)
                                {
                                    DebugMSG("New longest maze found, length: " + path.Count);
                                    curLongest = path.Count;
                                    curLongestMaze = m;
                                    pathlongest = path;
                                    m.SaveMazeAsImage("longestpath\\" + path.Count.ToString() + ".bmp", ImageFormat.Bmp, path, MazeSaveType.ColorDepth4Bits);
                                }
                            }

                        }
                    }
                }));



                DebugMSG("Done met dit setje :)");





            });
        }

        //Vind korste path
        private void button14_Click(object sender, EventArgs e)
        {
            forceesStoppenEnzo = false;
            int hoeveelHijErDoetPerKeer = 10000000;
            int size = 1024;

            Task.Run(() =>
            {
                object lockertje = new object();
                int curLongest = int.MaxValue;
                Maze curLongestMaze = null;
                List<MazePoint> pathshortest = null;

                Directory.CreateDirectory("shortestpath");

                foreach (String str in Directory.GetFiles("shortestpath"))
                {
                    String filename = Path.GetFileNameWithoutExtension(str);
                    int length = int.Parse(filename);
                    if (length < curLongest)
                    {
                        curLongest = length;
                    }
                }

                DebugMSG("Current shortest maze: " + curLongest);

                Random r = new Random();
                int starthere = r.Next(int.MaxValue - hoeveelHijErDoetPerKeer);

                Algorithm alg = new AlgorithmBacktrack();

                Parallel.For(starthere, starthere + hoeveelHijErDoetPerKeer, new Action<int>((i) =>
                {
                    if (forceesStoppenEnzo)
                    {

                    }
                    else
                    {
                        Maze m = alg.Generate(size, size, InnerMapType.BitArreintjeFast, i, null);
                        var path = PathFinderDepthFirstSmart.GoFind(m.InnerMap, null);

                        if (path.Count < curLongest)
                        {
                            lock (lockertje)
                            {
                                if (path.Count < curLongest)
                                {
                                    DebugMSG("New shortest maze found, length: " + path.Count);
                                    curLongest = path.Count;
                                    curLongestMaze = m;
                                    pathshortest = path;
                                    m.SaveMazeAsImage("shortestpath\\" + path.Count.ToString() + ".bmp", ImageFormat.Bmp, path, MazeSaveType.ColorDepth4Bits);
                                }
                            }
                        }
                    }
                }));

                DebugMSG("Done met dit setje :)");
            });
        }

        private void button15_Click(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            forceesStoppenEnzo = true;
        }

        private void button17_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                AlgorithmBacktrack back = new AlgorithmBacktrack();
                int width = 2500;
                int height = 600;

                List<InnerMapType> mapTypes = ((InnerMapType[])Enum.GetValues(typeof(InnerMapType))).ToList();
                List<Algorithm> algjes = new List<Algorithm>();
                algjes.Add(new AlgorithmBacktrack());
                algjes.Add(new AlgorithmBacktrackSmartMemory());
                algjes.Add(new AlgorithmBacktrackFastWithoutAction());
                algjes.Add(new AlgorithmBacktrackFastWithoutActionAndMaze());
                algjes.Add(new AlgorithmBacktrackFastWithoutActionAndMazeAndFastRandomFastStackList());
                algjes.Add(new AlgorithmBacktrackFastWithoutActionAndMazeAndFastRandomFastStackArray());
                algjes.Add(new AlgorithmBacktrackFastWithoutActionAndMazeAndFastRandomFastStackArray2());
                algjes.Add(new AlgorithmBacktrackFastWithoutActionAndMazeAndFastRandomFastStackArray3());

                DebugMSG("Comparing all backtrack algorithms + all InnerMaps...");
                DebugMSG("Generating reference maze...");
                Maze referenceMaze = back.Generate(width, height, InnerMapType.BooleanArray, 500, null);
                DebugMSG("Generating reference done, generating all the other stuff...");

                foreach (var algje in algjes)
                {
                    foreach (var innerMapType in mapTypes)
                    {
                        Maze tocompare = algje.Generate(width, height, innerMapType, 500, null);

                        Boolean theyarethesame = true;

                        try
                        {
                            for (int xx = 0; xx < width; xx++)
                            {
                                for (int yy = 0; yy < height; yy++)
                                {
                                    if (referenceMaze.InnerMap[xx, yy] != tocompare.InnerMap[xx, yy])
                                    {
                                        theyarethesame = false;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            theyarethesame = false;
                        }

                        if (!theyarethesame)
                        {
                            DebugMSG("ERRRROR: " + algje.GetType().Name + ", " + innerMapType);
                        }
                        else
                        {
                            DebugMSG(algje.GetType().Name + ", " + innerMapType + " success :)");
                        }
                        Thread.Sleep(50); //Just in case random filenames are the same
                    }
                }

                DebugMSG("Done comparing algorithms + innermaps");


                DebugMSG("Generating maze of size: " + width + " * " + height);
                Maze maze = back.Generate(width, height, InnerMapType.BitArrayMappedOnHardDisk, null);

                DebugMSG("Generating done :)");


                maze.SaveMazeAsImage("test-maze1.bmp", ImageFormat.Bmp);
                DebugMSG("Maze saved");


                //foreach (var v in maze.InnerMap)
                //{
                //    v.Print();
                //}

                DebugMSG("Finding walls...");
                var walls = maze.GenerateListOfMazeWalls();
                DebugMSG("Walls found");

                DebugMSG("Creating new Maze from walls...");

                Maze loadedfromwall = Maze.LoadMazeFromWalls(walls, width, height);

                DebugMSG("Created :), saving this maze...");
                loadedfromwall.SaveMazeAsImage("test-maze2.bmp", ImageFormat.Bmp);

                DebugMSG("Done saving, creating path for this maze...");

                var path = PathFinderDepthFirst.GoFind(loadedfromwall.InnerMap, null);
                var path2 = PathFinderDepthFirstSmart.GoFind(loadedfromwall.InnerMap, null);

                DebugMSG("Path found, comparing path from 2 different path finders...");

                if (path.Count != path2.Count)
                {
                    DebugMSG("ERRORRRRRRR: path length not equals!!!");
                }

                var itworkedhereforpathfinding = true;
                for (int i = 0; i < path.Count; i++)
                {
                    if (path[i].X != path2[i].X || path[i].Y != path2[i].Y)
                    {
                        itworkedhereforpathfinding = false;
                    }
                }
                if (!itworkedhereforpathfinding)
                {
                    DebugMSG("ERRORRRRRRR: path finder not working correctly!!!!");
                }

                DebugMSG("Pathfinding done, saving this maze + path...");

                loadedfromwall.SaveMazeAsImage("test-mazePath4bpp.bmp", ImageFormat.Bmp, path, MazeSaveType.ColorDepth4Bits);
                loadedfromwall.SaveMazeAsImage("test-mazePath32bpp.bmp", ImageFormat.Bmp, path, MazeSaveType.ColorDepth32Bits);
                loadedfromwall.SaveMazeAsImage("test-mazePath4bpp.png", ImageFormat.Png, path, MazeSaveType.ColorDepth4Bits);
                loadedfromwall.SaveMazeAsImage("test-mazePath32bpp.png", ImageFormat.Png, path, MazeSaveType.ColorDepth32Bits);

                DebugMSG("Saving done :)");

                DebugMSG("Generating walls from this thing again...");

                var walls2 = loadedfromwall.GenerateListOfMazeWalls();

                DebugMSG("Done with walls2");

                DebugMSG("Creating and saving new maze for this...");
                Maze mmmmmm = Maze.LoadMazeFromWalls(walls2, width, height);
                mmmmmm.SaveMazeAsImage("test-maze3.bmp", ImageFormat.Bmp);

                DebugMSG("Ok done :), comparing maze 1 and 3...");

                Boolean thesame = true;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (maze.InnerMap[x, y] != mmmmmm.InnerMap[x, y])
                        {
                            thesame = false;
                        }
                    }
                }

                if (thesame == false)
                {
                    DebugMSG("ERRORRRRRRR: Mazes are not the same...");
                }
                else
                {
                    DebugMSG("Mazes are the same :)");

                    DebugMSG("Check out if the generated files look correct (all files with test-....)");
                    DebugMSG("if they are (and no errors were displayed here), all tests succeeded");
                    DebugMSG("and the base of the framework should work :>");
                }

                //Do a garbage collection afterwards
                new Thread(() =>
                {
                    Thread.Sleep(5000);
                    GC.Collect(int.MaxValue, GCCollectionMode.Forced, true);
                }) { IsBackground = true }.Start();

                DebugMSG("Ok done :D");
            });
        }

        private void button18_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                AlgorithmBacktrack back = new AlgorithmBacktrack();

                int width = 1024 * 8;
                int height = 1024 * 8;

                DebugMSG("Generating maze of size: " + width + " * " + height);
                Maze maze = back.Generate(width, height, InnerMapType.BitArreintjeFast, null);

                DebugMSG("Generating done");

                DebugMSG("Saving as bmp");

                maze.SaveMazeAsImage("mazeSavedDirectly.bmp", ImageFormat.Bmp);

                DebugMSG("Saving done :)");

                DebugMSG("Generating walls...");

                var walls = maze.GenerateListOfMazeWalls();

                DebugMSG("Generating walls done :)");

                DebugMSG("Saving walls to file...");

                using (FileStream stream = new FileStream("mazeSavedAsWalls.txt", FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        foreach (MazeWall w in walls)
                        {
                            writer.Write(w.xstart);
                            writer.Write(w.xend);
                            writer.Write(w.ystart);
                            writer.Write(w.yend);
                        }
                    }
                }

                DebugMSG("Everything done \\o/");
            });
        }

        private void button19_Click(object sender, EventArgs e)
        {
            Task t = Task.Run(new Action(() =>
            {
                try
                {
                    DebugMSG("---------------");
                    int size = 2048 * 8;
                    GenerateMazeWithThisTypeAndShowTime(InnerMapType.BitArreintjeFast, size);
                    GenerateMazeWithThisTypeAndShowTime(InnerMapType.BooleanArray, size);
                    GenerateMazeWithThisTypeAndShowTime(InnerMapType.DotNetBitArray, size);
                    GenerateMazeWithThisTypeAndShowTime(InnerMapType.Hybrid, size);
                }
                catch (Exception eee)
                {
                    DebugMSG(eee.ToString());
                }
            }));
        }

        private void button20_Click(object sender, EventArgs e)
        {
            int size = int.Parse(comboBox3.Text);

            Task.Run(() =>
            {
                var g = this.CreateGraphics();

                int width = (this.Width / size - 12 / size) / 2 * 2;
                int height = (this.Height / size - 40 / size) / 2 * 2;

                g.FillRectangle(Brushes.Black, 0, 0, this.Width + 1, this.Height + 1);

                Algorithm curalg;
                if (r.Next(2) == 0)
                {
                    curalg = new AlgorithmBacktrack();
                }
                else
                {
                    curalg = new AlgorithmKruskal();
                }



                Maze m = curalg.Generate(width, height, InnerMapType.BitArreintjeFast, r.Next(), (x, y, cur, tot) =>
                {
                    curXInMaze = x;
                    curYInMaze = y;
                    currentStepsToCalcPercentage = cur;
                    totalStepsToCalcPercentage = tot;
                    g.FillRectangle(Brushes.White, x * size, y * size, size, size);
                    //Thread.Sleep(200);
                });

                var path = PathFinderDepthFirstSmart.GoFind(m.InnerMap, (x, y, pathThing) =>
                {
                    if (pathThing)
                    {
                        g.FillRectangle(Brushes.Green, x * size, y * size, size, size);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.Gray, x * size, y * size, size, size);
                    }

                });

            });
        }

        private void button21_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                int size = 2048 * 2;
                DebugMSG("Generating...");
                Maze m = new AlgorithmBacktrack().Generate(size, size, InnerMapType.BitArreintjeFast, null);
                var path = PathFinderDepthFirstSmart.GoFind(m.InnerMap, null);

                DebugMSG("Saving...");
                DebugMSG("0/4");
                m.SaveMazeAsImage("zz4bitpath.bmp", ImageFormat.Bmp, path, MazeSaveType.ColorDepth4Bits);
                DebugMSG("1/4");
                m.SaveMazeAsImage("zz32bitpath.bmp", ImageFormat.Bmp, path, MazeSaveType.ColorDepth32Bits);
                DebugMSG("2/4");
                m.SaveMazeAsImage("zz4bitpath.png", ImageFormat.Png, path, MazeSaveType.ColorDepth4Bits);
                DebugMSG("3/4");
                m.SaveMazeAsImage("zz32bitpath.png", ImageFormat.Png, path, MazeSaveType.ColorDepth32Bits);
                DebugMSG("4/4");

                DebugMSG("Done with images :)");
                DebugMSG("Just saving the maze as walls...");

                var walls = m.GenerateListOfMazeWalls();



                using (FileStream stream = new FileStream("mazeSavedAsWalls.txt", FileMode.Create))
                {
                    BinaryWriter writer = new BinaryWriter(stream);

                    foreach (MazeWall w in walls)
                    {
                        writer.Write(w.xstart);
                        writer.Write(w.xend);
                        writer.Write(w.ystart);
                        writer.Write(w.yend);
                    }
                }

                DebugMSG("Done with all :)");

            });
        }

        private void button2_Click(object sender, EventArgs e)
        {

            //this.BackColor = Color.Black;

            Task.Run(() =>
            {
                GenerateMazeInPanel1();
            });

        }

        private void GenerateMazeInPanel1()
        {
            var g = panel1.CreateGraphics();

            int sizemodifier = 1;

            this.Invoke(new Action(() =>
            {
                sizemodifier = int.Parse(comboBox1.SelectedItem.ToString());
            }));

            int width = panel1.Width;
            int height = panel1.Height;

            int mazeWidth = width / sizemodifier / 2 * 2;
            int mazeHeight = height / sizemodifier / 2 * 2;

            //int width = 100;
            //int height = 150;

            g.FillRectangle(Brushes.Black, 0, 0, width + 1, height + 1);

            Maze m = new AlgorithmBacktrack().Generate(mazeWidth, mazeHeight, InnerMapType.BitArreintjeFast, r.Next(), (x, y, cur, tot) =>
            {

                Thread.Sleep(curDelay);

                g.FillRectangle(Brushes.White, x * sizemodifier, y * sizemodifier, sizemodifier, sizemodifier);

                curXInMaze = x;
                curYInMaze = y;

                this.currentStepsToCalcPercentage = cur;
                this.totalStepsToCalcPercentage = tot;

                //Thread.Sleep(200);
            });


            var path = PathFinderDepthFirstSmart.GoFind(m.InnerMap, (x, y, pathThing) =>
            {

                Thread.Sleep(curDelay);

                if (pathThing)
                {
                    g.FillRectangle(Brushes.Green, x * sizemodifier, y * sizemodifier, sizemodifier, sizemodifier);
                }
                else
                {
                    g.FillRectangle(Brushes.Gray, x * sizemodifier, y * sizemodifier, sizemodifier, sizemodifier);
                }

            });

            foreach (var pathnode in path)
            {
                g.FillRectangle(Brushes.Red, pathnode.X * sizemodifier, pathnode.Y * sizemodifier, sizemodifier, sizemodifier);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
              {
                  while (true)
                  {

                      GenerateMazeInPanel1();

                  }
              });
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            curDelay = int.Parse(comboBox2.SelectedItem.ToString());
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Task.Run(new Action(() =>
            {
                AlgorithmBacktrack curalg = new AlgorithmBacktrack();
                Stopwatch w = new Stopwatch();
                w.Start();
                int size = (int)Math.Pow(2.0, 15.0);
                DebugMSG("Generating maze of size: " + size);
                DebugMSG("Saved size it should be: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 8.0 + " mb");
                DebugMSG("Or in GB: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 1024.0 / 8.0 + " gb");
                Maze maze = curalg.Generate(size, size, InnerMapType.BitArreintjeFast, 1337, (x, y, cur, tot) =>
                {
                    curXInMaze = x;
                    curYInMaze = y;
                    currentStepsToCalcPercentage = cur;
                    totalStepsToCalcPercentage = tot;
                });
                w.Stop();
                DebugMSG("Generating time: " + w.Elapsed.TotalSeconds);

            }));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            long perSecond = (long)Math.Max((1.0 / stopwatchTimeSinceLastTimerTick.Elapsed.TotalSeconds * (double)(currentStepsToCalcPercentage - lastSteps)), 0.0);
            lastSteps = currentStepsToCalcPercentage;
            stopwatchTimeSinceLastTimerTick.Restart();
            label10.Text = ConvertNumberToNiceString(perSecond);
            double percentage = (double)currentStepsToCalcPercentage / (double)totalStepsToCalcPercentage * 100.0;
            label5.Text = Math.Round(percentage, 2).ToString();

            label8.Text = ConvertNumberToNiceString(currentStepsToCalcPercentage) + " / " + ConvertNumberToNiceString(totalStepsToCalcPercentage);

            label13.Text = curXInMaze.ToString();
            label14.Text = curYInMaze.ToString();

            label16.Text = curMazeLineSaving + "/" + mazeLinesToSave;
        }


        private String ConvertNumberToNiceString(long number)
        {
            StringBuilder build = new StringBuilder();
            build.Append(number);
            int prelength = build.Length;
            for (int i = 3; i < prelength; i += 3)
            {
                build.Insert(prelength - i, ".");
            }
            return build.ToString();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int size = int.Parse(comboBox3.Text);

            Task.Run(() =>
            {
                var g = this.CreateGraphics();

                int width = (this.Width / size - 12 / size) / 2 * 2;
                int height = (this.Height / size - 40 / size) / 2 * 2;

                g.FillRectangle(Brushes.White, 0, 0, this.Width + 1, this.Height + 1);

                Maze m = new AlgorithmBacktrack().Generate(width, height, InnerMapType.BitArreintjeFast, r.Next(), (x, y, cur, tot) =>
                {
                    curXInMaze = x;
                    curYInMaze = y;
                    currentStepsToCalcPercentage = cur;
                    totalStepsToCalcPercentage = tot;
                    //g.FillRectangle(Brushes.White, x, y, 1, 1);
                    //Thread.Sleep(200);
                });

                //Random sorting stuff for cool drawing effects :)
                int a = r.Next(12);
                var walls = m.GenerateListOfMazeWalls();
                if (a == 0)
                {
                    //Normal
                }
                else if (a == 1)
                {
                    //Create diamond
                    walls = walls.OrderBy(x => Math.Abs(width / 2 - x.xstart) + Math.Abs(height / 2 - x.ystart)).ToList();
                }
                else if (a == 2)
                {
                    //Random stuff
                    walls = walls.RandomPermutation().ToList();
                }
                else if (a == 3)
                {
                    //Circles everywhere
                    walls = walls.OrderBy(x => Math.Sin((double)x.xstart * 0.03) + Math.Sin((double)x.ystart * 0.03)).ToList();
                }
                else if (a == 4)
                {
                    //Thingy from top left
                    walls = walls.OrderBy(x => (Math.Atan2((double)x.xstart * 0.03, (double)x.ystart * 0.03))).ToList();
                }
                else if (a == 5)
                {
                    //Double thingy from mid
                    walls = walls.OrderBy(x => Math.Abs((Math.Atan2((double)x.xstart - width / 2, (double)x.ystart - height / 2)))).ToList();
                }
                else if (a == 6)
                {
                    //360 degrees thingy
                    walls = walls.OrderBy(x => ((Math.Atan2((double)x.xstart - width / 2, (double)x.ystart - height / 2)))).ToList();
                }
                else if (a == 7)
                {
                    //Bezier thingy or something from left top to right bot
                    walls = walls.OrderBy(x => Math.Log(x.xstart, Math.E) + Math.Log(x.ystart, Math.E)).ToList();
                }
                else if (a == 8)
                {
                    //Random arctan stuff
                    walls = walls.OrderBy(x => Math.Sin(Math.Atan2(Math.Sin(Math.Log(x.xstart, Math.E) + Math.Log(x.ystart, Math.E)) * 0.01, Math.Tan(x.xstart + x.ystart)))).ToList();
                }
                else if (a == 9)
                {
                    //Strange behaviour
                    walls = walls.OrderBy(x => Math.Pow(Math.E, x.xstart) + Math.Pow(x.ystart, x.xstart)).ToList();
                }
                else if (a == 10)
                {
                    //Random arctan stuff
                    walls = walls.OrderBy(x => Math.Atan2(Math.Sin(Math.Atan2(Math.Sin(Math.Log(x.xstart, Math.E) + Math.Log(x.ystart, Math.E)) * 0.01, Math.Tan(x.xstart + x.ystart))), Math.Log(Math.E, Math.Atan2(x.xstart, x.ystart)))).ToList();
                }
                else if (a == 11)
                {
                    //Circle
                    walls = walls.OrderBy(x => Math.Pow(x.xstart - width / 2, 2) + Math.Pow(x.ystart - height / 2, 2)).ToList();
                }

                foreach (var wall in walls)
                {
                    if (wall.xstart == wall.xend)
                    {
                        //Verticale wall
                        g.FillRectangle(Brushes.Black, wall.xstart * size, wall.ystart * size, size, (wall.yend - wall.ystart + 1) * size);
                    }
                    else
                    {
                        //Horizontale wall
                        g.FillRectangle(Brushes.Black, wall.xstart * size, wall.ystart * size, (wall.xend - wall.xstart + 1) * size, size);
                    }
                }

                var path = PathFinderDepthFirstSmart.GoFind(m.InnerMap, (x, y, pathThing) =>
                {
                    if (pathThing)
                    {
                        g.FillRectangle(Brushes.Green, x * size, y * size, size, size);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.Gray, x * size, y * size, size, size);
                    }

                });
            });
        }

        private void button22_Click(object sender, EventArgs e)
        {


            Task.Run(new Action(() =>
            {
                List<Algorithm> algjes = new List<Algorithm>();
                algjes.Add(new AlgorithmBacktrack());
                algjes.Add(new AlgorithmKruskal());

                for (int i = 0; i < algjes.Count; i++)
                {
                    Algorithm curalg = algjes[i];

                    Stopwatch w = new Stopwatch();
                    w.Start();
                    int size = (int)Math.Pow(2.0, 12.0);
                    DebugMSG("Generating maze of size " + curalg.GetType().ToString() + ": " + size);
                    DebugMSG("Saved size it should be: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 8.0 + " mb");
                    DebugMSG("Or in GB: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 1024.0 / 8.0 + " gb");
                    Maze maze = curalg.Generate(size, size, InnerMapType.BitArreintjeFast, 1337, (x, y, cur, tot) =>
                    {
                        curXInMaze = x;
                        curYInMaze = y;
                        currentStepsToCalcPercentage = cur;
                        totalStepsToCalcPercentage = tot;
                    });

                    w.Stop();
                    DebugMSG("Generating time " + curalg.GetType().ToString() + ": " + w.Elapsed.TotalSeconds);

                }

            }));
        }

        private void button23_Click(object sender, EventArgs e)
        {
            Task.Run(new Action(() =>
            {

                Algorithm curalg = new AlgorithmBacktrack();
                InnerMapType innerMapType = InnerMapType.BitArreintjeFast;

                Stopwatch w = new Stopwatch();
                w.Start();
                int size = 2048 * 8;
                DebugMSG("Generating maze of size: " + size);
                DebugMSG("Current algorithm: " + curalg.ToString());
                DebugMSG("Current InnerMapType: " + innerMapType.ToString());
                DebugMSG("Saved size it should be: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 8.0 + " mb");
                DebugMSG("Or in GB: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 1024.0 / 8.0 + " gb");
                Maze maze = curalg.Generate(size, size, innerMapType, 1337, (x, y, cur, tot) =>
                {
                    curXInMaze = x;
                    curYInMaze = y;
                    currentStepsToCalcPercentage = cur;
                    totalStepsToCalcPercentage = tot;
                });

                w.Stop();
                DebugMSG("Generating time: " + w.Elapsed.TotalSeconds);
                //DebugMSG("Finding Path...");
                //w.Reset();
                //w.Start();
                //var path = PathFinderDepthFirstSmart.GoFind(maze.InnerMap, null);
                //w.Stop();
                //DebugMSG("Done generating path: " + w.Elapsed.TotalSeconds);
                //DebugMSG("Saving...");
                //w.Reset();
                //w.Start();
                //maze.SaveMazeAsImage("benchmark 16k single core.png", ImageFormat.Png, path, MazeSaveType.ColorDepth32Bits);
                //w.Stop();
                //DebugMSG("Done saving: " + w.Elapsed.TotalSeconds);
            }));

        }

        private void button24_Click(object sender, EventArgs e)
        {
            int size = 1;

            Form f = new Form();
            f.Size = new System.Drawing.Size(1100, 1100);
            f.Show();

            Task.Run(() =>
            {

                var g = f.CreateGraphics();

                int width = 1024;
                int height = 1024;

                g.FillRectangle(Brushes.Black, 0, 0, width + 1, height + 1);

                for (int y = 0; y < 10; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        g.DrawRectangle(Pens.Red, x * 128, y * 128, 128, 128);
                    }
                }


                Algorithm curalg = new AlgorithmBacktrack();

                Maze m = curalg.Generate(width, height, InnerMapType.Hybrid, r.Next(), (x, y, cur, tot) =>
                {
                    curXInMaze = x;
                    curYInMaze = y;
                    currentStepsToCalcPercentage = cur;
                    totalStepsToCalcPercentage = tot;
                    g.FillRectangle(Brushes.White, x * size, y * size, size, size);
                    //Thread.Sleep(200);
                });

                var path = PathFinderDepthFirstSmart.GoFind(m.InnerMap, (x, y, pathThing) =>
                {
                    if (pathThing)
                    {
                        g.FillRectangle(Brushes.Green, x * size, y * size, size, size);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.Gray, x * size, y * size, size, size);
                    }

                });

                m.SaveMazeAsImage("hybridmaze.png", ImageFormat.Png, path, MazeSaveType.ColorDepth32Bits);
            });
        }


        private void button26_Click(object sender, EventArgs e)
        {
            int size = 5;

            Form f = new Form();
            f.Size = new System.Drawing.Size(1100, 1100);
            f.Show();

            Task.Run(() =>
            {

                var g = f.CreateGraphics();

                int width = 1024;
                int height = 1024;

                g.FillRectangle(Brushes.Black, 0, 0, width + 1, height + 1);

                //for (int y = 0; y < 10; y++)
                //{
                //    for (int x = 0; x < 10; x++)
                //    {
                //        g.DrawRectangle(Pens.Red, x * 128, y * 128, 128, 128);
                //    }
                //}


                AlgorithmBacktrackSmartMemory curalg = new AlgorithmBacktrackSmartMemory();

                Maze m = curalg.Generate(width / size, height / size, InnerMapType.Hybrid, 5, (x, y, cur, tot) =>
                {
                    curXInMaze = x;
                    curYInMaze = y;
                    currentStepsToCalcPercentage = cur;
                    totalStepsToCalcPercentage = tot;
                    g.FillRectangle(Brushes.Red, x * size, y * size, size, size);
                    //Thread.Sleep(20);
                    g.FillRectangle(Brushes.White, x * size, y * size, size, size);
                });

                var path = PathFinderDepthFirstSmart.GoFind(m.InnerMap, (x, y, pathThing) =>
                {
                    if (pathThing)
                    {
                        g.FillRectangle(Brushes.Green, x * size, y * size, size, size);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.Gray, x * size, y * size, size, size);
                    }

                });

                m.SaveMazeAsImage("hybridmaze.png", ImageFormat.Png, path, MazeSaveType.ColorDepth32Bits);
            });
        }

        private void button27_Click(object sender, EventArgs e)
        {
            String mazedir = "multimaze";
            Directory.CreateDirectory(mazedir);

            Task.Run(() =>
            {
                var alg = new AlgorithmBacktrack();

                for (int i = 11; i < 17; i++)
                {
                    int size = (int)Math.Pow(2, i);

                    DebugMSG("Generating maze of size: " + size);
                    var maze = alg.Generate(size, size, InnerMapType.BitArreintjeFast, (x, y, cur, tot) =>
                    {
                        curXInMaze = x;
                        curYInMaze = y;
                        currentStepsToCalcPercentage = cur;
                        totalStepsToCalcPercentage = tot;
                    });

                    DebugMSG("Finding path...");

                    var path = PathFinderDepthFirstSmart.GoFind(maze.InnerMap, null);

                    DebugMSG("Saving...");

                    maze.SaveMazeAsImage(Path.Combine(mazedir, size + " (" + ConvertNumberToNiceString(path.Count) + ").png"), ImageFormat.Png, path, MazeSaveType.ColorDepth4Bits);
                }

                DebugMSG("Done :)");
            });
        }

        private void button28_Click(object sender, EventArgs e)
        {
            TestThisAlg(new AlgorithmBacktrack());
            TestThisAlg(new AlgorithmBacktrackSmartMemory());
            TestThisAlg(new AlgorithmBacktrackFastWithoutAction());
            TestThisAlg(new AlgorithmBacktrackFastWithoutActionAndMaze());
            TestThisAlg(new AlgorithmBacktrackFastWithoutActionAndMazeAndFastRandomFastStackList());
            TestThisAlg(new AlgorithmBacktrackFastWithoutActionAndMazeAndFastRandomFastStackArray());
            TestThisAlg(new AlgorithmBacktrackFastWithoutActionAndMazeAndFastRandomFastStackArray2());
            TestThisAlg(new AlgorithmBacktrackFastWithoutActionAndMazeAndFastRandomFastStackArray3());
        }

        private void TestThisAlg(Algorithm alg)
        {
            int size = 2048 * 8;
            var w = Stopwatch.StartNew();
            var maze2 = alg.Generate(size, size, InnerMapType.BooleanArray, 1337, null);
            w.Stop();
            //var path = PathFinderDepthFirstSmart.GoFind(maze2.InnerMap, null);
            //maze2.SaveMazeAsImage(alg.GetType().ToString() + ".png", ImageFormat.Png, path, MazeSaveType.ColorDepth32Bits);
            DebugMSG(alg.GetType() + ": " + w.Elapsed.TotalSeconds);
        }

        private void button25_Click(object sender, EventArgs e)
        {
            int sizezzz = int.Parse(comboBox4.SelectedItem.ToString().Replace(".", ""));

            //Passing an int into the action like this is faster because else it will get stuck on the heap/stack or w/e I guess :o
            var actionToRun = new Action<int>((size) =>
            {
                Algorithm curalg = new AlgorithmBacktrackSmartMemory();
                Stopwatch w = new Stopwatch();
                w.Start();
                //int size = (int)Math.Pow(2.0, 19.0);                

                DebugMSG("Generating maze of size: " + size);
                DebugMSG("Saved size it should be: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 8.0 + " mb");
                DebugMSG("Or in GB: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 1024.0 / 8.0 + " gb");
                lastMegaTerrorMaze = curalg.Generate(size, size, InnerMapType.Hybrid, 1337, (x, y, cur, tot) =>
                {
                    curXInMaze = x;
                    curYInMaze = y;
                    currentStepsToCalcPercentage = cur;
                    totalStepsToCalcPercentage = tot;
                });
                w.Stop();
                DebugMSG("Generating time: " + w.Elapsed.TotalSeconds);
                TrimAndGCCollect();
            });

            Task.Run(() => { actionToRun(sizezzz); TrimAndGCCollect(); });
        }

        private void button29_Click(object sender, EventArgs e)
        {
            lastMegaTerrorMaze = null;
            lastMegaTerrorMazePath = null;
            lastMegaTerrorMazePathPos = null;
            lastMegaTerrorMazeQuatroDirections = null;
            TrimAndGCCollect();
        }

        private void button30_Click(object sender, EventArgs e)
        {
            lastMegaTerrorMaze.SaveAsBinaryFile("megaterrormaze.bin");
        }

        private void button32_Click(object sender, EventArgs e)
        {
            int sizezzz = int.Parse(comboBox4.SelectedItem.ToString().Replace(".", ""));

            //Passing an int into the action like this is faster because else it will get stuck on the heap/stack or w/e I guess :o
            var aaaa = new Action<int>((size) =>
            {
                Algorithm curalg = new AlgorithmBacktrackSmartMemory();
                Stopwatch w = new Stopwatch();
                w.Start();
                //int size = (int)Math.Pow(2.0, 19.0);                

                DebugMSG("Generating maze of size: " + size);
                DebugMSG("Saved size it should be: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 8.0 + " mb");
                DebugMSG("Or in GB: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 1024.0 / 8.0 + " gb");
                lastMegaTerrorMaze = curalg.Generate(size, size, InnerMapType.BitArreintjeFast, 1337, (x, y, cur, tot) =>
                {
                    curXInMaze = x;
                    curYInMaze = y;
                    currentStepsToCalcPercentage = cur;
                    totalStepsToCalcPercentage = tot;
                });
                w.Stop();
                DebugMSG("Generating time: " + w.Elapsed.TotalSeconds);
            });

            Task.Run(() => { aaaa(sizezzz); TrimAndGCCollect(); });
        }

        private void button31_Click(object sender, EventArgs e)
        {
            int sizezzz = int.Parse(comboBox4.SelectedItem.ToString().Replace(".", ""));

            //Passing an int into the action like this is faster because else it will get stuck on the heap/stack or w/e I guess :o
            var aaaa = new Action<int>((size) =>
            {
                Algorithm curalg = new AlgorithmBacktrack();
                Stopwatch w = new Stopwatch();
                w.Start();
                //int size = (int)Math.Pow(2.0, 19.0);                

                DebugMSG("Generating maze of size: " + size);
                DebugMSG("Saved size it should be: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 8.0 + " mb");
                DebugMSG("Or in GB: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 1024.0 / 8.0 + " gb");
                lastMegaTerrorMaze = curalg.Generate(size, size, InnerMapType.BitArreintjeFast, 1337, (x, y, cur, tot) =>
                {
                    curXInMaze = x;
                    curYInMaze = y;
                    currentStepsToCalcPercentage = cur;
                    totalStepsToCalcPercentage = tot;
                });
                w.Stop();
                DebugMSG("Generating time: " + w.Elapsed.TotalSeconds);
            });

            Task.Run(() => { aaaa(sizezzz); TrimAndGCCollect(); });
        }

        private void button33_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var g = panel1.CreateGraphics();

                int sizemodifier = 20;

                int width = panel1.Width;
                int height = panel1.Height;

                int mazeWidth = 12;
                int mazeHeight = 12;

                //int width = 100;
                //int height = 150;

                g.FillRectangle(Brushes.Black, 0, 0, width + 1, height + 1);





                int randomnow = r.Next();
                Maze m = new AlgorithmKruskal().Generate(mazeWidth, mazeHeight, InnerMapType.BitArreintjeFast, randomnow, (x, y, cur, tot) =>
                {

                    //Thread.Sleep(curDelay);

                    g.FillRectangle(Brushes.White, x * sizemodifier, y * sizemodifier, sizemodifier, sizemodifier);

                    curXInMaze = x;
                    curYInMaze = y;

                    this.currentStepsToCalcPercentage = cur;
                    this.totalStepsToCalcPercentage = tot;

                    //Thread.Sleep(200);
                });


                var path = PathFinderDepthFirstSmart.GoFind(m.InnerMap, (x, y, pathThing) =>
                {

                    Thread.Sleep(curDelay);

                    if (pathThing)
                    {
                        g.FillRectangle(Brushes.Green, x * sizemodifier, y * sizemodifier, sizemodifier, sizemodifier);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.Gray, x * sizemodifier, y * sizemodifier, sizemodifier, sizemodifier);
                    }

                });

                foreach (var pathnode in path)
                {
                    g.FillRectangle(Brushes.Red, pathnode.X * sizemodifier, pathnode.Y * sizemodifier, sizemodifier, sizemodifier);
                }
            });
        }

        private void button34_Click(object sender, EventArgs e)
        {
            Task.Run(new Action(() =>
            {

                Algorithm curalg = new AlgorithmBacktrack();
                InnerMapType innerMapType = InnerMapType.BitArreintjeFast;

                Stopwatch w = new Stopwatch();
                w.Start();
                int size = 2048 * 8;
                DebugMSG("Generating maze of size: " + size);
                DebugMSG("Current algorithm: " + curalg.ToString());
                DebugMSG("Current InnerMapType: " + innerMapType.ToString());
                DebugMSG("Saved size it should be: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 8.0 + " mb");
                DebugMSG("Or in GB: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 1024.0 / 8.0 + " gb");
                Maze maze = curalg.Generate(size, size, innerMapType, 1337, (x, y, cur, tot) =>
                {
                    curXInMaze = x;
                    curYInMaze = y;
                    currentStepsToCalcPercentage = cur;
                    totalStepsToCalcPercentage = tot;
                });

                w.Stop();
                DebugMSG("Generating time: " + w.Elapsed.TotalSeconds);



                DebugMSG("Finding Path...");
                w.Reset();
                w.Start();
                var path = PathFinderDepthFirst.GoFind(maze.InnerMap, null);
                w.Stop();
                DebugMSG("PathFinderDepthFirst: " + w.Elapsed.TotalSeconds);

                w.Reset();
                w.Start();
                var path2 = PathFinderDepthFirstSmart.GoFind(maze.InnerMap, null);
                w.Stop();
                DebugMSG("PathFinderDepthFirstSmart: " + w.Elapsed.TotalSeconds);

                DebugMSG("Comparing paths to be sure they match...");
                if (path.Count != path2.Count)
                {
                    DebugMSG("ERRORRRRRRR: path length not equals!!!");
                    return;
                }

                var itworkedhereforpathfinding = true;
                for (int i = 0; i < path.Count; i++)
                {
                    if (path[i].X != path2[i].X || path[i].Y != path2[i].Y)
                    {
                        itworkedhereforpathfinding = false;
                    }
                }
                if (!itworkedhereforpathfinding)
                {
                    DebugMSG("ERRORRRRRRR: path finder not working correctly!!!!");
                }
                else
                {
                    DebugMSG("Paths match :)");
                }

                DebugMSG("Done :)");
            }));
        }

        private void button35_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (lastMegaTerrorMaze == null)
                {
                    DebugMSG("No Mega Terror Maze in memory, generate one first...");
                    return;
                }

                DebugMSG("Finding path for current Mega Terror Maze...");
                var w = new Stopwatch();
                w.Start();
                lastMegaTerrorMazePath = PathFinderDepthFirstSmart.GoFind(lastMegaTerrorMaze.InnerMap, null);
                w.Stop();
                DebugMSG("Path found in " + w.Elapsed.TotalSeconds + " seconds, length: " + lastMegaTerrorMazePath.Count);

                TrimAndGCCollect();
            });
        }

        private void button36_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (lastMegaTerrorMaze == null)
                {
                    DebugMSG("No Mega Terror Maze in memory, generate one first...");
                    return;
                }

                DebugMSG("Saving Mega Terror Maze as image...");

                var w = new Stopwatch();
                w.Start();

                try
                {
                    if (lastMegaTerrorMazePath == null)
                    {
                        DebugMSG("No path found, saving maze without path...");
                        lastMegaTerrorMaze.SaveMazeAsImage("megaterrormaze-" + DateTime.Now.Ticks + ".png", ImageFormat.Png);
                    }
                    else
                    {
                        DebugMSG("Path found, saving maze with path...");
                        lastMegaTerrorMaze.SaveMazeAsImage("megaterrormaze-" + DateTime.Now.Ticks + ".png", ImageFormat.Png, lastMegaTerrorMazePath, MazeSaveType.ColorDepth4Bits);
                    }

                    DebugMSG("Saving time: " + w.Elapsed.TotalSeconds + " seconds.");
                }
                catch (Exception ex)
                {
                    DebugMSG("Exception occured while saving the maze (after " + w.Elapsed.TotalSeconds + " seconds):");
                    DebugMSG(ex.ToString());
                }
            });
        }

        private void button37_Click(object sender, EventArgs e)
        {
            int sizezzz = int.Parse(comboBox4.SelectedItem.ToString().Replace(".", ""));

            //Passing an int into the action like this is faster because else it will get stuck on the heap/stack or w/e I guess :o
            var aaaa = new Action<int>((size) =>
            {
                AlgorithmBacktrackFastWithoutActionAndMazeAndFastRandomFastStackArray2 curalg = new AlgorithmBacktrackFastWithoutActionAndMazeAndFastRandomFastStackArray2();
                Stopwatch w = new Stopwatch();
                w.Start();
                //int size = (int)Math.Pow(2.0, 19.0);                

                DebugMSG("Generating maze of size: " + size);
                DebugMSG("Saved size it should be: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 8.0 + " mb");
                DebugMSG("Or in GB: " + Math.Pow((double)size, 2.0) / 1024.0 / 1024.0 / 1024.0 / 8.0 + " gb");
                lastMegaTerrorMaze = curalg.Generate(size, size, InnerMapType.BitArreintjeFast, 1337, (x, y, cur, tot) =>
                {
                    curXInMaze = x;
                    curYInMaze = y;
                    currentStepsToCalcPercentage = cur;
                    totalStepsToCalcPercentage = tot;
                });
                w.Stop();
                DebugMSG("Generating time: " + w.Elapsed.TotalSeconds);
            });

            Task.Run(() => { aaaa(sizezzz); TrimAndGCCollect(); });
        }

        private void button39_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (lastMegaTerrorMaze == null)
                {
                    DebugMSG("No Mega Terror Maze in memory, generate one first...");
                    return;
                }

                DebugMSG("Finding path with pos for current Mega Terror Maze...");
                var w = new Stopwatch();
                w.Start();
                lastMegaTerrorMazePathPos = PathFinderDepthFirstSmartWithPos.GoFind(lastMegaTerrorMaze.InnerMap, null);
                w.Stop();
                DebugMSG("Path found in " + w.Elapsed.TotalSeconds + " seconds, length: " + lastMegaTerrorMazePathPos.Count);

                TrimAndGCCollect();
            });
        }

        private void button40_Click(object sender, EventArgs e)
        {
            TrimAndGCCollect();
        }

        private void TrimAndGCCollect()
        {
            if (lastMegaTerrorMazePath != null)
            {
                lastMegaTerrorMazePath.TrimExcess();
            }
            if (lastMegaTerrorMazePathPos != null)
            {
                lastMegaTerrorMazePathPos.TrimExcess();
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void button41_Click(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 19;
            Task.Run(() =>
            {
                var g = panel1.CreateGraphics();

                int sizemodifier = 1;

                this.Invoke(new Action(() =>
                {
                    sizemodifier = int.Parse(comboBox1.SelectedItem.ToString());
                }));

                int width = panel1.Width;
                int height = panel1.Height;

                int mazeWidth = width / sizemodifier / 2 * 2;
                int mazeHeight = height / sizemodifier / 2 * 2;

                //int width = 100;
                //int height = 150;

                g.FillRectangle(Brushes.Black, 0, 0, width + 1, height + 1);

                var intrandomvalue = 284434191;
                DebugMSG("Random: " + intrandomvalue);

                Maze m = new AlgorithmKruskal().Generate(mazeWidth, mazeHeight, InnerMapType.BitArreintjeFast, intrandomvalue, (x, y, cur, tot) =>
                {

                    //Thread.Sleep(curDelay);

                    g.FillRectangle(Brushes.White, x * sizemodifier, y * sizemodifier, sizemodifier, sizemodifier);

                    curXInMaze = x;
                    curYInMaze = y;

                    this.currentStepsToCalcPercentage = cur;
                    this.totalStepsToCalcPercentage = tot;

                    //Thread.Sleep(200);
                });

                var directions = PathFinderDepthFirstSmartAndSmartMemory.GoFind(m.InnerMap, (x, y, pathFinderAction) =>
                {

                    int sleepTime = 25;

                    Thread.Sleep(sleepTime);

                    Brush colorToUse = Brushes.Pink;
                    g.FillRectangle(colorToUse, x * sizemodifier, y * sizemodifier, sizemodifier, sizemodifier);

                    Thread.Sleep(sleepTime);

                    switch (pathFinderAction)
                    {
                        case PathFinderAction.Step:
                            colorToUse = Brushes.Green;
                            break;
                        case PathFinderAction.Backtrack:
                            colorToUse = Brushes.Gray;
                            break;
                        case PathFinderAction.Junction:
                            colorToUse = Brushes.Blue;
                            break;
                        case PathFinderAction.RemovingJunction:
                            colorToUse = Brushes.Red;
                            break;
                        case PathFinderAction.RefoundJunction:
                            colorToUse = Brushes.Purple;
                            break;
                        default:
                            break;
                    }

                    g.FillRectangle(colorToUse, x * sizemodifier, y * sizemodifier, sizemodifier, sizemodifier);


                });

                //PrintQuatroList(path);

                var path = PathFinderDepthFirstSmartAndSmartMemory.DeterminePathFromDirections(directions, m.InnerMap);

                foreach (var pathnode in path)
                {
                    g.FillRectangle(Brushes.DarkBlue, pathnode.X * sizemodifier, pathnode.Y * sizemodifier, sizemodifier, sizemodifier);
                    Thread.Sleep(50);
                }
            });
        }

        private void PrintQuatroList(QuatroStack quatroStack)
        {
            var count = quatroStack.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                switch (quatroStack.InnerList[i])
                {
                    case 0:
                        DebugMSG("UP");
                        Console.WriteLine("UP");
                        break;
                    case 1:
                        DebugMSG("RIGHT");
                        Console.WriteLine("RIGHT");
                        break;
                    case 2:
                        DebugMSG("DOWN");
                        Console.WriteLine("DOWN");
                        break;
                    case 3:
                        DebugMSG("LEFT");
                        Console.WriteLine("LEFT");
                        break;
                    default:
                        break;
                }
            }
        }

        private void button42_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (lastMegaTerrorMaze == null)
                {
                    DebugMSG("No Mega Terror Maze in memory, generate one first...");
                    return;
                }

                DebugMSG("Finding directions for current Mega Terror Maze...");
                var w = new Stopwatch();
                w.Start();
                lastMegaTerrorMazeQuatroDirections = PathFinderDepthFirstSmartAndSmartMemory.GoFind(lastMegaTerrorMaze.InnerMap, null);
                w.Stop();
                DebugMSG("Directions found in " + w.Elapsed.TotalSeconds + " seconds, length: " + lastMegaTerrorMazeQuatroDirections.Count);
                //PrintQuatroList(lastMegaTerrorMazeQuatroDirections);
                TrimAndGCCollect();
            });
        }

        private void button43_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (lastMegaTerrorMaze == null)
                {
                    DebugMSG("No Mega Terror Maze in memory, generate one first...");
                    return;
                }

                if (lastMegaTerrorMazeQuatroDirections == null)
                {
                    DebugMSG("No QuatroDirections found in memory, generate these first...");
                    return;
                }

                DebugMSG("Finding path based on directions for current Mega Terror Maze...");
                var w = new Stopwatch();
                w.Start();
                var generatedPathCount = PathFinderDepthFirstSmartAndSmartMemory.DeterminePathFromDirections(lastMegaTerrorMazeQuatroDirections, lastMegaTerrorMaze.InnerMap).LongCount();
                w.Stop();
                DebugMSG("Path based on directions found in " + w.Elapsed.TotalSeconds + " seconds, length: " + generatedPathCount);
                //PrintQuatroList(lastMegaTerrorMazeQuatroDirections);
            });
        }

        private void button44_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (lastMegaTerrorMaze == null)
                {
                    DebugMSG("No Mega Terror Maze in memory, generate one first...");
                    return;
                }

                if (lastMegaTerrorMazeQuatroDirections == null)
                {
                    DebugMSG("No QuatroDirections found in memory, generate these first...");
                    return;
                }

                DebugMSG("Finding path based on directions for current Mega Terror Maze...");
                var w = new Stopwatch();
                w.Start();
                var ienumerablePath = PathFinderDepthFirstSmartAndSmartMemory.DeterminePathFromDirections(lastMegaTerrorMazeQuatroDirections, lastMegaTerrorMaze.InnerMap);
                var filteredPath = ienumerablePath.Where(t => t.Y >= 0 && t.Y < Maze.LineChunks);
                var longCount = filteredPath.LongCount();
                w.Stop();
                DebugMSG("Path based on directions found and filtered in " + w.Elapsed.TotalSeconds + " seconds, length of the first '" + Maze.LineChunks + "' Y lines: " + longCount);
                //PrintQuatroList(lastMegaTerrorMazeQuatroDirections);
            });
        }


        private void buttonSaveMazeAsImageDeluxe_Click(object sender, EventArgs e)
        {
            var useTiles = checkBoxUseTiles.Checked;
            var useColorMap = checkBoxUseColorMap.Checked;

            var selectedImageSaveTypeValue = comboBoxImageSaveType.SelectedItem;
            if (!(selectedImageSaveTypeValue is MazeSaveFileType))
            {
                DebugMSG("This is not a valid MazeSaveFileType... (Please not not manually edit this Combobox...)");
                return;
            }
            var mazeSaveFileType = (MazeSaveFileType)selectedImageSaveTypeValue;

            Task.Run(() =>
            {
                if (lastMegaTerrorMaze == null)
                {
                    DebugMSG("No Mega Terror Maze in memory, generate one first...");
                    return;
                }

                DebugMSG("Saving Mega Terror Maze as image...");

                var w = new Stopwatch();
                w.Start();

                try
                {
                    Action<int, int> callback = (cur, tot) =>
                    {
                        this.curMazeLineSaving = cur;
                        this.mazeLinesToSave = tot;
                    };

                    var formattedFileName = string.Format("MegaTerrorMaze-{0}-{1}-{2}.{3}", "{0}", useTiles ? "WithTiles" : "NoTiles", useColorMap ? "WithColorMap" : "NoColorMap", mazeSaveFileType.ToString().ToLower());

                    Boolean saveResult = false;

                    if (lastMegaTerrorMazeQuatroDirections != null)
                    {
                        formattedFileName = string.Format(formattedFileName, "DynamicPath");
                        DebugMSG("Path with directions found, dynamically generating path and saving maze... (" + formattedFileName + ")");
                        var ienumerablePathBasedOnDirections = PathFinderDepthFirstSmartAndSmartMemory.DeterminePathFromDirections(lastMegaTerrorMazeQuatroDirections, lastMegaTerrorMaze.InnerMap);
                        saveResult = lastMegaTerrorMaze.SaveMazeAsImageDeluxeWithDynamicallyGeneratedPath(mazeSaveFileType, formattedFileName, ienumerablePathBasedOnDirections, callback, useTiles, useColorMap, (x) => DebugMSG("MazeDbg: " + x));
                    }
                    else if (lastMegaTerrorMazePathPos != null)
                    {
                        formattedFileName = string.Format(formattedFileName, "PathPos");
                        DebugMSG("Path with pos found, saving maze with path... (" + formattedFileName + ")");
                        saveResult = lastMegaTerrorMaze.SaveMazeAsImageDeluxe(mazeSaveFileType, formattedFileName, lastMegaTerrorMazePathPos, callback, useTiles, useColorMap);
                    }
                    else if (lastMegaTerrorMazePath != null)
                    {
                        formattedFileName = string.Format(formattedFileName, "Path");
                        DebugMSG("Path found, saving maze with path... (" + formattedFileName + ")");
                        saveResult = lastMegaTerrorMaze.SaveMazeAsImageDeluxe(mazeSaveFileType, formattedFileName, lastMegaTerrorMazePath, callback, useTiles, useColorMap);
                    }
                    else
                    {
                        formattedFileName = string.Format(formattedFileName, "NoPath");
                        DebugMSG("No path found, saving maze without path... (" + formattedFileName + ")");
                        saveResult = lastMegaTerrorMaze.SaveMazeAsImageDeluxe(mazeSaveFileType, formattedFileName, new List<MazePointPos>(), callback, useTiles, useColorMap);
                    }

                    if (saveResult)
                    {
                        DebugMSG("Saving time: " + w.Elapsed.TotalSeconds + " seconds.");
                    }
                    else
                    {
                        DebugMSG("This saving mode is not supported, please try something else :)");
                    }
                }
                catch (Exception ex)
                {
                    DebugMSG("Exception occured while saving the maze (after " + w.Elapsed.TotalSeconds + " seconds):");
                    DebugMSG(ex.ToString());
                }
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            debugMsgWriter.Flush();
            debugMsgWriter.Dispose();
        }
    }
}
