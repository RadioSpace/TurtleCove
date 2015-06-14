using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ShaderClassGenerator
{
    class Program
    {
        static void Main(string[] args)
        {

            //accepts compiled shader file path as input

            //idea: argument 2 could be namespace

            if (args.Length > 0)
            {
                string filepath = args[0];

                if (File.Exists(filepath) && Path.GetExtension(filepath) == ".cso")
                {
                    CompiledShaderReader reader = CompiledShaderReader.ReadCompiledShader(filepath);

                    ShaderLibraryGenerator gen = new ShaderLibraryGenerator();
                    gen.Generate(Path.ChangeExtension(filepath,".cs"),reader);

                }
            }
        }
    }
}
