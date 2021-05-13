using System;
using System.Numerics;
using System.Drawing;
using PathTracer.FrameRecorders;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;

namespace PathTracer
{
    internal static class Program
    {
        #region Static Methods

        private static void Main(string[] args)
        {
            if (Vector.IsHardwareAccelerated)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Hardware Accelerated");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Not Hardware Accelerated");
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            IPathTracerFrameRecorder frameRecorder = new PngPathTracerFrameRecorder();
            for (int i = 0; i < 10; i++)
            {
                // Scene
                PathTracerScene scene = new PathTracerScene();
                scene.Camera = new PathTracerCamera();
                scene.Camera.Position = new Vector3(0, 0, 100);
                scene.Camera.Direction = new Vector3(0, 0, -100);
                scene.Camera.Up = new Vector3(0, 1, 0);
                scene.Camera.AntiAliased = true;
                scene.BackgroundMaterial = new PathTracerMaterial();
                scene.BackgroundMaterial.IsLight = true;
                scene.BackgroundMaterial.Color = new PathTracerColor(1, 20, 4, 4);
                scene.FogColor = PathTracerColor.Black;
                scene.FogDistance = 0;

                // Scene Generation (Light Box)
                //PathTracerSceneGenerator.GenerateLightBox(scene, new Vector3(-200, -200, -200), new Vector3(200, 200, 200));

                // Scene Generation (Abstract)
                PathTracerSceneGenerator.GenerateAbstract(scene, new Vector3(-100, -100, -100), new Vector3(100, 100, 100), 80, 0.1F, 0F);

                // Options
                ConsolePercentageDisplay consolePercentageDisplay = new ConsolePercentageDisplay();
                PathTracerOptions optionsLow = new PathTracerOptions();
                optionsLow.Width = 800;
                optionsLow.Height = 500;
                optionsLow.BounceCount = 4;
                optionsLow.SamplesPerPixel = 5;
                optionsLow.FrameCount = 1;
                optionsLow.MaxDegreeOfParallelism = 14;
                optionsLow.PercentageDisplay = consolePercentageDisplay.Display;
                optionsLow.RenderMode = PathTracerRenderMode.PathTracer;
                optionsLow.PixelSampleRate = 1F;

                consolePercentageDisplay = new ConsolePercentageDisplay();
                PathTracerOptions optionsHigh = new PathTracerOptions();
                optionsHigh.Width = 800;
                optionsHigh.Height = 500;
                optionsHigh.BounceCount = 8;
                optionsHigh.SamplesPerPixel = 100;
                optionsHigh.FrameCount = 1;
                optionsHigh.MaxDegreeOfParallelism = 14;
                optionsHigh.PercentageDisplay = consolePercentageDisplay.Display;
                optionsHigh.RenderMode = PathTracerRenderMode.PathTracer;
                optionsHigh.PixelSampleRate = 1F;

                // Engine
                PathTracerEngine engine = new PathTracerEngine();

                Stopwatch sw = new Stopwatch();

                // Render
                Console.WriteLine("Low quality render: ");
                var (path, skippedSamples) = engine.Render(scene, optionsLow, frameRecorder);
                float[,] estimatedDifficulty = EstimatePixelDifficulty(path);
                CreateDifficultyImage(estimatedDifficulty, Path.GetDirectoryName(path));
                
                Console.WriteLine("High quality render with difficulties: ");
                sw.Start();
                (path, skippedSamples) = engine.Render(scene, optionsHigh, frameRecorder, estimatedDifficulty);
                sw.Stop();
                long hDiff = sw.ElapsedMilliseconds;

                Console.WriteLine("Similar time required render without difficulties: ");
                consolePercentageDisplay = new ConsolePercentageDisplay();
                //                      expected number of samples used                                         skipped samples         number of pixels
                float samplesPerPixel = (optionsHigh.Width * optionsHigh.Height * optionsHigh.SamplesPerPixel - (float)skippedSamples) / (float)(optionsHigh.Width * optionsHigh.Height);
                Console.WriteLine("Difficulty should be about equal to rendering whole image with {0} samples per pixel. \nRendering at {1} samples per pixel", samplesPerPixel, (int) samplesPerPixel + 1);
                consolePercentageDisplay = new ConsolePercentageDisplay();
                PathTracerOptions optionsCustom = optionsHigh;
                optionsCustom.PercentageDisplay = consolePercentageDisplay.Display;
                optionsCustom.SamplesPerPixel = (int)samplesPerPixel+1;
                sw.Reset();
                sw.Start();
                (path, skippedSamples) = engine.Render(scene, optionsCustom, frameRecorder);
                sw.Stop();
                long custom = sw.ElapsedMilliseconds;

                Console.WriteLine("High quality rendering, without difficulties: ");
                consolePercentageDisplay = new ConsolePercentageDisplay();
                optionsHigh.PercentageDisplay = consolePercentageDisplay.Display;
                optionsHigh.SamplesPerPixel = 100;
                sw.Reset();
                sw.Start();
                engine.Render(scene, optionsHigh, frameRecorder);
                sw.Stop();
                long full = sw.ElapsedMilliseconds;

                Console.WriteLine("Elapsed milliseconds:\nWith focused rendering: {0}\nNo focus, similar number of samples: {1}\nWithout focused rendering, all samples: {2}", hDiff, custom, full);
            }

