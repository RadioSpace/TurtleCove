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
                targetClass.Members.Add(new CodeMemberField("InputLayout", "inputLayout"));

                //create a class to store vertex  information
                CodeTypeDeclaration vertexClass = new CodeTypeDeclaration(targetClass.Name + "_Vertex");
                vertexClass.IsStruct = true;

                //a list to store the codemembers that are vectors so we can write the serialization code
                List<CodeTypeMember> Vectors = new List<CodeTypeMember>(); // vectors are not serializable

                ShaderParameterDescription[] paramdesciptions = reader.GetParameterDescription();

                int sizeInBytes = 0;



                foreach (ShaderParameterDescription paramdesc in paramdesciptions)
                {
                    string fieldName = paramdesc.SemanticName + paramdesc.SemanticIndex.ToString();//this 

                    //Usage Mask was not set-up correctly in SharpDX so we need to mask it to get the expected results
                    //https://github.com/sharpdx/SharpDX/issues/565

                    int wtf = (int)paramdesc.UsageMask;
                    int wtf2 = wtf & (int)RegisterComponentMaskFlags.All;

                    switch (wtf2)
                    {
                        case (int)RegisterComponentMaskFlags.ComponentX:

                            switch (paramdesc.ComponentType)
                            {
                                case RegisterComponentType.Float32:
                                    vertexClass.Members.Add(new CodeMemberField("float", fieldName));
                                    sizeInBytes += 4;
                                    break;
                                case RegisterComponentType.SInt32:
                                    vertexClass.Members.Add(new CodeMemberField("int", fieldName));
                                    sizeInBytes += 4;
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
                        case (int)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY):
                            switch (paramdesc.ComponentType)
                            {
                                case RegisterComponentType.Float32:
                                    CodeMemberField vector = new CodeMemberField("Vector2", fieldName);
                                    vector.UserData.Add("Type", "Vector2");
                                    vector.UserData.Add("Name", fieldName);

                                    vertexClass.Members.Add(vector);
                                    sizeInBytes += 8;

                                    Vectors.Add(vector);

                                    break;
                                case RegisterComponentType.SInt32:
                                    vertexClass.Members.Add(new CodeMemberField("int", fieldName + "_X"));
                                    vertexClass.Members.Add(new CodeMemberField("int", fieldName + "_Y"));
                                    sizeInBytes += 8;
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
                        case (int)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY | RegisterComponentMaskFlags.ComponentZ):
                            switch (paramdesc.ComponentType)
                            {
                                case RegisterComponentType.Float32:
                                    CodeMemberField vector = new CodeMemberField("Vector3", fieldName);
                                    vector.UserData.Add("Type", "Vector3");
                                    vector.UserData.Add("Name", fieldName);

                                    vertexClass.Members.Add(vector);
                                    sizeInBytes += 12;

                                    Vectors.Add(vector);
                                    break;
                                case RegisterComponentType.SInt32:
                                    vertexClass.Members.Add(new CodeMemberField("int", fieldName + "_X"));
                                    vertexClass.Members.Add(new CodeMemberField("int", fieldName + "_Y"));
                                    vertexClass.Members.Add(new CodeMemberField("int", fieldName + "_Z"));
                                    sizeInBytes += 12;
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
                        case (int)RegisterComponentMaskFlags.All:
                            switch (paramdesc.ComponentType)
                            {
                                case RegisterComponentType.Float32:
                                    CodeMemberField vector = new CodeMemberField("Vector4", fieldName);
                                    vector.UserData.Add("Type", "Vector4");
                                    vector.UserData.Add("Name", fieldName);

                                    vertexClass.Members.Add(vector);
                                    sizeInBytes += 16;

                                    Vectors.Add(vector);
                                    break;
                                case RegisterComponentType.SInt32:
                                    vertexClass.Members.Add(new CodeMemberField("int", fieldName + "_X"));
                                    vertexClass.Members.Add(new CodeMemberField("int", fieldName + "_Y"));
                                    vertexClass.Members.Add(new CodeMemberField("int", fieldName + "_Z"));
                                    vertexClass.Members.Add(new CodeMemberField("int", fieldName + "_W"));
                                    sizeInBytes += 16;
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
                        case (int)RegisterComponentMaskFlags.None:
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
                            new CodeAttributeArgument("Size",new CodeSnippetExpression(sizeInBytes.ToString()))
                        }),
                    });


                //build serialization method
                if (Vectors.Count > 0)
                {
                    //implement Iserializable

                    //add serilization code
                    CodeMemberMethod serializeMethod = new CodeMemberMethod();
                    serializeMethod.Name = "GetObjectData";

                    serializeMethod.Parameters.Add(new CodeParameterDeclarationExpression("SerializationInfo", "info"));
                    serializeMethod.Parameters.Add(new CodeParameterDeclarationExpression("StreamingContext", "context"));

                    foreach (CodeMemberField field in Vectors)
                    {
                        string typeString = (string)field.UserData["Type"];
                        string fName = (string)field.UserData["Name"];

                        switch (typeString)
                        {
                            case "Vector2":
                                serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                                serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));
                                break;
                            case "Vector3":
                                serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                                serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));
                                serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Z\", " + fName + ".Z)"));
                                break;
                            case "Vector4":
                                serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                                serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));
                                serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Z\", " + fName + ".Z)"));
                                serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_W\", " + fName + ".W)"));
                                break;
                            default: throw new NotImplementedException("the type " + typeString + ", is not supported");
                        }

                    }

                    CodeConstructor serializationConstructor = new CodeConstructor();
                    serializationConstructor.Parameters.Add(new CodeParameterDeclarationExpression("SerializationInfo", "info"));
                    serializationConstructor.Parameters.Add(new CodeParameterDeclarationExpression("StreamingContext", "context"));

                    foreach (CodeMemberField field in Vectors)
                    {
                        string typeString = (string)field.UserData["Type"];
                        string fName = (string)field.UserData["Name"];

                        switch (typeString)
                        {
                            case "Vector2":
                                serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector2(info.GetSingle(\"" + fName + "_X\"),info.GetSingle(\"" + fName + "_Y\"))"));
                                break;
                            case "Vector3":
                                serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector3(info.GetSingle(\"" + fName + "_X\"),info.GetSingle(\"" + fName + "_Y\"), info.GetSingle(\"" + fName + "_Z\"))"));
                                break;
                            case "Vector4":
                                serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector4(info.GetSingle(\"" + fName + "_X\"),info.GetSingle(\"" + fName + "_Y\"), info.GetSingle(\"" + fName + "_Z\"), info.GetSingle(\"" + fName + "_W\"))"));
                                break;
                            default: throw new NotImplementedException("the type " + typeString + ", is not supported");

                        }

                    }

                    //Add serilization method

                    vertexClass.Members.Add(serializeMethod);
                    vertexClass.Members.Add(serializationConstructor);


                    //add input type to namespace
                    targetNamespace.Types.Add(vertexClass);
                }

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
