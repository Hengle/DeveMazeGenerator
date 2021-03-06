﻿using BitMiracle.LibTiff.Classic;
using DeveMazeGenerator.Generators;
using DeveMazeGenerator.InnerMaps;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveMazeGenerator
{
    public partial class Maze
    {
        public static int LineChunks = 20000;
        public static int LineChunkCount = 4;

        /// <summary>
        /// Saves the maze with a specified path
        /// Note: The method with PathPos is prefered though
        /// </summary>
        /// <param name="mazeSaveFileType">Wether to save as PNG or TIF</param>
        /// <param name="fileName">The filename of the file</param>
        /// <param name="path">The path (can be generated by calling PathFinderDepthFirst.GoFind)</param>
        /// <param name="lineSavingProgress">An action that will be called to obtain the status of the saving.</param>
        /// <param name="useTiles">Wether to save the maze with tiles (Might be faster) (Only works for TIFF)</param>
        /// <param name="useColorMap">Wether to save the maze ColorMap (Is faster and uses less disk space) (Only works for TIFF) (Note: Apparently GigaPan does not support this)</param>
        /// <returns>Boolean wether this mode is supported or not</returns>
        public bool SaveMazeAsImageDeluxe(MazeSaveFileType mazeSaveFileType, String fileName, List<MazePoint> path, Action<int, int> lineSavingProgress = null, Boolean useTiles = false, Boolean useColorMap = false)
        {
            List<MazePointPos> pathPosjes = new List<MazePointPos>(path.Count);

            for (int i = 0; i < path.Count; i++)
            {
                var curPathNode = path[i];
                byte formulathing = (byte)((double)i / (double)path.Count * 255.0);
                var pathPos = new MazePointPos(curPathNode.X, curPathNode.Y, formulathing);
                pathPosjes.Add(pathPos);
            }
            return SaveMazeAsImageDeluxe(mazeSaveFileType, fileName, pathPosjes, lineSavingProgress, useTiles, useColorMap);
        }

        /// <summary>
        /// Saves the maze with a specified path
        /// Note: Either this method or the one with the Dynamic path (for really big mazes) is prefered for saving a Maze to an image
        /// </summary>
        /// <param name="mazeSaveFileType">Wether to save as PNG or TIF</param>
        /// <param name="fileName">The filename of the file</param>
        /// <param name="path">The path (can be generated by calling PathFinderDepthFirst.GoFind)</param>
        /// <param name="lineSavingProgress">An action that will be called to obtain the status of the saving.</param>
        /// <param name="useTiles">Wether to save the maze with tiles (Might be faster) (Only works for TIFF)</param>
        /// <param name="useColorMap">Wether to save the maze ColorMap (Is faster and uses less disk space) (Only works for TIFF) (Note: Apparently GigaPan does not support this)</param>
        /// <returns>Boolean wether this mode is supported or not</returns>
        public bool SaveMazeAsImageDeluxe(MazeSaveFileType mazeSaveFileType, String fileName, List<MazePointPos> pathPosjes, Action<int, int> lineSavingProgress = null, Boolean useTiles = false, Boolean useColorMap = false)
        {
            if (lineSavingProgress == null)
            {
                lineSavingProgress = (cur, tot) => { };
            }


            if (mazeSaveFileType == MazeSaveFileType.Png)
            {
                if (useTiles == false && useColorMap == false)
                {
                    SaveMazeAsImageDeluxePng(fileName, pathPosjes, lineSavingProgress);
                }
                else
                {
                    //No other modes are supported here
                    return false;
                }
            }
            else if (mazeSaveFileType == MazeSaveFileType.Tif)
            {
                if (useTiles == false && useColorMap == false)
                {
                    SaveMazeAsImageDeluxeTiff(fileName, pathPosjes, lineSavingProgress);
                }
                else if (useTiles == true && useColorMap == false)
                {
                    SaveMazeAsImageDeluxeTiffWithChunks(fileName, pathPosjes, lineSavingProgress);
                }
                else if (useTiles == false && useColorMap == true)
                {
                    SaveMazeAsImageDeluxeTiffWithColorMap(fileName, pathPosjes, lineSavingProgress);
                }
                else
                {
                    //Using both chunks and color mapping is not supported currently
                    return false;
                }
            }


            return true;

        }


        /// <summary>
        /// Saves the maze with a specified path
        /// Note: This is the method that should be used together with the PathFinderDepthFirstSmartAndSmartMemory.DeterminePathFromDirections method
        /// </summary>
        /// <param name="mazeSaveFileType">What format to save the maze in. Tif is currently the best to choose</param>
        /// <param name="fileName">The filename of the file</param>
        /// <param name="dynamicallyGeneratedPath">The dynamically generated path</param>
        /// <param name="lineSavingProgress">An action that will be called to obtain the status of the saving.</param>
        /// <param name="useTiles">Wether to save the maze with tiles (Might be faster) (Only works for TIFF)</param>
        /// <param name="useColorMap">Wether to save the maze ColorMap (Is faster and uses less disk space) (Only works for TIFF) (Note: Apparently GigaPan does not support this)</param>
        /// <param name="debugMessageCallback">Some more advanced saving algorithms also return some debug messages, (e.g. Tif)</param>
        /// <returns>Boolean wether this mode is supported or not</returns>
        public bool SaveMazeAsImageDeluxeWithDynamicallyGeneratedPath(MazeSaveFileType mazeSaveFileType, String fileName, IEnumerable<MazePointPos> dynamicallyGeneratedPath, Action<int, int> lineSavingProgress = null, Boolean useTiles = false, Boolean useColorMap = false, Boolean saveAsSplittedImages = false, Action<string> debugMessageCallback = null)
        {
            if (lineSavingProgress == null)
            {
                lineSavingProgress = (cur, tot) => { };
            }

            if (mazeSaveFileType == MazeSaveFileType.Png)
            {
                if (useTiles == false && useColorMap == false && saveAsSplittedImages == false)
                {
                    SaveMazeAsImageDeluxePngWithDynamicallyGeneratedPath(fileName, dynamicallyGeneratedPath, lineSavingProgress);
                }
                else if (useTiles == true)
                {
                    SaveMazeAsImageDeluxePngWithDynamicallyGeneratedPathWithAnalysis(fileName, dynamicallyGeneratedPath, lineSavingProgress, debugMessageCallback);
                }
                else
                {
                    //No other modes are supported here
                    return false;
                }
            }
            else if (mazeSaveFileType == MazeSaveFileType.Tif)
            {
                if (useTiles == false && useColorMap == false && saveAsSplittedImages == false)
                {
                    SaveMazeAsImageDeluxeTiffWithDynamicallyGeneratedPathWithAnalysis(fileName, dynamicallyGeneratedPath, lineSavingProgress, debugMessageCallback);
                }
                else if (saveAsSplittedImages)
                {
                    SaveMazeAsImageDeluxeTiffWithDynamicallyGeneratedPathWithAnalysisAndSplitImages(Path.GetFileNameWithoutExtension(fileName), dynamicallyGeneratedPath, lineSavingProgress, debugMessageCallback);
                }
                else
                {
                    //No other modes are supported here
                    return false;
                }
            }

            return true;
        }
    }
}
