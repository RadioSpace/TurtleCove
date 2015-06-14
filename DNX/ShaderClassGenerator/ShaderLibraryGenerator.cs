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
            CodeTypeDeclaration inputClass = new CodeTypeDeclaration(targetClass.Name + "_Vertex");
            inputClass.IsStruct = true;
            inputClass.CustomAttributes = new CodeAttributeDeclarationCollection(
                new CodeAttributeDeclaration[]
                {
                    new CodeAttributeDeclaration("Serializable"),
                    new CodeAttributeDeclaration("StructLayout",new CodeAttributeArgument[]
                    {
                        new CodeAttributeArgument(new CodeSnippetExpression("LayoutKind.Sequential")),
                        new CodeAttributeArgument("Size",new CodeSnippetExpression("LayoutKind.Sequential"))//testing code
                    }),
                });



            //add input type to namespace
            targetNamespace.Types.Add(inputClass);

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
