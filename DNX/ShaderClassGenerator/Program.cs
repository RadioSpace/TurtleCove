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
            //idea: argument 3 could be shader type hint.  a vertex shader has a few more needs than other shaders. and if a vertex shader is writtin for not using a vertex buffer it would be different still

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
