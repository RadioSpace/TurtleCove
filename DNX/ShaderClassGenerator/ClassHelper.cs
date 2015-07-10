using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.CodeDom;
using SharpDX;
using SharpDX.D3DCompiler;

namespace ShaderClassGenerator
{
    public class ClassHelper
    {

        static CodeTypeReference
            intRef = new CodeTypeReference(typeof(int)),
            uintRef = new CodeTypeReference(typeof(uint)),
            shortRef = new CodeTypeReference(typeof(short)),
            byteRef = new CodeTypeReference(typeof(byte)),
            floatRef = new CodeTypeReference(typeof(float)),
            doubleRef = new CodeTypeReference(typeof(double)),
            Vector2Ref = new CodeTypeReference(typeof(Vector2)),
            Vector3Ref = new CodeTypeReference(typeof(Vector3)),
            Vector4Ref = new CodeTypeReference(typeof(Vector4)),
            matrixRef = new CodeTypeReference(typeof(Matrix)),
            objectRef = new CodeTypeReference(typeof(object));

        public static CodeTypeReference GetCodeTypeReference(string typeName)
        {
            CodeTypeReference result = objectRef;

            switch (typeName)
            {
                case "int":
                    result = intRef;
                    break;
                case "uint":
                    result = uintRef;
                    break;
                case "short":
                    result = shortRef;
                    break;
                case "byte":
                    result = byteRef;
                    break;
                case "float":
                    result = floatRef;
                    break;
                case "double":
                    result = doubleRef;
                    break;
                case "Vector2":
                    result = Vector2Ref;
                    break;
                case "Vector3":
                    result = Vector3Ref;
                    break;
                case "Vector4":
                    result = Vector4Ref;
                    break;
                case "Matrix":
                    result = matrixRef;
                    break;
                //default is objectRef but result is already set                    
            }

            return result;
        }

        public static CodeTypeDeclaration[] GenerateConstantBufferClasses(params ConstantBuffer[] buffers)
        {

            //create class list
            List<CodeTypeDeclaration> cBufferClassList = new List<CodeTypeDeclaration>();


            //declare types CodeTypeReference


            List<CodeMemberField> primitiveFields = new List<CodeMemberField>();
            List<CodeMemberField> complexFields = new List<CodeMemberField>();

            List<CodeMemberProperty> primitiveProps = new List<CodeMemberProperty>();
            List<CodeMemberProperty> complexProps = new List<CodeMemberProperty>();


            //create buffer types
            foreach (ConstantBuffer cbuffer in buffers)
            {
                //start class
                CodeTypeDeclaration constantBufferClass = new CodeTypeDeclaration();
                constantBufferClass.Attributes = MemberAttributes.Public;


                constantBufferClass.Name = cbuffer.Description.Name;



                //create variables and don't forget the user data for GenerateEqualityCode()
                switch (cbuffer.Description.Type)
                {
                    case ConstantBufferType.ConstantBuffer:

                        int size = 0;

                        //add fields and properties
                        for (int x = 0; x < cbuffer.Description.VariableCount; x++)
                        {
                            ShaderReflectionVariable variable = cbuffer.GetVariable(x);
                            ShaderReflectionType variableType = variable.GetVariableType();

                            string fieldName = "_" + variable.Description.Name;
                            string propName = new string(variable.Description.Name.ToUpper().Take(1).ToArray()) + new string(variable.Description.Name.ToLower().Skip(1).ToArray());

                            //todo: implement the different Matrix types possible in HLSL 

                            //modify typename
                            string typeName = variableType.Description.Name;
                            if ((variableType.Description.Name == "<anonymous>") && (variableType.Description.Class == ShaderVariableClass.Vector))
                            {
                                switch (variableType.Description.ColumnCount)
                                {
                                    case 1:
                                        switch (variableType.Description.Type)
                                        {
                                            case ShaderVariableType.Float:
                                                typeName = "float";
                                                break;
                                            case ShaderVariableType.Int:
                                                typeName = "int";
                                                break;
                                            case ShaderVariableType.UInt:
                                                typeName = "uint";
                                                break;
                                            case ShaderVariableType.Double:
                                                typeName = "double";
                                                break;
                                        }

                                        break;
                                    case 2:
                                        switch (variableType.Description.Type)
                                        {
                                            case ShaderVariableType.Float:
                                                typeName = "float2";
                                                break;
                                            case ShaderVariableType.Int:
                                                typeName = "int2";
                                                break;
                                            case ShaderVariableType.UInt:
                                                typeName = "uint2";
                                                break;
                                            case ShaderVariableType.Double:
                                                typeName = "double2";
                                                break;
                                        }
                                        break;
                                    case 3:
                                        switch (variableType.Description.Type)
                                        {
                                            case ShaderVariableType.Float:
                                                typeName = "float3";
                                                break;
                                            case ShaderVariableType.Int:
                                                typeName = "int3";
                                                break;
                                            case ShaderVariableType.UInt:
                                                typeName = "uint3";
                                                break;
                                            case ShaderVariableType.Double:
                                                typeName = "double3";
                                                break;
                                        }
                                        break;
                                    case 4:
                                        switch (variableType.Description.Type)
                                        {
                                            case ShaderVariableType.Float:
                                                typeName = "float4";
                                                break;
                                            case ShaderVariableType.Int:
                                                typeName = "int4";
                                                break;
                                            case ShaderVariableType.UInt:
                                                typeName = "uint4";
                                                break;
                                            case ShaderVariableType.Double:
                                                typeName = "double4";
                                                break;
                                        }
                                        break;
                                }
                            }

                            switch (typeName)
                            {
                                case "float4x4":
                                    {
                                        //add matrixRef
                                        CodeMemberField newField = new CodeMemberField(matrixRef, fieldName);
                                        newField.UserData.Add("Type", "Matrix");
                                        newField.UserData.Add("Name", fieldName);
                                        CodeMemberProperty newProp = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName,
                                            Type = matrixRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newField.Name)));
                                        newProp.UserData.Add("Type", "Matrix");
                                        newProp.UserData.Add("Name", propName);

                                        constantBufferClass.Members.Add(newField);
                                        constantBufferClass.Members.Add(newProp);

                                        complexFields.Add(newField);
                                        complexProps.Add(newProp);

                                        size += 64;

                                        break;
                                    }
                                case "float":
                                    {
                                        CodeMemberField newField = new CodeMemberField(floatRef, fieldName + "_X");
                                        newField.UserData.Add("Type", "float");
                                        newField.UserData.Add("Name", fieldName + "_X");
                                        CodeMemberProperty newProp = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_X",
                                            Type = floatRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newField.Name)));
                                        newProp.UserData.Add("Type", "float");
                                        newProp.UserData.Add("Name", propName + "_X");

