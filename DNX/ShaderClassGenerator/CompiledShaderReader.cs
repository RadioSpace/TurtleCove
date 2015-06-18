using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using SharpDX;
using SharpDX.D3DCompiler;


namespace ShaderClassGenerator
{
    public class CompiledShaderReader : IDisposable
    {

        #region static
        public static CompiledShaderReader ReadCompiledShader(string path)
        {

            if (File.Exists(path) && (Path.GetExtension(path) == ".cso"))
            {

                bool isVertexShader = false;

                byte[] bytes = File.ReadAllBytes(path);
                ShaderReflection reflecter = new ShaderReflection(bytes);

                ShaderBytecode bytecode = new ShaderBytecode(bytes);
                ShaderProfile profile = bytecode.GetVersion();
                if (profile.GetTypePrefix() == "vs")
                {
                    isVertexShader = true;
                }

                ConstantBuffer[] cbuffers = new ConstantBuffer[reflecter.Description.ConstantBuffers];
                for (int i = 0; i < reflecter.Description.ConstantBuffers; i++)
                {
                    cbuffers[i] = reflecter.GetConstantBuffer(i);//this might not work
                }

                ShaderParameterDescription[] paramdescriptions = new ShaderParameterDescription[reflecter.Description.InputParameters];
                for (int i = 0; i < reflecter.Description.InputParameters; i++)
                {
                    paramdescriptions[i] = reflecter.GetInputParameterDescription(i);
                }

                InputBindingDescription[] bindings = new InputBindingDescription[reflecter.Description.BoundResources];
                for (int i = 0; i < reflecter.Description.BoundResources; i++)
                {
                    bindings[i] = reflecter.GetResourceBindingDescription(i);
                }

                ConstantBuffer[] cbuffersCOPY = cbuffers.ToArray();

               

                return new CompiledShaderReader(path, isVertexShader, cbuffersCOPY, paramdescriptions.ToArray(), bindings.ToArray(),reflecter,bytecode);


            }
            else throw new Exception("Bad path " + path);
        }
        #endregion

        string filename;
        /// <summary>
        /// gets the filename that the shader information came from
        /// </summary>
        public string FileName { get { return filename; } }

        ShaderReflection reflector;
        ShaderBytecode bytecode;

        ConstantBuffer[] cBuffers;
        ShaderParameterDescription[] parameterDescriptions;
        InputBindingDescription[] resourceBindings;

        bool isVertexShader;
        /// <summary>
        /// gets whether or not the code is for a vertex shader
        /// </summary>
        public bool IsVertexShader { get { return isVertexShader; } }


        CompiledShaderReader(string fn,bool _isVertexShader,ConstantBuffer[] cbs , ShaderParameterDescription[] pds,InputBindingDescription[] rbs,ShaderReflection reflec,ShaderBytecode bytec)
        {
            cBuffers = cbs;
            parameterDescriptions = pds;
            resourceBindings = rbs;
            filename = fn;
            isVertexShader = _isVertexShader;

            reflector = reflec;
            bytecode = bytec;
        }



        public ConstantBuffer[] GetConsttantBuffers()
        {
            return cBuffers;
        }

        public ShaderParameterDescription[] GetParameterDescription()
        {
            return parameterDescriptions;
        }

        public InputBindingDescription[] GetBindingDescription()
        {
            return resourceBindings;
        }


        public void Dispose()
        {
            if(cBuffers !=null)
            {
                foreach(ConstantBuffer cb in cBuffers)
                {
                    if(cb != null)cb.Dispose();
                }
                
            }

            bytecode.Dispose();
            reflector.Dispose();
            
        }
    }
}
