using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using MathNet.Numerics.Providers.FourierTransform;

namespace CameraTest
{
    internal class CNNEntryPoint
    {

        static readonly int LayerCount = TOMLHandle.GetHiddenLayerCount().Length + 1;
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






            int Passes = TrainingMenu.GetPassCount();
            int epochs = TrainingMenu.GetEpochs();

            Pass(Passes, epochs);
            

            Console.WriteLine(sw.Elapsed.ToString());
        }

        static void Pass(int Passes, int epoch)
        {
            ManageData manageData = new ManageData();



            NetInIt networkGen = new NetInIt(Passes, LayerCount, CNNCount);

            double learningRate = TOMLHandle.GetLearningRate();

            int numberOfThreads = 10;
            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = numberOfThreads
            };

            for (int j = 0; j < epoch; j++)
            {
                for (int batch = 0; batch <= Passes; batch += numberOfThreads)
                {
                    List<Task> tasks = new List<Task>();

                    List<Matrix<double>> weights = new List<Matrix<double>>();
                    List<Vector<double>> bias = new List<Vector<double>>();
                    List<Matrix<double>> kernels = new List<Matrix<double>>();

                    // Launch tasks for the current batch
                    for (int i = batch; i < batch + numberOfThreads && i <= Passes; i++)
                    {

                        int currentBatchIndex = i;
                        tasks.Add(Task.Run(() =>
                        {
                            MLP forwardPass = new MLP();
                            Matrix<double> input = manageData.LayerVectorGen(currentBatchIndex);
                            CNNLayers CNN = new CNNLayers(input);

                            if (!File.Exists($"Data\\Pass {currentBatchIndex}\\Output\\LayerVector.txt"))
                            {
                                CNN.Forwards(0, CNNCount, 200);
                                forwardPass.Forwards(CNN.MatrixToVector(CNN.result), 0, LayerCount);
                            }

                            Backpropagation backpropagation = new Backpropagation(LayerCount);
                            List<Vector<double>> Input = forwardPass.Cache;
                            backpropagation.BackProp(Input, ImageHandle.Labels[currentBatchIndex], learningRate, LayerCount);
                            backpropagation.BackpropagateConvLayers(manageData.getKernel()[0], learningRate, CNNCount);
                            List<Matrix<double>> averagedWeights = new List<Matrix<double>>();

                            for(int k = 0; k < LayerCount; k++)
                            {
                                if (weights.Count < LayerCount)
                                {
                                    weights.Add(backpropagation.ChangeInWeights[k]);
                                    bias.Add(backpropagation.ChangeInBias[k]);
                                    if(kernels.Count < CNNCount)
                                    {
                                        kernels.Add(backpropagation.ChangeInKernel[k]);
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        weights[k] += backpropagation.ChangeInWeights[k];
                                        bias[k] += backpropagation.ChangeInBias[k];
                                    }
                                    catch { }
                                    if (k < CNNCount)
                                    {
                                        kernels[k] += backpropagation.ChangeInKernel[k];
                                    }
                                }
                            }
                        }));
                    }

                    // Wait for all tasks in the current batch to complete
                    Task.WaitAll(tasks.ToArray());

                    weights = weights.Select(matrix => matrix/numberOfThreads).ToList();
                    bias = bias.Select(vector => vector/ numberOfThreads).ToList();
                    kernels = kernels.Select(matrix => matrix/ numberOfThreads).ToList();

                    for (int i = 0; i < LayerCount; i++)
                    {
                        manageData.SaveWeights(manageData.GetWeight(i) - weights[LayerCount - i - 1] ,i);
                        manageData.SaveBias(manageData.getBias(i) - bias[LayerCount - i - 1] ,i);
                    }
                    for(int i = 0;i < CNNCount; i++)
                    {
                        manageData.SaveKernel(manageData.getKernel()[i] - kernels[CNNCount - i - 1], i);
                    }

                }

                TrainingMenu.SetCost(cost / (Passes + 1));
                Console.WriteLine((cost / (Passes + 1)).ToString());
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
                fileCount += CountFiles(subDirectory) - 1;
            }

            return fileCount;
        }
  
    }
}
