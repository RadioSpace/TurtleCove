using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;



using System.CodeDom;
using System.CodeDom.Compiler;

using System.Reflection;

using SharpDX;
using SharpDX.D3DCompiler;

namespace ShaderClassGenerator
{
    
    public class ShaderLibraryGenerator
    {
        public void Generate(string filepath , CompiledShaderReader reader)
        {

            string classname = Path.GetFileNameWithoutExtension(filepath);
            string outputfilename = Path.ChangeExtension(filepath, ".cs");

            CodeCompileUnit targetUnit = new CodeCompileUnit();

           

            CodeNamespace targetNamespace = new CodeNamespace("ShaderClasses");
            SetCommonImports(targetNamespace);


            CodeTypeDeclaration targetClass = new CodeTypeDeclaration(Path.GetFileNameWithoutExtension(filepath));
            targetClass.IsClass = true;
            targetClass.TypeAttributes = TypeAttributes.Public;



            //create a type for the input




            if (reader.IsVertexShader)
            {
                //BUG: need to not make a vertex class if the Input is only SV_VertexID as this indicates that the vertex shader is not using a vetrex buffer
                
                
                //add a the input layout field
                targetClass.Members.Add(new CodeMemberField("LayoutInput", "inputLayout"));

                //create a class to store vertex  information
                CodeTypeDeclaration vertexClass = new CodeTypeDeclaration(targetClass.Name + "_Vertex");
                vertexClass.IsStruct = true;


                ShaderParameterDescription[] paramdesciptions = reader.GetParameterDescription();

                int sizeInBytes = 0;

                foreach (ShaderParameterDescription paramdesc in paramdesciptions)
                {
                    string fieldName = paramdesc.SemanticName + paramdesc.SemanticIndex.ToString();//this 

                    switch (paramdesc.UsageMask)
                    {
                        case RegisterComponentMaskFlags.ComponentX:

                            switch (paramdesc.ComponentType)
                            {
                                case RegisterComponentType.Float32:
                                    vertexClass.Members.Add(new CodeMemberField("float", fieldName));
                                    sizeInBytes += 4;
                                    break;
                                case RegisterComponentType.SInt32:
                                    vertexClass.Members.Add(new CodeMemberField("short", fieldName));
                                    sizeInBytes += 2;
                                    break;
                                case RegisterComponentType.UInt32:
                                    vertexClass.Members.Add(new CodeMemberField("uint", fieldName));
                                    sizeInBytes += 4;
                                    break;
                                case RegisterComponentType.Unknown:
                                default:
                                    break;
                            }
                            break;
                        case RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY:
                            switch (paramdesc.ComponentType)
                            {
                                case RegisterComponentType.Float32:
                                    vertexClass.Members.Add(new CodeMemberField("Vector2", fieldName));
                                    sizeInBytes += 8;
                                    break;
                                case RegisterComponentType.SInt32:
                                    vertexClass.Members.Add(new CodeMemberField("short", fieldName + "_X"));
                                    vertexClass.Members.Add(new CodeMemberField("short", fieldName + "_Y"));
                                    sizeInBytes += 4;
                                    break;
                                case RegisterComponentType.UInt32:
                                    vertexClass.Members.Add(new CodeMemberField("uint", fieldName + "_X"));
                                    vertexClass.Members.Add(new CodeMemberField("uint", fieldName + "_Y"));
                                    sizeInBytes += 8;
                                    break;
                                case RegisterComponentType.Unknown:
                                default:
                                    break;
                            }
                            break;
                        case RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY | RegisterComponentMaskFlags.ComponentZ:
                            switch (paramdesc.ComponentType)
                            {
                                case RegisterComponentType.Float32:
                                    vertexClass.Members.Add(new CodeMemberField("Vector3", fieldName));
                                    sizeInBytes += 12;
                                    break;
                                case RegisterComponentType.SInt32:
                                    vertexClass.Members.Add(new CodeMemberField("short", fieldName + "_X"));
                                    vertexClass.Members.Add(new CodeMemberField("short", fieldName + "_Y"));
                                    vertexClass.Members.Add(new CodeMemberField("short", fieldName + "_Z"));
                                    sizeInBytes += 6;
                                    break;
                                case RegisterComponentType.UInt32:
                                    vertexClass.Members.Add(new CodeMemberField("uint", fieldName + "_X"));
                                    vertexClass.Members.Add(new CodeMemberField("uint", fieldName + "_Y"));
                                    vertexClass.Members.Add(new CodeMemberField("uint", fieldName + "_Z"));
                                    sizeInBytes += 12;
                                    break;
                                case RegisterComponentType.Unknown:
                                default:
                                    break;
                            }
                            break;
                        case RegisterComponentMaskFlags.All:
                            switch (paramdesc.ComponentType)
                            {
                                case RegisterComponentType.Float32:
                                    vertexClass.Members.Add(new CodeMemberField("Vector4", fieldName));
                                    sizeInBytes += 16;
                                    break;
                                case RegisterComponentType.SInt32:
                                    vertexClass.Members.Add(new CodeMemberField("short", fieldName + "_X"));
                                    vertexClass.Members.Add(new CodeMemberField("short", fieldName + "_Y"));
                                    vertexClass.Members.Add(new CodeMemberField("short", fieldName + "_Z"));
                                    vertexClass.Members.Add(new CodeMemberField("short", fieldName + "_W"));
                                    sizeInBytes += 8;
                                    break;
                                case RegisterComponentType.UInt32:
                                    vertexClass.Members.Add(new CodeMemberField("uint", fieldName + "_X"));
                                    vertexClass.Members.Add(new CodeMemberField("uint", fieldName + "_Y"));
                                    vertexClass.Members.Add(new CodeMemberField("uint", fieldName + "_Z"));
                                    vertexClass.Members.Add(new CodeMemberField("uint", fieldName + "_W"));
                                    sizeInBytes += 16;
                                    break;
                                case RegisterComponentType.Unknown:
                                default://not sure what to do here 
                                    break;
                            }
                            break;
                        case RegisterComponentMaskFlags.None:
                        default://not sure what to do here
                            break;
                    }

                }

                //calculate size of input struct
                vertexClass.CustomAttributes = new CodeAttributeDeclarationCollection(
                    new CodeAttributeDeclaration[]
                {
                    new CodeAttributeDeclaration("Serializable"),
                    new CodeAttributeDeclaration("StructLayout",new CodeAttributeArgument[]
                    {
                        new CodeAttributeArgument(new CodeSnippetExpression("LayoutKind.Sequential")),
                        new CodeAttributeArgument("Size",new CodeSnippetExpression(sizeInBytes.ToString()))//testing code
                    }),
                });
                ;



                //add input type to namespace
                targetNamespace.Types.Add(vertexClass);
            }
            //classes for the constant buffers

            //add samplers to the shaderclass     

            





            targetNamespace.Types.Add(targetClass);

            targetUnit.Namespaces.Add(targetNamespace);

            
            using (Microsoft.CSharp.CSharpCodeProvider codeProvider = new Microsoft.CSharp.CSharpCodeProvider())
            {
                CodeGeneratorOptions options = new CodeGeneratorOptions();
                options.BracingStyle = "C";
                
                using (StreamWriter sourceWriter = new StreamWriter(outputfilename))
                {
                    codeProvider.GenerateCodeFromCompileUnit(
                        targetUnit, sourceWriter, options);
                }
            }
           
            

        }


        private static void SetCommonImports(CodeNamespace targetNamespace)
        {
            targetNamespace.Imports.Add(new CodeNamespaceImport("System"));
            targetNamespace.Imports.Add(new CodeNamespaceImport("System.Runtime.InteropServices"));
            targetNamespace.Imports.Add(new CodeNamespaceImport("System.Runtime.Serialization"));
            targetNamespace.Imports.Add(new CodeNamespaceImport("System.Runtime.Serialization.Formatters.Binary"));
            targetNamespace.Imports.Add(new CodeNamespaceImport("SharpDX"));
            targetNamespace.Imports.Add(new CodeNamespaceImport("SharpDX.Direct3D11"));
            
        }
    }
}
