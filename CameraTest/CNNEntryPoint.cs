using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace CameraTest
{
    internal class CNNEntryPoint
    {

        static readonly int LayerCount = 2;
        static int CNNCount;



        static public object filelock = new object();
        static public bool isfree = true;

        static public double cost = 0;

        public static void Entry()
        {
            TOMLHandle.GetToml("Data\\Configs\\config.toml");

            CNNCount = TOMLHandle.GetCNNLayerCount();

            ManageData manageData = new ManageData();

            Stopwatch sw = new Stopwatch();
            sw.Start();






            int Passes = CountFiles("Data\\Images") - 5000;
            int epochs = 1;

            Pass(Passes, epochs);
            

            Console.WriteLine(sw.Elapsed.ToString());
        }

        static void Pass(int Passes, int epoch)
        {
            ManageData manageData = new ManageData();



            NetInIt networkGen = new NetInIt(Passes, LayerCount, CNNCount);

            double learningRate = TOMLHandle.GetLearningRate();

            for (int j = 0; j < epoch; j++)
            {
                Parallel.For(0, Passes + 1, i =>
                {
                    MLP forwardPass = new MLP();

                    Matrix<double> input = manageData.LayerVectorGen(i);
                    CNNLayers CNN = new CNNLayers(input);


                    if (!File.Exists($"Data\\Pass {i}\\Output\\LayerVector.txt"))
                    {
                        CNN.Forwards(0, CNNCount, 200);
                        forwardPass.Forwards(CNN.MatrixToVector(CNN.result), 0, LayerCount);
                    }
                    Backpropagation backpropagation = new Backpropagation(LayerCount);

                    List<Vector<double>> Input = forwardPass.Cache;

                    backpropagation.BackProp(Input, ImageHandle.Labels[i], learningRate, LayerCount);
                    backpropagation.BackpropagateConvLayers(manageData.getKernel()[0],learningRate , CNNCount);
                });
                TrainingMenu.SetCost(cost / Passes);
                Console.WriteLine((cost/Passes).ToString());
                cost = 0;
            }


        }

        public static int CountFiles(string directory)
        {
            int fileCount = 0;

            // Count files in the current directory
            fileCount += Directory.GetFiles(directory).Length;

            // Count files in subdirectories recursively
            foreach (string subDirectory in Directory.GetDirectories(directory))
            {
                fileCount += CountFiles(subDirectory);
            }

            return fileCount;
        }
  
    }
}
