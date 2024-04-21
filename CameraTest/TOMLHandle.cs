using Nett;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraTest
{
    internal static class TOMLHandle
    {
        static TomlTable TOMLFILE;

        public static int LayerCount;

        public static void GetToml(string filename)
        {
            TOMLFILE = Toml.ReadFile(filename);
            LayerCount = GetHiddenLayerCount().Length;
        }

        public static string[] GetCNNStruct()
        {
            var algorithmsArray = TOMLFILE.Get<TomlTable>("CNNStruct").Get<TomlArray>("algorithms");

            string[] stringArray = algorithmsArray.Items.Select(item => item.Get<string>()).ToArray();

            return stringArray;
        }

        public static double GetKernelSize()
        {
            var KernelSize = TOMLFILE.Get<TomlTable>("CNNStruct").Get<double>("kernelSize");

            return KernelSize;
        }

        public static double GetKernelStep()
        {
            var KernelStep = TOMLFILE.Get<TomlTable>("CNNStruct").Get<double>("kernelSteps");

            return KernelStep;
        }

        public static int GetCNNLayerCount()
        {
            var CNNLayerCount = TOMLFILE.Get<TomlTable>("CNNStruct").Get<int>("CNNRepeats");

            System.Console.WriteLine(CNNLayerCount);
            return CNNLayerCount;
        }


        public static double GetLearningRate()
        {
            var LearningRate = TOMLFILE.Get<TomlTable>("MLPStruct").Get<double>("LearningRate");

            return LearningRate;
        }

        public static double[] GetHiddenLayerCount()
        {
            var HiddenLayerCount = TOMLFILE.Get<TomlTable>("MLPStruct").Get<TomlArray>("HiddenLayerCount");

            double[] HiddenCount = HiddenLayerCount.Items.Select(item => item.Get<double>()).ToArray();

            return HiddenCount;
        }

        public static string[] GetOutputClasses()
        {
            var OutputClasses = TOMLFILE.Get<TomlTable>("MLPStruct").Get<TomlArray>("OutputClasses");

            string[] StringArray = OutputClasses.Items.Select(item => item.Get<string>()).ToArray();

            return StringArray;
        }

        public static int GetOutputLayerCount()
        {
            var OutputClasses = TOMLFILE.Get<TomlTable>("MLPStruct").Get<TomlArray>("OutputClasses");

            return OutputClasses.Length;
        }

        public static int[] GetTargetResolution()
        {
            var TargetRes = TOMLFILE.Get<TomlTable>("CNNStruct").Get<TomlArray>("TargetResolution");

            int[] Scale = TargetRes.Items.Select(item => item.Get<int>()).ToArray();

            return Scale;
        }

        public static double GetPoolSize()
        {
            var PoolSize = TOMLFILE.Get<TomlTable>("CNNStruct").Get<double>("PoolSize");

            return PoolSize;
        }

        public static double GetTestCount()
        {
            var Testcount = TOMLFILE.Get<TomlTable>("Testing").Get<double>("TestCount");

            return Testcount;
        }

        public static double GetTestAcc()
        {
            var acc = TOMLFILE.Get<TomlTable>("Testing").Get<double>("TestAccuracy");

            return acc;
        }


    }
}
