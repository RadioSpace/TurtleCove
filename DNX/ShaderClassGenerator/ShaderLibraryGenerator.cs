using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;



using System.CodeDom;
using System.CodeDom.Compiler;

using System.Reflection;

using System.Runtime.Serialization;

using SharpDX;
using SharpDX.D3DCompiler;

namespace ShaderClassGenerator
{

    public class ShaderLibraryGenerator
    {
        
        public void Generate(string filepath, CompiledShaderReader reader)
        {
           CodeCommentStatement notNeededComment = new CodeCommentStatement("this is generated whether or not it is needed");


           string classname = Path.GetFileNameWithoutExtension(filepath);
           string outputfilename = Path.ChangeExtension(filepath, ".cs");

           CodeCompileUnit targetUnit = new CodeCompileUnit();



           CodeNamespace targetNamespace = new CodeNamespace("ShaderClasses");
           SetCommonImports(targetNamespace);


           CodeTypeDeclaration targetClass = new CodeTypeDeclaration(Path.GetFileNameWithoutExtension(filepath));
           targetClass.IsClass = true;
           targetClass.TypeAttributes = TypeAttributes.Public;




           if (reader.IsVertexShader)
            {
                //add a the input layout field

                CodeMemberField inputLayoutMemberField = new CodeMemberField("InputLayout", "inputLayout");
                inputLayoutMemberField.Comments.Add(notNeededComment);

                targetClass.Members.Add(inputLayoutMemberField);


                //create a class to store vertex  information
                CodeTypeDeclaration vertexClass = new CodeTypeDeclaration(targetClass.Name + "_Vertex");
                vertexClass.IsStruct = true;



                //a list to store the codemembers that are vectors so we can write the serialization code
                List<CodeTypeMember> Vectors = new List<CodeTypeMember>(); // vectors are not serializable
                List<CodeTypeMember> OtherMembers = new List<CodeTypeMember>();

                //get the input parameters to build our Vertex type
                ShaderParameterDescription[] paramdesciptions = reader.GetParameterDescription();

                int sizeInBytes = 0;

                //test for SV_VertexID only
                if (paramdesciptions.All(a => a.SemanticName == "SV_VertexID"))
                {
                    //no buffer needed
                }
                else
                {
                    //read the vertex input parameters
                    foreach (ShaderParameterDescription paramdesc in paramdesciptions)
                    {
                        string propertyname = new string(paramdesc.SemanticName.Take(1).ToArray()) + new string(paramdesc.SemanticName.ToLower().Skip(1).ToArray()) + paramdesc.SemanticIndex.ToString();//this may change if I find a better way to generate unique names that make sense
                        string fieldName = "_" + propertyname;

                        //NOTE: this contains the constructor code for all the fields as well


                        //Usage Mask was not set-up correctly in SharpDX so we need to mask it to get the expected results
                        //https://github.com/sharpdx/SharpDX/issues/565
                        
                        int UsageMask = ((int)paramdesc.UsageMask) & (int)RegisterComponentMaskFlags.All;

                        CodeMemberField iFX = new CodeMemberField(typeof(float), fieldName + "_X");
                        iFX.UserData.Add("Type", "float");
                        iFX.UserData.Add("Name", fieldName + "_X");
                        CodeMemberProperty iFXProp = new CodeMemberProperty()
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                            Name = propertyname + "_X",
                            Type = new CodeTypeReference(typeof(float)),
                            HasSet = false,
                            HasGet = true,
                            
                        };
                        iFXProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, iFX.Name)));
                        iFXProp.UserData.Add("Type", "int");
                        iFXProp.UserData.Add("Name", fieldName + "_X");

