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


                ShaderReflection reflecter = new ShaderReflection(File.ReadAllBytes(path));
                
                

                ConstantBuffer[] cbuffers = new ConstantBuffer[reflecter.Description.ConstantBuffers];
                for (int i = 0; i < reflecter.Description.ConstantBuffers; i++)
                {
                    cbuffers[i] = reflecter.GetConstantBuffer(i);
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

                reflecter.Dispose();

                return new CompiledShaderReader(path ,cbuffers, paramdescriptions, bindings);


            }
            else throw new Exception("Bad path " + path);
        }
        #endregion

        string filename;
        /// <summary>
        /// gets the filename that the shader information came from
        /// </summary>
        public string FileName { get { return filename; } }

        ConstantBuffer[] cBuffers;
        ShaderParameterDescription[] parameterDescriptions;
        InputBindingDescription[] resourceBindings;
        



        CompiledShaderReader(string fn,ConstantBuffer[] cbs , ShaderParameterDescription[] pds,InputBindingDescription[] rbs)
        {
            cBuffers = cbs;
            parameterDescriptions = pds;
            resourceBindings = rbs;
            filename = fn;
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
        }
    }
}
