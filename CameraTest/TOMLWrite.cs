using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nett;

namespace CameraTest
{
    static internal class TOMLWrite
    {
        static TomlTable TOMLFILE = Toml.ReadFile("config.toml");

        public static void WriteHiddenLayerCount(int[] layerCount)
        {



            //TOMLFILE.Get<TomlTable>("MLPStruct")["HiddenLayerCount"] = 


            // Write the modified config back to the TOML file
            //Toml.WriteFile(HiddenLayerCount, "config.toml");

        }
    }
}
