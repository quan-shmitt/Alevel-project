using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CameraTest
{

    internal class ManageData
    {

        public Matrix<double> weights;
        public Vector<double> BiasVector;


        public Matrix<double> GetWeight(int layer)
        {
            SetWeightDimentions(layer);

            string Filename = $"Data\\layer {layer}\\Weights.txt";
            int k = 0;


                
                lock (CNNEntryPoint.filelock)
                {
                    try
                    {
                        while (!CNNEntryPoint.isfree)
                        {
                            Monitor.Wait(CNNEntryPoint.filelock);
                        }
                     

                        string[] ValsList = new string[weights.ColumnCount * weights.RowCount];

                        if(File.Exists(Filename))
                        {
                            ValsList = File.ReadAllText(Filename).Split(',');
                        }

                        k = 0;
                        for (int i = 0; i < weights.ColumnCount; i++)
                        {
                            for (int j = 0; j < weights.RowCount; j++)
                            {
                                weights[j, i] = Convert.ToDouble(ValsList[k]);
                                k++;
                            }
                        }

                        return weights;
                    }
                    finally
                    {
                        CNNEntryPoint.isfree = true;
                        Monitor.Pulse(CNNEntryPoint.filelock);
                    }
                }
            
        }

        public Vector<double> getBias(int layer)
        {
            SetBiasDimentions(layer);

            string filename = $"Data\\layer {layer}\\Bias.txt";
            int k = 0;

                lock (CNNEntryPoint.filelock)
                {
                    try
                    {
                        while (!CNNEntryPoint.isfree)
                        {
                            Monitor.Wait(CNNEntryPoint.filelock);
                        }
                        CNNEntryPoint.isfree = false;

                        string content = File.ReadAllText(filename);
                        string[] vals = content.Split(',');
                        for (int i = 0; i < BiasVector.Count; i++)
                        {
                            BiasVector[i] = Convert.ToDouble(vals[k]);
                            k++;
                        }
                        return BiasVector;
                    }
                    finally
                    {
                        CNNEntryPoint.isfree = true;
                        Monitor.Pulse(CNNEntryPoint.filelock);
                    }
                }
            }


        public Matrix<double> LayerVectorGen(int Pass)
        {
            Matrix<double> LayerVector = NetInIt.Images[Pass];
            return LayerVector;
        }

        public void SaveWeights(Matrix<double> weights, int layer)
        {


            string filename = $"Data\\Layer {layer}\\Weights.txt";

            var weight = new string[weights.ColumnCount * weights.RowCount];

            int k = 0;

            for (int i = 0; i < weights.ColumnCount; i++)
            {
                for (int j = 0; j < weights.RowCount; j++)
                {
                    weight[k] = weights[j, i].ToString();
                    k++;
                }
            }

            lock (CNNEntryPoint.filelock)
            {
                try
                {
                    while (!CNNEntryPoint.isfree)
                    {
                        Monitor.Wait(CNNEntryPoint.filelock);
                    }
                    CNNEntryPoint.isfree = false;

                    try
                    {
                        File.WriteAllText(filename, string.Join(",", weight));
                    }
                    catch
                    { 
                        
                    }
                }
                finally
                {
                    CNNEntryPoint.isfree = true;
                    Monitor.Pulse(CNNEntryPoint.filelock);
                }
            }
        }

        public void SaveBias(Vector<double> Bias, int layer)
        {
            string filename = $"Data\\Layer {layer}\\Bias.txt";

            string[] bias = new string[Bias.Count];

            for (int i = 0; i < Bias.Count; i++)
            {
                bias[i] = Bias[i].ToString();
            }

            lock (CNNEntryPoint.filelock)
            {
                try
                {
                    while (!CNNEntryPoint.isfree)
                    {
                        Monitor.Wait(CNNEntryPoint.filelock);
                    }
                    CNNEntryPoint.isfree = false;

                    try
                    {
                        File.WriteAllText(filename, string.Join(",", bias));
                    }
                    catch
                    {

                    }
                }
                finally
                {
                    CNNEntryPoint.isfree = true;
                    Monitor.Pulse(CNNEntryPoint.filelock);
                }
            }
        }
        void SetWeightDimentions(int layer)
        {
            double dimention1;
            double dimention2;

            int[] TargetRes = TOMLHandle.GetTargetResolution();

            if (layer == 0)
            {
                dimention1 = TargetRes[0] * TargetRes[1];
                dimention2 = TOMLHandle.GetHiddenLayerCount()[layer];
            }
            else if (layer < TOMLHandle.LayerCount)
            {
                dimention1 = TOMLHandle.GetHiddenLayerCount()[layer];
                dimention2 = TOMLHandle.GetHiddenLayerCount()[layer + 1];
            }
            else
            {
                dimention1 = TOMLHandle.GetHiddenLayerCount()[layer - 1];
                dimention2 = TOMLHandle.GetOutputLayerCount();
            }

            weights = Matrix<double>.Build.Dense((int)dimention2, (int)dimention1);

        }
        void SetBiasDimentions(int layer)
        {
            double dimention2;

            if (layer < TOMLHandle.LayerCount)
            {
                dimention2 = TOMLHandle.GetHiddenLayerCount()[layer];
            }
            else
            {
                dimention2 = TOMLHandle.GetOutputLayerCount();
            }

            BiasVector = Vector<double>.Build.Dense((int)dimention2);

        }

        public List<Matrix<double>> getKernel()
        {
            List<Matrix<double>> Data = new List<Matrix<double>>();

            string directory = $"Data\\CNNLayer";

            lock (CNNEntryPoint.filelock)
            {
                try
                {
                    while (!CNNEntryPoint.isfree)
                    {
                        Monitor.Wait(CNNEntryPoint.filelock);
                    }
                    CNNEntryPoint.isfree = false;

                    foreach (string file in Directory.GetFiles(directory))
                    {
                        string content = File.ReadAllText(file);
                        string[] vals = content.Split(',');

                        double[,] doubleArray = new double[(int)Math.Sqrt(vals.Length), (int)Math.Sqrt(vals.Length)];
                        int k = 0;
                        for (int i = 0; i < Math.Sqrt(vals.Length); i++)
                        {
                            for (int j = 0; j < Math.Sqrt(vals.Length); j++)
                            {
                                doubleArray[i, j] = Convert.ToDouble(vals[k]);
                                k++;
                            }
                        }

                        Data.Add(Matrix<double>.Build.DenseOfArray(doubleArray));
                    }
                    return Data;
                }
                finally
                {
                    CNNEntryPoint.isfree = true;
                    Monitor.Pulse(CNNEntryPoint.filelock);
                }

            }
        }

        public void SaveKernel(Matrix<double> Kernel, int Layer)
        {
            string filename = $"Data\\CNNLayer\\Kernel {Layer}.txt";

            string[] kernel = new string[Kernel.RowCount * Kernel.RowCount];
            int k = 0;

            for(int i = 0;i < Kernel.RowCount; i++)
            {
                for(int j = 0;j < Kernel.ColumnCount; j++)
                {
                    kernel[k] = Kernel[i, j].ToString();
                    k++;
                }
            }

            lock (CNNEntryPoint.filelock)
            {
                try
                {
                    while (!CNNEntryPoint.isfree)
                    {
                        Monitor.Wait(CNNEntryPoint.filelock);
                    }
                    CNNEntryPoint.isfree = false;
                    try
                    {
                        File.WriteAllText(filename, string.Join(",", kernel));
                    }
                    catch { }
                }
                finally
                {
                    CNNEntryPoint.isfree = true;
                    Monitor.Pulse(CNNEntryPoint.filelock);
                }
            }
        }

    }
}
