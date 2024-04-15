using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraTest
{
    internal class PredictInput
    {
        readonly ManageData manageData = new ManageData();

        public string FindNumInPicture(int LayerCount, int threashold)
        {
            Matrix<double> LayerMatrix = manageData.GetImage();


            MLP forwardPass = new MLP();

            CNNLayers cnn = new CNNLayers(LayerMatrix);

            cnn.Forwards(0, 1, threashold);

            forwardPass.Forwards(cnn.MatrixToVector(cnn.Cache[cnn.Cache.Count - 1]), 0, 2);


            string PredictedNum =TOMLHandle.GetOutputClasses()[forwardPass.Cache[forwardPass.Cache.Count() - 1].MaximumIndex()];
            return PredictedNum;

        }


    }
}
