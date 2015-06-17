using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using ShaderClasses;

using SharpDX;

namespace Laboratory
{
    [TestClass]
    public class ShaderLibraryGeneratorTests
    {
        [TestMethod]
        public void TestSerialization()
        {   

            BinaryFormatter bf = new BinaryFormatter();

            MemoryStream ms = new MemoryStream();
            
            TestVertexShader_Vertex testWriteVertex = new TestVertexShader_Vertex(new Vector2(1,2),new Vector3(1,2,3),new Vector4(1,2,3,4),1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21);
            TestVertexShader_Vertex testReadVertex;

            bf.Serialize(ms,testWriteVertex);

            ms.Close();
            byte[] testVertexData = ms.ToArray();
            ms.Dispose();

            using (MemoryStream ms2 = new MemoryStream(testVertexData))
            {
                testReadVertex = (TestVertexShader_Vertex)bf.Deserialize(ms2);
            }

            Assert.IsFalse(testWriteVertex.Equals(testReadVertex));
            
            

        }
    }
}
