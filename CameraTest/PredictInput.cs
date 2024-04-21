using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraTest
{
    internal class PredictInput
    {
        readonly ManageData manageData = new ManageData();

        public string Test()
        {
            int Acc = 0;
            NetInIt netInIt = new NetInIt();

            Parallel.For(0, (int)TOMLHandle.GetTestCount(), i =>
            {

                MLP forwardPass = new MLP();

                Matrix<double> input = manageData.LayerVectorGen(i);
                CNNLayers CNN = new CNNLayers(input);



                CNN.Forwards(0, TOMLHandle.GetCNNLayerCount(), 200);
                forwardPass.Forwards(CNN.MatrixToVector(CNN.result), 0, TOMLHandle.GetHiddenLayerCount().Count() + 1);

                string PredictedNum = TOMLHandle.GetOutputClasses()[forwardPass.Cache[forwardPass.Cache.Count() - 1].MaximumIndex()];

                Console.WriteLine(PredictedNum);

                if (PredictedNum == TOMLHandle.GetOutputClasses()[ImageHandle.Labels[i].MaximumIndex()])
                {
                    Acc++;
                }

            });

            Console.WriteLine(Acc);
            return (Acc/TOMLHandle.GetTestCount()).ToString();

        }

        public string PredictContents(Bitmap image)
        {

            MLP forwardPass = new MLP();

            Matrix<double> input = BitmapToMatrix(image);
            CNNLayers CNN = new CNNLayers(input);



            CNN.Forwards(0, TOMLHandle.GetCNNLayerCount(), 200);
            forwardPass.Forwards(CNN.MatrixToVector(CNN.result), 0, TOMLHandle.GetHiddenLayerCount().Count() + 1);

            string Predicted = TOMLHandle.GetOutputClasses()[forwardPass.Cache[forwardPass.Cache.Count() - 1].MaximumIndex()];

            return Predicted;
        }

        public static Matrix<double> BitmapToMatrix(Bitmap bitmap)
        {
            try
            {
                int width = bitmap.Width;
                int height = bitmap.Height;

                Matrix<double> matrix = Matrix<double>.Build.Dense(height, width);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color pixelColor = bitmap.GetPixel(x, y);

                        double grayscale = (0.2989 * pixelColor.R + 0.5870 * pixelColor.G + 0.1140 * pixelColor.B) / 255.0;

                        matrix[y, x] = grayscale;
                    }
                }

                return matrix;
            }
            catch
            {
                throw new ArgumentNullException(nameof(bitmap), "Bitmap cannot be null");
            }
        }

    }
}
