using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CameraTest
{
    internal class ImageHandle
    {
        public List<Vector<double>> Label(int dimensionSize, string directoryPath)
        {
            List<Vector<double>> labels = new List<Vector<double>>();
            List<string> fileExtensions = new List<string> { ".jpg",".bmp", ".jpeg" };

            foreach (string d in Directory.GetDirectories(directoryPath))
            {
                foreach (string fileExt in fileExtensions)
                {
                    string[] files = Directory.GetFiles(d, $"*{fileExt}", SearchOption.AllDirectories);
                    foreach (string fileName in files)
                    {
                        string folderName = Path.GetFileName(Path.GetDirectoryName(fileName));

                        // Create a one-hot encoded vector for the label
                        Vector<double> label = Vector<double>.Build.Dense(dimensionSize);
                        int labelIndex = GetLabelIndex(folderName);
                        label[labelIndex] = 1; // Set the corresponding index to 1

                        labels.Add(label);
                    }
                }
            }

            return labels;
        }
        private int GetLabelIndex(string folderName)
        {
            string[] Classes = TOMLHandle.GetOutputClasses();

            if (Array.IndexOf(Classes, folderName) != -1)
            {
                int index = Array.IndexOf(Classes, folderName);
                return index;
            }

            return -1;
        }

        public List<Matrix<double>> NormRGB(string directoryPath)
        {

            List<Matrix<double>> allRGBValues = new List<Matrix<double>>();

            List<string> fileExtensions = new List<string> { ".jpg", ".bmp", ".jpeg" };

            // Create tasks to process images in parallel
            List<Task> tasks = new List<Task>();

            foreach (string d in Directory.GetDirectories(directoryPath))
            {
                foreach (string fileExt in fileExtensions)
                {
                    string[] files = Directory.GetFiles(d, $"*{fileExt}", SearchOption.AllDirectories);
                    foreach (string fileName in files)
                    {
                        // Start a new task to process each image
                        Task task = Task.Run(() =>
                        {
                            using (Bitmap image = new Bitmap(fileName))
                            {
                                Matrix<double> RGBVal = Matrix<double>.Build.Dense(image.Width, image.Height);

                                for (int y = 0; y < image.Height; y++)
                                {
                                    for (int x = 0; x < image.Width; x++)
                                    {
                                        Color color = image.GetPixel(x, y);
                                        double normColor = color.GetBrightness();
                                        RGBVal[x, y] = normColor;
                                    }
                                }

                                lock (allRGBValues)
                                {
                                    allRGBValues.Add(RGBVal);
                                }
                            }
                        });

                        tasks.Add(task);
                    }
                }
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());

            return allRGBValues;

        }


        private string SanitizeFolderName(string folderName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char invalidChar in invalidChars)
            {
                folderName = folderName.Replace(invalidChar.ToString(), "");
            }
            return folderName;
        }
    }
}
