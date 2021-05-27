using System;
using System.Numerics;
using System.Drawing;
using PathTracer.FrameRecorders;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace PathTracer
{
    internal static class Program
    {
        #region Static Methods

        private static void Main(string[] args)
        {
            CultureInfo culture = null;
            if (Thread.CurrentThread.CurrentCulture.Name != "en-US")
                culture = CultureInfo.CreateSpecificCulture("en-US");


            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

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
            for (int i = 0; i < 1; i++)
            {
                // Scene
                PathTracerScene scene = new PathTracerScene();
                scene.Camera = new PathTracerCamera();
                scene.Camera.Position = new Vector3(0, 0, 0);
                scene.Camera.Direction = new Vector3(1, 0, 0);
                scene.Camera.Up = new Vector3(0, 1, 0);
                scene.Camera.AntiAliased = true;
                scene.Camera.HorizontalFieldOfView = AngleHelper.ToRadians(30);
                scene.BackgroundMaterial = new PathTracerMaterial();
                scene.BackgroundMaterial.IsLight = true;
                scene.BackgroundMaterial.Color = new PathTracerColor(1, 1, 1, 1);
                scene.FogColor = PathTracerColor.Black;
                scene.FogDistance = 0;

                // Scene Generation (Light Box)
                //PathTracerSceneGenerator.GenerateLightBox(scene, new Vector3(-1000, -1000, -1000), new Vector3(1200, 1200, 1200));

                // Scene Generation (Abstract)
                //PathTracerSceneGenerator.GenerateAbstract(scene, new Vector3(-100, -100, -100), new Vector3(100, 100, 100), 30, 0.1F, 0F);
                Console.WriteLine("Reading objects");


                //scena1
                /*PathTracerMaterial wallMaterial = new PathTracerMaterial();
                wallMaterial.Gloss = 0.1f;
                wallMaterial.Reflectivity = 0.01f;
                scene.ReadObject(@"D:\Scene\scena1\stene.obj", texturePath: @"D:\textures\wall.jpg");

                PathTracerMaterial mizaMaterial = new PathTracerMaterial();
                mizaMaterial.Reflectivity = 0.1f;
                mizaMaterial.Gloss = 0.1f;
                mizaMaterial.Color = neutral;
                scene.ReadObject(@"D:\Scene\scena1\miza.obj", texturePath: @"D:\textures\wood.jpg", material: mizaMaterial);
                PathTracerMaterial lucMaterial = new PathTracerMaterial();
                PathTracerColor lucColor = new PathTracerColor(1, 55, 55, 55);
                lucMaterial.Color = lucColor;
                lucMaterial.IsLight = true;
                scene.ReadObject(@"D:\Scene\scena1\luc.obj", material: lucMaterial);
                PathTracerMaterial bananaMaterial = new PathTracerMaterial();
                bananaMaterial.Gloss = 0.05f;
                bananaMaterial.Color = neutral;
                scene.ReadObject(@"D:\Scene\scena1\banana.obj", material: bananaMaterial, texturePath: @"D:\textures\banana.jpg");
                PathTracerMaterial jabolkoMaterial = new PathTracerMaterial();
                jabolkoMaterial.Color = neutral;
                jabolkoMaterial.Reflectivity = 0.1f;
                jabolkoMaterial.Gloss = 0.4f;
                scene.ReadObject(@"D:\Scene\scena1\jabolko.obj", material: jabolkoMaterial, texturePath: @"D:\textures\apple.jpg");
                PathTracerMaterial vazaMaterial = new PathTracerMaterial();
                vazaMaterial.Color = neutral;
                vazaMaterial.Reflectivity = 0.9f;
                vazaMaterial.Gloss = 0.9f;
                scene.ReadObject(@"D:\Scene\scena1\vaza.obj", material: vazaMaterial, texturePath: @"D:\textures\marble.jpg");*/

                //scena2
                Scene1(scene);

                /*PathTracerMaterial yMaterial = new PathTracerMaterial();
                PathTracerColor green = new PathTracerColor(1, 0, 1, 0);
                PathTracerColor safeGreen = PathTracerColor.MakeMaterialSafe(ref green);
                yMaterial.Color = safeGreen;
                yMaterial.Gloss = 0.9f;
                //scene.ReadObject(@"D:\Objects\y.obj", material: yMaterial);
                PathTracerMaterial zMaterial = new PathTracerMaterial();
                PathTracerColor blue = new PathTracerColor(1, 0, 0, 1);
                PathTracerColor safeBlue = PathTracerColor.MakeMaterialSafe(ref blue);
                zMaterial.Color = safeBlue;
                zMaterial.Gloss = 0.9f;
                //scene.ReadObject(@"D:\Objects\z.obj", material: zMaterial);*/
                Console.WriteLine("Read objects");

                // Options
                ConsolePercentageDisplay consolePercentageDisplay = new ConsolePercentageDisplay();
                PathTracerOptions optionsLow = new PathTracerOptions();
                optionsLow.Width = 1200;
                optionsLow.Height = 800;
                optionsLow.BounceCount = 6;
                optionsLow.SamplesPerPixel = 5;
                optionsLow.FrameCount = 1;
                optionsLow.MaxDegreeOfParallelism = 15;
                optionsLow.PercentageDisplay = consolePercentageDisplay.Display;
                optionsLow.RenderMode = PathTracerRenderMode.PathTracer;
                //optionsLow.RenderMode = PathTracerRenderMode.Color;
                optionsLow.PixelSampleRate = 1F;

                consolePercentageDisplay = new ConsolePercentageDisplay();
                PathTracerOptions optionsHigh = new PathTracerOptions();
                optionsHigh.Width = 1200;
                optionsHigh.Height = 800;
                optionsHigh.BounceCount = 12;
                optionsHigh.SamplesPerPixel = 150;
                optionsHigh.FrameCount = 1;
                optionsHigh.MaxDegreeOfParallelism = 15;
                optionsHigh.PercentageDisplay = consolePercentageDisplay.Display;
                optionsHigh.RenderMode = PathTracerRenderMode.PathTracer;
                optionsHigh.PixelSampleRate = 1F;

                // Engine
                PathTracerEngine engine = new PathTracerEngine();

                Stopwatch sw = new Stopwatch();

                // Render image, albedo and normals
                Console.WriteLine("Low quality render: ");
                var (path, skippedSamples) = engine.Render(scene, optionsLow, frameRecorder);
                string renderPath = Path.Combine(Path.GetDirectoryName(path), "render.png");
                if (File.Exists(renderPath))
                    File.Delete(renderPath);
                File.Move(path, renderPath);
                string albedoPath = Path.Combine(Path.GetDirectoryName(path), "albedo.png");
                string normalsPath = Path.Combine(Path.GetDirectoryName(path), "normals.png");

                
                Console.WriteLine("Low quality render albedo: ");
                optionsLow.RenderMode = PathTracerRenderMode.Color;
                (path, skippedSamples) = engine.Render(scene, optionsLow, frameRecorder);
                
                if (File.Exists(albedoPath))
                    File.Delete(albedoPath);
                File.Move(path, albedoPath);

                Console.WriteLine("Low quality render normals: ");
                optionsLow.RenderMode = PathTracerRenderMode.Normals;
                (path, skippedSamples) = engine.Render(scene, optionsLow, frameRecorder);
                
                if (File.Exists(normalsPath))
                    File.Delete(normalsPath);
                File.Move(path, normalsPath);
                
                string denoisedPath = Denoise(renderPath, albedoPath, normalsPath, Path.Combine(Path.GetDirectoryName(path), "lq-denoised.png"));

                

                

                //string odg = Console.ReadLine();
                //Console.WriteLine("psnr: " + odg);

                float[,] estimatedDifficulty = EstimatePixelDifficulty(denoisedPath);
                CreateDifficultyImage(estimatedDifficulty, Path.GetDirectoryName(path));
                
                Console.WriteLine("High quality render with difficulties: ");
                sw.Start();
                (path, skippedSamples) = engine.Render(scene, optionsHigh, frameRecorder, estimatedDifficulty);
                sw.Stop();
                long hDiff = sw.ElapsedMilliseconds;
                string focusedPath = Path.Combine(Path.GetDirectoryName(path), "focused.png");
                if (File.Exists(focusedPath))
                    File.Delete(focusedPath);
                File.Move(path, focusedPath);

                string focusedDenoised = Denoise(focusedPath, albedoPath, normalsPath, Path.Combine(Path.GetDirectoryName(path), "focused-denoised.png"));


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
                string sameSampleRatePath = Path.Combine(Path.GetDirectoryName(path), "unfocused-sameNumOfSamples.png");
                if (File.Exists(sameSampleRatePath))
                    File.Delete(sameSampleRatePath);
                File.Move(path, sameSampleRatePath);

                string ssrDenoised = Denoise(sameSampleRatePath, albedoPath, normalsPath, Path.Combine(Path.GetDirectoryName(path), "ssr-denoised.png"));

                Console.WriteLine("High quality rendering, without difficulties: ");
                consolePercentageDisplay = new ConsolePercentageDisplay();
                optionsHigh.PercentageDisplay = consolePercentageDisplay.Display;
                optionsHigh.SamplesPerPixel = 500;
                sw.Reset();
                sw.Start();
                (path, skippedSamples) = engine.Render(scene, optionsHigh, frameRecorder);
                sw.Stop();
                long full = sw.ElapsedMilliseconds;
                string maxSampleRatePath = Path.Combine(Path.GetDirectoryName(path), "unfocused-maxSamplesEverywhere.png");
                if (File.Exists(maxSampleRatePath))
                    File.Delete(maxSampleRatePath);
                File.Move(path, maxSampleRatePath);

                string maxDenoised = Denoise(maxSampleRatePath, albedoPath, normalsPath, Path.Combine(Path.GetDirectoryName(path), "max-denoised.png"));

                /*
                Console.WriteLine("Groundtruth: ");
                consolePercentageDisplay = new ConsolePercentageDisplay();
                optionsHigh.PercentageDisplay = consolePercentageDisplay.Display;
                optionsHigh.SamplesPerPixel = 200;
                sw.Reset();
                sw.Start();
                engine.Render(scene, optionsHigh, frameRecorder);
                sw.Stop();
                long groundtruth = sw.ElapsedMilliseconds;
                string groundtruthPath = Path.Combine(Path.GetDirectoryName(path), "groundtruth.png");
                if (File.Exists(groundtruthPath))
                    File.Delete(groundtruthPath);
                File.Move(path, groundtruthPath);

                string groundtruthDenoised = Denoise(groundtruthPath, albedoPath, normalsPath, Path.Combine(Path.GetDirectoryName(path), "groundtruth-denoised.png"));
                */
                using (StreamWriter bw = new StreamWriter(File.Create(Path.Combine(Path.GetDirectoryName(path), "stats.txt")))) {
                    bw.WriteLine("Elapsed milliseconds:\nWith focused rendering: {0}\nNo focus, similar number of samples: {1}\nWithout focused rendering, all samples: {2}\n", hDiff, custom, full);
                    bw.Close();
                }
                Console.WriteLine("Elapsed milliseconds:\nWith focused rendering: {0}\nNo focus, similar number of samples: {1}\nWithout focused rendering, all samples: {2}\n", hDiff, custom, full);
            }

            // Done
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static void Scene1 (PathTracerScene scene) {
            PathTracerColor neutral = new PathTracerColor(1f, 0.5f, 0.5f, 0.5f);

            PathTracerMaterial mizaMaterial = new PathTracerMaterial();
            mizaMaterial.Reflectivity = 0.3f;
            mizaMaterial.Gloss = 0.6f;
            mizaMaterial.Color = neutral;
            scene.ReadObject(@"D:\Scene\scena2\miza2.obj", texturePath: @"D:\textures\darkwood.jpg", material: mizaMaterial);

            PathTracerMaterial stene = new PathTracerMaterial();
            stene.Reflectivity = 0f;
            stene.Gloss = 0f;
            stene.Color = neutral;
            scene.ReadObject(@"D:\Scene\scena2\zid.obj", texturePath: @"D:\textures\stena2.jpg", material: stene);

            PathTracerMaterial salica = new PathTracerMaterial();
            salica.Reflectivity = 0.1f;
            salica.Gloss = 0.5f;
            salica.Color = new PathTracerColor(1, 0.8f, 0.8f, 0.8f);
            scene.ReadObject(@"D:\Scene\scena2\salica.obj", material: salica);

            PathTracerMaterial kroznik = new PathTracerMaterial();
            kroznik.Reflectivity = 0.4f;
            kroznik.Gloss = 0.9f;
            kroznik.Color = new PathTracerColor(1, 0.9f, 0.9f, 0.9f);
            scene.ReadObject(@"D:\Scene\scena2\kroznik.obj", material: kroznik);

            PathTracerMaterial vaza = new PathTracerMaterial();
            vaza.Reflectivity = 0.3f;
            vaza.Gloss = 0.4f;
            vaza.Color = new PathTracerColor(1, 0.9f, 0.2f, 0.2f);
            //vaza.IsLight = true;
            //vaza.Color = new PathTracerColor(1, 90, 20, 20);
            scene.ReadObject(@"D:\Scene\scena2\vaza.obj", material: vaza);

            PathTracerMaterial vilica = new PathTracerMaterial();
            vilica.Reflectivity = 0.8f;
            vilica.Gloss = 0.9f;
            vilica.Color = new PathTracerColor(1, 0.4f, 0.4f, 0.4f);
            scene.ReadObject(@"D:\Scene\scena2\vilica.obj", material: vilica);

            PathTracerMaterial okno = new PathTracerMaterial();
            okno.Reflectivity = 0.1f;
            okno.Gloss = 0.1f;
            okno.Color = new PathTracerColor(1, 0.8f, 0.8f, 0.8f);
            scene.ReadObject(@"D:\Scene\scena2\okno.obj", material: okno);

            PathTracerMaterial tla = new PathTracerMaterial();
            tla.Reflectivity = 0.1f;
            tla.Gloss = 0.4f;
            tla.Color = new PathTracerColor(1, 0.8f, 0.8f, 0.8f);
            scene.ReadObject(@"D:\Scene\scena2\tla.obj", texturePath: @"D:\textures\wood.jpg", material: tla);
        }

        private static string Denoise(string renderPath, string albedoPath, string normalsPath, string outputPath) {
            //call denoising process. Denoiser_v1.5 must be unpacked in bin directory. 
            string denoisedPath = outputPath;
            ProcessStartInfo si = new ProcessStartInfo();
            si.FileName = @"./Denoiser_v1.5/Denoiser.exe";
            si.Arguments = "-i " + renderPath + " -o " + denoisedPath + " -a " + albedoPath + " -n " + normalsPath;
            Process denoiseProcess = Process.Start(si);

            //wait for denoising to finish
            while (!denoiseProcess.HasExited) {
                denoiseProcess.WaitForExit();
            }

            return denoisedPath;
        }

        private static void CompareImages(string groundtruth, string image) {
            ProcessStartInfo si = new ProcessStartInfo();
            si.FileName = @"./sewar.exe";
            si.Arguments = "psnr " + groundtruth + " " + image;
            si.RedirectStandardOutput = true;
            Process imageQualityProcess = Process.Start(si);

            string odg = imageQualityProcess.StandardOutput.ReadLine();
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
            int[,] importanceNumOfJpegs = new int[render.Width, render.Height];

            int n = 20; //chunk size
            long uncompressedSize = n * n * 3;
            //split the bitmap in n/2 x n/2 chunks. They overlap.
            for (int x = 0; x < render.Width; x+=n/2) {
                for (int y = 0; y < render.Height; y+=n/2) {
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
                            importanceMatrix[i + x, j + y] += importance;
                            importanceNumOfJpegs[i + x, j + y]++;
                        }
                    }

                }
            }

            //povprecevanje glede na to v kolikih chunkih je dolocen pixel
            float max = float.NegativeInfinity, min = float.PositiveInfinity;
            for (int i = 0; i < importanceMatrix.GetLength(0); i++) {
                for (int j = 0; j < importanceMatrix.GetLength(1); j++) {
                    importanceMatrix[i, j] /= importanceNumOfJpegs[i, j];
                    if(importanceMatrix[i,j] > max) {
                        max = importanceMatrix[i, j];
                    }
                    if(importanceMatrix[i, j] < min) {
                        min = importanceMatrix[i, j];
                    }
                }
            }

            NormalizeMatrix(importanceMatrix, min, max, minOutput: 0.1f);
            return importanceMatrix;

        }


        private static void NormalizeMatrix(float[,] data, float min, float max, float minOutput = 0f, float maxOutput = 1f) {

            for (int i = 0; i < data.GetLength(0); i++) {
                for (int j = 0; j < data.GetLength(1); j++) {
                    data[i,j] = minOutput + ((data[i,j] - min) * (maxOutput - minOutput) / (max - min));
                }
            }
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