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

        
        public static void GenerateConstantBuffers(CodeTypeDeclaration type,params ConstantBuffer[] buffers)
        {
            foreach (ConstantBuffer cbuffer in buffers)
            {

                try
                {
                    int i = cbuffer.Description.VariableCount;
                }
                catch(AccessViolationException EX)
                {
                    continue;
                }


                switch (cbuffer.Description.Type)
                {
                    case ConstantBufferType.ConstantBuffer:

                        for (int x = 0; x < cbuffer.Description.VariableCount; x++)
                        {
                           

                            ShaderReflectionVariable variable = cbuffer.GetVariable(x);
                            ShaderReflectionType variableType = variable.GetVariableType();
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

            }
        }
    }
}