                        CodeMemberField iSX = new CodeMemberField(typeof(int), fieldName + "_X");
                        iSX.UserData.Add("Type", "int");
                        iSX.UserData.Add("Name", fieldName + "_X");
                        CodeMemberProperty iSXProp = new CodeMemberProperty()
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                            Name = propertyname + "_X",
                            Type = new CodeTypeReference(typeof(int)),
                            HasSet = false,
                            HasGet = true
                        };
                        iSXProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, iSX.Name)));
                        iSXProp.UserData.Add("Type", "int");
                        iSXProp.UserData.Add("Name", fieldName + "_X");

                        CodeMemberField iSY = new CodeMemberField(typeof(int), fieldName + "_Y");
                        iSY.UserData.Add("Type", "int");
                        iSY.UserData.Add("Name", fieldName + "_Y");
                        CodeMemberProperty iSYProp = new CodeMemberProperty()
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                            Name = propertyname + "_Y",
                            Type = new CodeTypeReference(typeof(int)),
                            HasSet = false,
                            HasGet = true
                        };
                        iSYProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, iSY.Name)));
                        iSYProp.UserData.Add("Type", "int");
                        iSYProp.UserData.Add("Name", fieldName + "_Y");

                        CodeMemberField iSZ = new CodeMemberField(typeof(int), fieldName + "_Z");
                        iSZ.UserData.Add("Type", "int");
                        iSZ.UserData.Add("Name", fieldName + "_Z");
                        CodeMemberProperty iSZProp = new CodeMemberProperty()
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                            Name = propertyname + "_Z",
                            Type = new CodeTypeReference(typeof(int)),
                            HasSet = false,
                            HasGet = true
                        };
                        iSZProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, iSZ.Name)));
                        iSZProp.UserData.Add("Type", "int");
                        iSZProp.UserData.Add("Name", fieldName + "_Z");

                        CodeMemberField iSW = new CodeMemberField(typeof(int), fieldName + "_W");
                        iSW.UserData.Add("Type", "int");
                        iSW.UserData.Add("Name", fieldName + "_W");
                        CodeMemberProperty iSWProp = new CodeMemberProperty()
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                            Name = propertyname + "_W",
                            Type = new CodeTypeReference(typeof(int)),
                            HasSet = false,
                            HasGet = true
                        };
                        iSWProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, iSW.Name)));
                        iSWProp.UserData.Add("Type", "int");
                        iSWProp.UserData.Add("Name", fieldName + "_W");

                        CodeMemberField iUX = new CodeMemberField(typeof(uint), fieldName + "_X");
                        iUX.UserData.Add("Type", "uint");
                        iUX.UserData.Add("Name", fieldName + "_X");
                        CodeMemberProperty iUXProp = new CodeMemberProperty()
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                            Name = propertyname + "_X",
                            Type = new CodeTypeReference(typeof(uint)),
                            HasSet = false,
                            HasGet = true
                        };
                        iUXProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, iUX.Name)));
                        iUXProp.UserData.Add("Type", "uint");
                        iUXProp.UserData.Add("Name", fieldName + "_X");


                        CodeMemberField iUY = new CodeMemberField(typeof(uint), fieldName + "_Y");
                        iUY.UserData.Add("Type", "uint");
                        iUY.UserData.Add("Name", fieldName + "_Y");
                        CodeMemberProperty iUYProp = new CodeMemberProperty()
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                            Name = propertyname + "_Y",
                            Type = new CodeTypeReference(typeof(uint)),
                            HasSet = false,
                            HasGet = true
                        };
                        iUYProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, iUY.Name)));
                        iUYProp.UserData.Add("Type", "uint");
                        iUYProp.UserData.Add("Name", fieldName + "_Y");

                        CodeMemberField iUZ = new CodeMemberField(typeof(uint), fieldName + "_Z");
                        iUZ.UserData.Add("Type", "uint");
                        iUZ.UserData.Add("Name", fieldName + "_Z");
                        CodeMemberProperty iUZProp = new CodeMemberProperty()
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                            Name = propertyname + "_Z",
                            Type = new CodeTypeReference(typeof(uint)),
                            HasSet = false,
                            HasGet = true
                        };
                        iUZProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, iUZ.Name)));
                        iUZProp.UserData.Add("Type", "uint");
                        iUZProp.UserData.Add("Name", fieldName + "_Z");

                        CodeMemberField iUW = new CodeMemberField(typeof(uint), fieldName + "_W");
                        iUW.UserData.Add("Type", "uint");
                        iUW.UserData.Add("Name", fieldName + "_W");
                        CodeMemberProperty iUWProp = new CodeMemberProperty()
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                            Name = propertyname + "_W",
                            Type = new CodeTypeReference(typeof(uint)),
                            HasSet = false,
                            HasGet = true
                        };
                        iUWProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, iUW.Name)));
                        iUWProp.UserData.Add("Type", "uint");
                        iUWProp.UserData.Add("Name", fieldName + "_W");

                        switch (UsageMask)
                        {
                            case (int)RegisterComponentMaskFlags.ComponentX:

                                switch (paramdesc.ComponentType)
                                {
                                    case RegisterComponentType.Float32:
                                        vertexClass.Members.Add(iFX);
                                        vertexClass.Members.Add(iFXProp);
                                        sizeInBytes += 4;

                                        OtherMembers.Add(iFX);
                                        break;
                                    case RegisterComponentType.SInt32:
                                        vertexClass.Members.Add(iSX);
                                        vertexClass.Members.Add(iSXProp);
                                        sizeInBytes += 4;
                                        OtherMembers.Add(iSX);
                                        break;
                                    case RegisterComponentType.UInt32:
                                        vertexClass.Members.Add(iUX);
                                        vertexClass.Members.Add(iUXProp);
                                        sizeInBytes += 4;
                                        OtherMembers.Add(iUX);
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

                                        CodeMemberProperty vectorProp = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,
                                            Name = propertyname,
                                            Type = new CodeTypeReference(typeof(Vector2)),
                                            HasGet = true,
                                            HasSet = false,

                                        };
                                        vectorProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, fieldName)));
                                        vectorProp.UserData.Add("Type", "Vector2");
                                        vectorProp.UserData.Add("Name", fieldName);

                                        vertexClass.Members.Add(vector);
                                        vertexClass.Members.Add(vectorProp);
                                        sizeInBytes += 8;

                                        Vectors.Add(vector);

                                        break;
                                    case RegisterComponentType.SInt32:
                                        vertexClass.Members.Add(iSX);
                                        vertexClass.Members.Add(iSXProp);

                                        vertexClass.Members.Add(iSY);
                                        vertexClass.Members.Add(iSYProp);
                                        sizeInBytes += 8;

                                        OtherMembers.Add(iSX);
                                        OtherMembers.Add(iSY);

                                        break;
                                    case RegisterComponentType.UInt32:
                                        vertexClass.Members.Add(iUX);
                                        vertexClass.Members.Add(iUXProp);

                                        vertexClass.Members.Add(iUY);
                                        vertexClass.Members.Add(iUYProp);
                                        sizeInBytes += 8;

                                        OtherMembers.Add(iUX);
                                        OtherMembers.Add(iUY);

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

                                        CodeMemberProperty vectorProp = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,
                                            Name = propertyname,
                                            Type = new CodeTypeReference(typeof(Vector3)),
                                            HasGet = true,
                                            HasSet = false,

                                        };
                                        vectorProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, fieldName)));
                                        vectorProp.UserData.Add("Type", "Vector3");
                                        vectorProp.UserData.Add("Name", fieldName);


                                        vertexClass.Members.Add(vector);
                                        vertexClass.Members.Add(vectorProp);
                                        sizeInBytes += 12;

                                        Vectors.Add(vector);
                                        break;
                                    case RegisterComponentType.SInt32:
                                        vertexClass.Members.Add(iSX);
                                        vertexClass.Members.Add(iSY);
                                        vertexClass.Members.Add(iSZ);

                                        vertexClass.Members.Add(iSXProp);
                                        vertexClass.Members.Add(iSYProp);
                                        vertexClass.Members.Add(iSZProp);

                                        sizeInBytes += 12;

                                        OtherMembers.Add(iSX);
                                        OtherMembers.Add(iSY);
                                        OtherMembers.Add(iSZ);
                                        break;
                                    case RegisterComponentType.UInt32:
                                        vertexClass.Members.Add(iUX);
                                        vertexClass.Members.Add(iUY);
                                        vertexClass.Members.Add(iUZ);

                                        vertexClass.Members.Add(iUXProp);
                                        vertexClass.Members.Add(iUYProp);
                                        vertexClass.Members.Add(iUZProp);

                                        sizeInBytes += 12;

                                        OtherMembers.Add(iUX);
                                        OtherMembers.Add(iUY);
                                        OtherMembers.Add(iUZ);
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

                                        CodeMemberProperty vectorProp = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,
                                            Name = propertyname,
                                            Type = new CodeTypeReference(typeof(Vector4)),
                                            HasGet = true,
                                            HasSet = false,

                                        };
                                        vectorProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, fieldName)));
                                        vectorProp.UserData.Add("Type", "Vector4");
                                        vectorProp.UserData.Add("Name", fieldName);


                                        vertexClass.Members.Add(vector);
                                        vertexClass.Members.Add(vectorProp);
                                        sizeInBytes += 16;

                                        Vectors.Add(vector);
                                        break;
                                    case RegisterComponentType.SInt32:
                                        vertexClass.Members.Add(iSX);
                                        vertexClass.Members.Add(iSY);
                                        vertexClass.Members.Add(iSZ);
                                        vertexClass.Members.Add(iSW);

                                        vertexClass.Members.Add(iSXProp);
                                        vertexClass.Members.Add(iSYProp);
                                        vertexClass.Members.Add(iSZProp);
                                        vertexClass.Members.Add(iSWProp);

                                        sizeInBytes += 16;

                                        OtherMembers.Add(iSX);
                                        OtherMembers.Add(iSY);
                                        OtherMembers.Add(iSZ);
                                        OtherMembers.Add(iSW);
                                        break;
                                    case RegisterComponentType.UInt32:
                                        vertexClass.Members.Add(iUX);
                                        vertexClass.Members.Add(iUY);
                                        vertexClass.Members.Add(iUZ);
                                        vertexClass.Members.Add(iUW);

                                        vertexClass.Members.Add(iUXProp);
                                        vertexClass.Members.Add(iUYProp);
                                        vertexClass.Members.Add(iUZProp);
                                        vertexClass.Members.Add(iUWProp);

                                        sizeInBytes += 16;

                                        OtherMembers.Add(iUX);
                                        OtherMembers.Add(iUY);
                                        OtherMembers.Add(iUZ);
                                        OtherMembers.Add(iUW);
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

                    //calculate size of input struct and add attributes
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

                        ClassHelper.MakeSerilizable(vertexClass);
                       
                    }

                    //generate Equality code
                    ClassHelper.GenerateEqualityCode(vertexClass);


                    //add input type to namespace
                    targetNamespace.Types.Add(vertexClass);
                }

            }

           //add constant buffer classes

            

           CodeTypeDeclaration[] cBufferClasses = ClassHelper.GenerateConstantBufferClasses(reader.GetConsttantBuffers());
           targetNamespace.Types.AddRange(cBufferClasses);
          
            
           targetClass.Members.AddRange(cBufferClasses.ToList().ConvertAll(a => new CodeMemberField(a.Name, "_" + a.Name)).ToArray());
           targetClass.Members.AddRange(
               cBufferClasses.ToList().ConvertAll(a =>
                   {
                       CodeMemberProperty result  = new CodeMemberProperty();
                       result.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                       result.Type = new CodeTypeReference(a.Name);
                       result.Name = a.Name;
                       result.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, "_" + a.Name)));

                       return result;

                   }).ToArray());



          
           


            //add samplers to the shaderclass     



            //add directX objects 

            //add dispose code




            //wrap up /////////////////////////////////////////////////////////////////////////////////////////////////////
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
