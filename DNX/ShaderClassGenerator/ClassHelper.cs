using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.CodeDom;
using SharpDX;

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

                for (int x = 0; x < publicProperties.Count; x++)
                {
                    CodeMemberProperty property = (CodeMemberProperty)publicProperties[x];

                    if (property.HasGet)
                    {
                        string typeName = (string)property.UserData["Type"];
                        string fieldName = (string)property.UserData["Name"] + "_";


                        switch (typeName)
                        {
                            case "Vector2":                              
                            case "Vector3":                             
                            case "Vector4":
                                equals.Parameters.Add(new CodeParameterDeclarationExpression(typeName, fieldName));
                                break;
                            case "int":
                                equals.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), fieldName));
                                break;
                            case "uint":
                                equals.Parameters.Add(new CodeParameterDeclarationExpression(typeof(uint), fieldName));
                                break;
                            case "float":
                                equals.Parameters.Add(new CodeParameterDeclarationExpression(typeof(float), fieldName));
                                break;
                            default: throw new InvalidOperationException(typeName + " is not supported");
                        }
                        //add statments
                        for (int y = x; y < publicProperties.Count; y++)
                        {

                            equals.Statements.Add(
                                new CodeBinaryOperatorExpression(
                                    new CodePropertyReferenceExpression(null,publicProperties[x].Name),
                                    CodeBinaryOperatorType.IdentityEquality,
                                    new CodePropertyReferenceExpression(null,publicProperties[y].Name +"_")));
                        
                        }

                    }
                }

                type.Members.Add(equals);
            }

            
            //overide
                
            
        }

    }
}
