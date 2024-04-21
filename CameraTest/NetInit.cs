using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CameraTest
{
    internal class NetInIt
    {


        public Matrix<double> weights;

        public Matrix<double> LayerMatrix;


        public Vector<double> BiasVector;

        public static List<Matrix<double>> Images = ImageHandle.NormRGB("Data\\Images");


        public NetInIt(int Pass, int layerCount, int CNNCount)
        {

            //CreateParentDataset(Convert.ToUInt32(MaxImageCount));
            int[] dim = new int[] { (int)TOMLHandle.GetKernelSize(), (int)TOMLHandle.GetKernelSize() };

            LayerMatrix = new ManageData().LayerVectorGen(Pass);

            fileGen(layerCount, CNNCount, dim);

            LayerGen(Pass, layerCount - 1);

        }



        public NetInIt()
        {

        }

        void fileGen(int layer, int CNNlayer, int[] dim)
        {
            MLPGen(layer);
            CNNGen(CNNlayer, dim);
        }

        public void MLPGen(int layer)
        {
            for (int i = 0; i < layer; i++)
            {
                string filename = $"Data\\Layer {i}";
                if (!File.Exists(filename))
                {
                    Directory.CreateDirectory(filename);
                }
            }
        }
        void LayerGen(int Pass, int layer)
        {
            for (int i = 0; i <= layer; i++)
            {
                WeightGen(i);
                BiasGen(i);
            }
        }


        public void CNNGen(int CNNLayer, int[] dim)
        {

            string DirectoryName = $"Data\\CNNLayer";

            if (!Directory.Exists(DirectoryName))
            {
                Directory.CreateDirectory(DirectoryName);
            }

            for (int i = 0; i < CNNLayer; i++)
            {
                string filename = $"Data\\CNNLayer\\Kernel {i}.txt";
                if (!File.Exists(filename))
                {
                    File.WriteAllText(filename, string.Join(",", CNNLayerGen(dim)));
                }
            }
        }

        double[] CNNLayerGen(int[] dimentions)
        {
            Random random = new Random();
            double[] vals = new double[dimentions[0] * dimentions[1]];

            for (int i = 0; i < dimentions[0] * dimentions[1]; i++)
            {
                vals[i] = -1 + 2 * random.NextDouble();
            }

            return vals;

        }




        public void WeightGen(int layer)
        {
            Random random = new Random();

            SetWeightDimentions(layer);


            string Filename = $"Data\\Layer {layer}\\Weights.txt";
            int k = 0;



            if (File.Exists(Filename))
            {
                string content = File.ReadAllText(Filename);
                string[] vals = content.Split(',');
                for (int i = 0; i < weights.ColumnCount - 1; i++)
                {
                    for (int j = 0; j < weights.RowCount - 1; j++)
                    {
                        weights[j, i] = Convert.ToDouble(vals[k]);
                        k++;
                    }
                }

            }
            else
            {

                int batchSize = 1000; // Choose an appropriate batch size
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < weights.ColumnCount * weights.RowCount; i += batchSize)
                {
                    // Calculate the size of the current batch
                    int currentBatchSize = Math.Min(batchSize, weights.ColumnCount * weights.RowCount - i);

                    // Clear the StringBuilder for the new batch
                    builder.Clear();

                    // Process the current batch
                    for (int j = i; j < i + currentBatchSize; j++)
                    {
                        builder.Append((-1 + 2 * random.NextDouble()).ToString());
                        builder.Append(",");
                    }


                    string batchResult = builder.ToString();

                    File.AppendAllText(Filename, batchResult);
                }

                List<string> valsList = new List<string>();

                using (StreamReader sr = new StreamReader(Filename))
                {
                    string content = sr.ReadToEnd();
                    string[] chunkVals = content.Split(',');
                    valsList.AddRange(chunkVals);
                }

                string[] vals = valsList.ToArray();
                k = 0;

                for (int i = 0; i < weights.ColumnCount; i++)
                {
                    for (int j = 0; j < weights.RowCount; j++)
                    {
                        if (k < vals.Length && !string.IsNullOrEmpty(vals[k]))
                        {
                            weights[j, i] = Convert.ToDouble(vals[k]);
                            k++;
                        }
                        else
                        {
                            weights[i, j] = -1 + 2 * random.NextDouble();
                        }
                    }
                }



            }
        }


        public void BiasGen(int layer)
        {
            SetBiasDimentions(layer);

            string filename = $"Data\\layer {layer}\\Bias.txt";
            int k = 0;

            if (File.Exists(filename))
            {

                string content = File.ReadAllText(filename);
                string[] vals = content.Split(',');
                for (int i = 0; i < BiasVector.Count; i++)
                {
                    BiasVector[i] = Convert.ToDouble(vals[k]);
                    k++;
                }
            }
            else
            {
                string[] vals = new string[BiasVector.Count];
                for (int i = 0; i < BiasVector.Count; i++)
                {
                    vals[i] = "0";
                }

                try
                {
                    File.WriteAllText(filename, string.Join(",", vals));
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");

                }


                System.Array.Clear(vals, 0, vals.Length);

                string content = File.ReadAllText(filename);
                vals = content.Split(',');
                for (int i = 0; i < BiasVector.Count; i++)
                {
                    BiasVector[i] = Convert.ToDouble(vals[k]);
                    k++;
                }


            }
        }


        void SetWeightDimentions(int layer)
        {
            ManageData getData = new ManageData();

            double dimention1;
            double dimention2;

            if (layer == 0)
            {
                dimention1 = TOMLHandle.GetTargetResolution()[0] * TOMLHandle.GetTargetResolution()[1];

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
            ManageData getData = new ManageData();

            double dimention2;

            if (layer < TOMLHandle.LayerCount)
            {
                dimention2 = TOMLHandle.GetHiddenLayerCount()[layer];
            }
            else
            {
                dimention2 = TOMLHandle.GetOutputLayerCount();
            }
            BiasVector = Vector<double>.Build.DenseOfArray(new double[(int)dimention2]);

        }


    }


    
}
