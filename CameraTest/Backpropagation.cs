using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace CameraTest
{
    internal class Backpropagation
    {
        public ManageData getData = new ManageData();

        public List<Matrix<double>> Weights = new List<Matrix<double>>();
        public List<Vector<double>> Bias = new List<Vector<double>>();
        public List<Matrix<double>> kernels = new List<Matrix<double>>();

        public List<Matrix<double>> ChangeInWeights = new List<Matrix<double>>();
        public List<Vector<double>> ChangeInBias = new List<Vector<double>>();
        public List<Matrix<double>> ChangeInKernel = new List<Matrix<double>>();

        Vector<double> upstreamGradient;

        public Backpropagation(int layer)
        {

            for (int i = 0; i < layer; i++)
            {
                Weights.Add(getData.GetWeight(i));
            }

            for (int i = 0; i < layer; i++)
            {
                Bias.Add(getData.getBias(i));
            }
        }

        public void BackProp(List<Vector<double>> LayerVectors, Vector<double> Target, double LearningRate, int layer)
        {
            UpdateTextBox("Backpropagating");

            CNNEntryPoint.cost += CalculateSparseCategoricalCrossEntropy(LayerVectors[layer], Convert.ToInt32(Target.MaximumIndex()));

            var gradientWrtWeights = (LayerVectors[layer] - Target).ToColumnMatrix() * LayerVectors[layer - 1].ToRowMatrix();

            var gradientWrtBias = LayerVectors[layer] - Target;

            ChangeInWeights.Add(LearningRate * gradientWrtWeights);

            ChangeInBias.Add(LearningRate * gradientWrtBias);

            upstreamGradient = LayerVectors[layer] - Target;

            layer--;

            while (layer > 0)
            {
                gradientWrtWeights = (ReLU_Derivative(LayerVectors[layer]).ToColumnMatrix() * LayerVectors[layer - 1].ToRowMatrix());

                gradientWrtBias = Vector<double>.Build.DenseOfArray(new double[Bias[layer - 1].Count]);

                var GradWrtLlogits = ReLU_Derivative(LayerVectors[layer]);

                int k = 0;
                for (int i = 0; i < Bias[layer - 1].Count; i++)
                {
                    gradientWrtBias[i] = GradWrtLlogits[i] * upstreamGradient[k];
                    k++;
                    if (k >= upstreamGradient.Count)
                    {
                        k = 0;
                    }
                }

                ChangeInWeights.Add(LearningRate * gradientWrtWeights);

                ChangeInBias.Add(LearningRate * gradientWrtBias);

                upstreamGradient = (Weights[layer].Transpose() * upstreamGradient);
                upstreamGradient = upstreamGradient.PointwiseMultiply(ReLU_Derivative(LayerVectors[layer]));

                layer--;
            }
            upstreamGradient = (Weights[layer].Transpose() * upstreamGradient);
            upstreamGradient = upstreamGradient.PointwiseMultiply(ReLU_Derivative(LayerVectors[layer]));

        }


        public void BackpropagateConvLayers( Matrix<double> kernel,double learningRate ,int layer)
        {
            layer--;

            if (layer < 0)
            {
                return;
            }

            Matrix<double> gradient = ComputeGradient(upstreamGradient, kernel);

            ChangeInKernel.Add(learningRate * gradient);

            kernels.Add(kernel);
            BackpropagateConvLayers(kernel, learningRate, layer);
        }

        private Matrix<double> ComputeGradient(Vector<double> upstreamGradient, Matrix<double> kernel)
        { 
            Matrix<double> computedGradient = Convolution2D(upstreamGradient, kernel);

            return computedGradient;
        }


        private Matrix<double> Convolution2D(Vector<double> input, Matrix<double> kernel)
        {
            var NewInput = VectorToMatrix(input, (int)Math.Sqrt(input.Count),(int)Math.Sqrt(input.Count));

            int kernelRows = kernel.RowCount;
            int kernelCols = kernel.ColumnCount;

            // Calculate output size
            int outputRows = (int)TOMLHandle.GetKernelSize();
            int outputCols = (int)TOMLHandle.GetKernelSize();

            // Create matrix to store the result of convolution
            Matrix<double> result;


            result = Matrix<double>.Build.Dense(outputRows, outputCols);
            // Perform convolution
            for (int i = 0; i < outputRows; i++)
            {
                for (int j = 0; j < outputCols; j++)
                {
                    double sum = 0.0;
                    for (int k = 0; k < kernelRows; k++)
                    {
                        for (int l = 0; l < kernelCols; l++)
                        {
                            sum += NewInput[i + k, j + l] * kernel[k, l];
                        }
                    }
                    result[i, j] = sum;
                }
            }
            return result;
        }


        public static Matrix<double> VectorToMatrix(Vector<double> vector, int width, int height)
        {
            Matrix<double> matrix = Matrix<double>.Build.Dense(height, width);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    matrix[i, j] = vector[i * width + j];
                }
            }

            return matrix;
        }

        public static Vector<double> MatrixToVector(Matrix<double> matrix)
        {
            return Vector<double>.Build.DenseOfArray(matrix.ToColumnMajorArray());
        }

        double CalculateSparseCategoricalCrossEntropy(Vector<double> predictedProbabilities, int trueLabel)
        {
            if (predictedProbabilities == null || predictedProbabilities.Count == 0)
            {
                throw new ArgumentException("Invalid predicted probabilities array");
            }

            if (trueLabel < 0 || trueLabel >= predictedProbabilities.Count)
            {
                throw new ArgumentException("Invalid true label index");
            }

            double epsilon = 1e-15; // Small value to prevent log(0) errors
            double[] logProbabilities = new double[predictedProbabilities.Count];

            // Calculate log probabilities
            for (int i = 0; i < predictedProbabilities.Count; i++)
            {
                logProbabilities[i] = Math.Log(Math.Max(predictedProbabilities[i], epsilon));
            }

            // Calculate sparse categorical cross entropy loss
            double loss = -logProbabilities[trueLabel];

            return loss;
        }


        Vector<double> ReLU(Vector<double> x)
        {
            return x.PointwiseMaximum(0);
        }

        // Derivative of ReLU activation function
        static Vector<double> ReLU_Derivative(Vector<double> logits)
        {
            return logits.Map(x => x > 0 ? 1.0 : 0.0);
        }

        Vector<double> Softmax(Vector<double> logits)
        {
            // Avoid numerical instability by subtracting the maximum logit
            double maxLogit = logits.Maximum();
            Vector<double> expLogits = logits.Subtract(maxLogit).PointwiseExp();

            // Calculate the sum of exponentials
            double sumExp = expLogits.Sum();

            // Compute the softmax probabilities
            Vector<double> probabilities = expLogits.Divide(sumExp);

            return probabilities;
        }

        Vector<double> SoftmaxDerivativeVector(Vector<double> logits)
        {
            int K = logits.Count;
            Vector<double> result = Vector<double>.Build.Dense(K, (i) =>
            {
                int row = i / K;
                int col = i % K;
                if (row == col)
                    return logits[row] * (1 - logits[row]);
                else
                    return -logits[row] * logits[col];
            });

            return result;
        }

        public void UpdateTextBox(string text)
        {
            // Check if invoking is required
            if (TrainingMenu.status.InvokeRequired)
            {
                // If it is, invoke the method on the UI thread
                TrainingMenu.status.Invoke((MethodInvoker)delegate {
                    TrainingMenu.status.Text = text;
                });
            }
            else
            {
                // If not, directly update the textbox
                TrainingMenu.status.Text = text;
            }
        }


    }
}
