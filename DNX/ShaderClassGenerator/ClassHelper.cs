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

        
        public static CodeTypeDeclaration[] GenerateConstantBufferClasses(CodeNamespace nameSpace, params ConstantBuffer[] buffers)
        {

            //create class list
            List<CodeTypeDeclaration> cBufferClassList = new List<CodeTypeDeclaration>();


            //declare types CodeTypeReference
            CodeTypeReference
                intRef = new CodeTypeReference(typeof(int)),
                uintRef = new CodeTypeReference(typeof(uint)),
                shortRef = new CodeTypeReference(typeof(short)),
                byteRef = new CodeTypeReference(typeof(byte)),
                floatRef = new CodeTypeReference(typeof(float)),
                doubleRef = new CodeTypeReference(typeof(double)),
                Vector2Ref = new CodeTypeReference(typeof(Vector2)),
                Vector3Ref = new CodeTypeReference(typeof(Vector3)),
                Vector4Ref = new CodeTypeReference(typeof(Vector4)),
                matrixRef = new CodeTypeReference(typeof(Matrix));

            List<CodeMemberField> primitiveFields = new List<CodeMemberField>();
            List<CodeMemberField> complexFields = new List<CodeMemberField>();

            List<CodeMemberProperty> primitiveProps = new List<CodeMemberProperty>();
            List<CodeMemberProperty> complexProps = new List<CodeMemberProperty>();



            foreach (ConstantBuffer cbuffer in buffers)
            {
                //start class
                CodeTypeDeclaration constantBufferClass = new CodeTypeDeclaration();
                constantBufferClass.Attributes = MemberAttributes.Public;
                constantBufferClass.CustomAttributes.Add(new CodeAttributeDeclaration("Serializable"));
                constantBufferClass.Name = cbuffer.Description.Name;

                

                //create variables and don't forget the user data for GenerateEqualityCode()
                switch (cbuffer.Description.Type)
                {
                    case ConstantBufferType.ConstantBuffer:
                        for (int x = 0; x < cbuffer.Description.VariableCount; x++)
                        {
                            ShaderReflectionVariable variable = cbuffer.GetVariable(x);
                            ShaderReflectionType variableType = variable.GetVariableType();

                            string fieldName = "_" + variable.Description.Name;
                            string propName = new string(variable.Description.Name.ToUpper().Take(1).ToArray()) + new string(variable.Description.Name.ToLower().Skip(1).ToArray());

                            



                            switch (variableType.Description.Name)
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
                                            Name = propName,
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
                                            Name = propName,
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
                                            Name = propName,
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
                                            Name = propName,
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
                                        
                                        break; 
                                    }
                                case "int3":
                                    { break; }
                                case "int4":
                                    { break; }
                                case "uint":
                                    { break; }
                                case "uint2":
                                    { break; }
                                case "uint3":
                                    { break; }
                                case "uint4":
                                    { break; }
                                case "double":
                                    { break; }
                                case "double2":
                                    { break; }
                                case "double3":
                                    { break; }
                                case "double4":
                                    { break; }
                                
                                  
                                default: throw new NotSupportedException(variableType.Description.Name + " is not suppoerted");
                            }

                        }

                        break;
                    case ConstantBufferType.InterfacePointers:
                        //??
                        break;
                    case ConstantBufferType.ResourceBindInformation:
                        //??
                        break;
                    case ConstantBufferType.TextureBuffer:
                        //Texture2D ? ?
                        break;
                    default:
                        break;
                }

                //add serilization code


                //add constructor code



                //add equals code


                //add to class list
                cBufferClassList.Add(constantBufferClass);


            }

            //return classes to codecompile unit

            return cBufferClassList.ToArray();

        }
    }
}