            // Done
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done");
            Console.ReadLine();
        }


        private static void CreateDifficultyImage(float[,] estimatedDifficulty, string path) {
            Bitmap diff = new Bitmap(estimatedDifficulty.GetLength(0), estimatedDifficulty.GetLength(1));
            for (int i = 0; i < estimatedDifficulty.GetLength(0); i++) {
                for (int j = 0; j < estimatedDifficulty.GetLength(1); j++) {
                    diff.SetPixel(i, j, GetColorOfDifficulty(estimatedDifficulty[i, j]));
                }
            }
            Console.WriteLine(path);
            diff.Save(Path.Combine(path, "difficulty.png"));
        }

        /// <summary>
        /// Returns a color on green-red linear scale for difficulty
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private static Color GetColorOfDifficulty(float v) {
            int r = (int)(v * 255);
            int g = 255 - r;
            return Color.FromArgb(r, g, 0);
        }

        /// <summary>
        /// Takes a path to an image of a low quality render out of which
        /// it calculates the importance (or difficulty) of pixels in the image
        /// which it returns in a 2D float array normalized from 0 to 1. 
        /// </summary>
        /// <param name="filePath">Path to a png file of the original rendered image. </param>
        /// <returns>float[,] representing pixel difficulties. </returns>
        private static float[,] EstimatePixelDifficulty(string filePath) {
            Bitmap render = (Bitmap)Image.FromFile(filePath);
            MemoryStream stream = new MemoryStream();

            float[,] importanceMatrix = new float[render.Width, render.Height];

            int n = 20; //chunk size
            long uncompressedSize = n * n * 3;
            //split the bitmap in nxn chunks
            for (int x = 0; x < render.Width; x+=n) {
                for (int y = 0; y < render.Height; y+=n) {
                    Bitmap chunk = new Bitmap(n, n);
                    for (int i = 0; i < 20 && i+x < render.Width; i++) {
                        for (int j = 0; j < 20 && j+y < render.Height; j++) {
                            chunk.SetPixel(i, j, render.GetPixel(x + i, y + j));
                        }
                    }
                    stream = new MemoryStream();
                    SaveJPG100(chunk, stream, 50);
                    long diff = uncompressedSize - stream.Length;
                    stream.Dispose();
                    float importance = 1f - (float)diff / uncompressedSize;

                    //trivial importance assignment
                    for (int i = 0; i < 20 && i + x < render.Width; i++) {
                        for (int j = 0; j < 20 && j + y < render.Height; j++) {
                            importanceMatrix[i + x, j + y] = importance;
                        }
                    }

                }
            }

            return importanceMatrix;

        }

        /// <summary>
        /// https://stackoverflow.com/questions/41665/bmp-to-jpg-png-in-c-sharp
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="stream"></param>
        public static void SaveJPG100(this Bitmap bmp, Stream stream, long quality) {
            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            bmp.Save(stream, GetEncoder(ImageFormat.Jpeg), encoderParameters);
        }
        /// <summary>
        /// https://stackoverflow.com/questions/41665/bmp-to-jpg-png-in-c-sharp
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static ImageCodecInfo GetEncoder(ImageFormat format) {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs) {
                if (codec.FormatID == format.Guid) {
                    return codec;
                }
            }

            return null;
        }

        #endregion
    }
}