                                        constantBufferClass.Members.Add(newField);
                                        constantBufferClass.Members.Add(newProp);

                                        primitiveFields.Add(newField);
                                        primitiveProps.Add(newProp);

                                        size += 4;

                                        break;
                                    }
                                case "float2":
                                    {
                                        CodeMemberField newField = new CodeMemberField(Vector2Ref, fieldName);
                                        newField.UserData.Add("Type", "Vector2");
                                        newField.UserData.Add("Name", fieldName);
                                        CodeMemberProperty newProp = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName,
                                            Type = Vector2Ref,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newField.Name)));
                                        newProp.UserData.Add("Type", "Vector2");
                                        newProp.UserData.Add("Name", propName);

                                        constantBufferClass.Members.Add(newField);
                                        constantBufferClass.Members.Add(newProp);

                                        complexFields.Add(newField);
                                        complexProps.Add(newProp);
                                        size += 8;

                                        break;
                                    }
                                case "float3":
                                    {
                                        CodeMemberField newField = new CodeMemberField(Vector3Ref, fieldName);
                                        newField.UserData.Add("Type", "Vector3");
                                        newField.UserData.Add("Name", fieldName);
                                        CodeMemberProperty newProp = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName,
                                            Type = Vector3Ref,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newField.Name)));
                                        newProp.UserData.Add("Type", "Vector3");
                                        newProp.UserData.Add("Name", propName);

                                        constantBufferClass.Members.Add(newField);
                                        constantBufferClass.Members.Add(newProp);

                                        complexFields.Add(newField);
                                        complexProps.Add(newProp);

                                        size += 12;

                                        break;
                                    }
                                case "float4":
                                    {
                                        CodeMemberField newField = new CodeMemberField(Vector4Ref, fieldName);
                                        newField.UserData.Add("Type", "Vector4");
                                        newField.UserData.Add("Name", fieldName);
                                        CodeMemberProperty newProp = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName,
                                            Type = Vector4Ref,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newField.Name)));
                                        newProp.UserData.Add("Type", "Vector4");
                                        newProp.UserData.Add("Name", propName);

                                        constantBufferClass.Members.Add(newField);
                                        constantBufferClass.Members.Add(newProp);

                                        complexFields.Add(newField);
                                        complexProps.Add(newProp);

                                        size += 16;

                                        break;
                                    }
                                case "int":
                                    {
                                        CodeMemberField newField = new CodeMemberField(intRef, fieldName + "_X");
                                        newField.UserData.Add("Type", "int");
                                        newField.UserData.Add("Name", fieldName + "_X");
                                        CodeMemberProperty newProp = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_X",
                                            Type = intRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newField.Name)));
                                        newProp.UserData.Add("Type", "int");
                                        newProp.UserData.Add("Name", propName + "_X");

                                        constantBufferClass.Members.Add(newField);
                                        constantBufferClass.Members.Add(newProp);

                                        primitiveFields.Add(newField);
                                        primitiveProps.Add(newProp);

                                        size += 4;

                                        break;
                                    }
                                case "int2":
                                    {
                                        //X//////////
                                        CodeMemberField newFieldX = new CodeMemberField(intRef, fieldName + "_X");
                                        newFieldX.UserData.Add("Type", "int");
                                        newFieldX.UserData.Add("Name", fieldName + "_X");
                                        CodeMemberProperty newPropX = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_X",
                                            Type = intRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropX.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldX.Name)));
                                        newPropX.UserData.Add("Type", "int");
                                        newPropX.UserData.Add("Name", propName + "_X");


                                        //Y //////
                                        CodeMemberField newFieldY = new CodeMemberField(intRef, fieldName + "_Y");
                                        newFieldY.UserData.Add("Type", "int");
                                        newFieldY.UserData.Add("Name", fieldName + "_Y");
                                        CodeMemberProperty newPropY = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Y",
                                            Type = intRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropY.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldY.Name)));
                                        newPropY.UserData.Add("Type", "int");
                                        newPropY.UserData.Add("Name", propName + "_Y");

                                        constantBufferClass.Members.Add(newFieldX);
                                        constantBufferClass.Members.Add(newPropX);
                                        constantBufferClass.Members.Add(newFieldY);
                                        constantBufferClass.Members.Add(newPropY);

                                        primitiveFields.Add(newFieldX);
                                        primitiveProps.Add(newPropX);
                                        primitiveFields.Add(newFieldY);
                                        primitiveProps.Add(newPropY);

                                        size += 8;

                                        break;
                                    }
                                case "int3":
                                    {

                                        //X//////////
                                        CodeMemberField newFieldX = new CodeMemberField(intRef, fieldName + "_X");
                                        newFieldX.UserData.Add("Type", "int");
                                        newFieldX.UserData.Add("Name", fieldName + "_X");
                                        CodeMemberProperty newPropX = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_X",
                                            Type = intRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropX.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldX.Name)));
                                        newPropX.UserData.Add("Type", "int");
                                        newPropX.UserData.Add("Name", propName + "_X");


                                        //Y //////
                                        CodeMemberField newFieldY = new CodeMemberField(intRef, fieldName + "_Y");
                                        newFieldY.UserData.Add("Type", "int");
                                        newFieldY.UserData.Add("Name", fieldName + "_Y");
                                        CodeMemberProperty newPropY = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Y",
                                            Type = intRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropY.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldY.Name)));
                                        newPropY.UserData.Add("Type", "int");
                                        newPropY.UserData.Add("Name", propName + "_Y");

                                        //Z//////////
                                        CodeMemberField newFieldZ = new CodeMemberField(intRef, fieldName + "_Z");
                                        newFieldZ.UserData.Add("Type", "int");
                                        newFieldZ.UserData.Add("Name", fieldName + "_Z");
                                        CodeMemberProperty newPropZ = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Z",
                                            Type = intRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropZ.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldZ.Name)));
                                        newPropZ.UserData.Add("Type", "int");
                                        newPropZ.UserData.Add("Name", propName + "_Z");

                                        constantBufferClass.Members.Add(newFieldX);
                                        constantBufferClass.Members.Add(newPropX);
                                        constantBufferClass.Members.Add(newFieldY);
                                        constantBufferClass.Members.Add(newPropY);
                                        constantBufferClass.Members.Add(newFieldZ);
                                        constantBufferClass.Members.Add(newPropZ);

                                        primitiveFields.Add(newFieldX);
                                        primitiveProps.Add(newPropX);
                                        primitiveFields.Add(newFieldY);
                                        primitiveProps.Add(newPropY);
                                        primitiveFields.Add(newFieldZ);
                                        primitiveProps.Add(newPropZ);

                                        size += 12;

                                        break;
                                    }
                                case "int4":
                                    {
                                        //X//////////
                                        CodeMemberField newFieldX = new CodeMemberField(intRef, fieldName + "_X");
                                        newFieldX.UserData.Add("Type", "int");
                                        newFieldX.UserData.Add("Name", fieldName + "_X");
                                        CodeMemberProperty newPropX = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_X",
                                            Type = intRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropX.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldX.Name)));
                                        newPropX.UserData.Add("Type", "int");
                                        newPropX.UserData.Add("Name", propName + "_X");


                                        //Y //////
                                        CodeMemberField newFieldY = new CodeMemberField(intRef, fieldName + "_Y");
                                        newFieldY.UserData.Add("Type", "int");
                                        newFieldY.UserData.Add("Name", fieldName + "_Y");
                                        CodeMemberProperty newPropY = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Y",
                                            Type = intRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropY.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldY.Name)));
                                        newPropY.UserData.Add("Type", "int");
                                        newPropY.UserData.Add("Name", propName + "_Y");

                                        //Z//////////
                                        CodeMemberField newFieldZ = new CodeMemberField(intRef, fieldName + "_Z");
                                        newFieldZ.UserData.Add("Type", "int");
                                        newFieldZ.UserData.Add("Name", fieldName + "_Z");
                                        CodeMemberProperty newPropZ = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Z",
                                            Type = intRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropZ.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldZ.Name)));
                                        newPropZ.UserData.Add("Type", "int");
                                        newPropZ.UserData.Add("Name", propName + "_Z");

                                        //W//////////
                                        CodeMemberField newFieldW = new CodeMemberField(intRef, fieldName + "_W");
                                        newFieldW.UserData.Add("Type", "int");
                                        newFieldW.UserData.Add("Name", fieldName + "_W");
                                        CodeMemberProperty newPropW = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_W",
                                            Type = intRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropW.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldW.Name)));
                                        newPropW.UserData.Add("Type", "int");
                                        newPropW.UserData.Add("Name", propName + "_W");

                                        constantBufferClass.Members.Add(newFieldX);
                                        constantBufferClass.Members.Add(newPropX);
                                        constantBufferClass.Members.Add(newFieldY);
                                        constantBufferClass.Members.Add(newPropY);
                                        constantBufferClass.Members.Add(newFieldZ);
                                        constantBufferClass.Members.Add(newPropZ);
                                        constantBufferClass.Members.Add(newFieldW);
                                        constantBufferClass.Members.Add(newPropW);

                                        primitiveFields.Add(newFieldX);
                                        primitiveProps.Add(newPropX);
                                        primitiveFields.Add(newFieldY);
                                        primitiveProps.Add(newPropY);
                                        primitiveFields.Add(newFieldZ);
                                        primitiveProps.Add(newPropZ);
                                        primitiveFields.Add(newFieldW);
                                        primitiveProps.Add(newPropW);

                                        size += 16;

                                        break;
                                    }
                                case "dword":
                                case "uint":
                                    {
                                        CodeMemberField newField = new CodeMemberField(uintRef, fieldName + "_X");
                                        newField.UserData.Add("Type", "uint");
                                        newField.UserData.Add("Name", fieldName + "_X");
                                        CodeMemberProperty newProp = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_X",
                                            Type = uintRef,
                                            HasSet = false,
                                            HasGet = true

                                        };
                                        newProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newField.Name)));
                                        newProp.UserData.Add("Type", "uint");
                                        newProp.UserData.Add("Name", propName + "_X");

                                        constantBufferClass.Members.Add(newField);
                                        constantBufferClass.Members.Add(newProp);

                                        primitiveFields.Add(newField);
                                        primitiveProps.Add(newProp);

                                        size += 4;

                                        break;
                                    }
                                case "dword2":
                                case "uint2":
                                    {

                                        //X//////////
                                        CodeMemberField newFieldX = new CodeMemberField(uintRef, fieldName + "_X");
                                        newFieldX.UserData.Add("Type", "uint");
                                        newFieldX.UserData.Add("Name", fieldName + "_X");
                                        CodeMemberProperty newPropX = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_X",
                                            Type = uintRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropX.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldX.Name)));
                                        newPropX.UserData.Add("Type", "uint");
                                        newPropX.UserData.Add("Name", propName + "_X");


                                        //Y //////
                                        CodeMemberField newFieldY = new CodeMemberField(intRef, fieldName + "_Y");
                                        newFieldY.UserData.Add("Type", "uint");
                                        newFieldY.UserData.Add("Name", fieldName + "_Y");
                                        CodeMemberProperty newPropY = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Y",
                                            Type = uintRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropY.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldY.Name)));
                                        newPropY.UserData.Add("Type", "uint");
                                        newPropY.UserData.Add("Name", propName + "_Y");

                                        constantBufferClass.Members.Add(newFieldX);
                                        constantBufferClass.Members.Add(newPropX);
                                        constantBufferClass.Members.Add(newFieldY);
                                        constantBufferClass.Members.Add(newPropY);

                                        primitiveFields.Add(newFieldX);
                                        primitiveProps.Add(newPropX);
                                        primitiveFields.Add(newFieldY);
                                        primitiveProps.Add(newPropY);

                                        size += 8;

                                        break;
                                    }
                                case "dword3":
                                case "uint3":
                                    {

                                        //X//////////
                                        CodeMemberField newFieldX = new CodeMemberField(uintRef, fieldName + "_X");
                                        newFieldX.UserData.Add("Type", "uint");
                                        newFieldX.UserData.Add("Name", fieldName + "_X");
                                        CodeMemberProperty newPropX = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_X",
                                            Type = uintRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropX.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldX.Name)));
                                        newPropX.UserData.Add("Type", "uint");
                                        newPropX.UserData.Add("Name", propName + "_X");


                                        //Y //////
                                        CodeMemberField newFieldY = new CodeMemberField(intRef, fieldName + "_Y");
                                        newFieldY.UserData.Add("Type", "uint");
                                        newFieldY.UserData.Add("Name", fieldName + "_Y");
                                        CodeMemberProperty newPropY = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Y",
                                            Type = uintRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropY.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldY.Name)));
                                        newPropY.UserData.Add("Type", "uint");
                                        newPropY.UserData.Add("Name", propName + "_Y");

                                        //Z//////////
                                        CodeMemberField newFieldZ = new CodeMemberField(uintRef, fieldName + "_Z");
                                        newFieldZ.UserData.Add("Type", "uint");
                                        newFieldZ.UserData.Add("Name", fieldName + "_Z");
                                        CodeMemberProperty newPropZ = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Z",
                                            Type = uintRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropZ.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldZ.Name)));
                                        newPropZ.UserData.Add("Type", "uint");
                                        newPropZ.UserData.Add("Name", propName + "_Z");

                                        constantBufferClass.Members.Add(newFieldX);
                                        constantBufferClass.Members.Add(newPropX);
                                        constantBufferClass.Members.Add(newFieldY);
                                        constantBufferClass.Members.Add(newPropY);
                                        constantBufferClass.Members.Add(newFieldZ);
                                        constantBufferClass.Members.Add(newPropZ);

                                        primitiveFields.Add(newFieldX);
                                        primitiveProps.Add(newPropX);
                                        primitiveFields.Add(newFieldY);
                                        primitiveProps.Add(newPropY);
                                        primitiveFields.Add(newFieldZ);
                                        primitiveProps.Add(newPropZ);

                                        size += 12;

                                        break;
                                    }
                                case "dword4":
                                case "uint4":
                                    {
                                        //X//////////
                                        CodeMemberField newFieldX = new CodeMemberField(uintRef, fieldName + "_X");
                                        newFieldX.UserData.Add("Type", "uint");
                                        newFieldX.UserData.Add("Name", fieldName + "_X");
                                        CodeMemberProperty newPropX = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_X",
                                            Type = uintRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropX.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldX.Name)));
                                        newPropX.UserData.Add("Type", "uint");
                                        newPropX.UserData.Add("Name", propName + "_X");


                                        //Y //////
                                        CodeMemberField newFieldY = new CodeMemberField(uintRef, fieldName + "_Y");
                                        newFieldY.UserData.Add("Type", "uint");
                                        newFieldY.UserData.Add("Name", fieldName + "_Y");
                                        CodeMemberProperty newPropY = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Y",
                                            Type = uintRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropY.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldY.Name)));
                                        newPropY.UserData.Add("Type", "uint");
                                        newPropY.UserData.Add("Name", propName + "_Y");

                                        //Z//////////
                                        CodeMemberField newFieldZ = new CodeMemberField(uintRef, fieldName + "_Z");
                                        newFieldZ.UserData.Add("Type", "uint");
                                        newFieldZ.UserData.Add("Name", fieldName + "_Z");
                                        CodeMemberProperty newPropZ = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Z",
                                            Type = uintRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropZ.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldZ.Name)));
                                        newPropZ.UserData.Add("Type", "uint");
                                        newPropZ.UserData.Add("Name", propName + "_Z");

                                        //W//////////
                                        CodeMemberField newFieldW = new CodeMemberField(uintRef, fieldName + "_W");
                                        newFieldW.UserData.Add("Type", "uint");
                                        newFieldW.UserData.Add("Name", fieldName + "_W");
                                        CodeMemberProperty newPropW = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_W",
                                            Type = uintRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropW.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldW.Name)));
                                        newPropW.UserData.Add("Type", "uint");
                                        newPropW.UserData.Add("Name", propName + "_W");

                                        constantBufferClass.Members.Add(newFieldX);
                                        constantBufferClass.Members.Add(newPropX);
                                        constantBufferClass.Members.Add(newFieldY);
                                        constantBufferClass.Members.Add(newPropY);
                                        constantBufferClass.Members.Add(newFieldZ);
                                        constantBufferClass.Members.Add(newPropZ);
                                        constantBufferClass.Members.Add(newFieldW);
                                        constantBufferClass.Members.Add(newPropW);

                                        primitiveFields.Add(newFieldX);
                                        primitiveProps.Add(newPropX);
                                        primitiveFields.Add(newFieldY);
                                        primitiveProps.Add(newPropY);
                                        primitiveFields.Add(newFieldZ);
                                        primitiveProps.Add(newPropZ);
                                        primitiveFields.Add(newFieldW);
                                        primitiveProps.Add(newPropW);

                                        size += 16;

                                        break;
                                    }
                                case "double":
                                    {

                                        CodeMemberField newField = new CodeMemberField(doubleRef, fieldName + "_X");
                                        newField.UserData.Add("Type", "double");
                                        newField.UserData.Add("Name", fieldName + "_X");
                                        CodeMemberProperty newProp = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_X",
                                            Type = doubleRef,
                                            HasSet = false,
                                            HasGet = true

                                        };
                                        newProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newField.Name)));
                                        newProp.UserData.Add("Type", "double");
                                        newProp.UserData.Add("Name", propName + "_X");

                                        constantBufferClass.Members.Add(newField);
                                        constantBufferClass.Members.Add(newProp);

                                        primitiveFields.Add(newField);
                                        primitiveProps.Add(newProp);

                                        size += 4;

                                        break;
                                    }
                                case "double2":
                                    {
                                        //X//////////
                                        CodeMemberField newFieldX = new CodeMemberField(doubleRef, fieldName + "_X");
                                        newFieldX.UserData.Add("Type", "double");
                                        newFieldX.UserData.Add("Name", fieldName + "_X");
                                        CodeMemberProperty newPropX = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_X",
                                            Type = doubleRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropX.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldX.Name)));
                                        newPropX.UserData.Add("Type", "double");
                                        newPropX.UserData.Add("Name", propName + "_X");


                                        //Y //////
                                        CodeMemberField newFieldY = new CodeMemberField(doubleRef, fieldName + "_Y");
                                        newFieldY.UserData.Add("Type", "double");
                                        newFieldY.UserData.Add("Name", fieldName + "_Y");
                                        CodeMemberProperty newPropY = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Y",
                                            Type = doubleRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropY.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldY.Name)));
                                        newPropY.UserData.Add("Type", "double");
                                        newPropY.UserData.Add("Name", propName + "_Y");

                                        constantBufferClass.Members.Add(newFieldX);
                                        constantBufferClass.Members.Add(newPropX);
                                        constantBufferClass.Members.Add(newFieldY);
                                        constantBufferClass.Members.Add(newPropY);

                                        primitiveFields.Add(newFieldX);
                                        primitiveProps.Add(newPropX);
                                        primitiveFields.Add(newFieldY);
                                        primitiveProps.Add(newPropY);

                                        size += 8;

                                        break;
                                    }
                                case "double3":
                                    {

                                        //X//////////
                                        CodeMemberField newFieldX = new CodeMemberField(doubleRef, fieldName + "_X");
                                        newFieldX.UserData.Add("Type", "double");
                                        newFieldX.UserData.Add("Name", fieldName + "_X");
                                        CodeMemberProperty newPropX = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_X",
                                            Type = doubleRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropX.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldX.Name)));
                                        newPropX.UserData.Add("Type", "double");
                                        newPropX.UserData.Add("Name", propName + "_X");


                                        //Y //////
                                        CodeMemberField newFieldY = new CodeMemberField(doubleRef, fieldName + "_Y");
                                        newFieldY.UserData.Add("Type", "double");
                                        newFieldY.UserData.Add("Name", fieldName + "_Y");
                                        CodeMemberProperty newPropY = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Y",
                                            Type = doubleRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropY.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldY.Name)));
                                        newPropY.UserData.Add("Type", "double");
                                        newPropY.UserData.Add("Name", propName + "_Y");

                                        //Z//////////
                                        CodeMemberField newFieldZ = new CodeMemberField(doubleRef, fieldName + "_Z");
                                        newFieldZ.UserData.Add("Type", "double");
                                        newFieldZ.UserData.Add("Name", fieldName + "_Z");
                                        CodeMemberProperty newPropZ = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Z",
                                            Type = doubleRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropZ.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldZ.Name)));
                                        newPropZ.UserData.Add("Type", "double");
                                        newPropZ.UserData.Add("Name", propName + "_Z");

                                        constantBufferClass.Members.Add(newFieldX);
                                        constantBufferClass.Members.Add(newPropX);
                                        constantBufferClass.Members.Add(newFieldY);
                                        constantBufferClass.Members.Add(newPropY);
                                        constantBufferClass.Members.Add(newFieldZ);
                                        constantBufferClass.Members.Add(newPropZ);

                                        primitiveFields.Add(newFieldX);
                                        primitiveProps.Add(newPropX);
                                        primitiveFields.Add(newFieldY);
                                        primitiveProps.Add(newPropY);
                                        primitiveFields.Add(newFieldZ);
                                        primitiveProps.Add(newPropZ);

                                        size += 12;

                                        break;
                                    }
                                case "double4":
                                    {

                                        //X//////////
                                        CodeMemberField newFieldX = new CodeMemberField(doubleRef, fieldName + "_X");
                                        newFieldX.UserData.Add("Type", "double");
                                        newFieldX.UserData.Add("Name", fieldName + "_X");
                                        CodeMemberProperty newPropX = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_X",
                                            Type = doubleRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropX.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldX.Name)));
                                        newPropX.UserData.Add("Type", "double");
                                        newPropX.UserData.Add("Name", propName + "_X");


                                        //Y //////
                                        CodeMemberField newFieldY = new CodeMemberField(doubleRef, fieldName + "_Y");
                                        newFieldY.UserData.Add("Type", "double");
                                        newFieldY.UserData.Add("Name", fieldName + "_Y");
                                        CodeMemberProperty newPropY = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Y",
                                            Type = doubleRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropY.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldY.Name)));
                                        newPropY.UserData.Add("Type", "double");
                                        newPropY.UserData.Add("Name", propName + "_Y");

                                        //Z//////////
                                        CodeMemberField newFieldZ = new CodeMemberField(doubleRef, fieldName + "_Z");
                                        newFieldZ.UserData.Add("Type", "double");
                                        newFieldZ.UserData.Add("Name", fieldName + "_Z");
                                        CodeMemberProperty newPropZ = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_Z",
                                            Type = doubleRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropZ.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldZ.Name)));
                                        newPropZ.UserData.Add("Type", "double");
                                        newPropZ.UserData.Add("Name", propName + "_Z");

                                        //W//////////
                                        CodeMemberField newFieldW = new CodeMemberField(doubleRef, fieldName + "_W");
                                        newFieldW.UserData.Add("Type", "double");
                                        newFieldW.UserData.Add("Name", fieldName + "_W");
                                        CodeMemberProperty newPropW = new CodeMemberProperty()
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Final,//final removes the virtual attribute
                                            Name = propName + "_W",
                                            Type = doubleRef,
                                            HasSet = false,
                                            HasGet = true,

                                        };
                                        newPropW.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, newFieldW.Name)));
                                        newPropW.UserData.Add("Type", "double");
                                        newPropW.UserData.Add("Name", propName + "_W");

                                        constantBufferClass.Members.Add(newFieldX);
                                        constantBufferClass.Members.Add(newPropX);
                                        constantBufferClass.Members.Add(newFieldY);
                                        constantBufferClass.Members.Add(newPropY);
                                        constantBufferClass.Members.Add(newFieldZ);
                                        constantBufferClass.Members.Add(newPropZ);
                                        constantBufferClass.Members.Add(newFieldW);
                                        constantBufferClass.Members.Add(newPropW);

                                        primitiveFields.Add(newFieldX);
                                        primitiveProps.Add(newPropX);
                                        primitiveFields.Add(newFieldY);
                                        primitiveProps.Add(newPropY);
                                        primitiveFields.Add(newFieldZ);
                                        primitiveProps.Add(newPropZ);
                                        primitiveFields.Add(newFieldW);
                                        primitiveProps.Add(newPropW);

                                        size += 16;

                                        break;
                                    }

                                default: throw new NotSupportedException(variableType.Description.Name + " is not supported");
                            }

                        }

                        //add attributes

                        constantBufferClass.CustomAttributes.AddRange(new CodeAttributeDeclarationCollection(
                        new CodeAttributeDeclaration[]
                        {
                            new CodeAttributeDeclaration("Serializable"),
                            new CodeAttributeDeclaration("StructLayout",new CodeAttributeArgument[]
                            {
                                new CodeAttributeArgument(new CodeSnippetExpression("LayoutKind.Sequential")),                                
                                new CodeAttributeArgument("Size",new CodeSnippetExpression((cbuffer.Description.Size).ToString()))                                
                            })
                        }));

                        //add serilization code

                        if (complexFields.Count > 0)
                        {//account for members that aren't marked serilizable
                            MakeSerilizable(constantBufferClass);
                        }

                        //add constructor code

                        AddConstructor(constantBufferClass);


                        //add equals code
                        GenerateEqualityCode(constantBufferClass);

                        //add to class list

                        cBufferClassList.Add(constantBufferClass);


                        break;
                    case ConstantBufferType.InterfacePointers:

                        //create a class that stores the ClassInstances
                        //this will be tricky 
                        for (int x = 0; x < cbuffer.Description.VariableCount; x++)
                        {
                            ShaderReflectionVariable variable = cbuffer.GetVariable(x);
                            ShaderReflectionType variableType = variable.GetVariableType();
                        }

                        break;
                    case ConstantBufferType.ResourceBindInformation:
                        //??
                        for (int x = 0; x < cbuffer.Description.VariableCount; x++)
                        {
                            ShaderReflectionVariable variable = cbuffer.GetVariable(x);
                            ShaderReflectionType variableType = variable.GetVariableType();
                        }
                        break;
                    case ConstantBufferType.TextureBuffer:
                        //Texture2D ??
                        for (int x = 0; x < cbuffer.Description.VariableCount; x++)
                        {
                            ShaderReflectionVariable variable = cbuffer.GetVariable(x);
                            ShaderReflectionType variableType = variable.GetVariableType();
                        }
                        break;
                    default:
                        break;
                }
            }

            //return classes to codecompile unit

            return cBufferClassList.ToArray();

        }


        
    

        /// <summary>
        /// adds the standard equality stuff to a type declration. (See notes for how to set up your public properties)
        /// </summary>
        /// <param name="type">the type to add the code to</param>
        /// <remarks>UserData must be included {"Type":"int" ; "Name":"_Position0_X"}</remarks>
        /// <example>
        /// string fieldName = codef.UserData["Name"];
        /// string typeName = codeobject.UserData
        /// </example>
        public static void GenerateEqualityCode(CodeTypeDeclaration type)
        { 
            //add IEquatable
            CodeTypeReference iEquatable = new CodeTypeReference("IEquatable<" + type.Name + ">");

            if(!type.BaseTypes.Contains(iEquatable))
            {
                type.BaseTypes.Add(iEquatable);

                //implement Iequatable
                CodeMemberMethod equals = new CodeMemberMethod();
                equals.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                equals.ReturnType = new CodeTypeReference(typeof(bool));
                equals.Name = "Equals";

                List<CodeTypeMember> publicProperties = new List<CodeTypeMember>();


                //we will need to depend on userdata for the type and feild name
                foreach (CodeTypeMember member in type.Members)
                {
                    //Member attributes is a union of 2 flag types but is not marked with the flags attribute
                    //WTF
                    
                    int wtf = (int)member.Attributes ;
                    int wtf2 = wtf & (int)MemberAttributes.AccessMask;

                    if (member is CodeMemberProperty && wtf2 == (int)MemberAttributes.Public)
                    {
                        publicProperties.Add(member);                    
                    }
                    
                }
                CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement();
                equals.Parameters.Add(new CodeParameterDeclarationExpression(type.Name, "other"));

                CodeBinaryOperatorExpression isEqualExpression = new CodeBinaryOperatorExpression();

                bool firsttime = true;
                string expression = "";

                for (int x = 0; x < publicProperties.Count; x++)
                {
                    CodeMemberProperty property = (CodeMemberProperty)publicProperties[x];


                    if (property.HasGet)
                    {
                        string typeName = (string)property.UserData["Type"];
                        string fieldName = (string)property.UserData["Name"];
                        string otherName = "other.";                      
                        
                        
                        //add statments
                        for (int y = x; y < publicProperties.Count; y++)
                        {
                            if(firsttime)
                            {
                                expression += "(" + otherName + fieldName + " == " + fieldName + ")";
                                firsttime = false;
                            }
                            else
                            {
                                expression += " && (" + otherName + fieldName + " == " + fieldName + ")"; 
                            }
                        }                        
                    }
                }

                returnStatement = new CodeMethodReturnStatement(new CodeSnippetExpression(expression));

                equals.Statements.Add(returnStatement);
                type.Members.Add(equals);

                //override equals
                CodeMemberMethod overrideEquals = new CodeMemberMethod();
                overrideEquals.Attributes = MemberAttributes.Override | MemberAttributes.Public ;
                overrideEquals.ReturnType = new CodeTypeReference(typeof(bool));
                overrideEquals.Name = "Equals";

                overrideEquals.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "other"));

                //if statement
                CodeConditionStatement ifOtherIsType = new CodeConditionStatement();
                ifOtherIsType.Condition = new CodeSnippetExpression("other is " + type.Name);
                ifOtherIsType.TrueStatements.Add(new CodeSnippetExpression("return Equals(other)"));
                ifOtherIsType.FalseStatements.Add(new CodeSnippetExpression("return false"));

                overrideEquals.Statements.Add(ifOtherIsType);

                type.Members.Add(overrideEquals);


                //static operator ==
                CodeParameterDeclarationExpression param_A = new CodeParameterDeclarationExpression(type.Name, "a");
                CodeParameterDeclarationExpression param_B = new CodeParameterDeclarationExpression(type.Name, "b");

                CodeMemberMethod isEqualOperator = new CodeMemberMethod();
                isEqualOperator.Attributes = MemberAttributes.Static | MemberAttributes.Public;
                isEqualOperator.ReturnType = new CodeTypeReference(typeof(bool));
                isEqualOperator.Name = "operator==";

                isEqualOperator.Parameters.Add(param_A);
                isEqualOperator.Parameters.Add(param_B);

                isEqualOperator.Statements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression("a.Equals(b)")));

                type.Members.Add(isEqualOperator);

                //static operator !=
                CodeMemberMethod isNotEqualOperator = new CodeMemberMethod();
                isNotEqualOperator.Attributes = MemberAttributes.Static | MemberAttributes.Public;
                isNotEqualOperator.ReturnType = new CodeTypeReference(typeof(bool));
                isNotEqualOperator.Name = "operator!=";

                isNotEqualOperator.Parameters.Add(param_A);
                isNotEqualOperator.Parameters.Add(param_B);

                isNotEqualOperator.Statements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression("!a.Equals(b)")));

                type.Members.Add(isNotEqualOperator);

                 //get hashcode

                CodeMemberMethod gethashCode = new CodeMemberMethod();
                gethashCode.Attributes = MemberAttributes.Public | MemberAttributes.Override;
                gethashCode.ReturnType = new CodeTypeReference(typeof(int));
                gethashCode.Name = "GetHashCode";

                gethashCode.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(int)),"hash = 17"));

                
                for (int x = 0; x < publicProperties.Count-1; x++)
                {                    
                   gethashCode.Statements.Add(new CodeSnippetExpression("hash = hash * 31 + " + publicProperties[x].Name + ".GetHashCode()"));
                }

                gethashCode.Statements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression("hash * 31 + " + publicProperties[publicProperties.Count - 1].Name + ".GetHashCode()")));

                type.Members.Add(gethashCode);

            }

            
           

            
                
            
        }

        
        
        private static void AddConstructor(CodeTypeDeclaration constantBufferClass)
        {
            CodeConstructor Constructor = new CodeConstructor();
            Constructor.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            
            foreach (CodeTypeMember member in constantBufferClass.Members)
            {
                if (member is CodeMemberField)
                {
                    CodeMemberField field = (CodeMemberField)member;                    

                    string typeString = (string)field.UserData["Type"];
                    string fName = (string)field.UserData["Name"];
                    string argName = fName + "_";

                    Constructor.Statements.Add(new CodeSnippetExpression(fName + " = " + argName));
                    
                    Constructor.Parameters.Add(new CodeParameterDeclarationExpression(GetCodeTypeReference(typeString), argName));
                }
            }

            constantBufferClass.Members.Add(Constructor);
        }

        /// <summary>
        /// makes the class serilizable 
        /// </summary>
        /// <param name="codeClass"></param>
        /// <param name="members"></param>
        public static void MakeSerilizable(CodeTypeDeclaration codeClass)
        {

            //initilize Iserializable object
            codeClass.BaseTypes.Add(typeof(System.Runtime.Serialization.ISerializable));

            CodeParameterDeclarationExpression serializeParam1 = new CodeParameterDeclarationExpression("SerializationInfo", "info");
            CodeParameterDeclarationExpression serializeParam2 = new CodeParameterDeclarationExpression("StreamingContext", "context");



            //add serilization method
            CodeMemberMethod serializeMethod = new CodeMemberMethod();
            serializeMethod.Name = "GetObjectData";
            serializeMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            serializeMethod.Parameters.Add(serializeParam1);
            serializeMethod.Parameters.Add(serializeParam2);

            CodeConstructor serializationConstructor = new CodeConstructor();
            serializationConstructor.Parameters.Add(serializeParam1);
            serializationConstructor.Parameters.Add(serializeParam2);

            List<CodeMemberField> fields = new List<CodeMemberField>();
  
            foreach(CodeTypeMember member in codeClass.Members)
            {
                if (member is CodeMemberField)
                {
                    fields.Add((CodeMemberField)member);
                }            
            }

            foreach(CodeMemberField field in fields)
            {

                    string typeString = (string)field.UserData["Type"];
                    string fName = (string)field.UserData["Name"];
                    string argName = fName + "_";

                    switch (typeString)
                    {
                        case "float":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "\", " + fName + ")"));
                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = info.GetSingle(\"" + fName + "\")"));
                            break;

                        case "Vector2":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));

                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector2(info.GetSingle(\"" + fName + "_X\"),info.GetSingle(\"" + fName + "_Y\"))"));
                            break;

                        case "Vector3":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Z\", " + fName + ".Z)"));

                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector3(info.GetSingle(\"" + fName + "_X\"),info.GetSingle(\"" + fName + "_Y\"), info.GetSingle(\"" + fName + "_Z\"))"));
                            break;

                        case "Vector4":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Z\", " + fName + ".Z)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_W\", " + fName + ".W)"));

                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector4(info.GetSingle(\"" + fName + "_X\"),info.GetSingle(\"" + fName + "_Y\"), info.GetSingle(\"" + fName + "_Z\"), info.GetSingle(\"" + fName + "_W\"))"));
                            break;

                        case "int":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "\", " + fName + ")"));
                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = info.GetInt32(\"" + fName + "\")"));
                            break;
                        case "int2":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));

                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector2(info.GetInt32(\"" + fName + "_X\"),info.GetInt32(\"" + fName + "_Y\"))"));
                            break;
                        case "int3":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Z\", " + fName + ".Z)"));

                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector3(info.GetInt32(\"" + fName + "_X\"),info.GetInt32(\"" + fName + "_Y\"), info.GetInt32(\"" + fName + "_Z\"))"));
                            break;
                        case "int4":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Z\", " + fName + ".Z)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_W\", " + fName + ".W)"));

                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector4(info.GetInt32(\"" + fName + "_X\"),info.GetInt32(\"" + fName + "_Y\"), info.GetInt32(\"" + fName + "_Z\"), info.GetInt32(\"" + fName + "_W\"))"));
                            break;

                        case "uint":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "\", " + fName + ")"));
                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = info.GetUInt32(\"" + fName + "\")"));
                            break;
                        case "uint2":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));

                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector2(info.GetUInt32(\"" + fName + "_X\"),info.GetUInt32(\"" + fName + "_Y\"))"));
                            break;
                        case "uint3":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Z\", " + fName + ".Z)"));

                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector3(info.GetUInt32(\"" + fName + "_X\"),info.GetUInt32(\"" + fName + "_Y\"), info.GetUInt32(\"" + fName + "_Z\"))"));
                            break;
                        case "uint4":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Z\", " + fName + ".Z)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_W\", " + fName + ".W)"));

                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector4(info.GetUInt32(\"" + fName + "_X\"),info.GetUInt32(\"" + fName + "_Y\"), info.GetUInt32(\"" + fName + "_Z\"), info.GetUInt32(\"" + fName + "_W\"))"));
                            break;

                        


                        case "double":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "\", " + fName + ")"));
                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = info.GetDouble(\"" + fName + "\")"));
                            break;
                        case "double2":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));

                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector2(info.GetDouble(\"" + fName + "_X\"),info.GetDouble(\"" + fName + "_Y\"))"));
                            break;
                        case "double3":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Z\", " + fName + ".Z)"));

                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector3(info.GetDouble(\"" + fName + "_X\"),info.GetDouble(\"" + fName + "_Y\"), info.GetDouble(\"" + fName + "_Z\"))"));
                            break;
                        case "double4":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_X\", " + fName + ".X)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Y\", " + fName + ".Y)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_Z\", " + fName + ".Z)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_W\", " + fName + ".W)"));

                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Vector4(info.GetDouble(\"" + fName + "_X\"),info.GetDouble(\"" + fName + "_Y\"), info.GetDouble(\"" + fName + "_Z\"), info.GetDouble(\"" + fName + "_W\"))"));
                            break;

                        case "Matrix":
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M11\", " + fName + ".M11)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M12\", " + fName + ".M12)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M13\", " + fName + ".M13)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M14\", " + fName + ".M14)"));

                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M21\", " + fName + ".M21)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M22\", " + fName + ".M22)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M23\", " + fName + ".M23)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M24\", " + fName + ".M24)"));

                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M31\", " + fName + ".M31)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M32\", " + fName + ".M32)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M33\", " + fName + ".M33)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M34\", " + fName + ".M34)"));

                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M41\", " + fName + ".M41)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M42\", " + fName + ".M42)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M43\", " + fName + ".M43)"));
                            serializeMethod.Statements.Add(new CodeSnippetExpression("info.AddValue(\"" + fName + "_M44\", " + fName + ".M44)"));

                            serializationConstructor.Statements.Add(new CodeSnippetExpression(fName + " = new Matrix(info.GetSingle(\"" +fName + "_M11\"), info.GetSingle(\"" +fName + "_M12\"), info.GetSingle(\"" +fName + "_M13\"), info.GetSingle(\"" +fName + "_M14\"), info.GetSingle(\"" +fName + "_M21\"), info.GetSingle(\"" +fName + "_M22\"), info.GetSingle(\"" +fName + "_M23\"), info.GetSingle(\"" +fName + "_M24\"), info.GetSingle(\"" +fName + "_M31\"), info.GetSingle(\"" +fName + "_M32\"), info.GetSingle(\"" +fName + "_M33\"), info.GetSingle(\"" +fName + "_M34\"), info.GetSingle(\"" +fName + "_M41\"), info.GetSingle(\"" +fName + "_M42\"), info.GetSingle(\"" +fName + "_M43\"), info.GetSingle(\"" +fName + "_M44\"))"));
                            break;

                        default: throw new NotImplementedException("the type " + typeString + ", is not supported");
                    }

                    
                    

                
            }
            //add code
            codeClass.Members.Add(serializeMethod);
            codeClass.Members.Add(serializationConstructor);



        }

        /// <summary>
        /// makes the class disposable but does not add the method
        /// </summary>
        /// <param name="codeClass">the class to make disposable</param>
        /// <returns>a Method you will need to add to the class later after you have added the disposal statements</returns>
        public static CodeMemberMethod MakeDisposable(CodeTypeDeclaration codeClass)
        {
            codeClass.BaseTypes.Add(new CodeTypeReference(typeof(IDisposable)));


            CodeMemberMethod disposeMethod = new CodeMemberMethod();
            disposeMethod.Name = "Dispose";
            disposeMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            return disposeMethod;
        }

        public static void AddDisposalOfMember(CodeMemberMethod disposeMethod, string memberName)
        {
            CodeBinaryOperatorExpression isnull = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(memberName), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
            CodeExpressionStatement callDispose = new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(memberName), "Dispose"));

            //add dispose code
            disposeMethod.Statements.Add(new CodeConditionStatement(isnull, callDispose));
        }
    }
}
