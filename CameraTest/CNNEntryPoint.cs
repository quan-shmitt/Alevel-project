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

            Datainit();




            int Passes = 500;
            int epochs = 5;

            Pass(Passes, epochs);
            

            Console.WriteLine(sw.Elapsed.ToString());
        }

        static void Pass(int Passes, int epoch)
        {
            ManageData manageData = new ManageData();


            ImageHandle image = new ImageHandle();

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

                    backpropagation.BackProp(Input, image.Label(Convert.ToInt32(i), 10), learningRate, LayerCount);
                    backpropagation.BackpropagateConvLayers(manageData.getKernel()[0],learningRate , CNNCount);
                });
                TrainingMenu.SetCost(cost / Passes);
                Console.WriteLine((cost/Passes).ToString());
                cost = 0;
            }


        }



        static void Datainit()
        {
            string path = "MNIST\\MNISTDataSet.exe";

            if (!File.Exists("MNIST\\images"))
            {
                Console.WriteLine("started processing");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                };

                try
                {
                    Process process = Process.Start(startInfo);
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
